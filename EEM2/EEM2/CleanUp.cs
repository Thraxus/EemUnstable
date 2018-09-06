using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Digi.Exploration
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class EemCleanUp : MySessionComponentBase
    {
        public override void LoadData()
        {
            Log.SetUp("EEM", 531659576); // mod name and workshop ID
        }

        private bool _init;
        private int _skip = SkipUpdates;
        private const int SkipUpdates = 100;

        public static int RangeSq = -1;
        public static readonly List<IMyPlayer> Players = new List<IMyPlayer>();
        public static readonly HashSet<IMyCubeGrid> Grids = new HashSet<IMyCubeGrid>();
        public static readonly List<IMySlimBlock> Blocks = new List<IMySlimBlock>(); // never filled

        public void Init()
        {
            _init = true;
            Log.Init();

            MyAPIGateway.Session.SessionSettings.MaxDrones = Constants.FORCE_MAX_DRONES;
        }

        protected override void UnloadData()
        {
            _init = false;
            Log.Close();

            Players.Clear();
            Grids.Clear();
            Blocks.Clear();
        }

        public override void UpdateBeforeSimulation()
        {
            if(!MyAPIGateway.Multiplayer.IsServer) // only server-side/SP
                return;

            if(!_init)
            {
                if(MyAPIGateway.Session == null)
                    return;

                Init();
            }

            if (++_skip < SkipUpdates) return;
            try
            {
                _skip = 0;

                // the range used to check player distance from ships before removing them
                RangeSq = Math.Max(MyAPIGateway.Session.SessionSettings.ViewDistance, Constants.CLEANUP_MIN_RANGE);
                RangeSq *= RangeSq;

                Players.Clear();
                MyAPIGateway.Players.GetPlayers(Players);

                Log.Info("player list updated; view range updated: " + Math.Round(Math.Sqrt(RangeSq), 1));
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        public static void GetAttachedGrids(IMyCubeGrid grid)
        {
            Grids.Clear();
            RecursiveGetAttachedGrids(grid);
        }

        private static void RecursiveGetAttachedGrids(IMyCubeGrid grid)
        {
            grid.GetBlocks(Blocks, GetAttachedGridsLoopBlocks);
        }

        private static bool GetAttachedGridsLoopBlocks(IMySlimBlock slim) // should always return false!
        {
            var block = slim.FatBlock;

            if(block == null)
                return false;

            IMyMotorStator rotorBase = block as IMyMotorStator;
            if (rotorBase != null)
            {
                IMyCubeGrid otherGrid = rotorBase.TopGrid;

                if (otherGrid != null && !Grids.Contains(otherGrid))
                {
                    Grids.Add(otherGrid);
                    RecursiveGetAttachedGrids(otherGrid);
                }

                return false;
            }

            IMyMotorRotor rotorTop = block as IMyMotorRotor;
            if (rotorTop != null)
            {
                IMyCubeGrid otherGrid = rotorTop.Base?.CubeGrid;

                if (otherGrid == null || Grids.Contains(otherGrid)) return false;
                Grids.Add(otherGrid);
                RecursiveGetAttachedGrids(otherGrid);

                return false;
            }

            IMyPistonBase pistonBase = block as IMyPistonBase;
            if (pistonBase != null)
            {
                IMyCubeGrid otherGrid = pistonBase.TopGrid;

                if (otherGrid == null || Grids.Contains(otherGrid)) return false;
                Grids.Add(otherGrid);
                RecursiveGetAttachedGrids(otherGrid);

                return false;
            }

            IMyPistonTop pistonTop = block as IMyPistonTop;
            if (pistonTop == null) return false;
            {
                IMyCubeGrid otherGrid = pistonTop.Piston?.CubeGrid;

                if (otherGrid == null || Grids.Contains(otherGrid)) return false;
                Grids.Add(otherGrid);
                RecursiveGetAttachedGrids(otherGrid);

                return false;
            }

        }
    }
    
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_RemoteControl), true)]
    public class EemRc : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }
        
        public override void UpdateAfterSimulation100()
        {
            try
            {
                if(!MyAPIGateway.Multiplayer.IsServer) // only server-side/SP
                    return;

                IMyRemoteControl rc = (IMyRemoteControl)Entity;
                IMyCubeGrid grid = rc.CubeGrid;

                if(grid.Physics == null || !rc.IsWorking || !Constants.NPC_FACTIONS.Contains(rc.GetOwnerFactionTag()))
                {
                    Log.Info(grid.DisplayName + " (" + grid.EntityId + " @ " + grid.WorldMatrix.Translation + ") is not valid; " + (grid.Physics == null ? "Phys=null" : "Phys OK") + "; " + (rc.IsWorking ? "RC OK" : "RC Not working!") + "; " + (!Constants.NPC_FACTIONS.Contains(rc.GetOwnerFactionTag()) ? "Owner faction tag is not in NPC list (" + rc.GetOwnerFactionTag() + ")" : "Owner Faction OK"));

                    return;
                }

                if(!rc.CustomData.Contains(Constants.CLEANUP_RC_TAG))
                {
                    Log.Info(grid.DisplayName + " (" + grid.EntityId + " @ " + grid.WorldMatrix.Translation + ") RC does not contain the " + Constants.CLEANUP_RC_TAG + "tag!");

                    return;
                }

                if(Constants.CLEANUP_RC_EXTRA_TAGS.Length > 0)
                {
                    bool hasExtraTag = Constants.CLEANUP_RC_EXTRA_TAGS.Any(tag => rc.CustomData.Contains(tag));

                    if(!hasExtraTag)
                    {
                        Log.Info(grid.DisplayName + " (" + grid.EntityId + " @ " + grid.WorldMatrix.Translation + ") RC does not contain one of the extra tags!");

                        return;
                    }
                }

                Log.Info("Checking RC '" + rc.CustomName + "' from grid '" + grid.DisplayName + "' (" + grid.EntityId + ") for any nearby players...");

                int rangeSq = EemCleanUp.RangeSq;
                Vector3D gridCenter = grid.WorldAABB.Center;

                if(rangeSq <= 0)
                {
                    Log.Info("- WARNING: Range not assigned yet, ignoring grid for now.");

                    return;
                }

                // check if any player is within range of the ship
                foreach(IMyPlayer player in EemCleanUp.Players)
                {
                    if (!(Vector3D.DistanceSquared(player.GetPosition(), gridCenter) <= rangeSq)) continue;
                    Log.Info(" - player '" + player.DisplayName + "' is within " + Math.Round(Math.Sqrt(rangeSq), 1) + "m of it, not removing.");

                    return;
                }

                Log.Info(" - no player is within " + Math.Round(Math.Sqrt(rangeSq), 1) + "m of it, removing...");

                Log.Info("NPC ship '" + grid.DisplayName + "' (" + grid.EntityId + ") removed.");

                EemCleanUp.GetAttachedGrids(grid); // this gets all connected grids and places them in Exploration.grids (it clears it first)

                foreach(IMyCubeGrid g in EemCleanUp.Grids)
                {
                    g.Close(); // this only works server-side
                    Log.Info("  - subgrid '" + g.DisplayName + "' (" + g.EntityId + ") removed.");
                }

                grid.Close(); // this only works server-side
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }
    }
}