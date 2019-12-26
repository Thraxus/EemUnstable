using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Utilities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Factions.Models
{
	public class RelationshipManager : LogBaseEvent
	{

		// TODO: https://steamcommunity.com/sharedfiles/filedetails/?id=1903401450 
		// Friendly faction for MES for once... probably should account for it...

		// TODO: Add system for players to be able to pay off negative rep (credit to Lucas for the idea)
		//			Added to this is the potential to pay up to +1500
		//			Should carry the penalty of potential negative police / military if caught "bribing" officials
		//				need to make it cost more per purchase too
		//				based on current standings
		//				so like, -550 to -500, not so expensive, but -1500 to -500, pretty damn expensive
		//				even -1500 - -1400 also pretty damn expensive
		//				or -500 to 0, also pretty expensive if they want to go up


		/// <summary>
		/// Normal rep controlled player factions
		/// </summary>
		private readonly Dictionary<long, IMyFaction> _playerFactionDictionary = new Dictionary<long, IMyFaction>();

		/// <summary>
		/// Players who have decided to opt out of the rep system (always hostile to NPCs)
		/// </summary>
		private readonly Dictionary<long, IMyFaction> _playerPirateFactionDictionary = new Dictionary<long, IMyFaction>();

		/// <summary>
		/// NPC factions who hate everyone
		/// </summary>
		private readonly Dictionary<long, IMyFaction> _pirateFactionDictionary = new Dictionary<long, IMyFaction>();

		/// <summary>
		/// NPC factions who hate people who hate other people
		/// </summary>
		private readonly Dictionary<long, IMyFaction> _enforcementFactionDictionary = new Dictionary<long, IMyFaction>();

		/// <summary>
		/// NPC factions who like to be nice to everyone
		/// </summary>
		private readonly Dictionary<long, IMyFaction> _lawfulFactionDictionary = new Dictionary<long, IMyFaction>();

		/// <summary>
		/// All EEM NPC factions; doesn't discriminate if they are an asshole or angel
		/// </summary>
		private readonly Dictionary<long, IMyFaction> _npcFactionDictionary = new Dictionary<long, IMyFaction>();

		/// <summary>
		/// All NPC factions that aren't controlled by EEM
		/// </summary>
		private readonly Dictionary<long, IMyFaction> _nonEemNpcFactionDictionary = new Dictionary<long, IMyFaction>();

		#region Save Data

		/// <summary>
		/// Keeper of the keys to the castle, gatekeeper of old, holds all relationship information
		/// </summary>
		public readonly Dictionary<long, FactionRelation> FactionRelationships = new Dictionary<long, FactionRelation>();

		/// <summary>
		/// Holds all identity based relationships
		/// </summary>
		public readonly Dictionary<long, IdentityRelation> IdentityRelationships = new Dictionary<long, IdentityRelation>();

		/// <summary>
		/// Responsible for managing external requests for reputation hits 
		/// </summary>
		private static readonly Queue<PendingWar> WarQueue = new Queue<PendingWar>();

		#endregion

		/// <summary>
		/// Used to keep the Identity List; avoids having to allocate a new list every time it's required
		/// </summary>
		protected readonly List<IMyIdentity> Identities = new List<IMyIdentity>();

		/// <summary>
		/// Populates the Identity list with a fresh set of identities
		/// </summary>
		/// <returns>All current known identities</returns>
		protected List<IMyIdentity> GetIdentities()
		{
			Identities.Clear();
			MyAPIGateway.Players.GetAllIdentites(Identities);
			return Identities;
		}

		/// <summary>
		/// Used to keep the Player List; avoids having to allocate a new list every time it's required
		/// </summary>
		protected readonly List<IMyPlayer> Players = new List<IMyPlayer>();

		/// <summary>
		/// Populates the player list with a fresh set of players
		/// </summary>
		/// <returns>All currently active players</returns>
		protected List<IMyPlayer> GetPlayers()
		{
			Players.Clear();
			MyAPIGateway.Players.GetPlayers(Players);
			return Players;
		}

		/// <summary>
		/// Holds the last known save game; only used to initialize factions when the game is loaded
		/// </summary>
		private SaveData _saveData;

		/// <summary>
		/// Ensures setup isn't run more than once for whatever reason
		/// </summary>
		private bool _setupComplete;
		
		/// <summary>
		/// Ctor; sets up the class so it can do it's job
		/// </summary>
		/// <param name="save">The last save</param>
		public RelationshipManager(SaveData save)
		{
			WriteToLog("RelationshipManager", $"Constructing!", LogType.General);
			_saveData = save;

			MyAPIGateway.Session.Factions.FactionStateChanged += FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated += FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited += FactionEdited;
			MyAPIGateway.Session.Factions.FactionAutoAcceptChanged += MonitorAutoAccept;
		}

		/// <summary>
		/// After the class is created, this is run to setup all dictionaries and relationships
		/// </summary>
		public void Run()
		{
			WriteToLog("RelationshipManager.Run", $"Warming up!", LogType.General);

			SetupFactionDictionaries();

			if (_saveData.IsEmpty)
				FirstRunSetup();
			else LoadSaveData(_saveData);

			SetupFactionDeletionProtection();
			SetupAutoRelations();

			MyAPIGateway.Entities.OnEntityAdd += PlayerNet;

			_setupComplete = true;
			WriteToLog("RelationshipManager.Run", $"At a full Sprint!", LogType.General);
		}

		/// <summary>
		/// Called when the game is unloading to ensure that all events are unregistered and collections cleared
		/// </summary>
		public void Close()
		{
			WriteToLog("RelationshipManager.Close", $"Cooling down...", LogType.General);
			MyAPIGateway.Session.Factions.FactionStateChanged -= FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated -= FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited -= FactionEdited;
			MyAPIGateway.Session.Factions.FactionAutoAcceptChanged -= MonitorAutoAccept;
			WarQueue.Clear();
			Players.Clear();
			Identities.Clear();
			_playerFactionDictionary.Clear();
			_playerPirateFactionDictionary.Clear();
			_pirateFactionDictionary.Clear();
			_enforcementFactionDictionary.Clear();
			_lawfulFactionDictionary.Clear();
			_npcFactionDictionary.Clear();
			_nonEemNpcFactionDictionary.Clear();
			IdentityRelationships.Clear();
			FactionRelationships.Clear();
			MyAPIGateway.Entities.OnEntityAdd -= PlayerNet;
			WriteToLog("RelationshipManager.Close", $"Ready for a vacation!", LogType.General);
		}

		/// <summary>
		/// Catches players as they join the game and/or are registered as identities (happens more than you think; such as leaving a cockpit)
		/// </summary>
		/// <param name="myEntity">The entity detected by the game</param>
		private void PlayerNet(IMyEntity myEntity)
		{	// Catches players joining the game / server.  GET IT?!
			IMyPlayer player = MyAPIGateway.Players.GetPlayerById(myEntity.EntityId);
			if (player == null) return;
			PlayerJoined(player);
			WriteToLog("Factions: OnEntityAdd", $"New Player: {player.DisplayName} - {player.IdentityId}", LogType.General);
		}

		/// <summary>
		/// Checks to see whether the player is new or known.  If new, passes the player off to be added to the reputation system
		/// </summary>
		/// <param name="player">IMyPlayer of the new player</param>
		private void PlayerJoined(IMyPlayer player)
		{
			IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
			if (playerFaction != null) return;  // If the player already has a faction then it can be assumed the players rep is being managed within that faction and they are not new to the game / server
			if (IdentityRelationships.ContainsKey(player.IdentityId)) return;   // Identity is already being tracked, safe to ignore
			AddNewIdentity(player.Identity);    // New blood! 
		}

		/// <summary>
		/// This pulls save data from IdentityRelations and FactionRelations and passes it off to be processed
		/// </summary>
		/// <returns></returns>
		public SaveData GetSaveData()
		{
			HashSet<RelationSave> factionRelations = new HashSet<RelationSave>();
			foreach (KeyValuePair<long, FactionRelation> factionRelationship in FactionRelationships)
			{
				factionRelations.Add(factionRelationship.Value.GetSaveState());
			}

			HashSet<RelationSave> identityRelations = new HashSet<RelationSave>();
			foreach (KeyValuePair<long, IdentityRelation> identityRelationship in IdentityRelationships)
			{
				identityRelations.Add(identityRelationship.Value.GetSaveState());
			}

			return new SaveData(factionRelations, identityRelations);
		}

		/// <summary>
		/// Parses out the save game and loads last known faction / identity reputations
		/// </summary>
		/// <param name="saveData"></param>
		private void LoadSaveData(SaveData saveData)
		{
			if(saveData.IsEmpty) FirstRunSetup();
			
			foreach (RelationSave factionRelation in saveData.FactionSave)
			{
				IMyFaction fromFaction = MyAPIGateway.Session.Factions.TryGetFactionById(factionRelation.FromId);
				if (fromFaction == null) continue;
				FactionRelationships.Add(fromFaction.FactionId, new FactionRelation(fromFaction));
				foreach (Relation relation in factionRelation.ToFactionRelations)
				{   //	TODO: This inner loop is the same in both faction and identity - perhaps extract to a common method instead?
					IMyFaction toFaction = MyAPIGateway.Session.Factions.TryGetFactionById(relation.FactionId);
					if (toFaction == null) // if the toFaction is null, this faction doesn't exist any longer.  Abandon ship! 
						continue;
					FactionRelationships[fromFaction.FactionId].AddNewRelation(relation.FactionId, relation.Rep);
				}
			}

			List<IMyIdentity> gameIdentities = GetIdentities();

			foreach (RelationSave identityRelation in saveData.IdentitySave)
			{
				if (!Utilities.StaticMethods.ValidPlayer(identityRelation.FromId)) continue;  // This stops bots from making it into the dictionary
				IMyIdentity myIdentity = GetIdentities().Find(x => x.IdentityId == identityRelation.FromId);
				if (myIdentity == null) continue;					// This automatically parses out all stale identities
				
				IMyFaction myFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(myIdentity.IdentityId);
				if (myFaction == null) continue;					// This player is in a faction.  Let the factions manage the relationships.
				
				IdentityRelationships.Add(myIdentity.IdentityId, new IdentityRelation(myIdentity));
				gameIdentities.Remove(myIdentity);

				foreach (Relation relation in identityRelation.ToFactionRelations)
				{	//	TODO: This inner loop is the same in both faction and identity - perhaps extract to a common method instead? 
					IMyFaction toFaction = MyAPIGateway.Session.Factions.TryGetFactionById(relation.FactionId);
					if (toFaction == null) // if the toFaction is null, this faction doesn't exist any longer.  Abandon ship! 
						continue;
					FactionRelationships[myIdentity.IdentityId].AddNewRelation(relation.FactionId, relation.Rep);
				}
			}

			// at this point the only thing left in gameIdentities are new identities, so set them up as first run
			foreach (IMyIdentity identity in gameIdentities)
			{
				AddNewIdentity(identity);
			}

			Identities.Clear();
		}

		/// <summary>
		/// When a player leaves a faction, this is called to make sure they are allocated to IdentityRelationships
		/// </summary>
		/// <param name="id">ID of the player who just left a faction</param>
		private void FactionTraitor(long id)
		{   // They abandoned their faction and are now loners.  Way to go, asshole.
			if (!Utilities.StaticMethods.ValidPlayer(id)) return;  // This stops bots from making it into the dictionary
			
			IMyIdentity myIdentity = GetIdentities().Find(x => x.IdentityId == id);
			if (myIdentity == null) return;                   // This automatically parses out all stale identities
			IdentityRelationships.Add(myIdentity.IdentityId, new IdentityRelation(myIdentity));

			foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
			{	// These reps can carry over; no need to change.
				IdentityRelationships[id].AddNewRelation(faction.Key);
			}

			foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
			{	// Just making sure evil is still evil
				IdentityRelationships[id].AddNewRelation(faction.Key, GeneralSettings.DefaultNegativeRep);
			}
			Identities.Clear();
		}

		/// <summary>
		/// This is only used when a new player is detected who hasn't otherwise been managed by the RelationshipManager
		/// Converts the long to an IMyIdentity and passes it to the proper method
		/// </summary>
		/// <param name="identity"></param>
		private void AddNewIdentity(long identity)
		{
			if (!Utilities.StaticMethods.ValidPlayer(identity)) return;
			List<IMyIdentity> gameIdentities = new List<IMyIdentity>();
			MyAPIGateway.Players.GetAllIdentites(gameIdentities);
			AddNewIdentity(gameIdentities.Find(x => x.IdentityId == identity));
		}

		/// <summary>
		/// This is only used when a new player is detected who hasn't otherwise been managed by the RelationshipManager
		/// Adds a new player to IdentityRelationships
		/// </summary>
		/// <param name="identity"></param>
		private void AddNewIdentity(IMyIdentity identity)
		{	
			if (!Utilities.StaticMethods.ValidPlayer(identity.IdentityId)) return;
			IdentityRelationships.Add(identity.IdentityId, new IdentityRelation(identity));

			foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
			{
				IdentityRelationships[identity.IdentityId].AddNewRelation(faction.Key, GeneralSettings.DefaultNeutralRep);
			}

			foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
			{
				IdentityRelationships[identity.IdentityId].AddNewRelation(faction.Key, GeneralSettings.DefaultNegativeRep);
			}
		}

		/// <summary>
		/// Pulls IMyFaction from the Faction ID and passes it to the actual AddNewFaction method
		/// </summary>
		/// <param name="factionId">ID of the new faction</param>
		/// <param name="hostile">Is this new faction hostile?</param>
		private void AddNewFaction(long factionId, bool? hostile = null)
		{	
			IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(factionId);
			if (faction == null) return;
			AddNewFaction(faction, hostile);
		}

		/// <summary>
		/// Adds a new faction FactionRelationships
		/// </summary>
		/// <param name="newFaction">IMyFaction for the new faction</param>
		/// <param name="hostile">Is this new faction hostile?</param>
		private void AddNewFaction(IMyFaction newFaction, bool? hostile = null)
		{
			if (hostile == null)
				hostile = GeneralSettings.PlayerFactionExclusionList.Contains(newFaction.Description);

			FactionRelationships.Add(newFaction.FactionId, new FactionRelation(newFaction));

			foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
			{
				if (newFaction.FactionId == faction.Key) continue;
				FactionRelationships[newFaction.FactionId].AddNewRelation(faction.Key, (bool) hostile ? GeneralSettings.DefaultNegativeRep : GeneralSettings.DefaultNeutralRep);
			}

			foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
			{
				if (newFaction.FactionId == faction.Key) continue;
				FactionRelationships[newFaction.FactionId].AddNewRelation(faction.Key, GeneralSettings.DefaultNegativeRep);
			}
		}

		/// <summary>
		/// Makes sure the reputation of a faction is balanced for the new member
		/// </summary>
		/// <param name="id">ID of the new member</param>
		private void NewFactionMember(long id)
		{   // Idea here is to make sure that when someone joins a faction, the rep is rebalanced 
			// there should always be a rep hit, but minor for like reps, but more major the greater the divergence 
			IMyFaction tmp = MyAPIGateway.Session.Factions.TryGetPlayerFaction(id);
			if (tmp == null)
				return; // Trying to account for errant calls to this method
			try
			{
				if(!FactionRelationships.ContainsKey(tmp.FactionId))
				{
					AddNewFaction(tmp);
					return;
				}
				FactionRelationships[tmp.FactionId].AddNewMember(id);
			}
			catch (Exception e)
			{
				WriteToLog("NewFactionMember", $"Exception! Identity {id} or some faction lookup likely didn't exist: \t{e}", LogType.Exception);
			}
		}

		private void NewPlayerPirateFaction(long id)
		{
			try
			{
				if (!FactionRelationships.ContainsKey(id))
				{
					AddNewFaction(id, true);
				}
				_playerPirateFactionDictionary.Remove(id);
				_playerFactionDictionary.Add(id, id.GetFactionById());
				FactionRelationships[id].SetAsPirate();
			}
			catch (Exception e)
			{
				WriteToLog("HandleFormerPlayerPirate", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private void FormerPlayerPirateFaction(long id)
		{
			try
			{
				_playerPirateFactionDictionary.Remove(id);
				_playerFactionDictionary.Add(id, id.GetFactionById());
				FactionRelationships[id].NoLongerPirate();
			}
			catch (Exception e)
			{
				WriteToLog("HandleFormerPlayerPirate", $"Exception!\t{e}", LogType.Exception);
			}
		}

		/// <summary>
		/// If no save game exists, this will set factions to their default / new values 
		/// </summary>
		private void FirstRunSetup()
		{	// Should only ever run when no save exists
			foreach (KeyValuePair<long, IMyFaction> faction in _playerFactionDictionary)
			{
				AddNewFaction(faction.Value, GeneralSettings.PlayerFactionExclusionList.Contains(faction.Value.Description));
			}

			foreach (KeyValuePair<long, IMyFaction> faction in _playerPirateFactionDictionary)
			{
				AddNewFaction(faction.Value, GeneralSettings.PlayerFactionExclusionList.Contains(faction.Value.Description));
			}
			
			List<IMyIdentity> gameIdentities = new List<IMyIdentity>();
			MyAPIGateway.Players.GetAllIdentites(gameIdentities);

			foreach (IMyIdentity identity in GetIdentities())
			{
				AddNewIdentity(identity);
			}
			Identities.Clear();
			// TODO: After initial trial runs with EEM are successful, expand this to support other mods npc factions
		}

		/// <summary>
		/// Responsible for calling the methods to decay reputation in all relations
		/// </summary>
		public void ReputationDecay()
		{	// TODO: Call this on a schedule; once every 1 minute.  
			// TODO: Check to see if updating rep is thread safe
			
			foreach (KeyValuePair<long, FactionRelation> relationship in FactionRelationships)
			{
				relationship.Value.DecayReputation();
			}

			foreach (KeyValuePair<long, IdentityRelation> relationship in IdentityRelationships)
			{
				relationship.Value.DecayReputation();
			}
		}

		/// <summary>
		/// Users for external access to the war reputation hit
		/// </summary>
		/// <param name="pendingWar"></param>
		public void DeclareWar(PendingWar pendingWar)
		{   // Leaving this a queue for threading purposes; if not threaded, could easily go direct
			WarQueue.Enqueue(pendingWar);
			ProcessWarQueue();
		}

		/// <summary>
		/// Runs through the queue every call to ensure all war requests are met
		/// </summary>
		private void ProcessWarQueue()
		{
			try
			{
				while (WarQueue.Count > 0)
				{
					PendingWar pendingWar = WarQueue.Dequeue();
					TriggerWar(pendingWar);
					WriteToLog("ProcessWarQueue", $"War Triggered! {pendingWar}", LogType.General);
				}
			}
			catch (Exception e)
			{
				WriteToLog("ProcessWarQueue", $"Exception!\t{e}", LogType.Exception);
			}
		}

		/// <summary>
		/// Triggers rep hits for negative actions
		/// </summary>
		/// <param name="pendingWar"></param>
		private void TriggerWar(PendingWar pendingWar)
		{
			IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(pendingWar.IdentityId);
			if (playerFaction == null)
			{
				if (!IdentityRelationships.ContainsKey(pendingWar.IdentityId))
					AddNewIdentity(pendingWar.IdentityId);
				IdentityRelationships[pendingWar.IdentityId].TriggerWar(pendingWar.Against);
				foreach (KeyValuePair<long, IMyFaction> enforcementFaction in _enforcementFactionDictionary)
				{
					if (pendingWar.Against == enforcementFaction.Key) continue;
					IdentityRelationships[pendingWar.IdentityId].TriggerWar(enforcementFaction.Key);
				}
				return;
			}
			FactionRelationships[playerFaction.FactionId].TriggerWar(pendingWar.Against);
			foreach (KeyValuePair<long, IMyFaction> enforcementFaction in _enforcementFactionDictionary)
			{
				if (pendingWar.Against == enforcementFaction.Key) continue;
				FactionRelationships[playerFaction.FactionId].TriggerWar(enforcementFaction.Key);
			}
		}


		// TODO: * Add SPID to the EEM protected faction list
		// TODO:	- This needs to include kicking players from it on parsing

		#region Faction States / Events / Triggers

		private void FactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
		{	//TODO: Review all of these for accuracy / use
			WriteToLog("FactionStateChanged",
				$"Action:\t{action.ToString()}\tfromFaction:\t{fromFactionId}\ttag:\t{fromFactionId.GetFactionById()?.Tag}\ttoFaction:\t{toFactionId}\ttag:\t{toFactionId.GetFactionById()?.Tag}\tplayerId:\t{playerId}\tsenderId:\t{senderId}",
				LogType.General);
			if (action != MyFactionStateChange.RemoveFaction)
				if (fromFactionId == 0L || toFactionId == 0L) return;

			switch (action)
			{
				case MyFactionStateChange.RemoveFaction:
					// TODO: Make sure all identities from this faction are added to the identity dictionary; scrub the faction dictionary
					FactionRemoved(fromFactionId);
					break;
				case MyFactionStateChange.SendPeaceRequest:
					// Note: No such thing anymore as far as EEM is concerned.
					break;
				case MyFactionStateChange.CancelPeaceRequest:
					// Note: No such thing anymore as far as EEM is concerned.
					break;
				case MyFactionStateChange.AcceptPeace:
					// Note: No such thing anymore as far as EEM is concerned.
					break;
				case MyFactionStateChange.DeclareWar:
					// TODO: See if this actually works still.  Of so, how it a person vs faction handled?  I'm guessing... not well.
					DeclareWar(new PendingWar(fromFactionId, toFactionId));
					break;
				case MyFactionStateChange.FactionMemberSendJoin:
					// Note: Unused
					break;
				case MyFactionStateChange.FactionMemberCancelJoin:
					// Note: Unused
					break;
				case MyFactionStateChange.FactionMemberAcceptJoin:
					// TODO: Tie into faction system for a new faction member - remove from identity dictionary, re-balance players new faction rep
					// fromFactionId and toFactionId are identical here, playerId is the new player 
					NewFactionMember(playerId);
					break;
				case MyFactionStateChange.FactionMemberKick:
					// TODO: Tie into faction system for a removed faction member - add to identity dictionary
					//FactionTraitor(AddFactionMember(fromFactionId.GetFactionById()));
					// TODO: Need the FactionStateChange information (4x longs) for this event
					break;
				case MyFactionStateChange.FactionMemberPromote:
					// Note: Unused
					break;
				case MyFactionStateChange.FactionMemberDemote:
					// Note: Unused
					break;
				case MyFactionStateChange.FactionMemberLeave:
					// TODO: Tie into faction system for a removed faction member - add to identity dictionary
					//FactionTraitor();
					// TODO: Need the FactionStateChange information (4x longs) for this event
					break;
				case MyFactionStateChange.FactionMemberNotPossibleJoin:
					// Note: Unused
					break;
				case MyFactionStateChange.SendFriendRequest:
					// TODO: Ensure this has nothing to do with a NPC faction
					// Note: Unused
					break;
				case MyFactionStateChange.CancelFriendRequest:
					// TODO: Ensure this has nothing to do with a NPC faction
					// Note: Unused
					break;
				case MyFactionStateChange.AcceptFriendRequest:
					// TODO: Ensure this has nothing to do with a NPC faction
					// Note: Unused
					break;
				default:
					WriteToLog("FactionStateChanged", $"Case not found:\t{nameof(action)}\t{action.ToString()}", LogType.General);
					break;
			}
		}

		private void FactionCreated(long factionId)
		{
			WriteToLog("FactionCreated", $"factionId:\t{factionId}", LogType.General);
			FactionEditedOrCreated(factionId, true);
		}

		private void FactionEdited(long factionId)
		{
			WriteToLog("FactionEdited", $"factionId:\t{factionId}", LogType.General);
			FactionEditedOrCreated(factionId);
		}

		private void FactionRemoved(long factionId)
		{
			WriteToLog("FactionRemoved", $"factionId:\t{factionId}", LogType.General);
			ScrubDictionaries(factionId);
		}

		private void FactionEditedOrCreated(long factionId, bool newFaction = false)
		{
			IMyFaction playerFaction = factionId.GetFactionById();
			if (playerFaction == null || playerFaction.IsEveryoneNpc()) return; // I'm not a player faction, or I don't exist.  Peace out, sucka!
			if (playerFaction.IsPlayerPirate() && _playerPirateFactionDictionary.ContainsKey(factionId)) return; // I'm a player pirate, and you know it.  Laterz!
			if (playerFaction.IsPlayerPirate() && !_playerPirateFactionDictionary.ContainsKey(factionId)) // I'm a player pirate, but this is news to you...
			{
				_playerPirateFactionDictionary.Add(factionId, playerFaction);
				NewPlayerPirateFaction(factionId);
				return;
			}
			if (!playerFaction.IsPlayerPirate() && _playerPirateFactionDictionary.ContainsKey(factionId)) // I was a player pirate, but uh, I changed... I swear... 
			{
				_playerPirateFactionDictionary.Remove(factionId);
				FormerPlayerPirateFaction(factionId);
				return;
			}
			if (!newFaction) return;
			AddNewFaction(factionId); // I'm new man, just throw me a bone.
		}

		#endregion
		
		#region Faction Dictionaries Setup / Management

		private void SetupFactionDictionaries()
		{
			foreach (KeyValuePair<long, IMyFaction> faction in MyAPIGateway.Session.Factions.Factions)
			{
				try
				{
					if (faction.Value == null) continue;
					if (GeneralSettings.EnforcementFactionsTags.Contains(faction.Value.Tag))
					{
						WriteToLog("SetupFactionDictionaries", $"AddToEnforcementFactionDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.General);
						AddToEnforcementFactionDictionary(faction.Key, faction.Value);
						AddToLawfulFactionDictionary(faction.Key, faction.Value);
						AddToNpcFactionDictionary(faction.Key, faction.Value);
						continue;
					}

					if (GeneralSettings.LawfulFactionsTags.Contains(faction.Value.Tag))
					{
						WriteToLog("SetupFactionDictionaries", $"AddToLawfulFactionDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.General);
						AddToLawfulFactionDictionary(faction.Key, faction.Value);
						AddToNpcFactionDictionary(faction.Key, faction.Value);
						continue;
					}

					if (faction.Value.IsEveryoneNpc() && GeneralSettings.AllNpcFactions.Contains(faction.Value.Tag))
					{ // If it's not an Enforcement or Lawful faction, it's a pirate.
						WriteToLog("SetupFactionDictionaries", $"AddToPirateFactionDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.General);
						AddToPirateFactionDictionary(faction.Key, faction.Value);
						AddToNpcFactionDictionary(faction.Key, faction.Value);
						continue;
					}

					if (faction.Value.IsPlayerPirate())
					{
						WriteToLog("SetupFactionDictionaries", $"PlayerFactionExclusionList.Add:\t{faction.Key}\t{faction.Value.Tag}", LogType.General);
						AddToPlayerPirateFactionDictionary(faction.Key, faction.Value);
						continue;
					}

					if (!faction.Value.IsEveryoneNpc()) // If it's not one of my NPC's, I don't care!
					{
						WriteToLog("SetupFactionDictionaries", $"PlayerFaction.Add:\t{faction.Key}\t{faction.Value.Tag}", LogType.General);
						AddToPlayerFactionDictionary(faction.Key, faction.Value);
						continue;
					}

					_nonEemNpcFactionDictionary.Add(faction.Key, faction.Value); // Ok, I may care one day.
				}
				catch (Exception e)
				{
					WriteToLog("SetupFactionDictionaries", $"Exception caught - e: {e}\tfaction.Key:\t{faction.Key}\tfaction.Value: {faction.Value}\tfaction.Tag:\t{faction.Value?.Tag}", LogType.Exception);
				}

			}
		}

		private void AddToLawfulFactionDictionary(long factionId, IMyFaction faction)
		{
			_lawfulFactionDictionary.Add(factionId, faction);
		}

		private void AddToEnforcementFactionDictionary(long factionId, IMyFaction faction)
		{
			_enforcementFactionDictionary.Add(factionId, faction);
		}

		private void AddToPirateFactionDictionary(long factionId, IMyFaction faction)
		{
			_pirateFactionDictionary.Add(factionId, faction);
		}

		private void AddToNpcFactionDictionary(long factionId, IMyFaction faction)
		{
			_npcFactionDictionary.Add(factionId, faction);
		}

		private void AddToPlayerFactionDictionary(long factionId, IMyFaction faction)
		{
			_playerFactionDictionary.Add(factionId, faction);
		}

		private void AddToPlayerPirateFactionDictionary(long factionId, IMyFaction faction)
		{
			_playerPirateFactionDictionary.Add(factionId, faction);
		}

		private void ScrubDictionaries(long factionId)
		{
			try
			{
				_playerFactionDictionary.Remove(factionId);
				_playerPirateFactionDictionary.Remove(factionId);
				_pirateFactionDictionary.Remove(factionId);
				_enforcementFactionDictionary.Remove(factionId);
				_lawfulFactionDictionary.Remove(factionId);
				_npcFactionDictionary.Remove(factionId);
				_nonEemNpcFactionDictionary.Remove(factionId);
			}
			catch (Exception e)
			{
				WriteToLog("ScrubDictionaries", $"Exception!\t{e}", LogType.Exception);
			}
		}
		
		#endregion

		#region Faction Proection Measures

		private void SetupFactionDeletionProtection()
		{
			foreach (KeyValuePair<long, IMyFaction> npcFaction in _npcFactionDictionary)
				AddFactionMember(npcFaction.Value);
		}

		private static void AddFactionMember(IMyFaction npcFaction)
		{
			if (!npcFaction.IsEveryoneNpc()) return;
			if (npcFaction.Members.Count < 2)
				MyAPIGateway.Session.Factions.AddNewNPCToFaction(
					npcFaction.FactionId,
					$"[{npcFaction.Tag}] {GeneralSettings.NpcFirstNames[GeneralSettings.Random.Next(0, GeneralSettings.NpcFirstNames.Count - 1)]}" +
					$" {GeneralSettings.NpcLastNames[GeneralSettings.Random.Next(0, GeneralSettings.NpcLastNames.Count - 1)]}");
		}

		private void SetupAutoRelations()
		{
			foreach (KeyValuePair<long, IMyFaction> npcFaction in _npcFactionDictionary)
			{
				foreach (KeyValuePair<long, IMyFaction> playerFaction in _playerFactionDictionary)
					MyAPIGateway.Session.Factions.ChangeAutoAccept(npcFaction.Key, playerFaction.Value.FounderId, false, false);

				foreach (KeyValuePair<long, IMyFaction> playerPirateFaction in _playerPirateFactionDictionary)
					MyAPIGateway.Session.Factions.ChangeAutoAccept(npcFaction.Key, playerPirateFaction.Value.FounderId, false, false);
			}
		}

		private void MonitorAutoAccept(long factionId, bool acceptPeace, bool acceptMember)
		{
			if (!_setupComplete) return;
			if (!acceptPeace && !acceptMember) return;
			if (!_npcFactionDictionary.ContainsKey(factionId) && !_nonEemNpcFactionDictionary.ContainsKey(factionId)) return;
			SetupAutoRelations();
			WriteToLog("MonitorAutoAccept", $"NPC Faction bypass detected, resetting relationship controls.", LogType.General);
		}

		#endregion

	}
}