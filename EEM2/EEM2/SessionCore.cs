using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace Cheetah.AI
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class AISessionCore : MySessionComponentBase
    {
        #region Settings
        // Here are the options which can be freely edited.

        /// <summary>
        /// This toggles the Debug mode. Without Debug, only critical messages are shown in chat.
        /// </summary>
        public static readonly bool Debug = true;

        /// <summary>
        /// This toggles the Debug Anti-Spam mode. With this, a single combination of ship name + message will be displayed only once per session.
        /// </summary>
        public static readonly bool DebugAntiSpam = false;
        #endregion
        
        public const string WarpinEffect = "EEMWarpIn";
        private readonly Timer _peaceRequestDelay = new Timer();
        private readonly Timer _factionClock = new Timer();
        private readonly List<long> _newFactionsIDs = new List<long>();

        /// <summary>
        /// This permits certain operations to throw custom exceptions in order to
        /// provide detailed descriptions of what gone wrong, over call stack.<para />
        /// BEWARE, every exception thrown must be explicitly provided with a catcher, or it will crash the entire game!
        /// </summary>
        public static readonly bool AllowThrowingErrors = true;

        public static bool IsServer => !MyAPIGateway.Multiplayer.MultiplayerActive || MyAPIGateway.Multiplayer.IsServer;

        private static readonly Dictionary<long, BotBase.OnDamageTaken> DamageHandlers = new Dictionary<long, BotBase.OnDamageTaken>();
        #region DictionaryAccessors
        public static void AddDamageHandler(long gridId, BotBase.OnDamageTaken handler)
        {
            DamageHandlers.Add(gridId, handler);
        }
        public static void AddDamageHandler(IMyCubeGrid grid, BotBase.OnDamageTaken handler)
        {
            AddDamageHandler(grid.GetTopMostParent().EntityId, handler);
        }
        public static void RemoveDamageHandler(long gridId)
        {
            DamageHandlers.Remove(gridId);
        }
        public static void RemoveDamageHandler(IMyCubeGrid grid)
        {
            RemoveDamageHandler(grid.GetTopMostParent().EntityId);
        }
        public static bool HasDamageHandler(long gridId)
        {
            return DamageHandlers.ContainsKey(gridId);
        }
        public static bool HasDamageHandler(IMyCubeGrid grid)
        {
            return HasDamageHandler(grid.GetTopMostParent().EntityId);
        }
        #endregion

        private bool _inited;
        private bool _factionsInited;

        public override void UpdateBeforeSimulation()
        {
            //if (MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Multiplayer.IsServer) return;
            if (_inited) return;

            InitNpcFactions();
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageRefHandler);
            //MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, GenericDamageHandler);
            MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, GenericDamageHandler);
            _inited = true;
        }

        public void DamageRefHandler(object damagedObject, ref MyDamageInformation damage)
        {
            GenericDamageHandler(damagedObject, damage);
        }

        public void GenericDamageHandler(object damagedObject, MyDamageInformation damage)
        {
            try
            {
                if (!(damagedObject is IMySlimBlock)) return;
                IMySlimBlock damagedBlock = (IMySlimBlock) damagedObject;
                IMyCubeGrid damagedGrid = damagedBlock.CubeGrid;
                long gridId = damagedGrid.GetTopMostParent().EntityId;
                if (DamageHandlers.ContainsKey(gridId))
                {
                    try
                    {
                        DamageHandlers[gridId].Invoke(damagedBlock, damage);
                    }
                    catch (Exception scrap)
                    {
                        LogError("DamageHandler.Invoke", scrap);
                    }
                }
            }
            catch (Exception scrap)
            {
                LogError("GenericDamageHandler", scrap);
            }
        }

        private void InitNpcFactions()
        {
            try
            {
                Diplomacy.Init();
                IMyFactionCollection factionSystem = MyAPIGateway.Session.Factions;

                HashSet<IMyFaction> factionsToMakePeaceWith = Diplomacy.LawfulFactions.ToHashSet();
                factionsToMakePeaceWith.UnionWith(factionSystem.Factions.Values.Where(x => x.IsPlayerFaction()));
                foreach (IMyFaction faction1 in Diplomacy.LawfulFactions)
                {
                    PeaceTimers.Add(faction1, new Dictionary<IMyFaction, DateTime>());
                    foreach (IMyFaction faction2 in factionsToMakePeaceWith)
                    {
                        if (faction1 == faction2) continue;
                        if (faction1.IsPeacefulTo(faction2)) continue;
                        faction1.ProposePeace(faction2, true);
                    }
                }
                
                #region Peace Request Delay
                _peaceRequestDelay.Interval = 5000;
                _peaceRequestDelay.AutoReset = false;
                _peaceRequestDelay.Elapsed += (trash1, trash2) =>
                {
                    List<IMyFaction> newFactions = new List<IMyFaction>();
                    foreach (long id in _newFactionsIDs)
                        newFactions.Add(factionSystem.TryGetFactionById(id));

                    _newFactionsIDs.Clear();
                    foreach (IMyFaction lawfulFaction in Diplomacy.LawfulFactions)
                    {
                        foreach (IMyFaction newFaction in newFactions)
                            lawfulFaction.ProposePeace(newFaction);
                    }
                    _peaceRequestDelay.Stop();
                };

                factionSystem.FactionCreated += (factionId) =>
                {
                    _newFactionsIDs.Add(factionId);
                    _peaceRequestDelay.Start();
                };
                #endregion

                #region Faction Clock
                _factionClock.Interval = 10000;
                _factionClock.AutoReset = true;
                _factionClock.Elapsed += (trash1, trash2) =>
                {
                    if (!_factionsInited)
                    {
                        foreach (IMyFaction faction1 in Diplomacy.LawfulFactions)
                        {
                            foreach (IMyFaction faction2 in Diplomacy.LawfulFactions)
                            {
                                if (faction1 == faction2) continue;
                                if (faction1.IsPeacefulTo(faction2)) continue;
                                faction1.AcceptPeace(faction2);
                            }
                        }
                        _factionsInited = true;
                    }

                    foreach (KeyValuePair<IMyFaction, Dictionary<IMyFaction, DateTime>> npcFactionTimer in PeaceTimers)
                    {
                        IMyFaction npcFaction = npcFactionTimer.Key;
                        List<IMyFaction> remove = new List<IMyFaction>();
                        foreach (KeyValuePair<IMyFaction, DateTime> timer in npcFactionTimer.Value)
                        {
                            IMyFaction faction = timer.Key;
                            if (DateTime.Now >= timer.Value)
                            {
                                npcFaction.ProposePeace(faction, true);
                                remove.Add(faction);
                            }
                        }

                        foreach (IMyFaction faction in remove)
                            npcFactionTimer.Value.Remove(faction);
                    }
                };
                _factionClock.Start();
                #endregion
            }
            catch (Exception scrap)
            {
                LogError("InitNPCFactions", scrap);
            }
        }

        public static Dictionary<IMyFaction, Dictionary<IMyFaction, DateTime>> PeaceTimers = new Dictionary<IMyFaction, Dictionary<IMyFaction, DateTime>>();
        public static void DeclareWar(IMyFaction ownFaction, IMyFaction hostileFaction, TimeSpan truceDelay)
        {
            if (!PeaceTimers.ContainsKey(ownFaction))
            {
                if (AllowThrowingErrors) throw new Exception($"PeaceTimers dictionary error: can't find {ownFaction.Tag} key!");
                return;
            }

            DateTime peaceTime = DateTime.Now.Add(truceDelay);

            Dictionary<IMyFaction, DateTime> timerdict = PeaceTimers[ownFaction];

            if (!timerdict.ContainsKey(hostileFaction))
            {
                timerdict.Add(hostileFaction, peaceTime);
            }
            else
            {
                timerdict[hostileFaction] = peaceTime;
            }

            if (!ownFaction.IsHostileTo(hostileFaction))
            {
                ownFaction.DeclareWar(hostileFaction, true);
            }

            //DebugWrite($"DeclareWarTimer[{ownFaction.Tag}]", $"Added peace timer. Current time: {DateTime.Now:HH:mm:ss} | Calmdown at: {peaceTime:HH:mm:ss} | Calmdown delay: {truceDelay.ToString()}", false);
        }
        

        public static void LogError(string source, Exception scrap, string debugPrefix = "SessionCore.")
        {
            DebugHelper.Print("SessionCore", $"Fatal error in '{debugPrefix + source}': {scrap.Message}. {scrap.InnerException?.Message ?? "No additional info was given by the game :("}");
        }

        public static void DebugWrite(string source, string message, bool antiSpam)
        {
            if (Debug)
            {
                DebugHelper.Print($"{source}", $"{message}");
            }
        }
    }
}