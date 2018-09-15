using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EemRdx.Extensions;
using EemRdx.Helpers;
using EemRdx.Networking;
using EemRdx.Scripts.Utilities;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.GameSystems;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

// ReSharper disable MemberCanBePrivate.Global

namespace EemRdx.Models
{
	public static class Factions
	{
		public static bool SetupComplete;

		internal static List<IMyFaction> LawfulFactions { get; private set; }

		internal static List<IMyFaction> EnforcementFactions { get; private set; }

		internal static List<FactionsAtWar> FactionsAtWar { get; set; }

		internal static List<IMyFaction> PlayerFactions { get; private set; }

		internal static List<IMyFaction> OtherFactions { get; private set; }

		/// <summary>
		/// These factions are considered lawful. When they go hostile towards someone,
		/// they also make the police (SEPD) and army (UCMF) go hostile.
		/// </summary>
		public static List<string> LawfulFactionsTags { get; } = new List<string>
		{
			"UCMF",
			"SEPD",
			"CIVL",
			"ISTG",
			"MA-I",
			"EXMC",
			"KUSS",
			"HS",
			"AMPH",
			"IMDC",
		};

		public static List<string> AllNpcFactions { get; } = new List<string>
		{
			"SPRT",
			"CIVL",
			"UCMF",
			"SEPD",
			"ISTG",
			"AMPH",
			"KUSS",
			"HS",
			"MMEC",
			"MA-I",
			"EXMC",
			"IMDC"
		};

		public static List<string> EnforcementFactionsTags { get; } = new List<string>
		{
			"SEPD", "UCMF"
		};

		#region Extension Methods

		/// <summary>
		/// Returns rue when a faction is considered Lawful, such as with SEPD and CIVL
		/// </summary>
		public static bool IsLawful(this IMyFaction checkFaction)
		{
			return LawfulFactions.Contains(checkFaction);
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
		/// Proposes peace to all AI factions
		/// </summary>
		/// <param name="playerFaction">The faction proposing peace to all AI factions</param>
		public static void ProposePeaceToAllAi(this IMyFaction playerFaction)
		{
			foreach (IMyFaction faction in LawfulFactions)
				if (!IsPeacefulTo(playerFaction, faction))
					MyAPIGateway.Session.Factions.SendPeaceRequest(faction.FactionId, playerFaction.FactionId);
		}

		/// <summary>
		/// Declares war between two factions
		/// </summary>
		/// <param name="leftFaction">The faction declaring war</param>
		/// <param name="rightFaction">The faction receiving a war declaration</param>
		public static void DeclareWar(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (leftFaction == null || rightFaction == null) return;
			if (!leftFaction.IsPeacefulTo(rightFaction)) return;
			MyAPIGateway.Session.Factions.DeclareWar(leftFaction.FactionId, rightFaction.FactionId);
			ClientWarDeclaration(leftFaction.FactionId, rightFaction.FactionId);
		}

		/// <summary>
		/// Proposes peace between two factions
		/// </summary>
		/// <param name="leftFaction">The faction proposing peace</param>
		/// <param name="rightFaction">The faction receiving the peace proposal</param>
		public static void ProposePeaceTo(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (leftFaction == null || rightFaction == null) return;
			if (!leftFaction.IsHostle(rightFaction)) return;
			MyAPIGateway.Session.Factions.SendPeaceRequest(leftFaction.FactionId, rightFaction.FactionId);
			ClientPeaceDeclaration(leftFaction.FactionId, rightFaction.FactionId);
		}

		/// <summary>
		/// Accepts a peace proposal from a faction
		/// </summary>
		/// <param name="leftFaction">The faction who to accept peace</param>
		/// <param name="rightFaction">The faction who has proposed peace</param>
		public static void AcceptPeaceFrom(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (leftFaction == null || rightFaction == null) return;
			if (!leftFaction.IsPeacePendingTo(rightFaction)) return;
			MyAPIGateway.Session.Factions.AcceptPeace(leftFaction.FactionId, rightFaction.FactionId);
			ClientPeaceAcceptance(leftFaction.FactionId, rightFaction.FactionId);
		}

		/// <summary>
		/// Accepts a peace proposal from a faction
		/// </summary>
		/// <param name="leftFaction">The faction who to accept peace</param>
		/// <param name="rightFaction">The faction who has proposed peace</param>
		public static void AutoPeace(this IMyFaction leftFaction, IMyFaction rightFaction)
		{
			if (leftFaction == null || rightFaction == null) return;
			leftFaction.ProposePeaceTo(rightFaction);
			rightFaction.AcceptPeaceFrom(leftFaction);
		}

		#endregion

		/// <summary>
		/// Initializes factions
		/// </summary>
		public static void Init()
		{
			EnforcementFactions = MyAPIGateway.Session.Factions.Factions.Values
				.Where(x => EnforcementFactionsTags.Contains(x.Tag)).ToList();
			LawfulFactions = MyAPIGateway.Session.Factions.Factions.Values
				.Where(x => LawfulFactionsTags.Contains(x.Tag)).ToList();
			FactionsAtWar = new List<FactionsAtWar>();
			NpcFactionPeace();
			SetupNpcFaction();
			SetupComplete = true;
		}

		/// <summary>
		/// Sets up a few conditions to make sure players can' tget into a NPC faction
		/// </summary>
		private static void SetupNpcFaction()
		{
			foreach (MyFactionDefinition faction in MyDefinitionManager.Static.GetDefaultFactions())
			{
				faction.AcceptHumans = false;
				faction.AutoAcceptMember = false;
			}
		}

		/// <summary>
		/// Ensures that all NPC factions that should be friendly to one another are actually friendly to one another
		/// </summary>
		public static void NpcFactionPeace()
		{
			foreach (IMyFaction leftFaction in LawfulFactions)
			{
				foreach (IMyFaction rightFaction in LawfulFactions)
					if (leftFaction != rightFaction)
						if (!leftFaction.IsPeacefulTo(rightFaction))
						{
							leftFaction.ProposePeaceTo(rightFaction);
							rightFaction.AcceptPeaceFrom(leftFaction);
						}
			}
		}

		private enum Disposition
		{
			Lawful, Enforcement, Pirate, Player
		}

		private static Dictionary<long, Disposition> FactionDictionary { get; set; }

		public static void Register()
		{
			MyAPIGateway.Session.Factions.FactionCreated += FactionCreated;
			MyAPIGateway.Session.Factions.FactionStateChanged += FactionStateChanged;
		}

		public static void Unload()
		{
			MyAPIGateway.Session.Factions.FactionCreated -= FactionCreated;
			MyAPIGateway.Session.Factions.FactionStateChanged -= FactionStateChanged;
		}

		private static void FactionStateChanged(MyFactionStateChange action, long fromFaction, long toFaction, long playerId, long senderId)
		{
			//throw new System.NotImplementedException();
		}

		private static void Initialize()
		{
			foreach (KeyValuePair<long, IMyFaction> factions in MyAPIGateway.Session.Factions.Factions)
			{
				if (LawfulFactionsTags.Contains(factions.Value.Tag))
				{
					FactionDictionary.Add(factions.Key, Disposition.Lawful);
					continue;
				}
				if (EnforcementFactionsTags.Contains(factions.Value.Tag))
				{
					FactionDictionary.Add(factions.Key, Disposition.Enforcement);
					continue;
				}
				if (factions.Value.IsEveryoneNpc())
				{
					FactionDictionary.Add(factions.Key, Disposition.Pirate);
					continue;
				}
				FactionDictionary.Add(factions.Key, Disposition.Player);
			}
		}

		private static void FactionCreated(long factionId)
		{
			//IMyFaction newFaction = factionId.GetFactionById();
			//if (newFaction == null) return;
			//if (newFaction.IsEveryoneNpc())
		}

		private static IMyFaction GetFactionById(this long factionId)
		{
			return MyAPIGateway.Session.Factions.TryGetFactionById(factionId);
		}

		public static void ManageFactions()
		{
			if (!SetupComplete) Init();                                     // If the faction system hasn't been setup, now's the time
			List<IMyPlayer> players = new List<IMyPlayer>();
			MyAPIGateway.Multiplayer.Players.GetPlayers(players);           // Fill a list full of players
			foreach (IMyPlayer player in players)                           // Iterates through the players on the server
			{
				IMyFaction playerFaction = player.GetFaction();             // Sees if they are in a faction
				List<IMyFaction> warringFactions = new List<IMyFaction>();
				if (playerFaction == null) continue;                        // If the player doesn't have a faction, we're out
				foreach (FactionsAtWar factionsAtWar in FactionsAtWar)      // Iterates through all current faction wars
					if (factionsAtWar.PlayerFaction == playerFaction)       // Check to see if this player is involved in a current war
						warringFactions.Add(factionsAtWar.AiFaction);       // If one is found, save the npc faction
				List<IMyFaction> proposePeaceFactions = LawfulFactions;     // Copy the current Lawful Factions list
				proposePeaceFactions.RemoveAll(warringFactions.Contains);   // Remove all warring factions
				foreach (IMyFaction npcfaction in proposePeaceFactions)     // Iterate through the remaining factions to propose peace to
				{
					if (npcfaction.IsPeacefulTo(playerFaction)) continue;   // If the factions are already peaceful, our work here is done
					npcfaction.ProposePeaceTo(playerFaction);               // Propose peace to the player
				}
			}
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
			foreach (IMyFaction enforcementFaction in EnforcementFactions)
			{
				FactionsAtWar war = new FactionsAtWar(enforcementFaction, playerFaction);
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
			if (!(AiSessionCore.IsServer)) return;
			foreach (IMyFaction enforcementFaction in Factions.LawfulFactions)
			{
				FactionsAtWar war = new FactionsAtWar(enforcementFaction, playerFaction);
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
