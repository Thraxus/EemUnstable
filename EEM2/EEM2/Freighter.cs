using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
//using IMyJumpDrive = Sandbox.ModAPI.IMyJumpDrive;
using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;
using Ingame = Sandbox.ModAPI.Ingame;


namespace Cheetah.AI
{
    public sealed class FreighterBot : BotBase
    {
        public static readonly BotTypes BotType = BotTypes.Freighter;
        private FreighterSettings _freighterSetup;

        private struct FreighterSettings
        {
            public bool FleeOnlyWhenDamaged;
            public float FleeTriggerDistance;
            public float FleeSpeedRatio;
            public float FleeSpeedCap;
            public float CruiseSpeed;

            public void Default()
            {
                if (FleeOnlyWhenDamaged == default(bool)) FleeOnlyWhenDamaged = false;
                if (Math.Abs(FleeTriggerDistance - default(float)) < 0) FleeTriggerDistance = 1000;
                if (Math.Abs(FleeSpeedRatio - default(float)) < 0) FleeSpeedRatio = 1.0f;
                if (Math.Abs(FleeSpeedCap - default(float)) < 0) FleeSpeedCap = 300;
            }
        }
        private bool _isFleeing;
        private bool _fleeTimersTriggered;
        
        public FreighterBot(IMyCubeGrid grid) : base(grid)
        {
        }

        public override bool Init(IMyRemoteControl rc = null)
        {
            if (!base.Init(rc)) return false;
            OnDamaged += DamageHandler;
            OnBlockPlaced += BlockPlacedHandler;

            SetFlightPath();

            Update |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            return true;
        }

        private void SetFlightPath()
        {
            float speed = (float)RC.GetShipSpeed();
            if (Math.Abs(_freighterSetup.CruiseSpeed - default(float)) > 0)
                (RC as MyRemoteControl)?.SetAutoPilotSpeedLimit(_freighterSetup.CruiseSpeed);
            else
                (RC as MyRemoteControl)?.SetAutoPilotSpeedLimit(speed > 5 ? speed : 30);
            (RC as MyRemoteControl)?.SetCollisionAvoidance(true);
        }

        private void DamageHandler(IMySlimBlock block, MyDamageInformation damage)
        {
            try
            {
                IMyPlayer damager;
                ReactOnDamage(block, damage, CalmdownTime, out damager);
                if (damager == null) return;
                _isFleeing = true;
                Flee();
            }
            catch (Exception scrap)
            {
                Grid.LogError("DamageHandler", scrap);
            }
        }

        public override void Main()
        {
            if (_isFleeing) Flee();
            else if (!_freighterSetup.FleeOnlyWhenDamaged)
            {
                List<Ingame.MyDetectedEntityInfo> enemiesAround = LookForEnemies(_freighterSetup.FleeTriggerDistance, true);
                if (enemiesAround.Count <= 0) return;
                _isFleeing = true;
                Flee(enemiesAround);
            }
        }

        private void Flee(List<Ingame.MyDetectedEntityInfo> radarData = null)
        {
            try
            {
                if (!_isFleeing) return;

                try
                {
                    if (!_fleeTimersTriggered) TriggerFleeTimers();

                    try
                    {
                        if (radarData == null) radarData = LookForEnemies(_freighterSetup.FleeTriggerDistance);
                        if (radarData.Count == 0) return;

                        try
                        {
                            Ingame.MyDetectedEntityInfo closestEnemy = radarData.OrderBy(x => GridPosition.DistanceTo(x.Position)).FirstOrDefault();

                            if (closestEnemy.IsEmpty())
                            {
                                Grid.DebugWrite("Flee", "Cannot find closest hostile");
                                return;
                            }

                            try
                            {
                                IMyEntity enemyEntity = MyAPIGateway.Entities.GetEntityById(closestEnemy.EntityId);
                                if (enemyEntity == null)
                                {
                                    Grid.DebugWrite("Flee", "Cannot find enemy entity from closest hostile ID");
                                    return;
                                }

                                try
                                {
                                    //Grid.DebugWrite("Flee", $"Fleeing from '{EnemyEntity.DisplayName}'. Distance: {Math.Round(GridPosition.DistanceTo(ClosestEnemy.Position))}m; FleeTriggerDistance: {FreighterSetup.FleeTriggerDistance}");

                                    Vector3D fleePoint = GridPosition.InverseVectorTo(closestEnemy.Position, 100 * 1000);
                                    RC.AddWaypoint(fleePoint, "Flee Point");
                                    (RC as MyRemoteControl)?.ChangeFlightMode(Ingame.FlightMode.OneWay);
                                    (RC as MyRemoteControl)?.SetAutoPilotSpeedLimit(DetermineFleeSpeed());
                                    RC.SetAutoPilotEnabled(true);
                                }
                                catch (Exception scrap)
                                {
                                    Grid.LogError("Flee.AddWaypoint", scrap);
                                }
                            }
                            catch (Exception scrap)
                            {
                                Grid.LogError("Flee.LookForEnemies.GetEntity", scrap);
                            }
                        }
                        catch (Exception scrap)
                        {
                            Grid.LogError("Flee.LookForEnemies.Closest", scrap);
                        }
                    }
                    catch (Exception scrap)
                    {
                        Grid.LogError("Flee.LookForEnemies", scrap);
                    }
                }
                catch (Exception scrap)
                {
                    Grid.LogError("Flee.TriggerTimers", scrap);
                }
            }
            catch (Exception scrap)
            {
                Grid.LogError("Flee", scrap);
            }
        }

        //private void JumpAway()
        //{
        //    List<IMyJumpDrive> jumpDrives = Term.GetBlocksOfType<IMyJumpDrive>(x => x.IsWorking);

        //    if (jumpDrives.Count > 0) jumpDrives.First().Jump(false);
        //}

        private void TriggerFleeTimers()
        {
            if (_fleeTimersTriggered) return;

            List<IMyTimerBlock> fleeTimers = new List<IMyTimerBlock>();
            Term.GetBlocksOfType(fleeTimers, x => x.IsFunctional && x.Enabled && (x.CustomName.Contains("Flee") || x.CustomData.Contains("Flee")));
            Grid.DebugWrite("TriggerFleeTimers", $"Flee timers found: {fleeTimers.Count}.{(fleeTimers.Count > 0 ? " Trying to activate..." : "")}");
            foreach (IMyTimerBlock timer in fleeTimers)
                timer.Trigger();

            _fleeTimersTriggered = true;
        }

        private float DetermineFleeSpeed()
        {
            return Math.Min(_freighterSetup.FleeSpeedCap, _freighterSetup.FleeSpeedRatio * RC.GetSpeedCap());
        }

        protected override bool ParseSetup()
        {
            if (ReadBotType(RC) != BotType) return false;

            List<string> customData = RC.CustomData.Trim().Replace("\r\n", "\n").Split('\n').ToList();
            foreach (string dataLine in customData)
            {
                if (dataLine.Contains("EEM_AI")) continue;
                if (dataLine.Contains("Type")) continue;
                string[] data = dataLine.Trim().Split(':');
                data[1] = data[1].Trim();
                switch (data[0].Trim())
                {
                    case "Faction":
                        break;
                    case "FleeOnlyWhenDamaged":
                        if (!bool.TryParse(data[1], out _freighterSetup.FleeOnlyWhenDamaged))
                        {
                            WriteToDebug("ParseSetup", "AI setup error: FleeOnlyWhenDamaged cannot be parsed");
                            return false;
                        }
                        break;
                    case "FleeTriggerDistance":
                        if (!float.TryParse(data[1], out _freighterSetup.FleeTriggerDistance))
                        {
                            WriteToDebug("ParseSetup", "AI setup error: FleeTriggerDistance cannot be parsed");
                            return false;
                        }
                        break;
                    case "FleeSpeedRatio":
                        if (!float.TryParse(data[1], out _freighterSetup.FleeSpeedRatio))
                        {
                            WriteToDebug("ParseSetup", "AI setup error: FleeSpeedRatio cannot be parsed");
                            return false;
                        }
                        break;
                    case "FleeSpeedCap":
                        if (!float.TryParse(data[1], out _freighterSetup.FleeSpeedCap))
                        {
                            WriteToDebug("ParseSetup", "AI setup error: FleeSpeedCap cannot be parsed");
                            return false;
                        }
                        break;
                    case "CruiseSpeed":
                        if (!float.TryParse(data[1], out _freighterSetup.CruiseSpeed))
                        {
                            WriteToDebug("ParseSetup", "AI setup error: CruiseSpeed cannot be parsed");
                            return false;
                        }
                        break;
                    default:
                        WriteToDebug("ParseSetup", $"AI setup error: Cannot parse '{dataLine}'");
                        return false;
                }
            }

            _freighterSetup.Default();
            return true;
        }

        private void WriteToDebug(string source, string message)
        {
            DebugWrite(source, message, Constants.FighterDebugPrefix);
        }
    }
}