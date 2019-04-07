using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Factions.Settings;
using Eem.Thraxus.Factions.Utilities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.Models
{
	public class RelationshipManager
	{
		private readonly Dictionary<long, IMyFaction> _playerFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _playerPirateFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _pirateFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _enforcementFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _lawfulFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _npcFactionDictionary;
		private readonly Dictionary<long, int> _newFactionDictionary;

		private List<TimedRelationship> TimedNegativeRelationships { get; }
		private List<MendingRelation> MendingRelationships { get; }

		public RelationshipManager()
		{
			FactionCore.WriteToLog("RelationshipManager", $"Constructing!");
			_playerFactionDictionary = new Dictionary<long, IMyFaction>();
			_pirateFactionDictionary = new Dictionary<long, IMyFaction>();
			_playerPirateFactionDictionary = new Dictionary<long, IMyFaction>();
			_enforcementFactionDictionary = new Dictionary<long, IMyFaction>();
			_lawfulFactionDictionary = new Dictionary<long, IMyFaction>();
			_npcFactionDictionary = new Dictionary<long, IMyFaction>();
			_newFactionDictionary = new Dictionary<long, int>();
			TimedNegativeRelationships = new List<TimedRelationship>();
			MendingRelationships = new List<MendingRelation>();
			MyAPIGateway.Session.Factions.FactionStateChanged += FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated += FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited += FactionEdited;
			SetupFactionRelations();
			FactionCore.WriteToLog("RelationshipManager", $"Constructed!");
		}

		public void Unload()
		{
			FactionCore.WriteToLog("RelationshipManager-Unload", $"Packing up shop...");
			MyAPIGateway.Session.Factions.FactionStateChanged -= FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated -= FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited -= FactionEdited;
			_playerFactionDictionary.Clear();
			_playerPirateFactionDictionary.Clear();
			_pirateFactionDictionary.Clear();
			_enforcementFactionDictionary.Clear();
			_lawfulFactionDictionary.Clear();
			_npcFactionDictionary.Clear();
			_newFactionDictionary.Clear();
			TimedNegativeRelationships.Clear();
			MendingRelationships.Clear();
			FactionCore.WriteToLog("RelationshipManager-Unload", $"Shop all packed up; I'm out!");
		}

		private void FactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
		{
			FactionCore.WriteToLog("FactionStateChanged", $"Action:\t{action.ToString()}\tfromFaction:\t{fromFactionId}\ttag:\t{fromFactionId.GetFactionById()?.Tag}\ttoFaction:\t{toFactionId}\ttag:\t{toFactionId.GetFactionById()?.Tag}\tplayerId:\t{playerId}\tsenderId:\t{senderId}");
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
					FactionCore.WriteToLog("FactionStateChanged", $"Case not found:\t{nameof(action)}\t{action.ToString()}");
					break;
			}
		}

		private void FactionCreated(long factionId)
		{
			FactionCore.WriteToLog("FactionCreated", $"factionId:\t{factionId}");
			FactionEditedOrCreated(factionId, true);
		}

		private void FactionEdited(long factionId)
		{
			FactionCore.WriteToLog("FactionEdited", $"factionId:\t{factionId}");
			FactionEditedOrCreated(factionId);
		}

		private void FactionRemoved(long factionId)
		{
			FactionCore.WriteToLog("FactionRemoved", $"factionId:\t{factionId}");
			ScrubDictionaries(factionId);
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
			_newFactionDictionary.Add(factionId, 0);  // I'm new man, just throw me a bone.
		}


		// So I remember how to do this later...
		//RequestDialog(toFaction.GetFactionById(), fromFaction.GetFactionById(), Dialogue.DialogType.PeaceRejected);

		private void PeaceRequestSent(long fromFactionId, long toFactionId)
		{   // So many reasons to clear peace...

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
			{   // This player was at war with an NPC by choice, so add them to the mending relationship category
				NewMendingRelationship(toFactionId, fromFactionId);
				return;
			}

			if (fromFactionId.GetFactionById().IsEveryoneNpc() && toFactionId.GetFactionById().IsEveryoneNpc())
			{   // Aww, look, the NPCs want to be friends!
				if (_pirateFactionDictionary.ContainsKey(fromFactionId) || _pirateFactionDictionary.ContainsKey(toFactionId))
				{   // No pirate friends!  NONE!  MY GOLD!!! 
					ClearPeace(fromFactionId, toFactionId);
					return;
				}
				MyAPIGateway.Session.Factions.AcceptPeace(toFactionId, fromFactionId);
				return;
			}

			if (fromFactionId.GetFactionById().IsEveryoneNpc() && !toFactionId.GetFactionById().IsEveryoneNpc())
			{   // The NPC wants to be friends with the player.  How cute.  
				MyAPIGateway.Session.Factions.AcceptPeace(toFactionId, fromFactionId);
				return;
			}

			// Condition not accounted for, just accept the request for now (get logs!)
			FactionCore.WriteToLog("PeaceRequestSent", $"Unknown peace condition detected, please review...\tfromFaction:\t{fromFactionId.GetFactionById().Tag}\ttoFaction:\t{toFactionId.GetFactionById().Tag}");
			DumpEverythingToTheLog();
			MyAPIGateway.Session.Factions.AcceptPeace(toFactionId, fromFactionId);

		}

		private void PeaceAccepted(long fromFactionId, long toFactionId)
		{   // Clearing those leftover flags
			ClearPeace(fromFactionId, toFactionId);
		}

		private void PeaceCancelled(long fromFactionId, long toFactionId)
		{   // The only time this matters is if a former player pirate declares war on a NPC, then declares peace, then revokes the peace declaration
			if (!CheckMentingRelationship(fromFactionId, toFactionId)) return;
			RemoveMendingRelationship(toFactionId, fromFactionId);
		}

		private void WarDeclared(long fromFactionId, long toFactionId)
		{   // Going to take the stance that if a war is declared by an NPC, it's a valid war
			if (fromFactionId.GetFactionById().IsEveryoneNpc())
				VetNewWar(fromFactionId, toFactionId);
		}


		// Dictionary methods

		private void SetupFactionRelations()
		{
			foreach (KeyValuePair<long, IMyFaction> factions in MyAPIGateway.Session.Factions.Factions)
			{
				try
				{
					if (factions.Value == null) continue;
					if (Constants.EnforcementFactionsTags.Contains(factions.Value.Tag))
					{
						FactionCore.WriteToLog("SetupFactionDictionaries", $"AddToEnforcementFactionDictionary:\t{factions.Key}\t{factions.Value.Tag}");
						AddToEnforcementFactionDictionary(factions.Key, factions.Value);
						AddToLawfulFactionDictionary(factions.Key, factions.Value);
						AddToNpcFactionDictionary(factions.Key, factions.Value);
						continue;
					}

					if (Constants.LawfulFactionsTags.Contains(factions.Value.Tag))
					{
						FactionCore.WriteToLog("SetupFactionDictionaries", $"AddToLawfulFactionDictionary:\t{factions.Key}\t{factions.Value.Tag}");
						AddToLawfulFactionDictionary(factions.Key, factions.Value);
						AddToNpcFactionDictionary(factions.Key, factions.Value);
						continue;
					}

					if (factions.Value.IsEveryoneNpc())
					{ // If it's not an Enforcement or Lawful faction, it's a pirate.
						FactionCore.WriteToLog("SetupFactionDictionaries", $"AddToPirateFactionDictionary:\t{factions.Key}\t{factions.Value.Tag}");
						AddToPirateFactionDictionary(factions.Key, factions.Value);
						AddToNpcFactionDictionary(factions.Key, factions.Value);
						continue;
					}

					if (CheckPiratePlayerOptIn(factions.Value))
					{
						FactionCore.WriteToLog("SetupFactionDictionaries", $"PlayerFactionExclusionList.Add:\t{factions.Key}\t{factions.Value.Tag}");
						AddToPlayerPirateFactionDictionary(factions.Key, factions.Value);
						continue;
					}

					FactionCore.WriteToLog("SetupFactionDictionaries", $"PlayerFaction.Add:\t{factions.Key}\t{factions.Value.Tag}");
					AddToPlayerFactionDictionary(factions.Key, factions.Value);
				}
				catch (Exception e)
				{
					FactionCore.WriteToLog("SetupFactionDictionaries", $"Exception caught - e: {e}\tfaction.Key:\t{factions.Key}\tfaction.Value: {factions.Value}\tfaction.Tag:\t{factions.Value?.Tag}");
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
					AutoPeace(playerFaction.Key, lawfulFaction.Key);
				}
			}
		}

		private void SetupNpcRelations()
		{
			foreach (KeyValuePair<long, IMyFaction> leftPair in _lawfulFactionDictionary)
			{
				foreach (KeyValuePair<long, IMyFaction> rightPair in _lawfulFactionDictionary)
				{
					if (leftPair.Key == rightPair.Key || !MyAPIGateway.Session.Factions.AreFactionsEnemies(leftPair.Key, rightPair.Key)) continue;
					AutoPeace(leftPair.Key, rightPair.Key);
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
					MyAPIGateway.Session.Factions.DeclareWar(faction.Key, pirate.Key);
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
			if (_newFactionDictionary.ContainsKey(factionId)) _newFactionDictionary.Remove(factionId);
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
			MyAPIGateway.Utilities.InvokeOnGameThread(() => MyAPIGateway.Session.Factions.SendPeaceRequest(fromFactionId, toFactionId));
			MyAPIGateway.Utilities.InvokeOnGameThread(() => MyAPIGateway.Session.Factions.AcceptPeace(toFactionId, fromFactionId));
			ClearPeace(fromFactionId, toFactionId);
		}

		private static void ClearPeace(long fromFactionId, long toFactionId)
		{   // Stops the flag from hanging out in the faction menu
			MyAPIGateway.Utilities.InvokeOnGameThread(() => MyAPIGateway.Session.Factions.CancelPeaceRequest(toFactionId, fromFactionId));
			MyAPIGateway.Utilities.InvokeOnGameThread(() => MyAPIGateway.Session.Factions.CancelPeaceRequest(fromFactionId, toFactionId));
		}

		private static void War(long npcFaction, long playerFaction)
		{   // Vanilla war declaration, ensures invoking on main thread
			MyAPIGateway.Utilities.InvokeOnGameThread(() => MyAPIGateway.Session.Factions.DeclareWar(npcFaction, playerFaction));
		}

		private bool CheckTimedNegativeRelationshipState(long npcFaction, long playerFaction)
		{
			return TimedNegativeRelationships.IndexOf(new TimedRelationship(npcFaction.GetFactionById(), playerFaction.GetFactionById(), 0)) > -1 || TimedNegativeRelationships.IndexOf(new TimedRelationship(playerFaction.GetFactionById(), npcFaction.GetFactionById(), 0)) > -1;
		}

		private bool CheckMentingRelationship(long fromFactionId, long toFactionId)
		{
			return MendingRelationships.Contains(new MendingRelation(fromFactionId, toFactionId));
		}


		// Methods that handle relationships

		private void DeclareFullNpcWar(long factionId)
		{
			for (int i = _lawfulFactionDictionary.Count - 1; i >= 0; i--)
			{
				War(_lawfulFactionDictionary[i].FactionId, factionId);
			}
		}

		private void DeclareEnforcementWar(long factionId)
		{
			FactionCore.WriteToLog("DeclareEnforcementWar", $"New enforcement war against {factionId}");
			try
			{
				foreach (KeyValuePair<long, IMyFaction> enforcementFaction in _enforcementFactionDictionary)
				{
					TimedRelationship newTimedRelationship = new TimedRelationship(enforcementFaction.Value, factionId.GetFactionById(), Helpers.Constants.FactionNegativeRelationshipCooldown);
					foreach (TimedRelationship timedNegativeRelationship in TimedNegativeRelationships)
					{
						if (timedNegativeRelationship.Equals(newTimedRelationship))
							timedNegativeRelationship.CooldownTime = Helpers.Constants.FactionNegativeRelationshipCooldown;
						else War(enforcementFaction.Key, factionId);
					}
				}
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("DeclareEnforcementWar", $"Exception!\t{e}", true);
				DumpEverythingToTheLog();
			}
		}

		private void DeclareFullNpcPeace(long factionId)
		{
			try
			{
				foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
				{
					AutoPeace(lawfulFaction.Key, factionId);
				}
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("DeclareFullNpcPeace", $"Exception!\t{e}", true);
				DumpEverythingToTheLog();
			}
		}

		public void ExternalWarDeclaration(long npcFactionId, long playerFactionId)
		{   // Used by BotBase to declare war until I have the time to redo bots/ai
			MyAPIGateway.Parallel.Start(delegate
			{
				try
				{
					TimedRelationship newTimedRelationship = new TimedRelationship(npcFactionId.GetFactionById(), playerFactionId.GetFactionById(), Helpers.Constants.FactionNegativeRelationshipCooldown);
					for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
					{
						if (!TimedNegativeRelationships[i].Equals(newTimedRelationship)) continue;
						TimedNegativeRelationships[i].CooldownTime = Helpers.Constants.FactionNegativeRelationshipCooldown;
						DeclareEnforcementWar(playerFactionId);
						return;
					}
					War(npcFactionId, playerFactionId);
				}
				catch (Exception e)
				{
					FactionCore.WriteToLog("ExternalWarDeclaration", $"Exception!\t{e}", true);
					DumpEverythingToTheLog();
				}
			});
		}

		private void VetNewWar(long npcFactionId, long playerFactionId)
		{
			try
			{
				if (_newFactionDictionary.ContainsKey(playerFactionId))
				{
					if (_lawfulFactionDictionary.ContainsKey(npcFactionId)) _newFactionDictionary[playerFactionId]++;
					if (_newFactionDictionary[playerFactionId] != _lawfulFactionDictionary.Count) return;
					DeclareFullNpcPeace(playerFactionId);
					_newFactionDictionary.Remove(playerFactionId);
					return;
				}
				NewTimedNegativeRelationship(npcFactionId, playerFactionId);
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("VetNewWar", $"Exception!\t{e}", true);
				DumpEverythingToTheLog();
			}
		}

		private void NewTimedNegativeRelationship(long npcFactionId, long playerFactionId)
		{
			AddToTimedNegativeRelationships(new TimedRelationship(npcFactionId.GetFactionById(), playerFactionId.GetFactionById(), Helpers.Constants.FactionNegativeRelationshipCooldown));
			DeclareEnforcementWar(playerFactionId);
			FactionTimer(MyUpdateOrder.BeforeSimulation);
		}

		private void NewMendingRelationship(long npcFactionId, long playerFactionId)
		{
			try
			{
				MendingRelation newMendingRelation = new MendingRelation(npcFactionId, playerFactionId);
				for (int i = MendingRelationships.Count - 1; i >= 0; i--)
				{
					if (MendingRelationships[i].Equals(newMendingRelation))
						return;
				}
				AddToMendingRelationships(newMendingRelation);
				FactionTimer(MyUpdateOrder.BeforeSimulation);
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("NewMendingRelationship", $"Exception!\t{e}", true);
				DumpEverythingToTheLog();
			}
		}

		private void RemoveMendingRelationship(long npcFactionId, long playerFactionId)
		{
			try
			{
				MendingRelation newMendingRelation = new MendingRelation(npcFactionId, playerFactionId);
				for (int i = MendingRelationships.Count - 1; i >= 0; i--)
				{
					if (MendingRelationships[i].Equals(newMendingRelation))
						MendingRelationships.RemoveAtFast(i);
					ClearPeace(playerFactionId, npcFactionId);
				}
				CheckCounts();
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("RemoveMendingRelationship", $"Exception!\t{e}", true);
				DumpEverythingToTheLog();
			}
		}

		private void AddToTimedNegativeRelationships(TimedRelationship newTimedRelationship)
		{
			FactionCore.WriteToLog("AddToTimedNegativeRelationships", $"newTimedRelationship:\t{newTimedRelationship}");
			TimedNegativeRelationships.Add(newTimedRelationship);
		}

		private void AddToMendingRelationships(MendingRelation newMendingRelation)
		{
			FactionCore.WriteToLog("AddToMendingRelationships", $"newTimedRelationship:\t{newMendingRelation}");
			MendingRelationships.Add(newMendingRelation);
		}

		private void HandleFormerPlayerPirate(long playerFactionId)
		{
			for (int i = _lawfulFactionDictionary.Count - 1; i >= 0; i--)
			{
				NewTimedNegativeRelationship(_lawfulFactionDictionary[i].FactionId, playerFactionId);
			}
		}

		private void AssessNegativeRelationships()
		{
			try
			{
				FactionCore.WriteToLog("AssessNegativeRelationships", $"TimedNegativeRelationships.Count:\t{TimedNegativeRelationships.Count}");
				DumpTimedNegativeFactionRelationships();
				for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
				{
					if ((TimedNegativeRelationships[i].CooldownTime -= Helpers.Constants.FactionNegativeRelationshipAssessment) > 0) continue;
					NewMendingRelationship(TimedNegativeRelationships[i].NpcFaction.FactionId, TimedNegativeRelationships[i].PlayerFaction.FactionId);
					TimedNegativeRelationships.RemoveAtFast(i);
				}
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("AssessNegativeRelationships", $"Exception!\t{e}", true);
				DumpEverythingToTheLog();
			}
		}

		private void AssessMendingRelationships()
		{
			try
			{
				FactionCore.WriteToLog("AssessMendingRelationships", $"MendingRelationships.Count:\t{TimedNegativeRelationships.Count}");
				DumpMendingRelationshipsRelationships();
				for (int i = MendingRelationships.Count - 1; i >= 0; i--)
				{
					if (Helpers.Constants.Random.Next(0, 100) < 75) continue;
					MendingRelation relationToRemove = MendingRelationships[i];
					MendingRelationships.RemoveAtFast(i);
					AutoPeace(relationToRemove.NpcFaction, relationToRemove.PlayerFaction);
				}
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("AssessMendingRelationships", $"Exception!\t{e}", true);
				DumpEverythingToTheLog();
			}
		}

		private void ClearRemovedFactionFromRelationships(long factionId)
		{
			try
			{
				for (int i = MendingRelationships.Count - 1; i >= 0; i--)
				{
					if (MendingRelationships[i].NpcFaction == factionId || MendingRelationships[i].PlayerFaction == factionId)
						MendingRelationships.RemoveAtFast(i);
				}
				for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
				{
					if (TimedNegativeRelationships[i].NpcFaction.FactionId == factionId || TimedNegativeRelationships[i].PlayerFaction.FactionId == factionId)
						TimedNegativeRelationships.RemoveAtFast(i);
				}
				CheckCounts();
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("ClearRemovedFactionFromRelationships", $"Exception!\t{e}", true);
				DumpEverythingToTheLog();
			}
		}

		// External calls to manage internal relationships

		public void CheckNegativeRelationships()
		{
			AssessNegativeRelationships();
			CheckCounts();
		}

		public void CheckMendingRelationships()
		{
			AssessMendingRelationships();
			CheckCounts();
		}

		private void CheckCounts()
		{
			if (MendingRelationships.Count == 0 && TimedNegativeRelationships.Count == 0) FactionTimer(MyUpdateOrder.NoUpdate);
			FactionCore.WriteToLog("CheckCounts", $"MendingRelationships:\t{MendingRelationships.Count}\tTimedNegativeRelationship:\t{TimedNegativeRelationships.Count}");
		}

		private static void FactionTimer(MyUpdateOrder updateOrder)
		{
			if (FactionCore.FactionCoreStaticInstance.UpdateOrder != updateOrder)
				MyAPIGateway.Utilities.InvokeOnGameThread(() => FactionCore.FactionCoreStaticInstance.SetUpdateOrder(updateOrder));
			MyAPIGateway.Utilities.InvokeOnGameThread(() => FactionCore.WriteToLog("FactionTimer", $"SetUpdateOrder:\t{updateOrder}\tActual:\t{FactionCore.FactionCoreStaticInstance.UpdateOrder}"));
		}


		//Debug Outputs

		private void DumpEverythingToTheLog()
		{
			if (!Helpers.Constants.DebugMode) return;
			try
			{
				const string callerName = "FactionsDump";
				List<TimedRelationship> tempTimedRelationship = TimedNegativeRelationships;
				foreach (TimedRelationship negativeRelationship in tempTimedRelationship)
					FactionCore.WriteToLog(callerName, $"negativeRelationship:\t{negativeRelationship}");
				List<MendingRelation> tempMendingRelations = MendingRelationships;
				foreach (MendingRelation mendingRelationship in tempMendingRelations)
					FactionCore.WriteToLog(callerName, $"mendingRelationship:\t{mendingRelationship}");
				Dictionary<long, IMyFaction> tempFactionDictionary = _enforcementFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					FactionCore.WriteToLog(callerName, $"enforcementDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				tempFactionDictionary = _lawfulFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					FactionCore.WriteToLog(callerName, $"lawfulDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				tempFactionDictionary = _pirateFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					FactionCore.WriteToLog(callerName, $"pirateDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				tempFactionDictionary = _npcFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					FactionCore.WriteToLog(callerName, $"npcDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				tempFactionDictionary = _playerFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					FactionCore.WriteToLog(callerName, $"playerDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				tempFactionDictionary = _playerPirateFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					FactionCore.WriteToLog(callerName, $"playerPirateDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				Dictionary<long, int> tempNewFactionDictioanry = _newFactionDictionary;
				foreach (KeyValuePair<long, int> faction in tempNewFactionDictioanry)
					FactionCore.WriteToLog(callerName, $"newFactionDictionary:\t{faction}\t{faction.Key.GetFactionById()?.Tag}");

				//foreach (TimedRelationship negativeRelationship in TimedNegativeRelationships)
				//	FactionCore.WriteToLog(callerName, $"negativeRelationship:\t{negativeRelationship}");
				//foreach (MendingRelation mendingRelationship in MendingRelationships)
				//	FactionCore.WriteToLog(callerName, $"mendingRelationship:\t{mendingRelationship}");
				//foreach (KeyValuePair<long, IMyFaction> faction in _enforcementFactionDictionary)
				//	FactionCore.WriteToLog(callerName, $"enforcementDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				//foreach (KeyValuePair<long, IMyFaction> faction in _lawfulFactionDictionary)
				//	FactionCore.WriteToLog(callerName, $"lawfulDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				//foreach (KeyValuePair<long, IMyFaction> faction in _pirateFactionDictionary)
				//	FactionCore.WriteToLog(callerName, $"pirateDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				//foreach (KeyValuePair<long, IMyFaction> faction in _npcFactionDictionary)
				//	FactionCore.WriteToLog(callerName, $"npcDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				//foreach (KeyValuePair<long, IMyFaction> faction in _playerFactionDictionary)
				//	FactionCore.WriteToLog(callerName, $"playerDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				//foreach (KeyValuePair<long, IMyFaction> faction in _playerPirateFactionDictionary)
				//	FactionCore.WriteToLog(callerName, $"playerPirateDictionary:\t{faction.Key}\t{faction.Value.Tag}");
				//foreach (KeyValuePair<long, int> faction in _newFactionList)
				//	FactionCore.WriteToLog(callerName, $"newFactionDictionary:\t{faction}\t{faction.Key.GetFactionById()?.Tag}");
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("DumpEverythingToTheLog", $"Exception!\t{e}");
			}
		}

		private void DumpTimedNegativeFactionRelationships()
		{
			if (!Helpers.Constants.DebugMode) return;
			try
			{
				const string callerName = "DumpTimedNegativeFactionRelationships";
				List<TimedRelationship> tempTimedRelationship = TimedNegativeRelationships;
				foreach (TimedRelationship negativeRelationship in tempTimedRelationship)
					FactionCore.WriteToLog(callerName, $"negativeRelationship:\t{negativeRelationship}");


				//foreach (TimedRelationship negativeRelationship in TimedNegativeRelationships)
				//	FactionCore.WriteToLog(callerName, $"negativeRelationship:\t{negativeRelationship}");
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("DumpTimedNegativeFactionRelationships", $"Exception!\t{e}");
			}
		}

		private void DumpMendingRelationshipsRelationships()
		{
			if (!Helpers.Constants.DebugMode) return;
			try
			{
				const string callerName = "DumpMendingRelationshipsRelationships";
				List<MendingRelation> tempMendingRelations = MendingRelationships;
				foreach (MendingRelation mendingRelationship in tempMendingRelations)
					FactionCore.WriteToLog(callerName, $"mendingRelationship:\t{mendingRelationship}");


				//foreach (MendingRelation mendingRelationship in MendingRelationships)
				//	FactionCore.WriteToLog(callerName, $"mendingRelationship:\t{mendingRelationship}");
			}
			catch (Exception e)
			{
				FactionCore.WriteToLog("DumpMendingRelationshipsRelationships", $"Exception!\t{e}");
			}
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
