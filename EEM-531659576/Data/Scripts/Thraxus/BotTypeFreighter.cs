using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Helpers;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
//using IMyJumpDrive = Sandbox.ModAPI.IMyJumpDrive;
using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;
using Ingame = Sandbox.ModAPI.Ingame;


namespace Eem.Thraxus
{
	public sealed class BotTypeFreighter : BotBase
	{
		public static readonly BotTypeBase BotType = BotTypeBase.Freighter;

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
				if (FleeTriggerDistance == default(float)) FleeTriggerDistance = 1000;
				if (FleeSpeedRatio == default(float)) FleeSpeedRatio = 1.0f;
				if (FleeSpeedCap == default(float)) FleeSpeedCap = 300;
			}
		}

		private bool IsFleeing { get; set; }

		private bool FleeTimersTriggered { get; set; }

		public BotTypeFreighter(IMyCubeGrid grid) : base(grid)
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
			float speed = (float)Rc.GetShipSpeed();
			Vector3D velocity = Rc.GetShipVelocities().LinearVelocity;
			if (speed > 5)
			{
				Vector3D endpoint = GridPosition + (Vector3D.Normalize(velocity) * 1000000);
			}
			else
			{
				Vector3D endpoint = GridPosition + (Rc.WorldMatrix.Forward * 1000000);
			}

			if (Math.Abs(_freighterSetup.CruiseSpeed - default(float)) > 0)
				(Rc as MyRemoteControl)?.SetAutoPilotSpeedLimit(_freighterSetup.CruiseSpeed);
			else
				(Rc as MyRemoteControl)?.SetAutoPilotSpeedLimit(speed > 5 ? speed : 30);

			(Rc as MyRemoteControl)?.SetCollisionAvoidance(true);
		}

		private void DamageHandler(IMySlimBlock block, MyDamageInformation damage)
		{
			try
			{
				IMyPlayer damager;
				ReactOnDamage(damage, out damager);
				if (damager == null) return;
				IsFleeing = true;
				Flee();
			}
			catch (Exception scrap)
			{
				Grid.LogError("DamageHandler", scrap);
			}
		}

		public override void Main()
		{
			if (IsFleeing) Flee();
			else if (!_freighterSetup.FleeOnlyWhenDamaged)
			{
				List<Ingame.MyDetectedEntityInfo> enemiesAround = LookForEnemies(_freighterSetup.FleeTriggerDistance, considerNeutralsAsHostiles: true);
				if (enemiesAround.Count <= 0) return;
				IsFleeing = true;
				Flee(enemiesAround);
			}
		}

		private void Flee(List<Ingame.MyDetectedEntityInfo> radarData = null)
		{
			try
			{
				if (!IsFleeing) return;

				try
				{
					if (!FleeTimersTriggered) TriggerFleeTimers();

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
									//ShowIngameMessage.ShowMessage($"Fleeing from '{enemyEntity.DisplayName}'. Distance: {Math.Round(GridPosition.DistanceTo(closestEnemy.Position))}m; FleeTriggerDistance: {_freighterSetup.FleeTriggerDistance}");
									Vector3D fleePoint = GridPosition.InverseVectorTo(closestEnemy.Position, 100 * 1000);
									//ShowIngameMessage.ShowMessage($"Flee point: {fleePoint} which is {GridPosition.DistanceTo(fleePoint)}m from me and enemy {enemyEntity.DisplayName}");
									//ShowIngameMessage.ShowMessage($"Fleeing at: {DetermineFleeSpeed()}m/s...");
									Rc.AddWaypoint(fleePoint, "Flee Point");
									(Rc as MyRemoteControl)?.ChangeFlightMode(Ingame.FlightMode.OneWay);
									(Rc as MyRemoteControl)?.SetAutoPilotSpeedLimit(DetermineFleeSpeed());
									Rc.SetAutoPilotEnabled(true);
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
		//    List<IMyJumpDrive> jumpDrives = Term.GetBlocksOfType<IMyJumpDrive>(collect: x => x.IsWorking);

		//    if (jumpDrives.Count > 0) jumpDrives.First().Jump(false);
		//}

		private void TriggerFleeTimers()
		{
			if (FleeTimersTriggered) return;
			
			List<IMyTimerBlock> fleeTimers = new List<IMyTimerBlock>();
			Term.GetBlocksOfType(fleeTimers, x => x.IsFunctional && x.Enabled && (x.CustomName.Contains("Flee") || x.CustomData.Contains("Flee")));
			//Grid.DebugWrite("TriggerFleeTimers", $"Flee timers found: {fleeTimers.Count}.{(fleeTimers.Count > 0 ? " Trying to activate..." : "")}");
			//ShowIngameMessage.ShowMessage($"Flee timers found: {fleeTimers.Count}.{(fleeTimers.Count > 0 ? " Trying to activate..." : "")}");
			foreach (IMyTimerBlock timer in fleeTimers)
				timer.Trigger();

			FleeTimersTriggered = true;
		}

		private float DetermineFleeSpeed()
		{
			//ShowIngameMessage.ShowMessage($"Flee speed cap: {_freighterSetup.FleeSpeedCap} -- Ratio: {_freighterSetup.FleeSpeedRatio} -- GetSpeedCap: {Rc.GetSpeedCap()}" );
			return Math.Min(_freighterSetup.FleeSpeedCap, _freighterSetup.FleeSpeedRatio * Rc.GetSpeedCap());
		}

		protected override bool ParseSetup()
		{
			if (ReadBotType(Rc) != BotType) return false;
			List<string> customData = Rc.CustomData.Trim().Replace("\r\n", "\n").Split('\n').ToList();
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
							DebugWrite("ParseSetup", "AI setup error: FleeOnlyWhenDamaged cannot be parsed");
							return false;
						}
						break;
					case "FleeTriggerDistance":
						if (!float.TryParse(data[1], out _freighterSetup.FleeTriggerDistance))
						{
							DebugWrite("ParseSetup", "AI setup error: FleeTriggerDistance cannot be parsed");
							return false;
						}
						break;
					case "FleeSpeedRatio":
						if (!float.TryParse(data[1], out _freighterSetup.FleeSpeedRatio))
						{
							DebugWrite("ParseSetup", "AI setup error: FleeSpeedRatio cannot be parsed");
							return false;
						}
						break;
					case "FleeSpeedCap":
						if (!float.TryParse(data[1], out _freighterSetup.FleeSpeedCap))
						{
							DebugWrite("ParseSetup", "AI setup error: FleeSpeedCap cannot be parsed");
							return false;
						}
						break;
					case "CruiseSpeed":
						if (!float.TryParse(data[1], out _freighterSetup.CruiseSpeed))
						{
							DebugWrite("ParseSetup", "AI setup error: CruiseSpeed cannot be parsed");
							return false;
						}
						break;
					default:
						DebugWrite("ParseSetup", $"AI setup error: Cannot parse '{dataLine}'");
						return false;
				}
			}
			_freighterSetup.Default();
			return true;
		}
	}
}