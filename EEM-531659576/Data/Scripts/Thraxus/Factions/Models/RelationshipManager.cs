using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Eem.Thraxus.Common.Utilities.Tools.Networking;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.Models
{
	public class RelationshipManager : LogBaseEvent
	{
		private readonly Dictionary<long, IMyFaction> _playerFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _playerPirateFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _pirateFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _enforcementFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _lawfulFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _npcFactionDictionary;
		private readonly Dictionary<long, IMyFaction> _nonEemNpcFactionDictionary;
		private readonly Dictionary<long, int> _newFactionDictionary;

		private static readonly Queue<PendingRelation> WarQueue = new Queue<PendingRelation>();

		private List<TimedRelationship> TimedNegativeRelationships { get; }
		private List<PendingRelation> MendingRelationships { get; }

		private bool _setupCompelete;

		private readonly Dialogue _dialogue;

		public RelationshipManager()
		{
			WriteToLog("RelationshipManager", $"Constructing!", LogType.General);
			_dialogue = new Dialogue();
			_playerFactionDictionary = new Dictionary<long, IMyFaction>();
			_pirateFactionDictionary = new Dictionary<long, IMyFaction>();
			_playerPirateFactionDictionary = new Dictionary<long, IMyFaction>();
			_enforcementFactionDictionary = new Dictionary<long, IMyFaction>();
			_lawfulFactionDictionary = new Dictionary<long, IMyFaction>();
			_npcFactionDictionary = new Dictionary<long, IMyFaction>();
			_newFactionDictionary = new Dictionary<long, int>();
			_nonEemNpcFactionDictionary = new Dictionary<long, IMyFaction>();
			TimedNegativeRelationships = new List<TimedRelationship>();
			MendingRelationships = new List<PendingRelation>();
			MyAPIGateway.Session.Factions.FactionStateChanged += FactionStateChanged;
			MyAPIGateway.Session.Factions.FactionCreated += FactionCreated;
			MyAPIGateway.Session.Factions.FactionEdited += FactionEdited;
			MyAPIGateway.Session.Factions.FactionAutoAcceptChanged += MonitorAutoAccept;
			SetupFactionRelations();
			WriteToLog("RelationshipManager", $"Constructed!", LogType.General);
		}

		public void Close()
		{
			WriteToLog("RelationshipManager-Unload", $"Packing up shop...", LogType.General);
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
			_newFactionDictionary.Clear();
			_nonEemNpcFactionDictionary.Clear();
			TimedNegativeRelationships.Clear();
			MendingRelationships.Clear();
			_dialogue.Unload();
			WriteToLog("RelationshipManager-Unload", $"Shop all packed up", LogType.General);
		}

		private void FactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
		{
			WriteToLog("FactionStateChanged",
				$"Action:\t{action.ToString()}\tfromFaction:\t{fromFactionId}\ttag:\t{fromFactionId.GetFactionById()?.Tag}\ttoFaction:\t{toFactionId}\ttag:\t{toFactionId.GetFactionById()?.Tag}\tplayerId:\t{playerId}\tsenderId:\t{senderId}",
				LogType.Debug);
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
			if (playerFaction == null || playerFaction.IsEveryoneNpc()) return; // I'm not a player faction, or I don't exist.  Peace out, suckas!
			if (CheckPiratePlayerOptIn(playerFaction) && _playerPirateFactionDictionary.ContainsKey(factionId)) return; // I'm a player pirate, and you know it.  Laterz!
			if (CheckPiratePlayerOptIn(playerFaction) && !_playerPirateFactionDictionary.ContainsKey(factionId)) // I'm a player pirate, but this is news to you...
			{
				_playerPirateFactionDictionary.Add(factionId, playerFaction);
				DeclarePermanentFullNpcWar(factionId);
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
				NewTimedNegativeRelationship(toFactionId, fromFactionId);
				return;
			}

			if (fromFactionId.GetFactionById().IsEveryoneNpc() && toFactionId.GetFactionById().IsEveryoneNpc())
			{   // Aww, look, the NPCs want to be friends!
				if (_pirateFactionDictionary.ContainsKey(fromFactionId) || _pirateFactionDictionary.ContainsKey(toFactionId))
				{   // No pirate friends!  NONE!  MY GOLD!!! 
					ClearPeace(fromFactionId, toFactionId);
					return;
				}
				AcceptPeace(toFactionId, fromFactionId);
				return;
			}

			if (fromFactionId.GetFactionById().IsEveryoneNpc() && !toFactionId.GetFactionById().IsEveryoneNpc())
			{   // The NPC wants to be friends with the player.  How cute.  
				AcceptPeace(toFactionId, fromFactionId);
				return;
			}

			// Condition not accounted for, just accept the request for now (get logs!)
			WriteToLog("PeaceRequestSent", $"Unknown peace condition detected, please review...\tfromFaction:\t{fromFactionId.GetFactionById().Tag}\ttoFaction:\t{toFactionId.GetFactionById().Tag}", LogType.General);
			DumpEverythingToTheLog();
			MyAPIGateway.Session.Factions.AcceptPeace(toFactionId, fromFactionId);

		}

		private void AcceptPeace(long fromFactionId, long toFactionId)
		{
			if (_newFactionDictionary.ContainsKey(fromFactionId))
			{
				if (_newFactionDictionary[fromFactionId] > 1)
					_newFactionDictionary[fromFactionId]--;
				else
				{
					RequestNewFactionDialog(fromFactionId);
					_newFactionDictionary.Remove(fromFactionId);
				}
			}
			MyAPIGateway.Session.Factions.AcceptPeace(fromFactionId, toFactionId);
		}

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

		private void SendFactionMessageToAllFactionMembers(string message, string messageSender, DictionaryReader<long, MyFactionMember> target, string color = MyFontEnum.Red)
		{
			try
			{
				foreach (KeyValuePair<long, MyFactionMember> factionMember in target)
				{
					if (IsPlayerOnline(factionMember.Key))
						MyAPIGateway.Utilities.InvokeOnGameThread(() =>
							Messaging.SendMessageToPlayer($"{message}", messageSender, factionMember.Key, color));
				}
			}
			catch (Exception e)
			{
				WriteToLog("SendFactionMessageToAllFactionMembers", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private static bool IsPlayerOnline(long player)
		{
			List<IMyPlayer> players = new List<IMyPlayer>();
			MyAPIGateway.Multiplayer.Players.GetPlayers(players);
			return players.Any(x => x.IdentityId == player);
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

		// Dictionary methods

		private void SetupFactionRelations()
		{
			foreach (KeyValuePair<long, IMyFaction> faction in MyAPIGateway.Session.Factions.Factions)
			{
				try
				{
					if (faction.Value == null) continue;
					if (Settings.EnforcementFactionsTags.Contains(faction.Value.Tag))
					{
						WriteToLog("SetupFactionDictionaries", $"AddToEnforcementFactionDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.General);
						AddToEnforcementFactionDictionary(faction.Key, faction.Value);
						AddToLawfulFactionDictionary(faction.Key, faction.Value);
						AddToNpcFactionDictionary(faction.Key, faction.Value);
						continue;
					}

					if (Settings.LawfulFactionsTags.Contains(faction.Value.Tag))
					{
						WriteToLog("SetupFactionDictionaries", $"AddToLawfulFactionDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.General);
						AddToLawfulFactionDictionary(faction.Key, faction.Value);
						AddToNpcFactionDictionary(faction.Key, faction.Value);
						continue;
					}

					if (faction.Value.IsEveryoneNpc())
					{ // If it's not an Enforcement or Lawful faction, it's a pirate.
						WriteToLog("SetupFactionDictionaries", $"AddToPirateFactionDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.General);
						AddToPirateFactionDictionary(faction.Key, faction.Value);
						AddToNpcFactionDictionary(faction.Key, faction.Value);
						continue;
					}

					if (CheckPiratePlayerOptIn(faction.Value))
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

			SetupPlayerRelations();
			SetupNpcRelations();
			SetupPirateRelations();
			SetupAutoRelations();
			SetupFactionDeletionProtection();
			DumpEverythingToTheLog();
			_setupCompelete = true;
		}

		private void SetupFactionDeletionProtection()
		{
			foreach (KeyValuePair<long, IMyFaction> npcFaction in _npcFactionDictionary)
				AddFactionMember(npcFaction.Value);
		}

		private static void AddFactionMember(IMyFaction npcFaction)
		{
			if (!npcFaction.IsEveryoneNpc()) return;
			if (npcFaction.Members.Count < 2)
				MyAPIGateway.Session.Factions.AddNewNPCToFaction(npcFaction.FactionId);
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
					DeclareWar(faction.Key, pirate.Key);
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

		private void MonitorAutoAccept(long factionId, bool acceptPeace, bool acceptMember)
		{
			if (!_setupCompelete) return;
			if (!acceptPeace && !acceptMember) return;
			if (!_npcFactionDictionary.ContainsKey(factionId)) return;
			SetupAutoRelations();
			WriteToLog("MonitorAutoAccept", $"NPC Faction bypass detected, resetting relationship controls.", LogType.General);
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
				_newFactionDictionary.Remove(factionId);
				ClearRemovedFactionFromRelationships(factionId);
			}
			catch (Exception e)
			{
				WriteToLog("ScrubDictionaries", $"Exception!\t{e}", LogType.Exception);
			}
		}


		// Checks and balances, internal and external, mostly static

		private static bool CheckPiratePlayerOptIn(IMyFaction faction)
		{
			return Settings.PlayerFactionExclusionList.Any(x => faction.Description.StartsWith(x));
		}

		private static bool CheckEitherFactionForNpc(long leftFactionId, long rightFactionId)
		{
			return leftFactionId.GetFactionById().IsEveryoneNpc() || rightFactionId.GetFactionById().IsEveryoneNpc();
		}

		private static void AutoPeace(long fromFactionId, long toFactionId)
		{
			MyAPIGateway.Utilities.InvokeOnGameThread(() =>
			{
				MyAPIGateway.Session.Factions.SendPeaceRequest(fromFactionId, toFactionId);
				MyAPIGateway.Session.Factions.AcceptPeace(toFactionId, fromFactionId);
				ClearPeace(fromFactionId, toFactionId);
			});
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
		{	// Vanilla war declaration, ensures invoking on main thread
			MyAPIGateway.Utilities.InvokeOnGameThread(() => MyAPIGateway.Session.Factions.DeclareWar(npcFaction, playerFaction));
		}
		
		private bool CheckTimedNegativeRelationshipState(long npcFaction, long playerFaction)
		{
			return TimedNegativeRelationships.IndexOf(new TimedRelationship(npcFaction.GetFactionById(), playerFaction.GetFactionById(), 0)) > -1 || TimedNegativeRelationships.IndexOf(new TimedRelationship(playerFaction.GetFactionById(), npcFaction.GetFactionById(), 0)) > -1;
		}

		private bool CheckMentingRelationship(long fromFactionId, long toFactionId)
		{
			return MendingRelationships.Contains(new PendingRelation(fromFactionId, toFactionId));
		}


		// Methods that handle relationships
		
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
		{	// Used for when a player declares war on a NPC
			DeclareWar(npcFaction, playerFaction);
		}

		private void DeclarePermanentFullNpcWar(long playerFaction)
		{	// Used to declare war against a player pirate
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

		private void HandleFormerPlayerPirate(long playerFactionId)
		{
			try
			{
				foreach (KeyValuePair<long, IMyFaction> lawfulFaction in _lawfulFactionDictionary)
					NewTimedNegativeRelationship(lawfulFaction.Key, playerFactionId);
				RequestNewPirateDialog(playerFactionId);
			}
			catch (Exception e)
			{
				WriteToLog("HandleFormerPlayerPirate", $"Exception!\t{e}", LogType.Exception);
			}
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
						TimedRelationship newTimedRelationship = new TimedRelationship(tmpRelation.NpcFaction.GetFactionById(), tmpRelation.PlayerFaction.GetFactionById(), Settings.FactionNegativeRelationshipCooldown);
					for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
					{
						if (!TimedNegativeRelationships[i].Equals(newTimedRelationship)) continue;
						TimedNegativeRelationships[i].CooldownTime = Settings.FactionNegativeRelationshipCooldown;
						found = true;
					}
					if (!found) War(tmpRelation.NpcFaction, tmpRelation.PlayerFaction);
				
					foreach (KeyValuePair<long, IMyFaction> enforcementFaction in _enforcementFactionDictionary)
					{
						found = false;
						newTimedRelationship = new TimedRelationship(enforcementFaction.Value, tmpRelation.PlayerFaction.GetFactionById(), Settings.FactionNegativeRelationshipCooldown);
						for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
						{
							if (!TimedNegativeRelationships[i].Equals(newTimedRelationship)) continue;
							TimedNegativeRelationships[i].CooldownTime = Settings.FactionNegativeRelationshipCooldown;
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
		
		public void WarDeclaration(long npcFactionId, long playerFactionId)
		{   // Used by BotBase to declare war until I have the time to redo bots/ai
			// May revisit parallel threading for this in the future, for now, it's fine as is
			//MyAPIGateway.Parallel.Start(delegate
			//{ 
				WarQueue.Enqueue(new PendingRelation(npcFactionId, playerFactionId));
				ProcessWarQueue();
			//});
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
					return;
				}

				if (_playerPirateFactionDictionary.ContainsKey(playerFactionId)) return;
				WarDeclaration(npcFactionId, playerFactionId);
			}
			catch (Exception e)
			{
				WriteToLog("VetNewWar", $"Exception!\t{e}", LogType.Exception);
			}
		}

		// Relationship Managers

		private void NewMendingRelationship(long npcFactionId, long playerFactionId)
		{
			try
			{
				PendingRelation newMendingRelation = new PendingRelation(npcFactionId, playerFactionId);
				for (int i = MendingRelationships.Count - 1; i >= 0; i--)
				{
					if (MendingRelationships[i].Equals(newMendingRelation))
						return;
				}
				RequestDialog(npcFactionId.GetFactionById(), playerFactionId.GetFactionById(), Dialogue.DialogType.PeaceConsidered);
				AddToMendingRelationships(newMendingRelation);
				FactionTimer(MyUpdateOrder.BeforeSimulation);
			}
			catch (Exception e)
			{
				WriteToLog("NewMendingRelationship", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private void RemoveMendingRelationship(long npcFactionId, long playerFactionId)
		{
			try
			{
				PendingRelation newMendingRelation = new PendingRelation(npcFactionId, playerFactionId);
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
				WriteToLog("RemoveMendingRelationship", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private void NewTimedNegativeRelationship(long npcFactionId, long playerFactionId)
		{
			int cooldown = Settings.FactionNegativeRelationshipCooldown + Settings.Random.Next(Settings.TicksPerSecond * 30, Settings.TicksPerMinute * 2);
			AddToTimedNegativeRelationships(new TimedRelationship(npcFactionId.GetFactionById(), playerFactionId.GetFactionById(), cooldown));
		}

		private void AddToTimedNegativeRelationships(TimedRelationship newTimedRelationship)
		{
			WriteToLog("AddToTimedNegativeRelationships", $"newTimedRelationship:\t{newTimedRelationship}", LogType.Debug);
			TimedNegativeRelationships.Add(newTimedRelationship);
			RemoveMendingRelationship(newTimedRelationship.NpcFaction.FactionId, newTimedRelationship.PlayerFaction.FactionId);
			DumpEverythingToTheLog();
			FactionTimer(MyUpdateOrder.BeforeSimulation);
		}

		private void AddToMendingRelationships(PendingRelation newMendingRelation)
		{
			WriteToLog("AddToMendingRelationships", $"newTimedRelationship:\t{newMendingRelation}", LogType.Debug);
			MendingRelationships.Add(newMendingRelation);
		}
		
		private void AssessNegativeRelationships()
		{
			try
			{
				WriteToLog("AssessNegativeRelationships", $"TimedNegativeRelationships.Count:\t{TimedNegativeRelationships.Count}", LogType.Debug);
				DumpTimedNegativeFactionRelationships();
				for (int i = TimedNegativeRelationships.Count - 1; i >= 0; i--)
				{
					if ((TimedNegativeRelationships[i].CooldownTime -= Settings.FactionNegativeRelationshipAssessment) > 0) continue;
					NewMendingRelationship(TimedNegativeRelationships[i].NpcFaction.FactionId, TimedNegativeRelationships[i].PlayerFaction.FactionId);
					TimedNegativeRelationships.RemoveAtFast(i);
				}
			}
			catch (Exception e)
			{
				WriteToLog("AssessNegativeRelationships", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private void AssessMendingRelationships()
		{
			try
			{
				WriteToLog("AssessMendingRelationships", $"MendingRelationships.Count:\t{TimedNegativeRelationships.Count}", LogType.Debug);
				DumpMendingRelationshipsRelationships();
				for (int i = MendingRelationships.Count - 1; i >= 0; i--)
				{
					if (Settings.Random.Next(0, 100) < 75) continue;
					PendingRelation relationToRemove = MendingRelationships[i];
					MendingRelationships.RemoveAtFast(i);
					RequestDialog(relationToRemove.NpcFaction.GetFactionById(), relationToRemove.PlayerFaction.GetFactionById(), Dialogue.DialogType.PeaceAccepted);
					AutoPeace(relationToRemove.NpcFaction, relationToRemove.PlayerFaction);
				}
			}
			catch (Exception e)
			{
				WriteToLog("AssessMendingRelationships", $"Exception!\t{e}", LogType.Exception);
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
				WriteToLog("ClearRemovedFactionFromRelationships", $"Exception!\t{e}", LogType.Exception);
			}
		}
		
		private void CheckCounts()
		{
			if (MendingRelationships.Count == 0 && TimedNegativeRelationships.Count == 0) FactionTimer(MyUpdateOrder.NoUpdate);
			WriteToLog("CheckCounts", $"MendingRelationships:\t{MendingRelationships.Count}\tTimedNegativeRelationship:\t{TimedNegativeRelationships.Count}", LogType.Debug);
		}

		private static void FactionTimer(MyUpdateOrder updateOrder)
		{
			if (FactionCore.FactionCoreStaticInstance.UpdateOrder != updateOrder)
				MyAPIGateway.Utilities.InvokeOnGameThread(() => FactionCore.FactionCoreStaticInstance.SetUpdateOrder(updateOrder));
			//MyAPIGateway.Utilities.InvokeOnGameThread(() => WriteToLog("FactionTimer", $"SetUpdateOrder:\t{updateOrder}\tActual:\t{FactionCore.FactionCoreStaticInstance.UpdateOrder}"));
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

		//Debug Outputs

		private void DumpEverythingToTheLog()
		{
			if (!Common.Settings.Settings.DebugMode) return;
			try
			{
				const string callerName = "FactionsDump";
				List<TimedRelationship> tempTimedRelationship = TimedNegativeRelationships;
				foreach (TimedRelationship negativeRelationship in tempTimedRelationship)
					WriteToLog(callerName, $"negativeRelationship:\t{negativeRelationship}", LogType.Debug);
				List<PendingRelation> tempMendingRelations = MendingRelationships;
				foreach (PendingRelation mendingRelationship in tempMendingRelations)
					WriteToLog(callerName, $"mendingRelationship:\t{mendingRelationship}", LogType.Debug);
				Dictionary<long, IMyFaction> tempFactionDictionary = _enforcementFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					WriteToLog(callerName, $"enforcementDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
				tempFactionDictionary = _lawfulFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					WriteToLog(callerName, $"lawfulDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
				tempFactionDictionary = _pirateFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					WriteToLog(callerName, $"pirateDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
				tempFactionDictionary = _npcFactionDictionary; //_nonEemNpcFactionDictionary
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					WriteToLog(callerName, $"npcDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
				tempFactionDictionary = _nonEemNpcFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					WriteToLog(callerName, $"_nonEemNpcFactionDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
				tempFactionDictionary = _playerFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					WriteToLog(callerName, $"playerDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
				tempFactionDictionary = _playerPirateFactionDictionary;
				foreach (KeyValuePair<long, IMyFaction> faction in tempFactionDictionary)
					WriteToLog(callerName, $"playerPirateDictionary:\t{faction.Key}\t{faction.Value.Tag}", LogType.Debug);
				Dictionary<long, int> tempNewFactionDictioanry = _newFactionDictionary;
				foreach (KeyValuePair<long, int> faction in tempNewFactionDictioanry)
					WriteToLog(callerName, $"newFactionDictionary:\t{faction}\t{faction.Key.GetFactionById()?.Tag}", LogType.Debug);
			}
			catch (Exception e)
			{
				WriteToLog("DumpEverythingToTheLog", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private void DumpNewFactionDictionary()
		{
			try
			{
				WriteToLog("DumpNewFactionDictionary", $"newFactionDictionary.Count:\t{_newFactionDictionary.Count}", LogType.Debug);
				Dictionary<long, int> tempNewFactionDictioanry = _newFactionDictionary;
				foreach (KeyValuePair<long, int> faction in tempNewFactionDictioanry)
					WriteToLog("DumpNewFactionDictionary", $"newFactionDictionary:\t{faction}\t{faction.Key.GetFactionById()?.Tag}", LogType.Debug);
			}
			catch (Exception e)
			{
				WriteToLog("DumpNewFactionDictionary", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private void DumpTimedNegativeFactionRelationships()
		{
			if (!Common.Settings.Settings.DebugMode) return;
			try
			{
				WriteToLog("DumpTimedNegativeFactionRelationships", $"TimedNegativeRelationships.Count:\t{TimedNegativeRelationships.Count}", LogType.Debug);
				const string callerName = "DumpTimedNegativeFactionRelationships";
				List<TimedRelationship> tempTimedRelationship = TimedNegativeRelationships;
				foreach (TimedRelationship negativeRelationship in tempTimedRelationship)
					WriteToLog(callerName, $"negativeRelationship:\t{negativeRelationship}", LogType.Debug);
			}
			catch (Exception e)
			{
				WriteToLog("DumpTimedNegativeFactionRelationships", $"Exception!\t{e}", LogType.Exception);
			}
		}

		private void DumpMendingRelationshipsRelationships()
		{
			if (!Common.Settings.Settings.DebugMode) return;
			try
			{
				const string callerName = "DumpMendingRelationshipsRelationships";
				List<PendingRelation> tempMendingRelations = MendingRelationships;
				foreach (PendingRelation mendingRelationship in tempMendingRelations)
					WriteToLog(callerName, $"mendingRelationship:\t{mendingRelationship}", LogType.Debug);
			}
			catch (Exception e)
			{
				WriteToLog("DumpMendingRelationshipsRelationships", $"Exception!\t{e}", LogType.Exception);
			}
		}

		//private void WriteToLog(string caller, string message)
		//{
		//	WriteToLog(caller, message, true);
		//	if(!"DumpEverythingToTheLog, DumpTimedNegativeFactionRelationships, DumpMendingRelationshipsRelationships, DumpNewFactionDictionary".Contains(caller))
		//		DumpEverythingToTheLog(true);
		//}

		// Structs and other enums as necessary

		private struct PendingRelation
		{
			public readonly long NpcFaction;
			public readonly long PlayerFaction;

			/// <inheritdoc />
			public override string ToString()
			{
				return $"NpcFaction:\t{NpcFaction}\t{NpcFaction.GetFactionById()?.Tag}\tNpcFaction:\t{PlayerFaction}\t{PlayerFaction.GetFactionById()?.Tag}";
			}

			public PendingRelation(long npcFactionId, long playerFactionId)
			{
				NpcFaction = npcFactionId;
				PlayerFaction = playerFactionId;
			}
		}
	}
}
