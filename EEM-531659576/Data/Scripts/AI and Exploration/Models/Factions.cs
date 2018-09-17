using System.Collections.Generic;
using System.Linq;
using EemRdx.Extensions;
using EemRdx.Helpers;
using EemRdx.Networking;
using EemRdx.Scripts.Utilities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

// ReSharper disable MemberCanBePrivate.Global

namespace EemRdx.Models
{
	public static class Factions
	{
		public static bool SetupComplete;
		
		internal static List<FactionsAtWar> FactionsAtWar { get; set; }

		/// <summary>
		/// These factions are considered lawful. When they go hostile towards someone,
		/// they also make the police (SEPD) and army (UCMF) go hostile.
		/// </summary>
		public static List<string> LawfulFactionsTags { get; } = new List<string>
		{
			"UCMF", "SEPD", "CIVL", "ISTG", "MA-I", "EXMC", "KUSS", "HS", "AMPH", "IMDC" };

		public static List<string> AllNpcFactions { get; } = new List<string>
		{
			"SPRT", "CIVL", "UCMF", "SEPD", "ISTG", "AMPH", "KUSS", "HS", "MMEC", "MA-I", "EXMC", "IMDC"
		};

		public static List<string> EnforcementFactionsTags { get; } = new List<string>
		{
			"SEPD", "UCMF"
		};

		public static IEnumerable<string> PlayerFactionExclusionList { get; } = new List<string>
		{
			"Pirate", "Rogue", "Outlaw", "Bandit"
		};

		private enum Disposition
		{
			Lawful, Enforcement, Pirate, Player
		}

		#region Extension Methods

		/// <summary>
		/// Returns rue when a faction is considered Lawful, such as with SEPD and CIVL
		/// </summary>
		public static bool IsLawful(this IMyFaction checkFaction)
		{
			return LawfulFactionDictionary.ContainsKey(checkFaction.FactionId);
		}

		/// <summary>
		/// Returns true if the given factions are peaceful to one another
		/// </summary>
		public static bool IsPeacefulTo(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (leftFaction == null || rightFaction == null) return false;
			return MyAPIGateway.Session.Factions.GetRelationBetweenFactions(leftFaction.FactionId,
					   rightFaction.FactionId) != MyRelationsBetweenFactions.Enemies;
		}

		/// <summary>
		/// Returns true when there is a peace proposal pending between two factions
		/// </summary>
		public static bool IsPeacePendingTo(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (leftFaction == null || rightFaction == null) return false;
			return MyAPIGateway.Session.Factions.IsPeaceRequestStatePending(leftFaction.FactionId,
				rightFaction.FactionId);
		}

		/// <summary>
		/// Returns true when factions are hostile to one another
		/// </summary>
		public static bool IsHostle(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			return MyAPIGateway.Session.Factions.GetRelationBetweenFactions(leftFaction.FactionId,
					   rightFaction.FactionId) == MyRelationsBetweenFactions.Enemies;
		}

		/// <summary>
		/// Declares war between two factions
		/// </summary>
		/// <param name="leftFaction">The faction declaring war</param>
		/// <param name="rightFaction">The faction receiving a war declaration</param>
		public static void DeclareWar(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if ((leftFaction == null || rightFaction == null) || !leftFaction.IsPeacefulTo(rightFaction)) return;
			MyAPIGateway.Session.Factions.DeclareWar(leftFaction.FactionId, rightFaction.FactionId);
			ClientWarDeclaration(leftFaction.FactionId, rightFaction.FactionId);
			AiSessionCore.DebugLog?.WriteToLog("DeclareWar", $"leftFaction: {leftFaction.Tag} rightFaction: {rightFaction.Tag}");
		}

		/// <summary>
		/// Proposes peace between two factions
		/// </summary>
		/// <param name="leftFaction">The faction proposing peace</param>
		/// <param name="rightFaction">The faction receiving the peace proposal</param>
		public static void ProposePeaceTo(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (!ValidateNonPirateFactions(leftFaction, rightFaction) || !leftFaction.IsHostle(rightFaction)) return;
			MyAPIGateway.Session.Factions.SendPeaceRequest(leftFaction.FactionId, rightFaction.FactionId);
			ClientPeaceDeclaration(leftFaction.FactionId, rightFaction.FactionId);
			AiSessionCore.DebugLog?.WriteToLog("ProposePeaceTo", $"leftFaction: {leftFaction.Tag} rightFaction: {rightFaction.Tag}");
		}

		/// <summary>
		/// Accepts a peace proposal from a faction
		/// </summary>
		/// <param name="leftFaction">The faction who to accept peace</param>
		/// <param name="rightFaction">The faction who has proposed peace</param>
		public static void AcceptPeaceFrom(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (!ValidateNonPirateFactions(leftFaction, rightFaction) || !leftFaction.IsPeacePendingTo(rightFaction)) return; 
			MyAPIGateway.Session.Factions.AcceptPeace(leftFaction.FactionId, rightFaction.FactionId);
			ClientPeaceAcceptance(leftFaction.FactionId, rightFaction.FactionId);
			AiSessionCore.DebugLog?.WriteToLog("AcceptPeaceFrom", $"leftFaction: {leftFaction.Tag} rightFaction: {rightFaction.Tag}");
		}

		/// <summary>
		/// Accepts a peace proposal from a faction
		/// </summary>
		/// <param name="leftFaction">The faction who to accept peace</param>
		/// <param name="rightFaction">The faction who has proposed peace</param>
		public static void AutoPeace(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (!ValidateNonPirateFactions(leftFaction, rightFaction) || leftFaction.IsPeacefulTo(rightFaction)) return;
			if (!leftFaction.IsPeacePendingTo(rightFaction)) leftFaction.ProposePeaceTo(rightFaction);
			rightFaction.AcceptPeaceFrom(leftFaction);
			AiSessionCore.DebugLog?.WriteToLog("AutoPeace", $"leftFaction: {leftFaction.Tag} rightFaction: {rightFaction.Tag}");
		}

		private static IMyFaction GetFactionById(this long factionId)
		{
			return MyAPIGateway.Session.Factions.TryGetFactionById(factionId);
		}

		public static void NewPirate(this IMyFaction newPirate)
		{
			foreach (KeyValuePair<long, IMyFaction> factions in MyAPIGateway.Session.Factions.Factions)
			{
				if (factions.Value == newPirate) continue;
				factions.Value.DeclareWar(newPirate);
			}
		}

		public static void NewFriendly(this IMyFaction newFriendly)
		{
			foreach (KeyValuePair<long, IMyFaction> factions in MyAPIGateway.Session.Factions.Factions)
			{
				if (factions.Value == newFriendly) continue;
				factions.Value.AutoPeace(newFriendly);
			}
		}

		private static bool ValidateNonPirateFactions(IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (leftFaction == null || rightFaction == null) return false;
			return !PirateFactionDictionary.ContainsKey(leftFaction.FactionId) && !PirateFactionDictionary.ContainsKey(rightFaction.FactionId);
		}

		#endregion
		
		public static void Register()
		{
			MyAPIGateway.Session.Factions.FactionCreated += FactionCreated;
			MyAPIGateway.Session.Factions.FactionStateChanged += FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionEdited += FactionEdited;
		}

		public static void Unload()
		{
			MyAPIGateway.Session.Factions.FactionCreated -= FactionCreated;
			MyAPIGateway.Session.Factions.FactionStateChanged -= FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionEdited -= FactionEdited;
		}

		private static void FactionStateChanged(MyFactionStateChange action, long fromFaction, long toFaction, long playerId, long senderId)
		{
			AiSessionCore.DebugLog?.WriteToLog("FactionStateChanged", $"Action: {action} fromFaction: {fromFaction.GetFactionById().Tag} toFaction: {toFaction.GetFactionById().Tag} playerId: {playerId} senderId: {senderId} isServer: {AiSessionCore.IsServer}");
			if (action == MyFactionStateChange.SendPeaceRequest && fromFaction.GetFactionById().IsNeutral(toFaction.GetFactionById().FounderId))
				MyAPIGateway.Session.Factions.CancelPeaceRequest(fromFaction, toFaction);

			//TODO: Finish this
		}

		private static void FactionCreated(long factionId)
		{
			if (PlayerFactionDictionary.ContainsKey(factionId) ||
			    PirateFactionDictionary.ContainsKey(factionId)) return;
			AiSessionCore.DebugLog?.WriteToLog("FactionCreated", $"newFaction Entered: {factionId} IsServer: {AiSessionCore.IsServer}");
			IMyFaction newFaction;
			if (!ValidateFactionEvents(factionId, out newFaction)) return;
			if (newFaction.IsEveryoneNpc() || PlayerFactionExclusionList.Any(x => newFaction.Description.StartsWith(x)))
			{
				AddToPirateFactionDictionary(factionId, newFaction);
				newFaction.NewPirate();
				return;
			}
			AddToPlayerFactionDictionary(factionId, newFaction);
			newFaction.NewFriendly();
			AiSessionCore.DebugLog?.WriteToLog("FactionCreated", $"newFaction: {newFaction.Tag}");
		}

		private static void FactionEdited(long factionId)
		{
			AiSessionCore.DebugLog?.WriteToLog("FactionEdited", $"editedFaction Enter: {factionId} IsServer: {AiSessionCore.IsServer}");
			IMyFaction editedFaction;
			if (!ValidateFactionEvents(factionId, out editedFaction) || editedFaction.IsEveryoneNpc()) return;
			if (!PlayerFactionExclusionList.Any(x => editedFaction.Description.StartsWith(x)) && PirateFactionDictionary.ContainsKey(factionId))
			{
				RemoveFromPirateFactionDictionary(factionId);
				AddToPlayerFactionDictionary(factionId, factionId.GetFactionById());
				editedFaction.NewFriendly();
				//TODO: need to determine peace conditions (cooldown?) newFaction.NewFriendly(); is a placeholder
				return;
			}
			if (!PlayerFactionExclusionList.Any(x => editedFaction.Description.StartsWith(x) && PlayerFactionDictionary.ContainsKey(factionId))) return;
			RemoveFromPlayerFactionDictionary(editedFaction.FactionId);
			AddToPirateFactionDictionary(factionId, editedFaction);
			editedFaction.NewPirate();
			AiSessionCore.DebugLog?.WriteToLog("FactionEdited", $"editedFaction Leave: {editedFaction.Tag}");
		}

		private static bool ValidateFactionEvents(long factionId, out IMyFaction newFaction)
		{
			newFaction = factionId.GetFactionById();
			return SetupComplete && newFaction != null;
		}

		private static List<BadRelation> BadRelations { get; set; }

		private struct BadRelation
		{
			public Dictionary<long, IMyFaction> PlayerFaction;
			public Dictionary<long, IMyFaction> NpcFaction;

			public BadRelation(Dictionary<long, IMyFaction> playerFaction, Dictionary<long, IMyFaction> npcFaction)
			{
				PlayerFaction = playerFaction;
				NpcFaction = npcFaction;
			}
		}

		public static Dictionary<long, IMyFaction> PlayerFactionDictionary { get; set; }

		public static Dictionary<long, IMyFaction> PirateFactionDictionary { get; set; }

		public static Dictionary<long, IMyFaction> EnforcementFactionDictionary { get; set; }

		public static Dictionary<long, IMyFaction> LawfulFactionDictionary { get; set; }

		public static void Initialize()
		{
			using (new Profiler("NewFactionInit"))
			{
				if (SetupComplete) return;
				Register();
				FactionsAtWar = new List<FactionsAtWar>();
				BadRelations = new List<BadRelation>();
				PlayerFactionDictionary = new Dictionary<long, IMyFaction>();
				PirateFactionDictionary = new Dictionary<long, IMyFaction>();
				EnforcementFactionDictionary = new Dictionary<long, IMyFaction>();
				LawfulFactionDictionary = new Dictionary<long, IMyFaction>();
				SetupFactionDictionaries();
				SetupPlayerRelations();
				SetupNpcRelations();
				SetupPirateRelations();
				SetupComplete = true;
			}
		}

		public static void SetupFactionDictionaries()
		{
			AiSessionCore.DebugLog?.WriteToLog("SetupFactionDictionaries", $"IsServer: {AiSessionCore.IsServer}");
			foreach (KeyValuePair<long, IMyFaction> factions in MyAPIGateway.Session.Factions.Factions)
			{
				if (LawfulFactionsTags.Contains(factions.Value.Tag))
				{
					AddToLawfulFactionDictionary(factions.Key, factions.Value);
					continue;
				}
				if (EnforcementFactionsTags.Contains(factions.Value.Tag))
				{
					AddToEnforcementFactionDictionary(factions.Key, factions.Value);
					continue;
				}
				if (factions.Value.IsEveryoneNpc())
				{
					AddToPirateFactionDictionary(factions.Key, factions.Value);
					continue;
				}
				if (PlayerFactionExclusionList.Any(x => factions.Value.Description.StartsWith(x)))
				{
					AddToPirateFactionDictionary(factions.Key, factions.Value);
					continue;
				}
				AddToPlayerFactionDictionary(factions.Key, factions.Value);
			}
		}

		public static void SetupPlayerRelations()
		{
			AiSessionCore.DebugLog?.WriteToLog("SetupPlayerRelations", $"IsServer: {AiSessionCore.IsServer}");
			foreach (KeyValuePair<long, IMyFaction> playerFaction in PlayerFactionDictionary)
			{
				foreach (KeyValuePair<long, IMyFaction> lawfulFaction in LawfulFactionDictionary)
				{
					playerFaction.Value.AutoPeace(lawfulFaction.Value);
				}
			}
		}

		public static void SetupNpcRelations()
		{
			AiSessionCore.DebugLog?.WriteToLog("SetupNpcRelations", $"IsServer: {AiSessionCore.IsServer}");
			foreach (KeyValuePair<long, IMyFaction> leftPair in LawfulFactionDictionary)
			{
				foreach (KeyValuePair<long, IMyFaction> rightPair in LawfulFactionDictionary)
				{
					if (leftPair.Key == rightPair.Key) continue;
					leftPair.Value.AutoPeace(rightPair.Value);
				}
			}
		}

		public static void SetupPirateRelations()
		{
			AiSessionCore.DebugLog?.WriteToLog("SetupPirateRelations", $"IsServer: {AiSessionCore.IsServer}");
			foreach (KeyValuePair<long, IMyFaction> factions in MyAPIGateway.Session.Factions.Factions)
			{
				foreach (KeyValuePair<long, IMyFaction> pirates in PirateFactionDictionary)
				{
					if(factions.Key == pirates.Key) continue;
					factions.Value.DeclareWar(pirates.Value);
				}
			}
		}

		private static void AddToLawfulFactionDictionary(long factionId, IMyFaction faction)
		{
			AiSessionCore.DebugLog?.WriteToLog("AddToLawfulFactionDictionary", $"factionId: {factionId} IsServer: {AiSessionCore.IsServer}");
			LawfulFactionDictionary.Add(factionId, faction);
		}

		private static void AddToEnforcementFactionDictionary(long factionId, IMyFaction faction)
		{
			AiSessionCore.DebugLog?.WriteToLog("AddToEnforcementFactionDictionary", $"factionId: {factionId} IsServer: {AiSessionCore.IsServer}");
			EnforcementFactionDictionary.Add(factionId, faction);
		}

		private static void AddToPirateFactionDictionary(long factionId, IMyFaction faction)
		{
			AiSessionCore.DebugLog?.WriteToLog("AddToPirateFactionDictionary", $"factionId: {factionId} IsServer: {AiSessionCore.IsServer}");
			PirateFactionDictionary.Add(factionId, faction);
		}

		private static void AddToPlayerFactionDictionary(long factionId, IMyFaction faction)
		{
			AiSessionCore.DebugLog?.WriteToLog("AddToPlayerFactionDictionary", $"factionId: {factionId} IsServer: {AiSessionCore.IsServer}");
			PlayerFactionDictionary.Add(factionId, faction);
		}

		private static void RemoveFromLawfulFactionDictionary(long factionId)
		{
			LawfulFactionDictionary.Remove(factionId);
		}

		private static void RemoveFromEnforcementFactionDictionary(long factionId)
		{
			EnforcementFactionDictionary.Remove(factionId);
		}

		private static void RemoveFromPirateFactionDictionary(long factionId)
		{
			PirateFactionDictionary.Remove(factionId);
		}

		private static void RemoveFromPlayerFactionDictionary(long factionId)
		{
			PlayerFactionDictionary.Remove(factionId);
		}

		/// <summary>
		/// Polls the current recorded wars and determines if it's time to declare peace or not
		/// </summary>
		public static void AssessFactionWar()
		{
			for (int counter = FactionsAtWar.Count - 1; counter >= 0; counter--)
				if ((FactionsAtWar[counter].CooldownTime -= Constants.WarAssessmentCounterLimit) <= 0)
				{
					ProposePeaceTo(FactionsAtWar[counter].AiFaction, FactionsAtWar[counter].PlayerFaction);
					if (FactionsAtWar[counter].AiFaction.IsEveryoneNpc() && FactionsAtWar[counter].PlayerFaction.IsEveryoneNpc())
						AcceptPeaceFrom(FactionsAtWar[counter].PlayerFaction, FactionsAtWar[counter].AiFaction);
					FactionsAtWar.RemoveAtFast(counter);
				}
		}

		/// <summary>
		/// Declares a war against the player with the provided AI faction
		/// </summary>
		/// <param name="aiFaction">Faction to declare ar against the player with</param>
		/// <param name="player">The player</param>
		public static void DeclareFactionWar(IMyFaction aiFaction, IMyPlayer player)
		{
			DeclareFactionWar(aiFaction, player.GetFaction());
		}

		/// <summary>
		/// Declares war against players with the provided AI faction
		/// </summary>
		/// <param name="aiFaction">Computer controlled faction</param>
		/// <param name="players"></param>
		public static void DeclareFactionWar(IMyFaction aiFaction, IEnumerable<IMyPlayer> players)
		{
			if (!(AiSessionCore.IsServer)) return;
			List<IMyFaction> factions = players.GroupBy(x => x.GetFaction()).Select(x => x.Key).ToList();
			foreach (IMyFaction faction in factions) DeclareFactionWar(aiFaction, faction);
		}

		/// <summary>
		/// Declares a war against the player with the provided AI faction
		/// </summary>
		/// <param name="aiFaction">Faction to declare ar against the player with</param>
		/// <param name="playerFaction">Players faction</param>
		public static void DeclareFactionWar(IMyFaction aiFaction, IMyFaction playerFaction)
		{
			FactionsAtWar war = new FactionsAtWar(aiFaction, playerFaction);
			int index = FactionsAtWar.IndexOf(war);
			if (index != -1)
				FactionsAtWar[index] = war;
			else
				FactionsAtWar.Add(war);
			if (aiFaction.IsLawful()) DeclareLawfulWar(playerFaction);
		}

		/// <summary>
		/// Declares a war with all lawful entities against the provided faction
		/// </summary>
		/// <param name="playerFaction">Faction to go to war with</param>
		private static void DeclareLawfulWar(IMyFaction playerFaction)
		{
			foreach (KeyValuePair<long, IMyFaction> enforcementFaction in EnforcementFactionDictionary)
			{
				FactionsAtWar war = new FactionsAtWar(enforcementFaction.Value, playerFaction);
				int index = FactionsAtWar.IndexOf(war);
				if (index != -1)
					FactionsAtWar[index] = war;
				else
					FactionsAtWar.Add(war);
			}
		}

		/// <summary>
		/// Used as a last resort - this declares war against the player from all neutral factions
		/// </summary>
		/// <param name="playerFaction">Player controlled faction</param>
		internal static void DeclareFullAiWar(IMyFaction playerFaction)
		{
			foreach (KeyValuePair<long, IMyFaction> enforcementFaction in LawfulFactionDictionary)
			{
				FactionsAtWar war = new FactionsAtWar(enforcementFaction.Value, playerFaction);
				int index = FactionsAtWar.IndexOf(war);
				if (index != -1)
					FactionsAtWar[index] = war;
				else
					FactionsAtWar.Add(war);
			}
		}

		/// <summary>
		/// Ensures PlayerInitFactions doesn't spam the server with requests to init factions
		/// Bug: This is required to address a current SE issue with the server ignoring all faction status change requests 
		/// </summary>
		internal static bool PlayerFactionInitComplete;

		/// <summary>
		/// Init factions from the client since the server can't process the request
		/// Bug: This is required to address a current SE issue with the server ignoring all faction status change requests
		/// </summary>
		internal static void PlayerInitFactions()
		{
			if (PlayerFactionInitComplete) return;
			Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.InitFactionsMessagePrefix, 0, 0));
			PlayerFactionInitComplete = true;
		}

		/// <summary>
		/// Passes a peace declaration request to the client from the server
		/// </summary>
		/// Bug: This is required to address a current SE issue with the server ignoring all faction status change requests
		/// <param name="factionOne">Self documented name</param>
		/// <param name="factionTwo">Self documented name</param>
		private static void ClientPeaceDeclaration(long factionOne, long factionTwo)
		{
			if (!AiSessionCore.IsServer) return;
			Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.DeclarePeaceMessagePrefix, factionOne, factionTwo));
		}

		/// <summary>
		/// Passes a peace accept request to the client from the server
		/// </summary>
		/// Bug: This is required to address a current SE issue with the server ignoring all faction status change requests
		/// <param name="factionOne">Self documented name</param>
		/// <param name="factionTwo">Self documented name</param>
		private static void ClientPeaceAcceptance(long factionOne, long factionTwo)
		{
			if (!AiSessionCore.IsServer) return;
			Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.AcceptPeaceMessagePrefix, factionOne, factionTwo));
		}

		/// <summary>
		/// Passes a war declaration request to the client from the server
		/// </summary>
		/// Bug: This is required to address a current SE issue with the server ignoring all faction status change requests
		/// <param name="factionOne">Self documented name</param>
		/// <param name="factionTwo">Self documented name</param>
		private static void ClientWarDeclaration(long factionOne, long factionTwo)
		{
			if (!AiSessionCore.IsServer) return;
			Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.DeclareWarMessagePrefix, factionOne, factionTwo));
		}
	}
}
