using EemRdx.Helpers;
using EemRdx.Models;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace EemRdx.Networking
{
	public static class ChatMessages
	{
		internal const string EemChatCommandPrefix = "/eemdev";
		private const string HelpPrefix = "help";
		private const string InitFactionPrefix = "initfactions";
		private const string GetCivlStandingsPrefix = "getcivlstandings";
		private const string SetPeacePrefix = "setpeace";

		public static void HandleChatMessage(string message)
		{
			IMyPlayer localPlayer = MyAPIGateway.Session.Player;
			if (localPlayer.PromoteLevel < MyPromoteLevel.Admin)
			{
				Messaging.ShowLocalNotification($"You must be an Administrator to invoke EEM Chat Commands.  Current Rank: {localPlayer.PromoteLevel.ToString()}");
				return;
			}

			string[] chatCommand = message.Split(' ');

			if (chatCommand.Length < 2)
			{
				PrintHelpCommands();
				return;
			}
				

			switch (chatCommand[1])
			{
				case HelpPrefix:
					PrintHelpCommands();
					break;
				case SetPeacePrefix:
					if (chatCommand.Length < 4)
					{
						Messaging.ShowLocalNotification($"{SetPeacePrefix}: Invalid number of factions (2 required)");
						break;
					}
					IMyFaction leftPeaceFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(chatCommand[2]);
					IMyFaction rightPeaceFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(chatCommand[3]);
					if (leftPeaceFaction == null)
					{
						Messaging.ShowLocalNotification($"{SetPeacePrefix}: Faction tag {chatCommand[2]} is invalid.");
						break;
					}
					if (rightPeaceFaction == null)
					{
						Messaging.ShowLocalNotification($"{SetPeacePrefix}: Faction tag {chatCommand[3]} is invalid.");
						break;
					}
					Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.DeclarePeaceMessagePrefix, leftPeaceFaction.FactionId, rightPeaceFaction.FactionId));
					Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.AcceptPeaceMessagePrefix, rightPeaceFaction.FactionId, leftPeaceFaction.FactionId));
					break;
				case InitFactionPrefix:
					Messaging.ShowLocalNotification($"InitFactionPrefix: AiSessionCore.IsServer{AiSessionCore.IsServer} MyAPIGateway.Multiplayer.IsServer: {MyAPIGateway.Multiplayer.IsServer}");
					//Factions.NpcFactionPeace();
					foreach (IMyFaction leftFaction in Factions.LawfulFactions)
					{
						foreach (IMyFaction rightFaction in Factions.LawfulFactions)
							if (leftFaction != rightFaction)
								if (!leftFaction.IsPeacefulTo(rightFaction))
								{
									Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.DeclarePeaceMessagePrefix, leftFaction.FactionId, rightFaction.FactionId));
									Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.AcceptPeaceMessagePrefix, rightFaction.FactionId, leftFaction.FactionId));
								}
					}
					break;
				case GetCivlStandingsPrefix:
					Messaging.ShowLocalNotification($"GetCivlStandings: AiSessionCore.IsServer{AiSessionCore.IsServer} MyAPIGateway.Multiplayer.IsServer: {MyAPIGateway.Multiplayer.IsServer}");
					IMyFaction civl = MyAPIGateway.Session.Factions.TryGetFactionByTag("CIVL");
					foreach (IMyFaction rightFaction in Factions.LawfulFactions)
						Messaging.ShowLocalNotification($"The relationship between {civl.Tag} and {rightFaction.Tag} is {MyAPIGateway.Session.Factions.GetRelationBetweenFactions(civl.FactionId, rightFaction.FactionId)}");
					break;
				default:
					PrintHelpCommands();
					return;
			}
		}

		/// <summary>
		/// Prints a list of available commands
		/// </summary>  
		private static void PrintHelpCommands()
		{
			Messaging.ShowLocalNotification($"'{EemChatCommandPrefix} {HelpPrefix}' will show this message");
			Messaging.ShowLocalNotification($"'{EemChatCommandPrefix} {InitFactionPrefix}' will initialize factions to their default settings with one another");
			Messaging.ShowLocalNotification($"'{EemChatCommandPrefix} {GetCivlStandingsPrefix}' will show the standings between CIVL and all other lawful factions");
			Messaging.ShowLocalNotification($"'{EemChatCommandPrefix} {SetPeacePrefix} <tag1> <tag2>' will declare peace between the two factions");
		}
	}
}
