﻿using System;
using System.Collections.Generic;
using System.Linq;
using EemRdx.Extensions;
using EemRdx.Helpers;
using EemRdx.Networking;
using EemRdx.Scripts.Utilities;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ModAPI;
using EemRdx.Factions.Messages;
using VRage.Game.ModAPI.Interfaces;

// ReSharper disable MemberCanBePrivate.Global

namespace EemRdx.Factions
{
    public static class Factions
    {
        public static bool SetupComplete;

        public static Random FactionsRandom { get; set; }

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
            ScrubRelationLists(newPirate.FactionId);
            foreach (KeyValuePair<long, IMyFaction> faction in MyAPIGateway.Session.Factions.Factions)
            { // If this is the same faction, or if the session Faction is a player faction, skip this round (players can handle their own relations)
                if (faction.Value == newPirate || !faction.Value.IsEveryoneNpc()) continue;                  
                faction.Value.DeclareWar(newPirate);
            }
            //TODO: Cancel pending peace requests when this happens
        }

        public static void NewFriendly(this IMyFaction newFriendly)
        {
            foreach (KeyValuePair<long, IMyFaction> faction in MyAPIGateway.Session.Factions.Factions)
            { // If this is the same faction, or if the session Faction is a player faction, skip this round (players can handle their own relations)
                if (faction.Value == newFriendly || !faction.Value.IsEveryoneNpc() || faction.Key.IsPirate()) continue;                  
                HandleRemorsefulErrantPlayerFaction(newFriendly.FactionId, faction.Value.FactionId);
            }
        }

        private static bool IsPirate(this long faction)
        {
            return PirateFactionDictionary.ContainsKey(faction);
        }

        private static bool IsLawful(this long faction)
        {
            return LawfulFactionDictionary.ContainsKey(faction);
        }

        private static bool IsEnforcement(this long faction)
        {
            return EnforcementFactionDictionary.ContainsKey(faction);
        }

        private static bool ValidateNonPirateFactions(IMyFaction leftFaction, IMyFaction rightFaction)
        {
            if (leftFaction == null || rightFaction == null) return false;
            return !PirateFactionDictionary.ContainsKey(leftFaction.FactionId) && !PirateFactionDictionary.ContainsKey(rightFaction.FactionId);
        }

        private static bool IsFactionErrant(this long playerFactionId, long npcFactionId)
        {
            return BadRelations.IndexOf(new BadRelation(playerFactionId, npcFactionId)) != -1;
        }

        private static bool IsFactionPenitent(this long playerFactionId, long npcFactionId)
        {
            return PenitentFactions.IndexOf(new BadRelation(playerFactionId, npcFactionId)) != -1;
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

        private static Dictionary<string, Func<string>> FactionPeaceAcceptedDialog { get; set; }

        private static Dictionary<string, Func<string>> FactionPeaceConsideredDialog { get; set; }

        private static Dictionary<string, Func<string>> FactionPeaceProposedDialog { get; set; }

        private static Dictionary<string, Func<string>> FactionPeaceRejectedDialog { get; set; }

        private static Dictionary<string, Func<string>> FactionWarDeclaredDialog { get; set; }

        private static Dictionary<string, Func<string>> FactionWarReceivedDialog { get; set; }

        private static void InitMessageDictionaries()
        {
            FactionPeaceAcceptedDialog = new Dictionary<string, Func<string>>
            {
                {Amph.Tag, Amph.PeaceAccepted}, {Civl.Tag, Civl.PeaceAccepted}, {Exmc.Tag, Exmc.PeaceAccepted},
                {Hs.Tag, Hs.PeaceAccepted}, {Imdc.Tag, Imdc.PeaceAccepted}, {Istg.Tag, Istg.PeaceAccepted},
                {Kuss.Tag, Kuss.PeaceAccepted}, {Mai.Tag, Mai.PeaceAccepted}, {Mmec.Tag, Mmec.PeaceAccepted},
                {Sepd.Tag, Sepd.PeaceAccepted}, {Sprt.Tag, Sprt.PeaceAccepted}, {Ucmf.Tag, Ucmf.PeaceAccepted}
            };
            FactionPeaceConsideredDialog = new Dictionary<string, Func<string>>
            {
                {Amph.Tag, Amph.PeaceConsidered}, {Civl.Tag, Civl.PeaceConsidered}, {Exmc.Tag, Exmc.PeaceConsidered},
                {Hs.Tag, Hs.PeaceConsidered}, {Imdc.Tag, Imdc.PeaceConsidered}, {Istg.Tag, Istg.PeaceConsidered},
                {Kuss.Tag, Kuss.PeaceConsidered}, {Mai.Tag, Mai.PeaceConsidered}, {Mmec.Tag, Mmec.PeaceConsidered},
                {Sepd.Tag, Sepd.PeaceConsidered}, {Sprt.Tag, Sprt.PeaceConsidered}, {Ucmf.Tag, Ucmf.PeaceConsidered}
            };
            FactionPeaceProposedDialog = new Dictionary<string, Func<string>>
            {
                {Amph.Tag, Amph.PeaceProposed}, {Civl.Tag, Civl.PeaceProposed}, {Exmc.Tag, Exmc.PeaceProposed},
                {Hs.Tag, Hs.PeaceProposed}, {Imdc.Tag, Imdc.PeaceProposed}, {Istg.Tag, Istg.PeaceProposed},
                {Kuss.Tag, Kuss.PeaceProposed}, {Mai.Tag, Mai.PeaceProposed}, {Mmec.Tag, Mmec.PeaceProposed},
                {Sepd.Tag, Sepd.PeaceProposed}, {Sprt.Tag, Sprt.PeaceProposed}, {Ucmf.Tag, Ucmf.PeaceProposed}
            };
            FactionPeaceRejectedDialog = new Dictionary<string, Func<string>>
            {
                {Amph.Tag, Amph.PeaceRejected}, {Civl.Tag, Civl.PeaceRejected}, {Exmc.Tag, Exmc.PeaceRejected},
                {Hs.Tag, Hs.PeaceRejected}, {Imdc.Tag, Imdc.PeaceRejected}, {Istg.Tag, Istg.PeaceRejected},
                {Kuss.Tag, Kuss.PeaceRejected}, {Mai.Tag, Mai.PeaceRejected}, {Mmec.Tag, Mmec.PeaceRejected},
                {Sepd.Tag, Sepd.PeaceRejected}, {Sprt.Tag, Sprt.PeaceRejected}, {Ucmf.Tag, Ucmf.PeaceRejected}
            };
            FactionWarDeclaredDialog = new Dictionary<string, Func<string>>
            {
                {Amph.Tag, Amph.WarDeclared}, {Civl.Tag, Civl.WarDeclared}, {Exmc.Tag, Exmc.WarDeclared}, {Hs.Tag, Hs.WarDeclared},
                {Imdc.Tag, Imdc.WarDeclared}, {Istg.Tag, Istg.WarDeclared}, {Kuss.Tag, Kuss.WarDeclared}, {Mai.Tag, Mai.WarDeclared},
                {Mmec.Tag, Mmec.WarDeclared}, {Sepd.Tag, Sepd.WarDeclared}, {Sprt.Tag, Sprt.WarDeclared}, {Ucmf.Tag, Ucmf.WarDeclared}
            };
            FactionWarReceivedDialog = new Dictionary<string, Func<string>>
            {
                {Amph.Tag, Amph.WarReceived}, {Civl.Tag, Civl.WarReceived}, {Exmc.Tag, Exmc.WarReceived}, {Hs.Tag, Hs.WarReceived},
                {Imdc.Tag, Imdc.WarReceived}, {Istg.Tag, Istg.WarReceived}, {Kuss.Tag, Kuss.WarReceived}, {Mai.Tag, Mai.WarReceived},
                {Mmec.Tag, Mmec.WarReceived}, {Sepd.Tag, Sepd.WarReceived}, {Sprt.Tag, Sprt.WarReceived}, {Ucmf.Tag, Ucmf.WarReceived}
            };
        }

        private enum DialogType
        {
            CollectiveDisappointment, CollectiveReprieve, PeaceAccepted, PeaceConsidered, PeaceProposed, PeaceRejected, WarDeclared, WarReceived
        }

        private static void RequestDialog(IMyFaction npcFaction, IMyFaction playerFaction, DialogType type)
        {
            AiSessionCore.DebugLog?.WriteToLog("RequestDialog", $"npcFaction: {npcFaction?.Tag}  playerFaction: {playerFaction?.Tag} DialogType: {type}");
            Func<string> message = DefaultDialogs.CatchAll;
            Func<string> tmpMessage;
            string tag = DefaultDialogs.DefaultTag;
            switch (type)
            {
                case DialogType.CollectiveDisappointment:
                    message = DefaultDialogs.CollectiveDisappointment;
                    tag = DefaultDialogs.DefaultTag;
                    break;
                case DialogType.CollectiveReprieve:
                    message = DefaultDialogs.CollectiveReprieve;
                    tag = DefaultDialogs.CollectiveTag;
                    break;
                case DialogType.PeaceAccepted:
                    if (FactionPeaceAcceptedDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
                    {
                        message = tmpMessage;
                        tag = npcFaction.Tag;
                    }
                    break;
                case DialogType.PeaceConsidered:
                    if (FactionPeaceConsideredDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
                    {
                        message = tmpMessage;
                        tag = npcFaction.Tag;
                    }
                    break;
                case DialogType.PeaceProposed:
                    if (FactionPeaceProposedDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
                    {
                        message = tmpMessage;
                        tag = npcFaction.Tag;
                    }
                    break;
                case DialogType.PeaceRejected:
                    if (FactionPeaceRejectedDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
                    {
                        message = tmpMessage;
                        tag = npcFaction.Tag;
                    }
                    break;
                case DialogType.WarDeclared:
                    if (FactionWarDeclaredDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
                    {
                        message = tmpMessage;
                        tag = npcFaction.Tag;
                    }
                    break;
                case DialogType.WarReceived:
                    if (FactionWarReceivedDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
                    {
                        message = tmpMessage;
                        tag = npcFaction.Tag;
                    }
                    break;
                default:
                    break;
            }
            SendFactionMessageToAllFactionMembers(message.Invoke(), tag, playerFaction.Members);
        }

        private static void CleanupFactions(long faction)
        {
            PlayerFactionDictionary.Remove(faction);
            PirateFactionDictionary.Remove(faction);
            EnforcementFactionDictionary.Remove(faction);
            LawfulFactionDictionary.Remove(faction);
            for (int index = FactionsAtWar.Count - 1; index >= 0; index--)
            {
                FactionsAtWar war = FactionsAtWar[index];
                if (war.PlayerFaction.FactionId == faction || war.AiFaction.FactionId == faction)
                    FactionsAtWar.RemoveAtFast(index);
            }
            ScrubRelationLists(faction);
        }

        private static void ScrubRelationLists(long faction)
        {
            for (int index = BadRelations.Count - 1; index >= 0; index--)
            {
                BadRelation war = BadRelations[index];
                if (war.PlayerFaction == faction || war.NpcFaction == faction)
                    BadRelations.RemoveAtFast(index);
            }

            for (int index = PenitentFactions.Count - 1; index >= 0; index--)
            {
                BadRelation war = PenitentFactions[index];
                if (war.PlayerFaction == faction || war.NpcFaction == faction)
                    PenitentFactions.RemoveAtFast(index);
            }
        }


        private static void FactionStateChanged(MyFactionStateChange action, long fromFaction, long toFaction, long playerId, long senderId)
        {
            AiSessionCore.DebugLog?.WriteToLog("FactionStateChanged", $"Action: {action} fromFaction: {fromFaction} toFaction: {toFaction} playerId: {playerId} senderId: {senderId} isServer: {AiSessionCore.IsServer}");
            switch (action)
            {
                case MyFactionStateChange.SendPeaceRequest:

                    if (PirateFactionDictionary.ContainsKey(toFaction))                                 // Is this player a pirate?
                    {
                        RequestDialog(toFaction.GetFactionById(), fromFaction.GetFactionById(), DialogType.PeaceRejected);
                        MyAPIGateway.Session.Factions.CancelPeaceRequest(fromFaction, toFaction);
                        break;
                    }

                    if (fromFaction.GetFactionById().IsNeutral(toFaction.GetFactionById().FounderId)    // Are these factions already neutral, or is the requesting faction an NPC faction and the player is currently errant
                        || (fromFaction.GetFactionById().IsEveryoneNpc() && toFaction.IsFactionErrant(fromFaction)))
                    {
                        MyAPIGateway.Session.Factions.CancelPeaceRequest(fromFaction, toFaction);
                        break;
                    }

                    if (!fromFaction.GetFactionById().IsEveryoneNpc() &&                                // Is this a player faction, and is this player errant
                        fromFaction.IsFactionErrant(toFaction))
                    {
                        RequestDialog(toFaction.GetFactionById(), fromFaction.GetFactionById(), DialogType.PeaceConsidered);
                        HandleRemorsefulErrantPlayerFaction(fromFaction, toFaction);
                        break;
                    }
                    if (!fromFaction.GetFactionById().IsEveryoneNpc() &&                                // Is this a player faction, and is this player penitent
                        fromFaction.IsFactionPenitent(toFaction))
                    { // We don't want to do anything here, just let it ride and escape the switch
                        break;
                    }
                    if (fromFaction.GetFactionById().IsEveryoneNpc() || toFaction.GetFactionById().IsEveryoneNpc())
                        toFaction.GetFactionById().AcceptPeaceFrom(fromFaction.GetFactionById());       // Condition not accounted for, either the to or from is an NPC, just accept the request
                    break;
                case MyFactionStateChange.RemoveFaction:
                    CleanupFactions(fromFaction);                                                       // Remove the faction from all dictionaries and lists
                    break;
                case MyFactionStateChange.CancelPeaceRequest:
                    BadRelation noLongerRemorseful = new BadRelation(fromFaction, toFaction);           // fromFaction will always be a player, so this will always be in the player, npc order (I wrote the code, believe me)
                    if (PenitentFactions.IndexOf(noLongerRemorseful) != -1)                             // They were remorseful, now they aren't. Asshats!  So much extra code...
                    {
                        RequestDialog(fromFaction.GetFactionById(), toFaction.GetFactionById(), DialogType.PeaceRejected);
                        HandleNoLongerRemorsefulErrantPlayerFaction(fromFaction, toFaction);
                    }
                    break;
                case MyFactionStateChange.AcceptPeace:
                    break;
                case MyFactionStateChange.DeclareWar:
                    if (fromFaction.GetFactionById().IsEveryoneNpc() && !toFaction.GetFactionById().IsEveryoneNpc())
                    {                                                                                   // Make sure this is an NPC declaring war on a player
                        RequestDialog(fromFaction.GetFactionById(), toFaction.GetFactionById(), DialogType.WarDeclared);
                        HandleNewErrantPlayerFaction(fromFaction, toFaction);
                        AiSessionCore.DebugLog?.WriteToLog("FactionStateChanged", $"NPC War On Player");
                    }
                    if (!fromFaction.GetFactionById().IsEveryoneNpc() && toFaction.GetFactionById().IsEveryoneNpc())
                    {                                                                                   // Make sure this is an player declaring war on a NPC
                        RequestDialog(toFaction.GetFactionById(), fromFaction.GetFactionById(), DialogType.WarReceived);
                        HandleNewErrantPlayerFaction(fromFaction, toFaction);
                        AiSessionCore.DebugLog?.WriteToLog("FactionStateChanged", $"Player War On NPC");
                    }
                    break;
                case MyFactionStateChange.FactionMemberSendJoin:
                    break;
                case MyFactionStateChange.FactionMemberCancelJoin:
                    break;
                case MyFactionStateChange.FactionMemberAcceptJoin:
                    break;
                case MyFactionStateChange.FactionMemberKick:
                    break;
                case MyFactionStateChange.FactionMemberPromote:
                    break;
                case MyFactionStateChange.FactionMemberDemote:
                    break;
                case MyFactionStateChange.FactionMemberLeave:
                    break;
                case MyFactionStateChange.FactionMemberNotPossibleJoin:
                    break;
                default:
                    AiSessionCore.GeneralLog?.WriteToLog(nameof(action), action.ToString());
                    break;
            }
        }

        private static void FactionCreated(long factionId)
        {
            if (PlayerFactionDictionary.ContainsKey(factionId) ||
                PirateFactionDictionary.ContainsKey(factionId)) return;
            IMyFaction newFaction;
            if (!ValidateFactionEvents(factionId, out newFaction)) return;
            if (newFaction.IsEveryoneNpc() || PlayerFactionExclusionList.Any(x => newFaction.Description.StartsWith(x)))
            {
                AddToPirateFactionDictionary(factionId, newFaction);
                newFaction.NewPirate();
                return;
            }
            AddToPlayerFactionDictionary(factionId, newFaction);
            AiSessionCore.DebugLog?.WriteToLog("FactionCreated", $"newFaction: {newFaction.Tag}");
        }

        private static void FactionEdited(long factionId)
        {
            IMyFaction editedFaction;
            if (!ValidateFactionEvents(factionId, out editedFaction) || editedFaction.IsEveryoneNpc()) return;
            if (!PlayerFactionExclusionList.Any(x => editedFaction.Description.StartsWith(x)) && PirateFactionDictionary.ContainsKey(factionId))
            {
                RemoveFromPirateFactionDictionary(factionId);
                AddToPlayerFactionDictionary(factionId, factionId.GetFactionById());
                editedFaction.NewFriendly();
                RequestDialog(null, factionId.GetFactionById(), DialogType.CollectiveReprieve);
                //TODO: need to determine peace conditions (cooldown?) newFaction.NewFriendly(); is a placeholder
                //TODO: Idea - add player to PenitentFactions list and toss them into the rift for the 25% per pass for peace
                //TODO: However, need to account for player going rogue again (pirate); need to account for this anyhow with the penitent faction stuff still
                return;
            }
            if (!PlayerFactionExclusionList.Any(x => editedFaction.Description.StartsWith(x) && PlayerFactionDictionary.ContainsKey(factionId))) return;
            RemoveFromPlayerFactionDictionary(editedFaction.FactionId);
            AddToPirateFactionDictionary(factionId, editedFaction);
            editedFaction.NewPirate();
            RequestDialog(null, factionId.GetFactionById(), DialogType.CollectiveDisappointment);

            AiSessionCore.DebugLog?.WriteToLog("FactionEdited", $"editedFaction Leave: {editedFaction.Tag}");
        }

        private static bool ValidateFactionEvents(long factionId, out IMyFaction newFaction)
        {
            newFaction = factionId.GetFactionById();
            return SetupComplete && newFaction != null;
        }

        private static List<BadRelation> BadRelations { get; set; }

        private static List<BadRelation> PenitentFactions { get; set; }

        private struct BadRelation
        {
            public readonly long PlayerFaction;
            public readonly long NpcFaction;

            public BadRelation(long playerFaction, long npcFaction)
            {
                PlayerFaction = playerFaction;
                NpcFaction = npcFaction;
            }
        }

        private static void HandleNewErrantPlayerFaction(long playerFactionId, long npcFactionId)
        {
            BadRelation newEvil = new BadRelation(playerFactionId, npcFactionId);
            if (BadRelations.IndexOf(newEvil) != -1)
                return;
            BadRelations.Add(newEvil);
        }

        private static void HandleRemorsefulErrantPlayerFaction(long playerFactionId, long npcFactionId)
        {
            BadRelation penitentFaction = new BadRelation(playerFactionId, npcFactionId);
            BadRelations.Remove(penitentFaction);
            AiSessionCore.DebugLog?.WriteToLog("HandleRemorsefulErrantPlayerFaction", $"BadRelations.Count: {BadRelations.Count} PenitentFactions.Count: {PenitentFactions.Count}", true);
            PenitentFactions.Add(penitentFaction);
            if (!playerFactionId.GetFactionById().IsPeacePendingTo(npcFactionId.GetFactionById()))
                playerFactionId.GetFactionById().ProposePeaceTo(npcFactionId.GetFactionById());
        }

        private static void HandleNoLongerRemorsefulErrantPlayerFaction(long playerFactionId, long npcFactionId)
        {
            BadRelation noLongerPenitentFaction = new BadRelation(playerFactionId, npcFactionId);
            lock (PenitentFactions)
                PenitentFactions.Remove(noLongerPenitentFaction);
            BadRelations.Add(noLongerPenitentFaction);
        }

        public static void FactionAssessment()
        {
            for (int i = PenitentFactions.Count - 1; i >= 0; i--)
            {
                int randomNumber = FactionsRandom.Next(0, 100);
                AiSessionCore.DebugLog?.WriteToLog("FactionAssessment", $"Random roll: {randomNumber} Iteration: {i} PenitentFactions.Count: {PenitentFactions.Count}", true);
                if (randomNumber < 75) continue;
                BadRelation timeServed = PenitentFactions[i];
                PenitentFactions.RemoveAtFast(i);
                timeServed.NpcFaction.GetFactionById().AcceptPeaceFrom(timeServed.PlayerFaction.GetFactionById());
                RequestDialog(timeServed.NpcFaction.GetFactionById(), timeServed.PlayerFaction.GetFactionById(), DialogType.PeaceAccepted);
            }
        }

        private static void SendFactionMessageToAllFactionMembers(string message, string messageSender, DictionaryReader<long, MyFactionMember> target, string color = MyFontEnum.Red)
        {
            foreach (KeyValuePair<long, MyFactionMember> factionMember in target)
            {
                Messaging.SendMessageToPlayer($"{message}", messageSender, factionMember.Key, color);
            }
        }

        //public static List<KeyValuePair<long, long>> BadRelations;

        public static Dictionary<long, IMyFaction> PlayerFactionDictionary { get; private set; }

        public static Dictionary<long, IMyFaction> PirateFactionDictionary { get; private set; }

        public static Dictionary<long, IMyFaction> EnforcementFactionDictionary { get; private set; }

        public static Dictionary<long, IMyFaction> LawfulFactionDictionary { get; private set; }

        public static void Initialize()
        {
            using (new Profiler("NewFactionInit"))
            {
                if (SetupComplete) return;
                Register();
                InitMessageDictionaries();
                FactionsRandom = new Random();
                BadRelations = new List<BadRelation>();
                PenitentFactions = new List<BadRelation>();
                FactionsAtWar = new List<FactionsAtWar>();
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
            foreach (KeyValuePair<long, IMyFaction> factions in MyAPIGateway.Session.Factions.Factions)
            {
                foreach (KeyValuePair<long, IMyFaction> pirates in PirateFactionDictionary)
                {
                    if (factions.Key == pirates.Key) continue;
                    factions.Value.DeclareWar(pirates.Value);
                }
            }
        }
        //TODO: Need to decide whether to combine all wars with SEPD and IMDC, or allow them to manage releases on their own
        private static void AddToLawfulFactionDictionary(long factionId, IMyFaction faction)
        {
            LawfulFactionDictionary.Add(factionId, faction);
        }

        private static void AddToEnforcementFactionDictionary(long factionId, IMyFaction faction)
        {
            EnforcementFactionDictionary.Add(factionId, faction);
        }

        private static void AddToPirateFactionDictionary(long factionId, IMyFaction faction)
        {
            PirateFactionDictionary.Add(factionId, faction);
        }

        private static void AddToPlayerFactionDictionary(long factionId, IMyFaction faction)
        {
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
