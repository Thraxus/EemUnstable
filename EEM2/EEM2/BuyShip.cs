using System;
using System.Collections.Generic;
using Cheetah.AI;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Digi.Exploration
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MyProgrammableBlock), false)]
    public class BuyShipPb : MyGameLogicComponent
    {
        private bool _first = true;
        private bool _ignore;
        private long _lastSpawned;
        private byte _skip = 200;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void Close()
        {
            try
            {
                NeedsUpdate = MyEntityUpdateEnum.NONE;

                BuyShipMonitor.ShopPBs.Remove(this);
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if(_ignore)
                return;

            try
            {
                if(_first)
                {
                    if(MyAPIGateway.Session == null) // wait until session is ready
                        return;

                    _first = false;

                    if (Entity is IMyTerminalBlock block)
                        _ignore = (block.CubeGrid.Physics == null || ((MyEntity) block.CubeGrid).IsPreview ||
                                   !Constants.NPC_FACTIONS.Contains(block.GetOwnerFactionTag()));

                    if(!_ignore)
                        BuyShipMonitor.ShopPBs.Add(this);

                    return;
                }

                if (++_skip <= 30) return;
                {
                    _skip = 0;

                    long timeTicks = DateTime.UtcNow.Ticks;

                    if(timeTicks < _lastSpawned)
                        return;

                    IMyTerminalBlock block = Entity as IMyTerminalBlock;

                    if(block != null && block.DetailedInfo == null)
                        return;

                    const string prefix = Constants.TradeEchoPrefix;

                    if (block != null)
                    {
                        int startIndex = block.DetailedInfo.IndexOf(prefix, StringComparison.Ordinal);

                        if(startIndex == -1)
                            return;

                        startIndex += prefix.Length;
                        int endIndex = block.DetailedInfo.IndexOf(prefix, startIndex, StringComparison.Ordinal);
                        string prefabName = (endIndex == -1 ? block.DetailedInfo.Substring(startIndex) : block.DetailedInfo.Substring(startIndex, endIndex - startIndex)).Trim();

                        if(string.IsNullOrEmpty(prefabName))
                            return;

                        _lastSpawned = timeTicks + (TimeSpan.TicksPerSecond * Constants.TradeDelaySeconds);

                        MyPrefabDefinition def = MyDefinitionManager.Static.GetPrefabDefinition(prefabName);

                        if(def?.CubeGrids == null)
                        {
                            if (def != null)
                            {
                                MyDefinitionManager.Static.ReloadPrefabsFromFile(def.PrefabPath);
                                def = MyDefinitionManager.Static.GetPrefabDefinition(def.Id.SubtypeName);
                            }
                        }

                        if(def?.CubeGrids == null)
                        {
                            Log.Error("Prefab '" + prefabName + "' not found!");
                            return;
                        }

                        Vector3D position = GetSpawnPosition();
                        BoundingSphereD sphere = new BoundingSphereD(position, Constants.SpawnAreaRadius);
                        List<IMyEntity> ents = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere);
                        MyCubeGrid grid = (MyCubeGrid)block.CubeGrid;
                        MyCubeGrid biggestInGroup = grid.GetBiggestGridInGroup();

                        foreach(IMyEntity myEntity in ents)
                        {
                            MyEntity ent = (MyEntity) myEntity;
                            // don't care about floating objects or asteroids/planets or physicsless or client side only entities blocking spawn zone
                            if(ent is IMyFloatingObject || ent is IMyVoxelBase || ent.Physics == null || ent.IsPreview)
                                continue;

                            if(ent.EntityId == block.CubeGrid.EntityId)
                                continue;

                            if(ent is MyCubeGrid g && g.GetBiggestGridInGroup() == biggestInGroup)
                                continue;

                            IMyProgrammableBlock pb = (IMyProgrammableBlock)Entity;

                            if (pb.TryRun(Constants.PbargFailPositionblocked)) return;
                            Log.Info("WARNING: PB couldn't ran with arg '" + Constants.PbargFailPositionblocked + "' for some reason.");

                            Vector3D camPos = MyAPIGateway.Session.Camera.WorldMatrix.Translation;

                            if(Vector3D.DistanceSquared(camPos, position) < (Constants.SpawnFailNotifyDistance * Constants.SpawnFailNotifyDistance))
                                MyAPIGateway.Utilities.ShowNotification("Can't buy ship - jump in position blocked by something!", 5000, MyFontEnum.Red);
                            return;
                        }

                        // spawn and change inventories only server side as it is synchronized automatically
                        if(MyAPIGateway.Multiplayer.IsServer)
                        {
                            const SpawningOptions flags = SpawningOptions.SetNeutralOwner; // | SpawningOptions.RotateFirstCockpitTowardsDirection;
                            List<IMyCubeGrid> grids = new List<IMyCubeGrid>();
                            MyAPIGateway.PrefabManager.SpawnPrefab(grids, prefabName, position, block.WorldMatrix.Backward, block.WorldMatrix.Up, block.CubeGrid.Physics.LinearVelocity, Vector3.Zero, null, flags, false);

                            // purge tagged inventories from PB's grid
                            ListReader<MyCubeBlock> blocks = grid.GetFatBlocks();

                            foreach(MyCubeBlock b in blocks)
                            {
                                if (!b.TryGetInventory(out MyInventoryBase inv) ||
                                    !Constants.NPC_FACTIONS.Contains(b.GetOwnerFactionTag())) continue;
                                if(!(b is IMyTerminalBlock t) || !t.CustomName.Contains(Constants.PostspawnEmptyinventoryTag))
                                    continue;
                                inv.GetItems().Clear();
                            }
                        }

                        BuyShipMonitor.Spawned.Add(new SpawnData()
                        {
                            Position = position,
                            ExpireTime = _lastSpawned,
                        });
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        public Vector3D GetSpawnPosition()
        {
            IMyTerminalBlock b = (IMyTerminalBlock)Entity;
            MatrixD m = b.WorldMatrix;
            return m.Translation + m.Up * Constants.SpawnRelativeOffsetUp + m.Left * Constants.SpawnRelativeOffsetLeft + m.Forward * Constants.SpawnRelativeOffsetForward;
        }
    }

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class BuyShipMonitor : MySessionComponentBase
    {
        private bool _init;
        private byte _skip;

        public static readonly List<SpawnData> Spawned = new List<SpawnData>();
        public static readonly List<BuyShipPb> ShopPBs = new List<BuyShipPb>();

        private static readonly MyStringId MaterialWhitedot = MyStringId.GetOrCompute("WhiteDot");
        private static readonly MyStringId MaterialSquare = MyStringId.GetOrCompute("Square");

        public override void UpdateAfterSimulation()
        {
            try
            {
                if(!_init)
                {
                    if(MyAPIGateway.Session == null)
                        return;

                    _init = true;

                    MyAPIGateway.Entities.OnEntityAdd += EntityAdded;
                }

                if(++_skip > 60)
                {
                    _skip = 0;
                    long timeTicks = DateTime.UtcNow.Ticks;

                    for(int i = Spawned.Count - 1; i >= 0; i--) // loop backwards to be able to remove elements mid-loop
                    {
                        if(Spawned[i].ExpireTime < timeTicks)
                            Spawned.RemoveAt(i);
                    }
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        public override void Draw()
        {
            try
            {
                if(!_init)
                    return;

                if (MyAPIGateway.Session.OnlineMode != MyOnlineModeEnum.OFFLINE) return;
                foreach(BuyShipPb logic in ShopPBs)
                {
                    IMyTerminalBlock block = (IMyTerminalBlock)logic.Entity;

                    if (!block.ShowOnHUD) continue;
                    Vector3D target = logic.GetSpawnPosition();
                    Vector3D camPos = MyAPIGateway.Session.Camera.WorldMatrix.Translation;
                    Vector3D dir = target - camPos;
                    float dist = (float)dir.Normalize();

                    if(dist > 300)
                        continue;

                    Vector3D pos = camPos + (dir * 0.05);
                    MyTransparentGeometry.AddPointBillboard(MaterialWhitedot, Color.Purple * 0.75f, pos, 0.01f * (1f / dist), 0);

                    MatrixD m = block.WorldMatrix;
                    m.Translation = target;
                    Color c = Color.Orange * 0.5f;
                    MySimpleObjectDraw.DrawTransparentSphere(ref m, Constants.SpawnAreaRadius, ref c, MySimpleObjectRasterizer.Wireframe, 20, null, MaterialSquare, 0.01f);
                }
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        protected override void UnloadData()
        {
            ShopPBs.Clear();
            Spawned.Clear();

            if (!_init) return;
            _init = false;
            MyAPIGateway.Entities.OnEntityAdd -= EntityAdded;
        }

        private static void EntityAdded(IMyEntity ent)
        {
            try
            {
                if(Spawned.Count == 0)
                    return;

                if(!(ent is MyCubeGrid grid) || grid.IsPreview || grid.Physics == null)
                    return;

                MyAPIGateway.Utilities.InvokeOnGameThread(delegate
                {
                    long timeTicks = DateTime.UtcNow.Ticks;
                    BoundingSphereD vol = grid.PositionComp.WorldVolume;
                    double radSq = vol.Radius;
                    radSq *= radSq;
                    Vector3D center = grid.PositionComp.WorldAABB.Center;

                    for(int i = Spawned.Count - 1; i >= 0; i--) // loop backwards to be able to remove elements mid-loop
                    {
                        if(Spawned[i].ExpireTime < timeTicks)
                        {
                            Spawned.RemoveAt(i); // expired
                            continue;
                        }

                        Vector3D pos = Spawned[i].Position;

                        if(Vector3D.DistanceSquared(center, pos) <= radSq)
                        {
                            Spawned.RemoveAt(i); // no longer need this position

                            // create the warp effect

                            if(MyParticlesManager.TryCreateParticleEffect(Constants.SpawnEffectName, out MyParticleEffect effect))
                            {
                                MatrixD em = grid.WorldMatrix;
                                em.Translation = center;
                                effect.WorldMatrix = em;
                                effect.UserScale = (float)vol.Radius * Constants.SpawnEffectScale;
                            }
                            else
                            {
                                Log.Error("Couldn't spawn particle effect: " + Constants.SpawnEffectName);
                            }

                            break;
                        }
                    }
                });
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }
    }

    public struct SpawnData
    {
        public Vector3D Position;
        public long ExpireTime;
    }
}