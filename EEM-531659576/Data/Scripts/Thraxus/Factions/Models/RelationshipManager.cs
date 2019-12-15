using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.SaveGame;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Eem.Thraxus.Common.Utilities.Tools.Networking;
using Eem.Thraxus.Debug;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Utilities;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageRender;

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

		#endregion

		//private static readonly Queue<PendingRelation> WarQueue = new Queue<PendingRelation>();

		private SaveData _saveData;

		private bool _setupComplete;

		private readonly Dialogue _dialogue;
		
		public SaveData GetSaveData()
		{
			List<FactionRelationSave> relationSaves = new List<FactionRelationSave>();
			foreach (KeyValuePair<long, FactionRelation> factionRelationship in FactionRelationships)
			{
				relationSaves.Add(factionRelationship.Value.GetSaveState());
			}

			List<IdentityRelationSave> identitySaves = new List<IdentityRelationSave>();
			foreach (KeyValuePair<long, IdentityRelation> identityRelationship in IdentityRelationships)
			{
				identitySaves.Add(identityRelationship.Value.GetSaveState());
			}

			return new SaveData(relationSaves, identitySaves);
		}

		private void LoadSaveData(SaveData saveData)
		{
			if(saveData.IsEmpty) FirstRunSetup();

			foreach (FactionRelationSave factionRelation in saveData.RelationSave)
			{
				IMyFaction fromFaction = MyAPIGateway.Session.Factions.TryGetFactionById(factionRelation.FromFactionId);
				IMyFaction toFaction = MyAPIGateway.Session.Factions.TryGetFactionById(factionRelation.ToFactionId);
				if (fromFaction == null || toFaction == null) continue; // This automatically parses out all stale factions
				FactionRelationships.Add(factionRelation.ToFactionId, new FactionRelation(fromFaction, toFaction, factionRelation.Rep));
			}

			List<IMyIdentity> gameIdentities = new List<IMyIdentity>();
			MyAPIGateway.Players.GetAllIdentites(gameIdentities);

			foreach (IdentityRelationSave identityRelation in saveData.IdentitySave)
			{
				IMyIdentity myIdentity = gameIdentities.Find(x => x.IdentityId == identityRelation.FromIdentityId);
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
		
		public void AddNewIdentity(long identity)
		{
			if (!ValidPlayer(identity)) return;
			List<IMyIdentity> gameIdentities = new List<IMyIdentity>();
			MyAPIGateway.Players.GetAllIdentites(gameIdentities);
			AddNewIdentity(gameIdentities.Find(x => x.IdentityId == identity));
		}

		public void AddNewIdentity(IMyIdentity identity)
		{
			if (!ValidPlayer(identity.IdentityId)) return;

			Dictionary<long, int> factionsDictionary = new Dictionary<long, int>();

			foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
				factionsDictionary.Add(faction.Key, GeneralSettings.DefaultNeutralRep);

			foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
				factionsDictionary.Add(faction.Key, GeneralSettings.DefaultNegativeRep);

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

		private void ReputationDecay()
		{	// Call this on a schedule; once every x minutes.  
			// TODO: Check to see if updating rep is thread safe
			List<long> parsedIdentities = new List<long>();
			List<long> remainingIdentities = new List<long>();
			
			//TODO: parse Factions first, collect list of identities covered, run individual updates on what's leftover
			

			//TODO: once all updates are parsed for factions and remaining identities, update identity dictionary with new faction rep values for those covered in the faction update
			
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

		// TODO: Move rep decay to the faction / identity relationship classes
		// TODO: Idea is to parse factions first, and collect an identityId for each player parsed.  
		// TODO: Using the returned list there, check that against the identity relationships and decay rep there for any identities not covered in faction relations


		// TODO: Player Leaves Faction -> Add to IdentityRelationships with reps from the faction they just left

		// TODO: Player Creates a Faction - Create the new FactionRelationships entry using existing player reputations, remove from IdentityRelationships

		// TODO: War conditions to set rep to desired values (-550 first offense, -20 per next offense.  if > -530, set back to -550, otherwise add the -20 (so -525 = -550, and -540 = -560)




		// TODO: * Method on timer that returns reputations to their natural disposition over time
		// TODO:	- This method will handle both retuning high values to default values over time (i.e. 1500 -> -500) as well as 
		// TODO:		handling the return to natural after wartime activities (i.e. -1000 -> -500)
		// TODO:	- The return from wartime activities should be much faster than rep decay

		// TODO: * Add SPID to the EEM protected faction list
		// TODO:	- This needs to include kicking players from it on parsing

		public RelationshipManager(SaveData save)
		{
			WriteToLog("RelationshipManager", $"Constructing!", LogType.General);
			_saveData = save;
			_dialogue = new Dialogue();
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
			


			_setupComplete = true;
			WriteToLog("RelationshipManager.Run", $"At a full Sprint!", LogType.General);
		}
		
		#region War

		private void SetIdentityHostile(long identityId)
		{
			if (IdentityRelationships.ContainsKey(identityId))
				IdentityRelationships[identityId].IsHostile = true;
			else AddNewIdentity(identityId, true);
		}

		#endregion


		#region Relationships

		//private void SetNewFactionMemberRep(long identityId, IMyFaction faction)
		//{
		//	foreach (KeyValuePair<long, IMyFaction> npcFaction in _lawfulFactionDictionary)
		//	{
		//		int newRep = GetIdentityToFactionReputation(identityId, npcFaction.Key);
		//		if (newRep < GeneralSettings.DefaultNeutralRep)
		//			SetEntireFactionRep(faction, npcFaction.Value, newRep);
		//		newRep = CalculateAverageFactionToFactionRep(faction, npcFaction.Value, identityId)
		//	}

		//}

		private int CalculateAverageFactionToFactionRep(IMyFaction fromFaction, IMyFaction toFaction, long excludeId = 0)
		{
			return fromFaction.Members.Where(member => member.Value.PlayerId != excludeId).Sum(member => GetFactionToFactionReputation(member.Value.PlayerId, toFaction.FactionId)) /
				   fromFaction.Members.Count;
			//foreach (KeyValuePair<long, MyFactionMember> member in faction.Members)
			//{
			//	if (member.Value.PlayerId == excludeId) continue;
			//	totalRep += GetFactionToFactionReputation(member.Value.PlayerId, toFactionId);
			//}
			//return totalRep / faction.Members.Count;
		}

		private void SetEntireFactionRep(IMyFaction fromFaction, IMyFaction toFaction, int value)
		{   // TODO Need to make sure this limits exploit actions such as where someone is at war and creates a new faction
			try
			{
				SetFactionFactionRep(fromFaction.FactionId, fromFaction.FactionId, value);
				foreach (KeyValuePair<long, MyFactionMember> factionMember in fromFaction.Members)
				{
					SetIdentityFactionRep(factionMember.Key, toFaction.FactionId, value);
				}
			}
			catch (Exception e)
			{
				WriteToLog("SetEntireFactionFactionRep", $"Exception! Faction {fromFaction?.Tag} likely didn't exist: \t{e}", LogType.Exception);
			}
		}

		private void SetFactionFactionRep(long leftFactionId, long rightFactionId, int value)
		{
			WriteToLog("SetIdentityFactionRep", $"Setting {leftFactionId} and {rightFactionId} to rep value {value}", LogType.General);
			FactionMessage(MyAPIGateway.Session.Factions.GetReputationBetweenFactions(leftFactionId, rightFactionId), value, leftFactionId, rightFactionId);
			MyAPIGateway.Session.Factions.SetReputation(leftFactionId, rightFactionId, value);
		}

		private void SetIdentityFactionRep(long identityId, long factionId, int value)
		{
			WriteToLog("SetIdentityFactionRep", $"Setting {identityId} and {factionId} to rep value {value}", LogType.General);
			MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(identityId, factionId, value);
		}

		private static int GetFactionToFactionReputation(long leftFactionId, long rightFactionId)
		{
			return MyAPIGateway.Session.Factions.GetReputationBetweenFactions(leftFactionId, rightFactionId);
		}

		private static int GetIdentityToFactionReputation(long identityId, long factionId)
		{
			return MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(identityId, identityId);
		}

		#endregion

		private void SetupNewFactionRelationship(long id, FactionType type)
		{
			switch (type)
			{
				case FactionType.Player:
					foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
					{   //DefaultNeutralRep;
						SetFactionFactionRep(id, faction.Key, GeneralSettings.DefaultNeutralRep);
					}

					foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
					{   //DefaultNegativeRep;
						SetFactionFactionRep(id, faction.Key, GeneralSettings.DefaultNegativeRep);
					}
					break;
				case FactionType.Npc:
					foreach (KeyValuePair<long, IMyFaction> faction in _npcFactionDictionary)
					{   //DefaultNegativeRep;
						SetFactionFactionRep(id, faction.Key, GeneralSettings.DefaultNegativeRep);
					}
					break;
				default:
					return;
			}
		}

		private void SetupNewIdentityRelationship(long id)
		{
			IMyFaction tmp = MyAPIGateway.Session.Factions.TryGetPlayerFaction(id);
			if (tmp != null)
				return; // If a new player is in a faction already, it happened outside of EEM being loaded, so just run with the faction rep instead

			foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
			{   //DefaultNeutralRep;
				SetIdentityFactionRep(id, faction.Key, GeneralSettings.DefaultNeutralRep);
			}

			foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
			{   //DefaultNegativeRep;
				SetIdentityFactionRep(id, faction.Key, GeneralSettings.DefaultNegativeRep);
			}

			foreach (KeyValuePair<long, IMyFaction> faction in _nonEemNpcFactionDictionary)
			{   //DefaultNegativeRep;
				SetIdentityFactionRep(id, faction.Key, GeneralSettings.DefaultNegativeRep);
			}
		}



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
				foreach (KeyValuePair<long, IMyFaction> faction in MyAPIGateway.Session.Factions.Factions)
				{   //DefaultNeutralRep;
					if (!faction.Value.IsEveryoneNpc() || tmp.FactionId == faction.Key) continue;
					int newMemberToFactionRep = GetIdentityToFactionReputation(id, faction.Key);
					int currentFactionFactionRep = GetFactionToFactionReputation(tmp.FactionId, faction.Key);
					int currentFactionMemberCount = MyAPIGateway.Session.Factions.Factions[tmp.FactionId].Members.Count;
					if (newMemberToFactionRep < GeneralSettings.DefaultNeutralRep)
						SetEntireFactionRep(tmp.FactionId, faction.Key, newMemberToFactionRep);
					else SetEntireFactionRep(tmp.FactionId, faction.Key, (
						((currentFactionFactionRep * currentFactionMemberCount - 1) + newMemberToFactionRep) / currentFactionMemberCount
					));
				}
			}
			catch (Exception e)
			{
				WriteToLog("NewFactionMember", $"Exception! Identity {id} or some faction lookup likely didn't exist: \t{e}", LogType.Exception);
			}
		}

		public void FactionTimer()
		{
			// Use this to do faction decay - faster from war to neutral than above neutral to baseline neutral - this clock will always run.
			// It should take around 15 or 20 minutes to decay -550 to -500 (1 war incident back to neutral)
			//  Need to check if adjusting rep is thread safe 

		}





		public void Close()
		{
			WriteToLog("RelationshipManager.Close", $"Cooling down...", LogType.General);
			MyAPIGateway.Session.Factions.FactionStateChanged -= FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated -= FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited -= FactionEdited;
			MyAPIGateway.Session.Factions.FactionAutoAcceptChanged -= MonitorAutoAccept;
			//WarQueue.Clear();
			_playerFactionDictionary.Clear();
			_playerPirateFactionDictionary.Clear();
			_pirateFactionDictionary.Clear();
			_enforcementFactionDictionary.Clear();
			_lawfulFactionDictionary.Clear();
			_npcFactionDictionary.Clear();
			_nonEemNpcFactionDictionary.Clear();
			_knownHostileIdentities.Clear();
			IdentityRelationships.Clear();
			FactionRelationships.Clear();
			//TimedNegativeRelationships.Clear();
			//MendingRelationships.Clear();
			_dialogue.Unload();
			WriteToLog("RelationshipManager.Close", $"Ready for a vacation!", LogType.General);
		}



		#region Relation State Changes
		// All relational state change code is here.  Everything else just supports one of these methods firing.
		private void AcceptPeace(long fromFactionId, long toFactionId)
		{
			//if (_newFactionDictionary.ContainsKey(fromFactionId))
			//{
			//	if (_newFactionDictionary[fromFactionId] > 1)
			//		_newFactionDictionary[fromFactionId]--;
			//	else
			//	{

			//		RequestNewFactionDialog(fromFactionId);
			//		_newFactionDictionary.Remove(fromFactionId);
			//	}
			//}

			//SetRep(fromFactionId,toFactionId,false);
			//MyAPIGateway.Session.Factions.AcceptPeace(fromFactionId, toFactionId);
		}

		private static void PeaceAccepted(long fromFactionId, long toFactionId)
		{   // Clearing those leftover flags
			//ClearPeace(fromFactionId, toFactionId);
		}


		private static void AutoPeace(long fromFactionId, long toFactionId)
		{
			//SetRep(fromFactionId, toFactionId, false);
			//MyAPIGateway.Utilities.InvokeOnGameThread(() =>
			//{
			//	MyAPIGateway.Session.Factions.SendPeaceRequest(fromFactionId, toFactionId);
			//	MyAPIGateway.Session.Factions.AcceptPeace(toFactionId, fromFactionId);
			//	ClearPeace(fromFactionId, toFactionId);
			//});
		}

		private static void ClearPeace(long fromFactionId, long toFactionId)
		{   // Stops the flag from hanging out in the faction menu
			MyAPIGateway.Utilities.InvokeOnGameThread(() =>
			{
				MyAPIGateway.Session.Factions.CancelPeaceRequest(toFactionId, fromFactionId);
				MyAPIGateway.Session.Factions.CancelPeaceRequest(fromFactionId, toFactionId);
			});
		}

		private static void DeclareWar(long npcFaction, long playerFaction)
		{   // Vanilla war declaration, ensures invoking on main thread
			//SetRep(npcFaction, playerFaction, true);
			//MyAPIGateway.Utilities.InvokeOnGameThread(() => MyAPIGateway.Session.Factions.DeclareWar(npcFaction, playerFaction));
		}

		// Peace

		private void DeclareFullNpcPeace(long factionId)
		{
			try
			{
				foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
					AutoPeace(lawfulFaction.Key, factionId);
			}
			catch (Exception e)
			{
				WriteToLog("DeclareFullNpcPeace", $"Exception!\t{e}", LogType.Exception);
			}
		}

		// War

		private void WarDeclared(long fromFactionId, long toFactionId)
		{   // Going to take the stance that if a war is declared by an NPC, it's a valid war
			// TODO Add dialogue for when a player declares war on an NPC directly
			if (fromFactionId.GetFactionById().IsEveryoneNpc())
				VetNewWar(fromFactionId, toFactionId);
			//// This is for when a player declares war on an NPC 
			//if (!fromFactionId.GetFactionById().IsEveryoneNpc() && toFactionId.GetFactionById().IsEveryoneNpc())
			//	DeclarePermanentNpcWar(toFactionId, fromFactionId);
		}

		private void War(long npcFactionId, long playerFactionId)
		{
			// TODO just a bookmark!
			WriteToLog("War", $"npcFaction:\t{npcFactionId}\tplayerFaction:\t{playerFactionId}", LogType.Debug);
			NewTimedNegativeRelationship(npcFactionId, playerFactionId);
			RequestDialog(npcFactionId.GetFactionById(), playerFactionId.GetFactionById(), Dialogue.DialogType.WarDeclared);
			DeclareWar(npcFactionId, playerFactionId);
		}

		private void DeclarePermanentNpcWar(long npcFaction, long playerFaction)
		{   // Used for when a player declares war on a NPC
			DeclareWar(npcFaction, playerFaction);
		}

		public void WarDeclaration(long npcFactionId, long playerFactionId)
		{   // Used by BotBase to declare war until I have the time to redo bots/ai
			// May revisit parallel threading for this in the future, for now, it's fine as is
			//MyAPIGateway.Parallel.Start(delegate
			//{ 
			WarQueue.Enqueue(new PendingRelation(npcFactionId, playerFactionId));
			ProcessWarQueue();
			//});
		}

		private void DeclareFullNpcWar(long playerFactionId)
		{   // TODO: Used to declare war against a player for violating the rules of engagement (unused for now, but in place for when it's required)
			try
			{
				foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
					NewTimedNegativeRelationship(lawfulFaction.Key, playerFactionId);
				//RequestNewPirateDialog(playerFactionId); replace this with collective disappointment
			}
			catch (Exception e)
			{
				WriteToLog("DeclareFullNpcWar", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private void DeclarePermanentFullNpcWar(long playerFaction)
		{   // Used to declare war against a player pirate
			try
			{
				foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
					DeclareWar(lawfulFaction.Key, playerFaction);
				RequestNewPirateDialog(playerFaction);
			}
			catch (Exception e)
			{
				WriteToLog("DeclarePermanentFullNpcWar", $"Exception!\t{e}", LogType.Exception);
			}
		}

		#endregion

		private void FactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
		{
			WriteToLog("FactionStateChanged",
				$"Action:\t{action.ToString()}\tfromFaction:\t{fromFactionId}\ttag:\t{fromFactionId.GetFactionById()?.Tag}\ttoFaction:\t{toFactionId}\ttag:\t{toFactionId.GetFactionById()?.Tag}\tplayerId:\t{playerId}\tsenderId:\t{senderId}",
				LogType.General);
			if (action != MyFactionStateChange.RemoveFaction)
				if (fromFactionId == 0L || toFactionId == 0L) return;

			switch (action)
			{
				case MyFactionStateChange.RemoveFaction:
					FactionRemoved(fromFactionId);
					break;
				case MyFactionStateChange.SendPeaceRequest:
					PeaceRequestSent(fromFactionId, toFactionId);
					break;
				case MyFactionStateChange.CancelPeaceRequest:
					PeaceCancelled(fromFactionId, toFactionId);
					break;
				case MyFactionStateChange.AcceptPeace:
					PeaceAccepted(fromFactionId, toFactionId);
					break;
				case MyFactionStateChange.DeclareWar:
					WarDeclared(fromFactionId, toFactionId);
					break;
				case MyFactionStateChange.FactionMemberSendJoin: // Unused
					break;
				case MyFactionStateChange.FactionMemberCancelJoin: // Unused
					break;
				case MyFactionStateChange.FactionMemberAcceptJoin:
					// fromFactionId and toFactionId are identical here, playerId is the new player 
					NewFactionMember(playerId);
					break;
				case MyFactionStateChange.FactionMemberKick:
					AddFactionMember(fromFactionId.GetFactionById());
					break;
				case MyFactionStateChange.FactionMemberPromote: // Unused
					break;
				case MyFactionStateChange.FactionMemberDemote: // Unused
					break;
				case MyFactionStateChange.FactionMemberLeave: // Unused
					break;
				case MyFactionStateChange.FactionMemberNotPossibleJoin: // Unused
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

		private void PeaceRequestSent(long fromFactionId, long toFactionId)
		{   // So many reasons to clear peace...
			// This is no longer a concept with EEM / new factions; leaving this method here for no reason at all.  
		}

		#region Dialogue triggers

		private void RequestDialog(IMyFaction npcFaction, IMyFaction playerFaction, Dialogue.DialogType type)
		{
			try
			{
				Func<string> message = _dialogue.RequestDialog(npcFaction, type);
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
				Func<string> message = _dialogue.RequestDialog(null, Dialogue.DialogType.CollectiveWelcome);
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
				Func<string> message = _dialogue.RequestDialog(null, Dialogue.DialogType.CollectiveDisappointment);
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
				Func<string> message = _dialogue.RequestDialog(null, Dialogue.DialogType.CollectiveReprieve);
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

		// Dictionary methods

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


		// Checks and balances, internal and external, mostly static

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

		private void ProcessWarQueue()
		{
			try
			{
				while (WarQueue.Count > 0)
				{
					bool found = false;
					PendingRelation tmpRelation = WarQueue.Dequeue();
					if (tmpRelation.NpcFaction == 0L || tmpRelation.PlayerFaction == 0L) continue;
					if (_playerPirateFactionDictionary.ContainsKey(tmpRelation.PlayerFaction) || _pirateFactionDictionary.ContainsKey(tmpRelation.NpcFaction)) continue;
					TimedRelationship newTimedRelationship = new TimedRelationship(tmpRelation.NpcFaction.GetFactionById(), tmpRelation.PlayerFaction.GetFactionById(), GeneralSettings.FactionNegativeRelationshipCooldown);
					for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
					{
						if (!TimedNegativeRelationships[i].Equals(newTimedRelationship)) continue;
						TimedNegativeRelationships[i].CooldownTime = GeneralSettings.FactionNegativeRelationshipCooldown;
						found = true;
					}
					if (!found) War(tmpRelation.NpcFaction, tmpRelation.PlayerFaction);

					foreach (KeyValuePair<long, IMyFaction> enforcementFaction in _enforcementFactionDictionary)
					{
						found = false;
						newTimedRelationship = new TimedRelationship(enforcementFaction.Value, tmpRelation.PlayerFaction.GetFactionById(), GeneralSettings.FactionNegativeRelationshipCooldown);
						for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
						{
							if (!TimedNegativeRelationships[i].Equals(newTimedRelationship)) continue;
							TimedNegativeRelationships[i].CooldownTime = GeneralSettings.FactionNegativeRelationshipCooldown;
							found = true;
						}
						if (!found) War(enforcementFaction.Key, tmpRelation.PlayerFaction);
					}
				}
			}
			catch (Exception e)
			{
				WriteToLog("ProcessWarQueue", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private void VetNewWar(long npcFactionId, long playerFactionId)
		{
			//try
			//{
			//	if (_newFactionDictionary.ContainsKey(playerFactionId))
			//	{
			//		if (_lawfulFactionDictionary.ContainsKey(npcFactionId)) _newFactionDictionary[playerFactionId]++;
			//		if (_newFactionDictionary[playerFactionId] != _lawfulFactionDictionary.Count) return;
			//		DeclareFullNpcPeace(playerFactionId);
			//		return;
			//	}

			//	if (_playerPirateFactionDictionary.ContainsKey(playerFactionId)) return;
			//	WarDeclaration(npcFactionId, playerFactionId);
			//}
			//catch (Exception e)
			//{
			//	WriteToLog("VetNewWar", $"Exception!\t{e}", LogType.Exception);
			//}
		}
		
	}
}






//private void SetupNewRelationship(long identityId, bool enforce = false)
//{   // This will setup a new identity to default relationship status
//	WriteToLog("SetupNewRelationship", $"Setting up a new relationship... {identityId}", LogType.General);
//	if (enforce) EnforceReputations(identityId); // The defaults have been set, now to force faction status
//}

//public void NewIdentityDetected(long identityId)
//{   // Idea here is that the entity manager will key off Factions to handle any new identity detected on the server (player or NPC)
//	WriteToLog("NewIdentityDetected", $"{identityId} is a new friend!!!", LogType.General);
//	if (_relationMaster.ContainsKey(identityId))
//		EnforceReputations(identityId); // If we already know who this identity is, just enforce the faction status
//	else SetupNewRelationship(identityId, true); // This identity is new, so set it up as such
//}

//private void EnforceReputations(long identityId)
//{
//	WriteToLog("EnforceReputations", $"Making them all play nice...", LogType.General);
//	IMyFaction identityFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(identityId);
//	if (identityFaction == null)	// identity has no faction, nothing to enforce
//		return;
//	// Get rep from faction -> faction relationships
//	// Iterate identity with all factions and 
//}

//private void EnforceAllReputations()
//{
//	WriteToLog("EnforceReputations", $"Making them all play nice...", LogType.General);
//}

//#region Eco Reputation Hooks for relationships

//private static void SetRep(long npcFactionId, long playerFactionId, bool hostile)
//{
//	int value;

//	if (hostile)
//		value = -550;
//	else
//		value = -500;

//	try
//	{
//		MyAPIGateway.Session.Factions.SetReputation(npcFactionId, playerFactionId, value);
//		MyAPIGateway.Session.Factions.SetReputation(playerFactionId, npcFactionId, value);

//		SetRepPlayers(npcFactionId, playerFactionId, hostile);
//	}
//	catch (Exception e)
//	{
//		FactionCore.FactionCoreStaticInstance.WriteToLog("SetRep", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private static void SetRepPlayers(long npcFactionId, long playerFactionId, bool hostile)
//{
//	IMyFaction npcFaction = npcFactionId.GetFactionById();
//	IMyFaction playerFaction = playerFactionId.GetFactionById();
//	int value;

//	if (hostile)
//		value = -550;
//	else
//		value = -500;

//	try
//	{
//		foreach (KeyValuePair<long, MyFactionMember> npcFactionMember in npcFaction.Members)
//		{
//			MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(npcFactionMember.Value.PlayerId,
//				playerFactionId, value);
//		}
//		foreach (KeyValuePair<long, MyFactionMember> playerFactionMember in playerFaction.Members)
//		{
//			MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(playerFactionMember.Value.PlayerId,
//				npcFactionId, value);
//		}
//	}
//	catch (Exception e)
//	{
//	 FactionCore.FactionCoreStaticInstance.WriteToLog("SetRepPlayers", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//public void SetRepDebug(int value)
//{
//	foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
//	{
//		foreach (KeyValuePair<long, IMyFaction> playerFaction in _playerFactionDictionary)
//		{
//			Messaging.ShowLocalNotification($"Adjusting rep with {lawfulFaction.Value.Tag} and {playerFaction.Value.Tag} by {value}");
//			WriteToLog("SetRepDebug", $"Adjusting rep with {lawfulFaction.Value.Tag} and {playerFaction.Value.Tag} by {value}", LogType.General);
//			SetRep(lawfulFaction.Key, playerFaction.Key, value);
//		}
//	}
//}

//private void SetRep(long npcFactionId, long playerFactionId, int value)
//{
//	try
//	{
//		MyAPIGateway.Session.Factions.SetReputation(npcFactionId, playerFactionId, MyAPIGateway.Session.Factions.GetReputationBetweenFactions(npcFactionId, playerFactionId) + value);
//		//MyAPIGateway.Session.Factions.SetReputation(playerFactionId, npcFactionId, MyAPIGateway.Session.Factions.GetReputationBetweenFactions(playerFactionId, npcFactionId) + value);

//		WriteToLog("SetRep-Npc -> Player", $"{_npcFactionDictionary[npcFactionId].Tag} | {_playerFactionDictionary[playerFactionId].Tag} | {MyAPIGateway.Session.Factions.GetReputationBetweenFactions(npcFactionId, playerFactionId)}", LogType.General);
//		WriteToLog("SetRep-Player -> Npc", $"{_playerFactionDictionary[playerFactionId].Tag} | {_npcFactionDictionary[npcFactionId].Tag} | {MyAPIGateway.Session.Factions.GetReputationBetweenFactions(npcFactionId, playerFactionId)}", LogType.General);
//		SetRepPlayers(npcFactionId, playerFactionId, value);
//	}
//	catch (Exception e)
//	{
//		WriteToLog("SetRep", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void SetRepPlayers(long npcFactionId, long playerFactionId, int value)
//{
//	IMyFaction npcFaction = npcFactionId.GetFactionById();
//	IMyFaction playerFaction = playerFactionId.GetFactionById();

//	List<IMyIdentity> myIdentities = new List<IMyIdentity>();

//	try
//	{
//		foreach (KeyValuePair<long, MyFactionMember> npcFactionMember in npcFaction.Members)
//		{
//			MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(npcFactionMember.Value.PlayerId,
//				playerFactionId, MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(npcFactionMember.Value.PlayerId, playerFactionId) + value);
//			myIdentities.Clear();
//			MyAPIGateway.Players.GetAllIdentites(myIdentities, x => x.IdentityId == npcFactionMember.Value.PlayerId);
//			WriteToLog("SetRepPlayers-NpcLoop", $"{myIdentities.FirstOrDefault()?.DisplayName} | {npcFactionMember.Value.PlayerId} | {MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(npcFactionMember.Value.PlayerId, playerFactionId)}", LogType.General);
//		}

//		foreach (KeyValuePair<long, MyFactionMember> playerFactionMember in playerFaction.Members)
//		{
//			MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(playerFactionMember.Value.PlayerId,
//				npcFactionId, MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(playerFactionMember.Value.PlayerId, npcFactionId) + value);
//			myIdentities.Clear();
//			MyAPIGateway.Players.GetAllIdentites(myIdentities, x => x.IdentityId == playerFactionMember.Value.PlayerId);
//			WriteToLog("SetRepPlayers-PlayerLoop", $"{myIdentities.FirstOrDefault()?.DisplayName} | {playerFactionMember.Value.PlayerId} | {MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(playerFactionMember.Value.PlayerId, npcFactionId)}", LogType.General);
//		}
//	}
//	catch (Exception e)
//	{
//		WriteToLog("SetRepPlayers", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//#endregion

//private void PeaceCancelled(long fromFactionId, long toFactionId)
//{   // The only time this matters is if a former player pirate declares war on a NPC, then declares peace, then revokes the peace declaration
//	if (!CheckMentingRelationship(fromFactionId, toFactionId)) return;
//	RemoveMendingRelationship(toFactionId, fromFactionId);
//}

//private void SetupPlayerRelations()
//{
//	foreach (KeyValuePair<long, IMyFaction> playerFaction in _playerFactionDictionary)
//	{
//		foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
//		{
//			AutoPeace(playerFaction.Key, lawfulFaction.Key);
//		}
//	}
//}

//private void SetupNpcRelations()
//{
//	foreach (KeyValuePair<long, IMyFaction> leftPair in _lawfulFactionDictionary)
//	{
//		foreach (KeyValuePair<long, IMyFaction> rightPair in _lawfulFactionDictionary)
//		{
//			if (leftPair.Key == rightPair.Key || !MyAPIGateway.Session.Factions.AreFactionsEnemies(leftPair.Key, rightPair.Key)) continue;
//			AutoPeace(leftPair.Key, rightPair.Key);
//		}
//	}
//}

//private void SetupPirateRelations()
//{
//	foreach (KeyValuePair<long, IMyFaction> faction in MyAPIGateway.Session.Factions.Factions)
//	{
//		foreach (KeyValuePair<long, IMyFaction> pirate in _pirateFactionDictionary)
//		{
//			if (faction.Key == pirate.Key) continue;
//			DeclareWar(faction.Key, pirate.Key);
//		}
//	}
//}

//private static bool CheckEitherFactionForNpc(long leftFactionId, long rightFactionId)
//{
//	return leftFactionId.GetFactionById().IsEveryoneNpc() || rightFactionId.GetFactionById().IsEveryoneNpc();
//}

//private bool CheckTimedNegativeRelationshipState(long npcFaction, long playerFaction)
//{
//	return TimedNegativeRelationships.IndexOf(new TimedRelationship(npcFaction.GetFactionById(), playerFaction.GetFactionById(), 0)) > -1 || TimedNegativeRelationships.IndexOf(new TimedRelationship(playerFaction.GetFactionById(), npcFaction.GetFactionById(), 0)) > -1;
//}

//private bool CheckMentingRelationship(long fromFactionId, long toFactionId)
//{
//	return MendingRelationships.Contains(new PendingRelation(fromFactionId, toFactionId));
//}

// Relationship Managers

//private void NewMendingRelationship(long npcFactionId, long playerFactionId)
//{
//	try
//	{
//		PendingRelation newMendingRelation = new PendingRelation(npcFactionId, playerFactionId);
//		for (int i = MendingRelationships.Count - 1; i >= 0; i--)
//		{
//			if (MendingRelationships[i].Equals(newMendingRelation))
//				return;
//		}
//		RequestDialog(npcFactionId.GetFactionById(), playerFactionId.GetFactionById(), Dialogue.DialogType.PeaceConsidered);
//		AddToMendingRelationships(newMendingRelation);
//		FactionTimer(MyUpdateOrder.BeforeSimulation);
//	}
//	catch (Exception e)
//	{
//		WriteToLog("NewMendingRelationship", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void RemoveMendingRelationship(long npcFactionId, long playerFactionId)
//{
//	try
//	{
//		PendingRelation newMendingRelation = new PendingRelation(npcFactionId, playerFactionId);
//		for (int i = MendingRelationships.Count - 1; i >= 0; i--)
//		{
//			if (MendingRelationships[i].Equals(newMendingRelation))
//				MendingRelationships.RemoveAtFast(i);
//			ClearPeace(playerFactionId, npcFactionId);
//		}
//		CheckCounts();
//	}
//	catch (Exception e)
//	{
//		WriteToLog("RemoveMendingRelationship", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void NewTimedNegativeRelationship(long npcFactionId, long playerFactionId)
//{
//	int cooldown = GeneralSettings.FactionNegativeRelationshipCooldown + GeneralSettings.Random.Next(GeneralSettings.TicksPerSecond * 30, GeneralSettings.TicksPerMinute * 2);
//	AddToTimedNegativeRelationships(new TimedRelationship(npcFactionId.GetFactionById(), playerFactionId.GetFactionById(), cooldown));
//}

//private void AddToTimedNegativeRelationships(TimedRelationship newTimedRelationship)
//{
//	WriteToLog("AddToTimedNegativeRelationships", $"newTimedRelationship:\t{newTimedRelationship}", LogType.Debug);
//	TimedNegativeRelationships.Add(newTimedRelationship);
//	RemoveMendingRelationship(newTimedRelationship.NpcFaction.FactionId, newTimedRelationship.PlayerFaction.FactionId);
//	DumpEverythingToTheLog();
//	FactionTimer(MyUpdateOrder.BeforeSimulation);
//}

//private void AddToMendingRelationships(PendingRelation newMendingRelation)
//{
//	WriteToLog("AddToMendingRelationships", $"newTimedRelationship:\t{newMendingRelation}", LogType.Debug);
//	MendingRelationships.Add(newMendingRelation);
//}

//private void AssessNegativeRelationships()
//{
//	try
//	{
//		WriteToLog("AssessNegativeRelationships", $"TimedNegativeRelationships.Count:\t{TimedNegativeRelationships.Count}", LogType.Debug);
//		DumpTimedNegativeFactionRelationships();
//		for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
//		{
//			if ((TimedNegativeRelationships[i].CooldownTime -= GeneralSettings.FactionNegativeRelationshipAssessment) > 0) continue;
//			NewMendingRelationship(TimedNegativeRelationships[i].NpcFaction.FactionId, TimedNegativeRelationships[i].PlayerFaction.FactionId);
//			TimedNegativeRelationships.RemoveAtFast(i);
//		}
//	}
//	catch (Exception e)
//	{
//		WriteToLog("AssessNegativeRelationships", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void AssessMendingRelationships()
//{
//	try
//	{
//		WriteToLog("AssessMendingRelationships", $"MendingRelationships.Count:\t{TimedNegativeRelationships.Count}", LogType.Debug);
//		DumpMendingRelationshipsRelationships();
//		for (int i = MendingRelationships.Count - 1; i >= 0; i--)
//		{
//			if (GeneralSettings.Random.Next(0, 100) < 75) continue;
//			PendingRelation relationToRemove = MendingRelationships[i];
//			MendingRelationships.RemoveAtFast(i);
//			RequestDialog(relationToRemove.NpcFaction.GetFactionById(), relationToRemove.PlayerFaction.GetFactionById(), Dialogue.DialogType.PeaceAccepted);
//			AutoPeace(relationToRemove.NpcFaction, relationToRemove.PlayerFaction);
//		}
//	}
//	catch (Exception e)
//	{
//		WriteToLog("AssessMendingRelationships", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void ClearRemovedFactionFromRelationships(long factionId)
//{
//	try
//	{
//		for (int i = MendingRelationships.Count - 1; i >= 0; i--)
//		{
//			if (MendingRelationships[i].NpcFaction == factionId || MendingRelationships[i].PlayerFaction == factionId)
//				MendingRelationships.RemoveAtFast(i);
//		}
//		for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
//		{
//			if (TimedNegativeRelationships[i].NpcFaction.FactionId == factionId || TimedNegativeRelationships[i].PlayerFaction.FactionId == factionId)
//				TimedNegativeRelationships.RemoveAtFast(i);
//		}
//		CheckCounts();
//	}
//	catch (Exception e)
//	{
//		WriteToLog("ClearRemovedFactionFromRelationships", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void CheckCounts()
//{
//	//if (MendingRelationships.Count == 0 && TimedNegativeRelationships.Count == 0) FactionTimer(MyUpdateOrder.NoUpdate);
//	WriteToLog("CheckCounts", $"MendingRelationships:\t{MendingRelationships.Count}\tTimedNegativeRelationship:\t{TimedNegativeRelationships.Count}", LogType.Debug);
//}



// External calls to manage internal relationships

//public void CheckNegativeRelationships()
//{
//	AssessNegativeRelationships();
//	CheckCounts();
//}

//public void CheckMendingRelationships()
//{
//	AssessMendingRelationships();
//	CheckCounts();
//}

//Debug Outputs

//private void DumpEverythingToTheLog()
//{
//	if (!GeneralSettings.DebugMode) return;
//	try
//	{
//		const string callerName = "FactionsDump";
//		List<TimedRelationship> tempTimedRelationship = TimedNegativeRelationships;
//		foreach (TimedRelationship negativeRelationship in tempTimedRelationship)
//			WriteToLog(callerName, $"negativeRelationship:\t{negativeRelationship}", LogType.Debug);
//		List<PendingRelation> tempMendingRelations = MendingRelationships;
//		foreach (PendingRelation mendingRelationship in tempMendingRelations)
//			WriteToLog(callerName, $"mendingRelationship:\t{mendingRelationship}", LogType.Debug);
//		Dictionary<long, IMyFaction> tempFactionDictionary = _enforcementFactionDictionary;
//		foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
//			WriteToLog(callerName, $"enforcementDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
//		tempFactionDictionary = _lawfulFactionDictionary;
//		foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
//			WriteToLog(callerName, $"lawfulDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
//		tempFactionDictionary = _pirateFactionDictionary;
//		foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
//			WriteToLog(callerName, $"pirateDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
//		tempFactionDictionary = _npcFactionDictionary; //_nonEemNpcFactionDictionary
//		foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
//			WriteToLog(callerName, $"npcDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
//		tempFactionDictionary = _nonEemNpcFactionDictionary;
//		foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
//			WriteToLog(callerName, $"_nonEemNpcFactionDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
//		tempFactionDictionary = _playerFactionDictionary;
//		foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
//			WriteToLog(callerName, $"playerDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
//		tempFactionDictionary = _playerPirateFactionDictionary;
//		foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
//			WriteToLog(callerName, $"playerPirateDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
//		Dictionary<long, int> tempNewFactionDictionary = _newFactionDictionary;
//		foreach (KeyValuePair<long, int> faction in tempNewFactionDictionary)
//			WriteToLog(callerName, $"newFactionDictionary:\t{faction}\t{faction.Key.GetFactionById()?.Tag}", LogType.Debug);
//	}
//	catch (Exception e)
//	{
//		WriteToLog("DumpEverythingToTheLog", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void DumpNewFactionDictionary()
//{
//	try
//	{
//		WriteToLog("DumpNewFactionDictionary", $"newFactionDictionary.Count:\t{_newFactionDictionary.Count}", LogType.Debug);
//		Dictionary<long, int> tempNewFactionDictioanry = _newFactionDictionary;
//		foreach (KeyValuePair<long, int> faction in tempNewFactionDictioanry)
//			WriteToLog("DumpNewFactionDictionary", $"newFactionDictionary:\t{faction}\t{faction.Key.GetFactionById()?.Tag}", LogType.Debug);
//	}
//	catch (Exception e)
//	{
//		WriteToLog("DumpNewFactionDictionary", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void DumpTimedNegativeFactionRelationships()
//{
//	if (!Common.Settings.GeneralSettings.DebugMode) return;
//	try
//	{
//		WriteToLog("DumpTimedNegativeFactionRelationships", $"TimedNegativeRelationships.Count:\t{TimedNegativeRelationships.Count}", LogType.Debug);
//		const string callerName = "DumpTimedNegativeFactionRelationships";
//		List<TimedRelationship> tempTimedRelationship = TimedNegativeRelationships;
//		foreach (TimedRelationship negativeRelationship in tempTimedRelationship)
//			WriteToLog(callerName, $"negativeRelationship:\t{negativeRelationship}", LogType.Debug);
//	}
//	catch (Exception e)
//	{
//		WriteToLog("DumpTimedNegativeFactionRelationships", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void DumpMendingRelationshipsRelationships()
//{
//	if (!Common.Settings.GeneralSettings.DebugMode) return;
//	try
//	{
//		const string callerName = "DumpMendingRelationshipsRelationships";
//		List<PendingRelation> tempMendingRelations = MendingRelationships;
//		foreach (PendingRelation mendingRelationship in tempMendingRelations)
//			WriteToLog(callerName, $"mendingRelationship:\t{mendingRelationship}", LogType.Debug);
//	}
//	catch (Exception e)
//	{
//		WriteToLog("DumpMendingRelationshipsRelationships", $"Exception!\t{e}", LogType.Exception);
//	}
//}

//private void WriteToLog(string caller, string message)
//{
//	WriteToLog(caller, message, true);
//	if(!"DumpEverythingToTheLog, DumpTimedNegativeFactionRelationships, DumpMendingRelationshipsRelationships, DumpNewFactionDictionary".Contains(caller))
//		DumpEverythingToTheLog(true);
//}

// Structs and other enums as necessary

//private struct PendingRelation
//{
//	public readonly long NpcFaction;
//	public readonly long PlayerFaction;

//	/// <inheritdoc />
//	public override string ToString()
//	{
//		return $"NpcFaction:\t{NpcFaction}\t{NpcFaction.GetFactionById()?.Tag}\tNpcFaction:\t{PlayerFaction}\t{PlayerFaction.GetFactionById()?.Tag}";
//	}

//	public PendingRelation(long npcFactionId, long playerFactionId)
//	{
//		NpcFaction = npcFactionId;
//		PlayerFaction = playerFactionId;
//	}
//}