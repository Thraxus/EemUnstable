using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Factions.Settings;
using Eem.Thraxus.Factions.Utilities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.Models
{
	public class RelationshipManager : FactionCore
	{
		public RelationshipManager()
		{
			_playerFactionDictionary = new Dictionary<long, IMyFaction>();
			_pirateFactionDictionary = new Dictionary<long, IMyFaction>();
			_playerPirateFactionDictionary = new Dictionary<long, IMyFaction>();
			_enforcementFactionDictionary = new Dictionary<long, IMyFaction>();
			_lawfulFactionDictionary = new Dictionary<long, IMyFaction>();
			_npcFactionDictionary = new Dictionary<long, IMyFaction>();
			MyAPIGateway.Session.Factions.FactionStateChanged += FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated += FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited += FactionEdited;
			SetupFactionRelations();
			TimedNegativeRelationships = new List<TimedRelationship>();
			MendingRelationships = new List<MendingRelation>();
		}

		public void Unload()
		{
			_playerFactionDictionary.Clear();
			_playerPirateFactionDictionary.Clear();
			_pirateFactionDictionary.Clear();
			_enforcementFactionDictionary.Clear();
			_lawfulFactionDictionary.Clear();
			_npcFactionDictionary.Clear();
			MyAPIGateway.Session.Factions.FactionStateChanged -= FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated -= FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited -= FactionEdited;
			TimedNegativeRelationships.Clear();
			MendingRelationships.Clear();
		}

		private readonly Dictionary<long, IMyFaction> _playerFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _playerPirateFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _pirateFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _enforcementFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _lawfulFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _npcFactionDictionary;

		private List<TimedRelationship> TimedNegativeRelationships { get; }

		private List<MendingRelation> MendingRelationships { get; }
		

		// Custom Events and Handler's

		/// <summary>
		/// Triggered whenever faction relations need a monitor / timer
		/// </summary>
		public event EventHandler EnableTimer;

		/// <summary>
		/// The handler for the EnableTimer event
		/// </summary>
		private void OnEnableTimer()
		{
			EnableTimer?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Triggered when both BadRelations and MendingRelations lists are cleared, tells the session to go to sleep
		/// </summary>
		public event EventHandler DisableTimer;

		/// <summary>
		/// 
		/// </summary>
		private void OnDisableTimer()
		{
			DisableTimer?.Invoke(this, EventArgs.Empty);
		}

		private void FactionEdited(long factionId)
		{
			FactionEditedOrCreated(factionId);
		}

		private void FactionCreated(long factionId)
		{
			FactionEditedOrCreated(factionId, true);
		}

		private void FactionEditedOrCreated(long factionId, bool newFaction = false)
		{
			IMyFaction playerFaction = factionId.GetFactionById();
			if (playerFaction == null || playerFaction.IsEveryoneNpc()) return; // I'm not a player faction, or I don't exist.  Peace out, suckas!
			if (CheckPiratePlayerOptIn(playerFaction) && _playerPirateFactionDictionary.ContainsKey(factionId)) return; // I'm a player pirate, and you know it.  Laterz!
			if (CheckPiratePlayerOptIn(playerFaction) && !_playerPirateFactionDictionary.ContainsKey(factionId)) // I'm a player pirate, but this is news to you...
			{
				_playerPirateFactionDictionary.Add(factionId, playerFaction);
				DeclareFullNpcWar(factionId);
				return;
			}
			if (!CheckPiratePlayerOptIn(playerFaction) && _playerPirateFactionDictionary.ContainsKey(factionId)) // I was a player pirate, but uh, I changed... I swear... 
			{
				_playerPirateFactionDictionary.Remove(factionId);
				HandleFormerPlayerPirate(factionId);
				return;
			}
			if (!newFaction) return;
			DeclareFullNpcPeace(factionId);  // I'm new man, just throw me a bone.
		}

		private void FactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
		{
			WriteToLog("FactionStateChanged", $"fromFaction:\t{fromFactionId.GetFactionById()?.Tag}\ttoFaction:\t{toFactionId.GetFactionById()?.Tag}\tAction:\t{action.ToString()}");

			if (CheckEitherFactionForNpc(fromFactionId, toFactionId)) return; // if at least one party in this adventure isn't an NPC, not my job!

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
				case MyFactionStateChange.AcceptPeace: // Unused
					break;
				case MyFactionStateChange.DeclareWar:
					WarDeclared(fromFactionId, toFactionId);
					break;
				case MyFactionStateChange.FactionMemberSendJoin: // Unused
					break;
				case MyFactionStateChange.FactionMemberCancelJoin: // Unused
					break;
				case MyFactionStateChange.FactionMemberAcceptJoin: // Unused
					break;
				case MyFactionStateChange.FactionMemberKick: // Unused
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
					WriteToLog("FactionStateChanged", $"Case not found:\t{nameof(action)}\t{action.ToString()}");
					break;
			}
		}

		private void FactionRemoved(long fromFactionId)
		{
			ScrubDictionaries(fromFactionId);
		}

		// So I remember how to do this later...
		//RequestDialog(toFaction.GetFactionById(), fromFaction.GetFactionById(), Dialogue.DialogType.PeaceRejected);

		private void PeaceRequestSent(long fromFactionId, long toFactionId)
		{	// So many reasons to clear peace...
			
			if ((_playerPirateFactionDictionary.ContainsKey(fromFactionId) || _playerPirateFactionDictionary.ContainsKey(toFactionId)) && CheckEitherFactionForNpc(fromFactionId, toFactionId))
			{   // Is this a player pirate somehow involved in peace accords with a NPC faction?
				ClearPeace(fromFactionId, toFactionId);
				return;
			}

			if (_lawfulFactionDictionary.ContainsKey(fromFactionId) && _pirateFactionDictionary.ContainsKey(toFactionId))
			{   // Is a NPC proposing peace to a player pirate?
				ClearPeace(fromFactionId, toFactionId);
				return;
			}
			
			if ((_pirateFactionDictionary.ContainsKey(toFactionId) || _pirateFactionDictionary.ContainsKey(fromFactionId)) && CheckEitherFactionForNpc(fromFactionId, toFactionId))
			{   // Pirates can't be friends (unless they are both players)!
				ClearPeace(fromFactionId, toFactionId);
				return;
			}

			if (fromFactionId.GetFactionById().IsNeutral(toFactionId.GetFactionById().FounderId))
			{   // Are these factions already neutral?
				ClearPeace(fromFactionId, toFactionId);
				return;
			}

			if (CheckTimedNegativeRelationshipState(fromFactionId, toFactionId))
			{   // Is either faction currently experiencing EEM controlled hostile relations?
				ClearPeace(fromFactionId, toFactionId);
				return;
			}

			if (!fromFactionId.GetFactionById().IsEveryoneNpc() && MyAPIGateway.Session.Factions.AreFactionsEnemies(fromFactionId, toFactionId))
			{	// This player was at war with an NPC by choice, so add them to the mending relationship category
				NewMendingRelationship(toFactionId, fromFactionId);
				return;
			}

			// Condition not accounted for, just accept the request for now (get logs!)
			WriteToLog("PeaceRequestSent", $"Unknown peace condition detected, please review...\tfromFaction:\t{fromFactionId.GetFactionById().Tag}\ttoFaction:\t{toFactionId.GetFactionById().Tag}");
			DumpEverythingToTheLog();
			MyAPIGateway.Session.Factions.AcceptPeace(toFactionId, fromFactionId);
			
		}

		private void PeaceCancelled(long fromFactionId, long toFactionId)
		{	// The only time this matters is if a former player pirate declares war on a NPC, then declares peace, then revokes the peace declaration
			if (!CheckMentingRelationship(fromFactionId, toFactionId)) return;
			RemoveMendingRelationship(toFactionId, fromFactionId);
		}

		private void WarDeclared(long fromFactionId, long toFactionId)
		{	// Going to take the stance that if a war is declared by an NPC, it's a valid war

			if (fromFactionId.GetFactionById().IsEveryoneNpc())
				NewTimedNegativeRelationship(fromFactionId, toFactionId);
		}

		// Dictionary methods

		private void SetupFactionRelations()
		{
			foreach (KeyValuePair<long, IMyFaction> factions in MyAPIGateway.Session.Factions.Factions)
			{
				WriteToLog("SetupFactionDictionaries", $"Faction loop:\tfaction.Key:\t{factions.Key}\tfaction.Value:\t{factions.Value}\tfaction.Tag:\t{factions.Value?.Tag}");
				try
				{
					if (factions.Value == null) continue;
					if (Constants.EnforcementFactionsTags.Contains(factions.Value.Tag))
					{
						WriteToLog("SetupFactionDictionaries", $"EnforcementFaction.Add:\t{factions.Key}\t{factions.Value.Tag}");
						AddToEnforcementFactionDictionary(factions.Key, factions.Value);
						AddToLawfulFactionDictionary(factions.Key, factions.Value);
						AddToNpcFactionDictionary(factions.Key, factions.Value);
						continue;
					}

					if (Constants.LawfulFactionsTags.Contains(factions.Value.Tag))
					{
						WriteToLog("SetupFactionDictionaries", $"AddToLawfulFactionDictionary.Add:\t{factions.Key}\t{factions.Value.Tag}");
						AddToLawfulFactionDictionary(factions.Key, factions.Value);
						AddToNpcFactionDictionary(factions.Key, factions.Value);
						continue;
					}

					if (factions.Value.IsEveryoneNpc())
					{ // If it's not an Enforcement or Lawful faction, it's a pirate.
						WriteToLog("SetupFactionDictionaries", $"AddToPirateFactionDictionary:\t{factions.Key}\t{factions.Value.Tag}");
						AddToPirateFactionDictionary(factions.Key, factions.Value);
						AddToNpcFactionDictionary(factions.Key, factions.Value);
						continue;
					}

					if (CheckPiratePlayerOptIn(factions.Value))
					{
						WriteToLog("SetupFactionDictionaries", $"PlayerFactionExclusionList.Add:\t{factions.Key}\t{factions.Value.Tag}");
						AddToPlayerPirateFactionDictionary(factions.Key, factions.Value);
						continue;
					}

					WriteToLog("SetupFactionDictionaries", $"PlayerFaction.Add:\t{factions.Key}\t{factions.Value.Tag}");
					AddToPlayerFactionDictionary(factions.Key, factions.Value);
				}
				catch (Exception e)
				{
					WriteToLog("SetupFactionDictionaries", $"Exception caught - e: {e}\tfaction.Key:\t{factions.Key}\tfaction.Value: {factions.Value}\tfaction.Tag:\t{factions.Value?.Tag}");
				}

			}

			SetupPlayerRelations();
			SetupNpcRelations();
			SetupPirateRelations();
			SetupAutoRelations();
		}

		private void SetupPlayerRelations()
		{
			foreach (KeyValuePair<long, IMyFaction> playerFaction in _playerFactionDictionary)
			{
				foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
				{
					WriteToLog("SetupPlayerRelations", $"SendPeace:\tplayerFaction:\t{playerFaction.Value.Tag}\tlawfulFaction:\t{lawfulFaction.Value.Tag}");
					WriteToLog("SetupPlayerRelations", $"AcceptPeace:\tlawfulFaction:\t{lawfulFaction.Value.Tag}\tplayerFaction:\t{playerFaction.Value.Tag}");
					AutoPeace(playerFaction.Key, lawfulFaction.Key);
					//playerFaction.Value.AutoPeace(lawfulFaction.Value);
				}
			}
		}

		private void SetupNpcRelations()
		{
			foreach (KeyValuePair<long, IMyFaction> leftPair in _lawfulFactionDictionary)
			{
				foreach (KeyValuePair<long, IMyFaction> rightPair in _lawfulFactionDictionary)
				{
					if (leftPair.Key == rightPair.Key) continue;
					WriteToLog("SetupNpcRelations", $"SendPeace:\tleftPair:\t{leftPair.Value.Tag}\trightPair:\t{rightPair.Value.Tag}");
					WriteToLog("SetupNpcRelations", $"AcceptPeace:\trightPair:\t{rightPair.Value.Tag}\tleftPair:\t{leftPair.Value.Tag}");
					AutoPeace(leftPair.Key, rightPair.Key);
					//leftPair.Value.AutoPeace(rightPair.Value);
				}
			}
		}

		private void SetupPirateRelations()
		{
			foreach (KeyValuePair<long, IMyFaction> faction in MyAPIGateway.Session.Factions.Factions)
			{
				foreach (KeyValuePair<long, IMyFaction> pirate in _pirateFactionDictionary)
				{
					if (faction.Key == pirate.Key) continue;
					WriteToLog("SetupPirateRelations", $"DeclareWar:\tfaction:\t{faction.Value.Tag}\tpirate:\t{pirate.Value.Tag}");
					MyAPIGateway.Session.Factions.DeclareWar(faction.Key, pirate.Key);
					//faction.Value.DeclareWar(pirate.Value);
				}
			}
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
			if (_lawfulFactionDictionary.ContainsKey(factionId)) _lawfulFactionDictionary.Remove(factionId);
			if (_enforcementFactionDictionary.ContainsKey(factionId)) _enforcementFactionDictionary.Remove(factionId);
			if (_pirateFactionDictionary.ContainsKey(factionId)) _pirateFactionDictionary.Remove(factionId);
			if (_playerFactionDictionary.ContainsKey(factionId)) _playerFactionDictionary.Remove(factionId);
			if (_playerPirateFactionDictionary.ContainsKey(factionId)) _playerPirateFactionDictionary.Remove(factionId);
			if (_npcFactionDictionary.ContainsKey(factionId)) _npcFactionDictionary.Remove(factionId);
			ClearRemovedFactionFromRelationships(factionId);
		}


		// Checks and balances, internal and external, mostly static

		private static bool CheckPiratePlayerOptIn(IMyFaction faction)
		{
			return Constants.PlayerFactionExclusionList.Any(x => faction.Description.StartsWith(x));
		}

		private static bool CheckEitherFactionForNpc(long leftFactionId, long rightFactionId)
		{
			return leftFactionId.GetFactionById().IsEveryoneNpc() || rightFactionId.GetFactionById().IsEveryoneNpc();
		}

		private static void AutoPeace(long fromFactionId, long toFactionId)
		{
			MyAPIGateway.Session.Factions.SendPeaceRequest(fromFactionId, toFactionId);
			MyAPIGateway.Session.Factions.AcceptPeace(toFactionId, fromFactionId);
			ClearPeace(fromFactionId, toFactionId);
		}

		private static void ClearPeace(long fromFactionId, long toFactionId)
		{ // Stops the flag from hanging out in the faction menu
			MyAPIGateway.Session.Factions.CancelPeaceRequest(toFactionId, fromFactionId);
			MyAPIGateway.Session.Factions.CancelPeaceRequest(fromFactionId, toFactionId);
		}

		private static void ProposePeace(long fromFactionId, long toFactionId)
		{
			MyAPIGateway.Session.Factions.SendPeaceRequest(fromFactionId, toFactionId);
		}

		private bool CheckTimedNegativeRelationshipState(long npcFaction, long playerFaction)
		{
			return TimedNegativeRelationships.Contains(new TimedRelationship(npcFaction.GetFactionById(), playerFaction.GetFactionById(), 0)) || TimedNegativeRelationships.Contains(new TimedRelationship(playerFaction.GetFactionById(), npcFaction.GetFactionById(), 0));
		}

		private bool CheckMentingRelationship(long fromFactionId, long toFactionId)
		{
			return MendingRelationships.Contains(new MendingRelation(fromFactionId, toFactionId));
		}


		// Methods that handle relationships

		private void DeclareFullNpcWar(long factionId)
		{
			foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
			{
				MyAPIGateway.Session.Factions.DeclareWar(lawfulFaction.Key, factionId);
			}
		}

		private void DeclareFullNpcPeace(long factionId)
		{
			foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
			{
				AutoPeace(lawfulFaction.Key, factionId);
			}
		}

		private void NewTimedNegativeRelationship(long npcFactionId, long playerfactionId)
		{
			TimedRelationship newTimedRelationship = new TimedRelationship(npcFactionId.GetFactionById(), playerfactionId.GetFactionById(), Helpers.Constants.FactionNegativeRelationshipCooldown);
			for (int i = TimedNegativeRelationships.Count - 1; i > 0; i--)
			{
				if (TimedNegativeRelationships[i].Equals(newTimedRelationship))
				{
					TimedNegativeRelationships[i] = newTimedRelationship;
					return;
				}
				TimedNegativeRelationships.Add(newTimedRelationship);
			}
			OnEnableTimer();
		}

		private void NewMendingRelationship(long npcFactionId, long playerFactionId)
		{
			MendingRelation newMendingRelation = new MendingRelation(npcFactionId, playerFactionId);
			for (int i = MendingRelationships.Count - 1; i > 0; i--)
			{
				if (MendingRelationships[i].Equals(newMendingRelation))
				{
					MendingRelationships[i] = newMendingRelation;
					return;
				}
				ProposePeace(playerFactionId, npcFactionId);
				MendingRelationships.Add(newMendingRelation);
			}
			OnEnableTimer();
		}

		private void RemoveMendingRelationship(long npcFactionId, long playerFactionId)
		{
			MendingRelation newMendingRelation = new MendingRelation(npcFactionId, playerFactionId);
			for (int i = MendingRelationships.Count - 1; i > 0; i--)
			{
				if (MendingRelationships[i].Equals(newMendingRelation))
				MendingRelationships.RemoveAtFast(i);
				ClearPeace(playerFactionId, npcFactionId);
			}
			CheckCounts();
		}
		
		private void HandleFormerPlayerPirate(long playerFactionId)
		{
			foreach (KeyValuePair<long, IMyFaction> npcFaction in _lawfulFactionDictionary)
			{
				NewTimedNegativeRelationship(npcFaction.Key, playerFactionId);
			}
		}
		
		private void AssessNegativeRelationships()
		{
			for (int i = TimedNegativeRelationships.Count - 1; i > 0; i--)
			{
				if ((TimedNegativeRelationships[i].CooldownTime -= Helpers.Constants.FactionNegativeRelationshipCooldown) > 0) continue;
				NewMendingRelationship(TimedNegativeRelationships[i].NpcFaction.FactionId, TimedNegativeRelationships[i].PlayerFaction.FactionId);
				TimedNegativeRelationships.RemoveAtFast(i);
			}
		}

		private void AssessMendingRelationships()
		{
			for (int i = MendingRelationships.Count - 1; i > 0; i--)
			{
				if (Helpers.Constants.Random.Next(0, 100) < 75) continue;
				MendingRelation relationToRemove = MendingRelationships[i];
				MendingRelationships.RemoveAtFast(i);
				AutoPeace(relationToRemove.NpcFaction, relationToRemove.PlayerFaction);
			}
		}

		private void ClearRemovedFactionFromRelationships(long factionId)
		{
			for (int i = MendingRelationships.Count - 1; i > 0; i--)
			{
				if (MendingRelationships[i].NpcFaction == factionId || MendingRelationships[i].PlayerFaction == factionId)
					MendingRelationships.RemoveAtFast(i);
			}
			for (int i = TimedNegativeRelationships.Count - 1; i > 0; i--)
			{
				if (TimedNegativeRelationships[i].NpcFaction.FactionId == factionId || TimedNegativeRelationships[i].PlayerFaction.FactionId == factionId)
					TimedNegativeRelationships.RemoveAtFast(i);
			}
			CheckCounts();
		}

		// External calls to manage internal relationships

		public void CheckNegativeRelationships()
		{
			CheckCounts();
			AssessNegativeRelationships();
		}

		public void CheckMendingRelationships()
		{
			CheckCounts();
			AssessMendingRelationships();
		}

		private void CheckCounts()
		{
			if (MendingRelationships.Count == 0 && TimedNegativeRelationships.Count == 0) OnDisableTimer();
		}

		private void DumpEverythingToTheLog()
		{
			const string callerName = "FactionsDump";
			foreach (TimedRelationship negativeRelationship in TimedNegativeRelationships)
				WriteToLog(callerName, $"negativeRelationship:\t{negativeRelationship}");
			foreach (MendingRelation mendingRelationship in MendingRelationships)
				WriteToLog(callerName, $"mendingRelationship:\t{mendingRelationship}");
			foreach (KeyValuePair<long, IMyFaction> faction in _enforcementFactionDictionary)
				WriteToLog(callerName, $"enforcementDictionary:\t{faction.Key}\t{faction.Value.Tag}");
			foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
				WriteToLog(callerName, $"lawfulDictionary:\t{faction.Key}\t{faction.Value.Tag}"); 
			foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
				WriteToLog(callerName, $"pirateDictionary:\t{faction.Key}\t{faction.Value.Tag}");
			foreach (KeyValuePair<long, IMyFaction> faction in _npcFactionDictionary)
				WriteToLog(callerName, $"npcDictionary:\t{faction.Key}\t{faction.Value.Tag}");
			foreach (KeyValuePair<long, IMyFaction> faction in _playerFactionDictionary)
				WriteToLog(callerName, $"playerDictionary:\t{faction.Key}\t{faction.Value.Tag}");
			foreach (KeyValuePair<long, IMyFaction> faction in _playerPirateFactionDictionary)
				WriteToLog(callerName, $"playerPirateDictionary:\t{faction.Key}\t{faction.Value.Tag}");
		}

		
		// Structs and other enums as necessary

		private struct MendingRelation
		{
			public readonly long NpcFaction;
			public readonly long PlayerFaction;

			/// <inheritdoc />
			public override string ToString()
			{
				return $"NpcFaction:\t{NpcFaction}\t{NpcFaction.GetFactionById()?.Tag}\tNpcFaction:\t{PlayerFaction}\t{PlayerFaction.GetFactionById()?.Tag}";
			}

			public MendingRelation(long npcFactionId, long playerFactionId)
			{
				NpcFaction = npcFactionId;
				PlayerFaction = playerFactionId;
			}
		}
	}
}
