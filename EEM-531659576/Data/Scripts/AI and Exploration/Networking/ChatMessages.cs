using System;
using System.Collections.Generic;
using EemRdx.Helpers;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace EemRdx.Networking
{
	public static class ChatMessages
	{
		internal const string EemChatCommandPrefix = "/eemdev";
		private const string HelpPrefix = "help";
		private const string GetCivlStandingsPrefix = "getcivlstandings";
		private const string ShowDebugLogPrefix = "showdebuglog";
		private const string ShowProfilingLogPrefix = "showprofilinglog";
		private const string ShowGeneralLogPrefix = "showgenerallog";

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
				case ShowDebugLogPrefix:
					if (!Constants.DebugMode)
					{
						Messaging.ShowLocalNotification("Debug mode is not enabled");
						break;
					}
					AiSessionCore.DebugLog.GetTailMessages();
					break;
				case ShowProfilingLogPrefix:
					if (!Constants.EnableProfilingLog)
					{
						Messaging.ShowLocalNotification("Profiling is not enabled");
						break;
					}
					AiSessionCore.ProfilingLog.GetTailMessages();
					break;
				case ShowGeneralLogPrefix:
					if (Constants.EnableGeneralLog)
					{
						Messaging.ShowLocalNotification("General logging is not enabled");
						break;
					}
					AiSessionCore.GeneralLog.GetTailMessages();
					break;
				case GetCivlStandingsPrefix:
					List<string> standings = new List<string>();
					Messaging.ShowLocalNotification($"GetCivlStandings: AiSessionCore.IsServer: {AiSessionCore.IsServer} MyAPIGateway.Multiplayer.IsServer: {MyAPIGateway.Multiplayer.IsServer} Factions.PlayerFactionInitComplete: {Factions.Factions.PlayerFactionInitComplete}");
					IMyFaction civl = MyAPIGateway.Session.Factions.TryGetFactionByTag("CIVL");
					foreach (KeyValuePair<long, IMyFaction> faction in MyAPIGateway.Session.Factions.Factions)
						standings.Add($"The relationship between {civl.Tag} and {faction.Value.Tag} is {MyAPIGateway.Session.Factions.GetRelationBetweenFactions(civl.FactionId, faction.Key)}");
					MyAPIGateway.Utilities.ShowMissionScreen("CIVL Standings", "", "", string.Join($"{Environment.NewLine}", standings.ToArray()));
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
			Messaging.ShowLocalNotification($"'{EemChatCommandPrefix} {GetCivlStandingsPrefix}' will show the standings between CIVL and all other lawful factions");
			Messaging.ShowLocalNotification($"'{EemChatCommandPrefix} {ShowDebugLogPrefix}' will show the last 20 lines of the Debug Log");
			Messaging.ShowLocalNotification($"'{EemChatCommandPrefix} {ShowProfilingLogPrefix}' will show the last 20 lines of the Profiling Log");
			Messaging.ShowLocalNotification($"'{EemChatCommandPrefix} {ShowGeneralLogPrefix}' will show the last 20 lines of the General Log");
		}
	}
}
