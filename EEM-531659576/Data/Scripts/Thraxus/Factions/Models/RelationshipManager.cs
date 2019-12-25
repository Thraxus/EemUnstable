using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Eem.Thraxus.Common.Utilities.Tools.Networking;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Utilities;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Factions.Models
{
	public class RelationshipManager : LogBaseEvent
	{

		// https://steamcommunity.com/sharedfiles/filedetails/?id=1903401450 
		// Friendly faction for MES for once... probably should account for it...

		// Normal rep controlled player factions
		private readonly Dictionary<long, IMyFaction> _playerFactionDictionary;

		// Players who have decided to opt out of the rep system (always hostile to NPCs)
		private readonly Dictionary<long, IMyFaction> _playerPirateFactionDictionary;

		// NPC factions who hate everyone
		private readonly Dictionary<long, IMyFaction> _pirateFactionDictionary;

		// NPC factions who hate people who hate other people
		private readonly Dictionary<long, IMyFaction> _enforcementFactionDictionary;

		// NPC factions who like to be nice to everyone
		private readonly Dictionary<long, IMyFaction> _lawfulFactionDictionary;

		// All EEM NPC factions; doesn't discriminate if they are an asshole or angel
		private readonly Dictionary<long, IMyFaction> _npcFactionDictionary;

		// All NPC factions that aren't controlled by EEM
		private readonly Dictionary<long, IMyFaction> _nonEemNpcFactionDictionary;

		#region Save Data

		// Keeper of the keys to the castle, gatekeeper of old, holds all relationship information
		public readonly Dictionary<long, FactionRelation> FactionRelationships;

		public readonly Dictionary<long, IdentityRelation> IdentityRelationships;

		private static readonly Queue<PendingWar> WarQueue = new Queue<PendingWar>();

		#endregion

		//private static readonly Queue<PendingRelation> WarQueue = new Queue<PendingRelation>();

		private SaveData _saveData;

		private bool _setupComplete;
		
		public RelationshipManager(SaveData save)
		{
			WriteToLog("RelationshipManager", $"Constructing!", LogType.General);
			_saveData = save;
			_playerFactionDictionary = new Dictionary<long, IMyFaction>();
			_pirateFactionDictionary = new Dictionary<long, IMyFaction>();
			_playerPirateFactionDictionary = new Dictionary<long, IMyFaction>();
			_enforcementFactionDictionary = new Dictionary<long, IMyFaction>();
			_lawfulFactionDictionary = new Dictionary<long, IMyFaction>();
			_npcFactionDictionary = new Dictionary<long, IMyFaction>();
			_nonEemNpcFactionDictionary = new Dictionary<long, IMyFaction>();
			IdentityRelationships = new Dictionary<long, IdentityRelation>();
			FactionRelationships = new Dictionary<long, FactionRelation>();

			MyAPIGateway.Session.Factions.FactionStateChanged += FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated += FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited += FactionEdited;
			MyAPIGateway.Session.Factions.FactionAutoAcceptChanged += MonitorAutoAccept;
		}

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

		public void Close()
		{
			WriteToLog("RelationshipManager.Close", $"Cooling down...", LogType.General);
			MyAPIGateway.Session.Factions.FactionStateChanged -= FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated -= FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited -= FactionEdited;
			MyAPIGateway.Session.Factions.FactionAutoAcceptChanged -= MonitorAutoAccept;
			WarQueue.Clear();
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

		private void PlayerNet(IMyEntity myEntity)
		{	// Catches players joining the game / server.  GET IT?!
			IMyPlayer player = MyAPIGateway.Players.GetPlayerById(myEntity.EntityId);
			if (player == null) return;
			PlayerJoined(player);
			WriteToLog("Factions: OnEntityAdd", $"New Player: {player.DisplayName} - {player.IdentityId}", LogType.General);
		}

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

		private void LoadSaveData(SaveData saveData)
		{
			if(saveData.IsEmpty) FirstRunSetup();

			foreach (RelationSave factionRelation in saveData.RelationSave)
			{
				IMyFaction fromFaction = MyAPIGateway.Session.Factions.TryGetFactionById(factionRelation.FromFactionId);
				IMyFaction toFaction = MyAPIGateway.Session.Factions.TryGetFactionById(factionRelation.ToFactionId);
				if (fromFaction == null || toFaction == null) continue; // This automatically parses out all stale factions
				FactionRelationships.Add(factionRelation.ToFactionId, new FactionRelation(fromFaction, toFaction, factionRelation.Rep));
			}

			List<IMyIdentity> gameIdentities = new List<IMyIdentity>();
			MyAPIGateway.Players.GetAllIdentites(gameIdentities);

			foreach (RelationSave identityRelation in saveData.IdentitySave)
			{
				IMyIdentity myIdentity = gameIdentities.Find(x => x.IdentityId == identityRelation.FromId);
				if (myIdentity == null) continue; // This automatically parses out all stale identities
				IdentityRelationships.Add(myIdentity.IdentityId, new IdentityRelation(myIdentity, identityRelation.ToFactionIds));
				gameIdentities.Remove(myIdentity);
			}

			CleanIdentityRelationships();

			// at this point the only thing left in gameIdentities are new identities, so set them up as first run
			foreach (IMyIdentity identity in gameIdentities)
			{
				AddNewIdentity(identity);
			}
		}
		
		private void AddNewIdentity(long identity)
		{
			if (!ValidPlayer(identity)) return;
			List<IMyIdentity> gameIdentities = new List<IMyIdentity>();
			MyAPIGateway.Players.GetAllIdentites(gameIdentities);
			AddNewIdentity(gameIdentities.Find(x => x.IdentityId == identity));
		}

		private void AddNewIdentity(IMyIdentity identity)
		{
			if (!ValidPlayer(identity.IdentityId)) return;

			HashSet<long> factionsDictionary = new HashSet<long>();

			foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
				factionsDictionary.Add(faction.Key);

			foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
				factionsDictionary.Add(faction.Key);

			IdentityRelationships.Add(identity.IdentityId, new IdentityRelation(identity, factionsDictionary));
		}

		private void AddNewFaction(long factionId, bool hostile)
		{
			IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(factionId);
			if (faction == null) return;
			AddNewFaction(faction, hostile);
		}

		private void AddNewFaction(IMyFaction newFaction, bool hostile)
		{
			foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
			{
				if (newFaction.FactionId == faction.Key) continue;
				FactionRelationships.Add(newFaction.FactionId, new FactionRelation(newFaction, faction.Value, hostile ? GeneralSettings.DefaultNegativeRep : GeneralSettings.DefaultNeutralRep));
			}

			foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
			{
				if (newFaction.FactionId == faction.Key) continue;
				FactionRelationships.Add(newFaction.FactionId, new FactionRelation(newFaction, faction.Value, GeneralSettings.DefaultNegativeRep));
			}
		}

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
			
			List<long> knownIdentities = new List<long>();
			foreach (KeyValuePair<long, FactionRelation> faction in FactionRelationships)
			{
				knownIdentities.AddRange(faction.Value.MemberList);
			}

			List<IMyIdentity> gameIdentities = new List<IMyIdentity>();
			MyAPIGateway.Players.GetAllIdentites(gameIdentities);

			foreach (IMyIdentity identity in gameIdentities.
				Where(identity => !ValidPlayer(identity.IdentityId)).				// Exclude NPCs
				Where(identity => !knownIdentities.Contains(identity.IdentityId)))	// Exclude known identities  
			{
				AddNewIdentity(identity);
			}

			// TODO: After initial trial runs with EEM are successful, expand this to support other mods npc factions
		}

		/// <summary>
		/// Removes all identities who are not players (aka, NPCs) and any player who is already in a faction
		/// </summary>
		private void CleanIdentityRelationships()
		{
			List<long> knownIdentities = new List<long>();
			foreach (KeyValuePair<long, FactionRelation> faction in FactionRelationships)
			{
				knownIdentities.AddRange(faction.Value.MemberList);
			}

			foreach (long identityId in (from identityRelationship in IdentityRelationships where !ValidPlayer(identityRelationship.Key) || knownIdentities.Contains(identityRelationship.Key) select identityRelationship.Key).ToList())
			{
				IdentityRelationships.Remove(identityId);
			}
		}

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

		private void PlayerJoined(IMyPlayer player)
		{
			IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
			if (playerFaction != null) return;	// If the player already has a faction then it can be assumed the players rep is being managed within that faction and they are not new to the game / server
			if (IdentityRelationships.ContainsKey(player.IdentityId)) return;	// Identity is already being tracked, safe to ignore
			AddNewIdentity(player.Identity);	// New blood! 
		}


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



		public void DeclareWar(PendingWar pendingWar)
		{	// Leaving this a queue for threading purposes; if not threaded, could easily go direct
			WarQueue.Enqueue(pendingWar);
			ProcessWarQueue();
		}

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
		/// Checks whether an identity is a bot or not
		/// </summary>
		/// <param name="identityId"></param>
		/// <returns>Returns true if the identity is not a bot</returns>
		private static bool ValidPlayer(long identityId)
		{
			return MyAPIGateway.Players.TryGetSteamId(identityId) == 0;
		}

		// TODO: Player Leaves Faction -> Add to IdentityRelationships with reps from the faction they just left

		// TODO: Player Creates a Faction - Create the new FactionRelationships entry using existing player reputations, remove from IdentityRelationships

		// TODO: War conditions to set rep to desired values (-550 first offense, -20 per next offense.  if > -530, set back to -550, otherwise add the -20 (so -525 = -550, and -540 = -560)
		
		// TODO: * Add SPID to the EEM protected faction list
		// TODO:	- This needs to include kicking players from it on parsing

		private void FactionMessage(int oldRep, int repChange, long leftFaction, long rightFaction)
		{   // TODO Finish this! 
			// If old rep is over default neutral and the rep change is positive, nothing to do
			if (oldRep > GeneralSettings.DefaultNeutralRep && repChange > 0) return;

			// If the old rep is less than default neutral rep and the rep change is also negative, nothing to do
			if (oldRep < GeneralSettings.DefaultNeutralRep && repChange < 0) return;

			// Now we need some simple math...
			int newRep = oldRep + repChange;

			// If the old rep is greater than or equal to the default neutral rep and the new rep is also greater than or equal to the default neutral rep, nothing to do
			if (oldRep >= GeneralSettings.DefaultNeutralRep && newRep >= GeneralSettings.DefaultNeutralRep) return;

			// If the old rep is less than the default neutral rep and the new rep is also less than the default neutral rep, nothing to do
			if (oldRep < GeneralSettings.DefaultNeutralRep && newRep < GeneralSettings.DefaultNeutralRep) return;

			// If the old rep is greater than or equal to the default neutral rep and the new rep is lower than the default neutral rep, war time baby!
			if (oldRep >= GeneralSettings.DefaultNeutralRep && newRep < GeneralSettings.DefaultNeutralRep)
			{

			}

			// If the old rep is less than the default neutral rep and the new rep is greater than or equal to the default neutral rep, shucks... peace time
			if (oldRep > GeneralSettings.DefaultNeutralRep && newRep >= GeneralSettings.DefaultNeutralRep)
			{

			}

		}



		private void NewFactionMember(long id)
		{   // Idea here is to make sure that when someone joins a faction, the rep is rebalanced 
			// there should always be a rep hit, but minor for like reps, but more major the greater the divergence 
			IMyFaction tmp = MyAPIGateway.Session.Factions.TryGetPlayerFaction(id);
			if (tmp == null)
				return; // Trying to account for errant calls to this method

			try
			{

			}
			catch (Exception e)
			{
				WriteToLog("NewFactionMember", $"Exception! Identity {id} or some faction lookup likely didn't exist: \t{e}", LogType.Exception);
			}
		}

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
					AddFactionMember(fromFactionId.GetFactionById());
					break;
				case MyFactionStateChange.FactionMemberPromote:
					// Note: Unused
					break;
				case MyFactionStateChange.FactionMemberDemote:
					// Note: Unused
					break;
				case MyFactionStateChange.FactionMemberLeave:
					// TODO: Tie into faction system for a removed faction member - add to identity dictionary
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
				DeclarePermanentFullNpcWar(factionId);
				return;
			}
			if (!playerFaction.IsPlayerPirate() && _playerPirateFactionDictionary.ContainsKey(factionId)) // I was a player pirate, but uh, I changed... I swear... 
			{
				_playerPirateFactionDictionary.Remove(factionId);
				HandleFormerPlayerPirate(factionId);
				return;
			}
			if (!newFaction) return;
			SetupNewFactionRelationship(factionId, FactionType.Player); // I'm new man, just throw me a bone.
		}

		private void NewPlayerPirateFaction(IMyFaction faction)
		{
			_playerFactionDictionary.Add(faction.FactionId, faction);
			if (!FactionRelationships.ContainsKey(faction.FactionId))
			{
				foreach (KeyValuePair<long, IMyFaction> npcFaction in _npcFactionDictionary)
				{
					
				}
				FactionRelationships.Add(faction.FactionId, new FactionRelation());
			}
		}

		private void PeaceRequestSent(long fromFactionId, long toFactionId)
		{   // So many reasons to clear peace...
			// This is no longer a concept with EEM / new factions; leaving this method here for no reason at all.  
		}

		#endregion



		#region Dialogue triggers

		private void RequestDialog(IMyFaction npcFaction, IMyFaction playerFaction, Enums.DialogType type)
		{
			try
			{
				Func<string> message = _dialogue.RequestDialog(npcFaction, type);x
				string npcFactionTag = npcFaction.Tag;
				if (playerFaction == null || _newFactionDictionary.ContainsKey(playerFaction.FactionId)) return;
				SendFactionMessageToAllFactionMembers(message.Invoke(), npcFactionTag, playerFaction.Members);
			}
			catch (Exception e)
			{
				WriteToLog("RequestDialog", $"npcFaction:\t{npcFaction.FactionId}\tplayerFaction:\t{playerFaction.FactionId}\tException!\t{e}", LogType.Exception);
			}
		}

		private void RequestNewFactionDialog(long playerFactionId)
		{
			const string npcFactionTag = "The Lawful";
			try
			{
				Func<string> message = _dialogue.RequestDialog(null, Enums.DialogType.CollectiveWelcome);
				if (playerFactionId.GetFactionById() == null || !_newFactionDictionary.ContainsKey(playerFactionId)) return;
				SendFactionMessageToAllFactionMembers(message.Invoke(), npcFactionTag, playerFactionId.GetFactionById().Members);
				_newFactionDictionary.Remove(playerFactionId);
			}
			catch (Exception e)
			{
				WriteToLog("RequestNewFactionDialog", $"playerFaction:\t{playerFactionId}\tException!\t{e}", LogType.Exception);
			}
		}

		private void RequestNewPirateDialog(long playerFactionId)
		{
			const string npcFactionTag = "The Lawful";
			try
			{
				Func<string> message = _dialogue.RequestDialog(null, Enums.DialogType.CollectiveDisappointment);
				if (playerFactionId.GetFactionById() == null || !_newFactionDictionary.ContainsKey(playerFactionId)) return;
				SendFactionMessageToAllFactionMembers(message.Invoke(), npcFactionTag, playerFactionId.GetFactionById().Members);
				_newFactionDictionary.Remove(playerFactionId);
			}
			catch (Exception e)
			{
				WriteToLog("RequestNewPirateDialog", $"playerFaction:\t{playerFactionId}\tException!\t{e}", LogType.Exception);
			}
		}

		private void RequestFormerPirateDialog(long playerFactionId)
		{
			const string npcFactionTag = "The Lawful";
			try
			{
				Func<string> message = _dialogue.RequestDialog(null, Enums.DialogType.CollectiveReprieve);
				if (playerFactionId.GetFactionById() == null || !_newFactionDictionary.ContainsKey(playerFactionId)) return;
				SendFactionMessageToAllFactionMembers(message.Invoke(), npcFactionTag, playerFactionId.GetFactionById().Members);
				_newFactionDictionary.Remove(playerFactionId);
			}
			catch (Exception e)
			{
				WriteToLog("RequestFormerPirateDialog", $"playerFaction:\t{playerFactionId}\tException!\t{e}", LogType.Exception);
			}
		}

		#endregion

		private void SendFactionMessageToAllFactionMembers(string message, string messageSender, DictionaryReader<long, MyFactionMember> target, string color = MyFontEnum.Red)
		{
			try
			{
				foreach (KeyValuePair<long, MyFactionMember> factionMember in target)
				{
					if (factionMember.Value.IsFactionMemberOnline())
						MyAPIGateway.Utilities.InvokeOnGameThread(() =>
							Messaging.SendMessageToPlayer($"{message}", messageSender, factionMember.Key, color));
				}
			}
			catch (Exception e)
			{
				WriteToLog("SendFactionMessageToAllFactionMembers", $"Exception!\t{e}", LogType.Exception);
			}
		}
		
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

		private void HandleFormerPlayerPirate(long playerFactionId)
		{
			try
			{
				_playerPirateFactionDictionary.Remove(playerFactionId);
				_playerFactionDictionary.Add(playerFactionId, playerFactionId.GetFactionById());
			}
			catch (Exception e)
			{
				WriteToLog("HandleFormerPlayerPirate", $"Exception!\t{e}", LogType.Exception);
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