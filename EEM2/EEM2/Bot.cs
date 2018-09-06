using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
//using Sandbox.ModAPI.Ingame;
//using IMyGridTerminalSystem = Sandbox.ModAPI.IMyGridTerminalSystem;
//using IMyProgrammableBlock = Sandbox.ModAPI.IMyProgrammableBlock;
//using IMyRadioAntenna = Sandbox.ModAPI.IMyRadioAntenna;
//using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;
//using IMyTerminalBlock = Sandbox.ModAPI.IMyTerminalBlock;
//using IMyThrust = Sandbox.ModAPI.IMyThrust;
//using IMyFaction = VRage.Game.ModAPI.IMyFaction;
//using IMyThrust = Sandbox.ModAPI.Ingame.IMyThrust;
using Ingame = Sandbox.ModAPI.Ingame;

namespace Cheetah.AI
{
    public enum BotTypes
    {
        None,
        Invalid,
        Station,
        Fighter,
        Freighter,
        Carrier
    }

    public static class BotFabric
    {
        public static BotBase FabricateBot(IMyCubeGrid grid, IMyRemoteControl remoteControl)
        {
            try
            {
                BotTypes botType = BotBase.ReadBotType(remoteControl);

                BotBase bot = null;
                switch (botType)
                {
                    case BotTypes.Fighter:
                        bot = new FighterBot(grid);
                        break;
                    case BotTypes.Freighter:
                        bot = new FreighterBot(grid);
                        break;
                    case BotTypes.Station:
                        bot = new StationBot(grid);
                        break;
                    case BotTypes.None:
                        break;
                    case BotTypes.Invalid:
                        break;
                    case BotTypes.Carrier:
                        break;
                    default:
                        if (AISessionCore.AllowThrowingErrors) throw new Exception("Invalid bot type");
                        break;
                }

                return bot;
            }
            catch (Exception scrap)
            {
                grid.LogError("BotFabric.FabricateBot", scrap);
                return null;
            }
        }
    }

    public abstract class BotBase
    {
        public IMyCubeGrid Grid { get; protected set; }
        protected readonly IMyGridTerminalSystem Term;
        public Vector3D GridPosition => Grid.GetPosition();
        public Vector3D GridVelocity => Grid.Physics.LinearVelocity;
        public float GridSpeed => (float)GridVelocity.Length();
        protected float GridRadius => (float)Grid.WorldVolume.Radius;
        protected readonly TimeSpan CalmdownTime = (!AISessionCore.Debug ? TimeSpan.FromMinutes(15) : TimeSpan.FromMinutes(3));

        protected bool InitX
        {
            get
            {
                try
                {
                    return Grid != null;
                }
                catch (Exception scrap)
                {
                    LogError("initialized", scrap);
                    return false;
                }
            }
        }
        public virtual bool Initialized => InitX;
        public MyEntityUpdateEnum Update { get; protected set; }
        public IMyRemoteControl RC { get; protected set; }
        private IMyFaction _ownerFaction;

        protected string DroneNameProvider => $"Drone_{RC.EntityId}";
        public string DroneName
        {
            get { return RC.Name; } 
            protected set
            {
                IMyEntity entity = RC;
                if (!string.IsNullOrEmpty(entity.Name)) return;
                entity.Name = value;
                MyAPIGateway.Entities.SetEntityName(entity);
                //DebugWrite("DroneName_Set", $"Drone EntityName set to {RC.Name}");
            }
        }
        protected bool GridoPerable
        {
            get
            {
                try
                {
                    return !Grid.MarkedForClose && !Grid.Closed && Grid.InScene;
                }
                catch (Exception scrap)
                {
                    LogError("gridoperable", scrap);
                    return false;
                }
            }
        }
        protected bool BotOperable;
        protected bool Closed;
        public virtual bool Operable
        {
            get
            {
                try
                {
                    return !Closed && Initialized && GridoPerable && RC.IsFunctional && BotOperable;
                }
                catch (Exception scrap)
                {
                    LogError("Operable", scrap);
                    return false;
                }
            }
        }
        public List<IMyRadioAntenna> Antennae { get; protected set; }
        public delegate void OnDamageTaken(IMySlimBlock damagedBlock, MyDamageInformation damage);
        protected event OnDamageTaken OnDamaged;
        public delegate void HOnBlockPlaced(IMySlimBlock block);
        protected event HOnBlockPlaced OnBlockPlaced;
        protected event Action Alert;

        protected BotBase(IMyCubeGrid grid)
        {
            if (grid == null) return;
            Grid = grid;
            Term = grid.GetTerminalSystem();
            Antennae = new List<IMyRadioAntenna>();
        }

        public static BotTypes ReadBotType(IMyRemoteControl remoteControl)
        {
            try
            {
                string customDataStr = remoteControl.CustomData.Trim().Replace("\r\n", "\n");
                List<string> customDataList = new List<string>(customDataStr.Split('\n'));

                if (customDataStr.IsNullEmptyOrWhiteSpace()) return BotTypes.None;
                if (customDataList.Count < 2)
                {
                    if (AISessionCore.AllowThrowingErrors) throw new Exception("CustomData is invalid", new Exception("CustomData consists of less than two lines"));
                    return BotTypes.Invalid;
                }
                if (customDataList[0].Trim() != "[EEM_AI]")
                {
                    if (AISessionCore.AllowThrowingErrors) throw new Exception("CustomData is invalid", new Exception($"AI tag invalid: '{customDataList[0]}'"));
                    return BotTypes.Invalid;
                }

                string[] bottype = customDataList[1].Split(':');
                if (bottype[0].Trim() != "Type")
                {
                    if (AISessionCore.AllowThrowingErrors) throw new Exception("CustomData is invalid", new Exception($"Type tag invalid: '{bottype[0]}'"));
                    return BotTypes.Invalid;
                }

                BotTypes botType = BotTypes.Invalid;
                switch (bottype[1].Trim())
                {
                    case "Fighter":
                        botType = BotTypes.Fighter;
                        break;
                    case "Freighter":
                        botType = BotTypes.Freighter;
                        break;
                    case "Carrier":
                        botType = BotTypes.Carrier;
                        break;
                    case "Station":
                        botType = BotTypes.Station;
                        break;
                }

                return botType;
            }
            catch (Exception scrap)
            {
                remoteControl.CubeGrid.LogError("[STATIC]BotBase.ReadBotType", scrap);
                return BotTypes.Invalid;
            }
        }

        protected virtual void DebugWrite(string source, string message, string debugPrefix)
        {
            Grid.DebugWrite(debugPrefix + source, message);
        }

        public virtual bool Init(IMyRemoteControl rc = null)
        {
            RC = rc ?? Term.GetBlocksOfType<IMyRemoteControl>(x => x.IsFunctional).FirstOrDefault();
            if (rc == null) return false;
            DroneName = DroneNameProvider;

            Antennae = Term.GetBlocksOfType<IMyRadioAntenna>(x => x.IsFunctional);

            bool hasSetup = ParseSetup();
            if (!hasSetup) return false;

            AISessionCore.AddDamageHandler(Grid, (block, damage) =>
            {
                if (OnDamaged == null) return;
                OnDamaged(block, damage);
            });
            Grid.OnBlockAdded += (block) =>
            {
                if (OnBlockPlaced == null) return;
                OnBlockPlaced(block);
            };
            _ownerFaction = Grid.GetOwnerFaction(true);
            BotOperable = true;
            return true;
        }

        public virtual void RecompilePBs()
        {
            foreach (IMyProgrammableBlock pb in Term.GetBlocksOfType<IMyProgrammableBlock>())
            {
                pb.Recompile();
            }
        }

        protected virtual void ReactOnDamage(IMySlimBlock block, MyDamageInformation damage, TimeSpan truceDelay, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                if (damage.IsMeteor())
                {
                    Grid.DebugWrite("ReactOnDamage", "Grid was damaged by meteor. Ignoring.");
                    return;
                }

                if (damage.IsThruster())
                {
                    if (block != null && !block.IsDestroyed)
                    {
                        Grid.DebugWrite("ReactOnDamage", "Grid was slighly damaged by thruster. Ignoring.");
                        return;
                    }
                }

                try
                {
                    if (damage.IsDoneByPlayer(out damager) && damager != null)
                    {
                        try
                        {
                            Grid.DebugWrite("ReactOnDamage", $"Grid is damaged by player {damager.DisplayName}. Trying to activate alert.");
                            RegisterHostileAction(damager, truceDelay);
                        }
                        catch (Exception scrap)
                        {
                            Grid.LogError("ReactOnDamage.GetDamagerFaction", scrap);
                        }
                    }
                    else Grid.DebugWrite("ReactOnDamage", "Grid is damaged, but damage source is not recognized as player.");
                }
                catch (Exception scrap)
                {
                    Grid.LogError("ReactOnDamage.IsDamageDoneByPlayer", scrap);
                }
            }
            catch (Exception scrap)
            {
                Grid.LogError("ReactOnDamage", scrap);
            }
        }

        protected virtual void BlockPlacedHandler(IMySlimBlock block)
        {
            if (block == null) return;

            try
            {
                IMyPlayer builder;
                if (!block.IsPlayerBlock(out builder)) return;
                IMyFaction faction = builder.GetFaction();
                if (faction == null) return;
                RegisterHostileAction(faction, CalmdownTime);
            }
            catch (Exception scrap)
            {
                Grid.LogError("BlockPlacedHandler", scrap);
            }
        }

        protected virtual void RegisterHostileAction(IMyPlayer player, TimeSpan truceDelay)
        {
            try
            {
                #region Sanity checks
                if (player == null)
                {
                    Grid.DebugWrite("RegisterHostileAction", "Error: Damager is null.");
                    return;
                }

                if (_ownerFaction == null)
                {
                    _ownerFaction = Grid.GetOwnerFaction();
                }

                //if (_ownerFaction == null || !_ownerFaction.IsNpc())
                //{
                //    Grid.DebugWrite("RegisterHostileAction", $"Error: {(_ownerFaction == null ? "can't find own faction" : "own faction isn't recognized as NPC.")}");
                //    return;
                //}
                #endregion

                IMyFaction hostileFaction = player.GetFaction();
                if (hostileFaction == null)
                {
                    Grid.DebugWrite("RegisterHostileAction", "Error: can't find damager's faction");
                    return;
                }
                if (hostileFaction == _ownerFaction)
                {
                    _ownerFaction.Kick(player);
                    return;
                }

                AISessionCore.DeclareWar(_ownerFaction, hostileFaction, truceDelay);
                if (_ownerFaction.IsLawful())
                {
                    AISessionCore.DeclareWar(Diplomacy.Police, hostileFaction, truceDelay);
                    AISessionCore.DeclareWar(Diplomacy.Army, hostileFaction, truceDelay);
                }
            }
            catch (Exception scrap)
            {
                LogError("RegisterHostileAction", scrap);
            }
        }

        protected virtual void RegisterHostileAction(IMyFaction hostileFaction, TimeSpan truceDelay)
        {
            try
            {
                if (hostileFaction == null)
                {
                    Grid.DebugWrite("RegisterHostileAction", "Error: can't find damager's faction");
                    return;
                }
                AISessionCore.DeclareWar(_ownerFaction, hostileFaction, truceDelay);
                if (_ownerFaction.IsLawful())
                {
                    AISessionCore.DeclareWar(Diplomacy.Police, hostileFaction, truceDelay);
                    AISessionCore.DeclareWar(Diplomacy.Army, hostileFaction, truceDelay);
                }
            }
            catch (Exception scrap)
            {
                LogError("RegisterHostileAction", scrap);
            }
        }

        protected List<Ingame.MyDetectedEntityInfo> LookAround(float radius, Func<Ingame.MyDetectedEntityInfo, bool> filter = null)
        {
            List<Ingame.MyDetectedEntityInfo> radarData = new List<Ingame.MyDetectedEntityInfo>();
            BoundingSphereD lookaroundSphere = new BoundingSphereD(GridPosition, radius);

            List<IMyEntity> entitiesAround = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref lookaroundSphere);
            entitiesAround.RemoveAll(x => x == Grid || GridPosition.DistanceTo(x.GetPosition()) < GridRadius * 1.5);

            long ownerId;
            if (_ownerFaction != null)
            {
                ownerId = _ownerFaction.FounderId;
                //Grid.DebugWrite("LookAround", "Found owner via faction owner");
                //LogWriter.WriteMessage($@"LookAround: Found owner via faction owner");
            }
            else
            {
                ownerId = RC.OwnerId;
                //Grid.DebugWrite("LookAround", "OWNER FACTION NOT FOUND, found owner via RC owner");
                //LogWriter.WriteMessage($@"LookAround: OWNER FACTION NOT FOUND, found owner via RC owner");
            }

            foreach (IMyEntity detectedEntity in entitiesAround)
            {
                Ingame.MyDetectedEntityInfo radarDetectedEntity = MyDetectedEntityInfoHelper.Create(detectedEntity as MyEntity, ownerId);
                if (radarDetectedEntity.Type == Ingame.MyDetectedEntityType.None || radarDetectedEntity.Type == Ingame.MyDetectedEntityType.Unknown) continue;
                if (filter == null || filter(radarDetectedEntity)) radarData.Add(radarDetectedEntity);
            }

            //DebugWrite("LookAround", $"Radar entities detected: {String.Join(" | ", RadarData.Select(x => $"{x.Name}"))}");
            return radarData;
        }

        protected List<Ingame.MyDetectedEntityInfo> LookForEnemies(float radius, bool ConsiderNeutralsAsHostiles = false, Func<Ingame.MyDetectedEntityInfo, bool> filter = null)
        {
            if (!ConsiderNeutralsAsHostiles)
                return LookAround(radius, x => x.IsHostile() && (filter == null || filter(x)));
            return LookAround(radius, x => x.IsNonFriendly() && (filter == null || filter(x)));
        }

        /// <summary>
        /// Returns distance from the grid to an object.
        /// </summary>
        protected float Distance(Ingame.MyDetectedEntityInfo target)
        {
            return (float)Vector3D.Distance(GridPosition, target.Position);
        }

        /// <summary>
        /// Returns distance from the grid to an object.
        /// </summary>
        protected float Distance(IMyEntity target)
        {
            return (float)Vector3D.Distance(GridPosition, target.GetPosition());
        }

        protected Vector3 RelVelocity(Ingame.MyDetectedEntityInfo target)
        {
            return target.Velocity - GridVelocity;
        }

        protected float RelSpeed(Ingame.MyDetectedEntityInfo target)
        {
            return (float)(target.Velocity - GridVelocity).Length();
        }

        protected Vector3 RelVelocity(IMyEntity target)
        {
            return target.Physics.LinearVelocity - GridVelocity;
        }

        protected float RelSpeed(IMyEntity target)
        {
            return (float)(target.Physics.LinearVelocity - GridVelocity).Length();
        }

        protected virtual List<IMyTerminalBlock> GetHackedBlocks()
        {
            List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> hackedBlocks = new List<IMyTerminalBlock>();

            Term.GetBlocks(terminalBlocks);

            foreach (IMyTerminalBlock block in terminalBlocks)
                if (block.IsBeingHacked) hackedBlocks.Add(block);

            return hackedBlocks;
        }

        protected virtual List<IMySlimBlock> GetDamagedBlocks()
        {
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            Grid.GetBlocks(blocks, x => x.CurrentDamage > 10);
            return blocks;
        }

        protected bool HasModdedThrusters => SpeedmoddedThrusters.Count > 0;
        protected List<IMyThrust> SpeedmoddedThrusters = new List<IMyThrust>();

        protected void ApplyThrustMultiplier(float thrustMultiplier)
        {
            DemultiplyThrusters();
            //foreach (IMyThrust thruster in Term.GetBlocksOfType<IMyThrust>(collect: x => x.IsOwnedByNPC(AllowNobody: false, CheckBuilder: true)))
            //{
            //    thruster.ThrustMultiplier = thrustMultiplier;
            //    thruster.OwnershipChanged += Thruster_OnOwnerChanged;
            //    SpeedmoddedThrusters.Add(thruster);
            //}
        }
        
        protected void DemultiplyThrusters()
        {
            if (!HasModdedThrusters) return;
            foreach (IMyThrust thruster in SpeedmoddedThrusters)
            {
                if (Math.Abs(thruster.ThrustMultiplier - 1) > 0) thruster.ThrustMultiplier = 1;
            }
            SpeedmoddedThrusters.Clear();
        }

        //private void Thruster_OnOwnerChanged(IMyTerminalBlock thrust)
        //{
        //    IMyThrust thruster = (IMyThrust) thrust;
        //    try
        //    {
        //        if (Math.Abs(Math.Abs(thruster.ThrustMultiplier - 1)) > 0) thruster.ThrustMultiplier = 1;
        //    }
        //    catch (Exception scrap)
        //    {
        //        Grid.DebugWrite("Thruster_OnOwnerChanged", $"{thrust.CustomName} OnOwnerChanged failed: {scrap.Message}");
        //    }
        //}

        protected abstract bool ParseSetup();

        public abstract void Main();

        public virtual void Shutdown()
        {
            Closed = true;
            if (HasModdedThrusters) DemultiplyThrusters();
            AISessionCore.RemoveDamageHandler(Grid);
        }

        public void LogError(string source, Exception scrap, string debugPrefix = "BotBase.")
        {
            Grid.LogError(debugPrefix + source, scrap);
        }

        protected virtual void OnAlert()
        {
            Alert?.Invoke();
        }
    }

    /*public sealed class InvalidBot : BotBase
    {
        static public readonly BotTypes BotType = BotTypes.None;

        public override bool Operable
        {
            get
            {
                return false;
            }
        }

        public InvalidBot(IMyCubeGrid Grid = null) : base(Grid)
        {
        }

        public override bool Init(IMyRemoteControl RC = null)
        {
            return false;
        }

        public override void Main()
        {
            // Empty
        }

        protected override bool ParseSetup()
        {
            return false;
        }
    }*/
}