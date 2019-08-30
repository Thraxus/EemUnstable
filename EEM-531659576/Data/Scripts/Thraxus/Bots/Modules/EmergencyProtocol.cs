using System;
using System.Collections.Generic;
using System.Threading;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;
using VRageMath;
using IMyDoor = Sandbox.ModAPI.IMyDoor;
using IMyLargeTurretBase = Sandbox.ModAPI.IMyLargeTurretBase;

namespace Eem.Thraxus.Bots.Modules
{
	public class EmergencyProtocol : LogBaseEvent
	{
		private bool _alertEnabled;

		private readonly long _gridOwnerId;

		private readonly MyCubeGrid _thisGrid;

		private readonly List<GridAirVents> _gridAirVentSettings;
		private readonly List<GridDoors> _gridDoorSettings;
		private readonly List<GridGravityGenerators> _gridGravityGenerators;
		private readonly List<GridSphericalGravityGenerators> _gridSphericalGravityGenerators;
		private readonly List<GridTimers> _gridTimers;
		private readonly List<GridTurrets> _gridTurretSettings;

		public EmergencyProtocol(MyCubeGrid myCubeGrid, long ownerId)
		{
			// TODO 08.28.2019: Feature complete at this point as far as functionality.  Need to add in custom data scanning to make sure all items are parsed properly.				
			// TODO					An example of this is the term "alert" on a timer telling me it's one i need to track.  No custom data, no timer added to the alert list
			// TODO					Timers may need to come in pairs; one for on, one for off.
			// 
			// TODO Timers for existing flavor emergency conditions (possibly scan for settings related to what this code controls now and remove it)
			//		Example from Helios: [HELIOS|Tme] [Alert_On] Timer Alarm is the alert timer, trigger for on, trigger for off?  need to research.
			// TODO Antenna for custom drone spawning for special conditions / alert if enemy seen within some range - may move this idea to a separate module though

			_thisGrid = myCubeGrid;
			_gridOwnerId = ownerId;
			
			_gridAirVentSettings = new List<GridAirVents>();
			_gridDoorSettings = new List<GridDoors>();
			_gridGravityGenerators = new List<GridGravityGenerators>();
			_gridSphericalGravityGenerators = new List<GridSphericalGravityGenerators>();
			_gridTimers = new List<GridTimers>();
			_gridTurretSettings = new List<GridTurrets>();
		}

		private enum CubeType
		{
			AirVent, Door, GravityGenerator, SphericalGravityGenerator, Timer, Turret
		}

		private enum EmergencySetting
		{
			PeaceTime, Wartime
		}

		private struct AirVentSettings
		{
			public readonly bool Depressurize;
			public readonly bool Enabled;

			public AirVentSettings(bool enabled, bool depressurize)
			{
				Enabled = enabled;
				Depressurize = depressurize;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled} | {Depressurize}";
			}
		}

		private struct DoorSettings
		{
			public readonly bool Enabled;
			public readonly bool IsClosed;

			public DoorSettings(bool enabled, bool isClosed)
			{
				Enabled = enabled;
				IsClosed = isClosed;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled} {IsClosed}";
			}
		}

		private struct GravityGeneratorSettings
		{
			public readonly bool Enabled;
			public readonly Vector3 FieldSize;
			public readonly float FieldStrength;

			public GravityGeneratorSettings(bool enabled, Vector3 fieldSize, float fieldStrength)
			{
				Enabled = enabled;
				FieldSize = fieldSize;
				FieldStrength = fieldStrength;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled} | {FieldSize} | {FieldStrength}";
			}
		}

		private struct SphericalGravityGeneratorSettings
		{
			public readonly bool Enabled;
			public readonly float FieldSize;
			public readonly float FieldStrength;

			public SphericalGravityGeneratorSettings(bool enabled, float fieldSize, float fieldStrength)
			{
				Enabled = enabled;
				FieldSize = fieldSize;
				FieldStrength = fieldStrength;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled} | {FieldSize} | {FieldStrength}";
			}
		}

		private struct TimerSettings
		{
			public readonly bool Enabled;

			public TimerSettings(bool enabled)
			{
				Enabled = enabled;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled}";
			}
		}

		private struct TurretSettings
		{
			public readonly bool Enabled;
			public readonly bool EnableIdleRotation;
			public readonly bool TargetCharacters;
			public readonly bool TargetLargeShips;
			public readonly bool TargetMeteors;
			public readonly bool TargetMissiles;
			public readonly bool TargetNeutrals;
			public readonly bool TargetSmallShips;
			public readonly bool TargetStations;

			public readonly float Range;

			public TurretSettings(bool enabled, bool enableIdleRotation, bool targetCharacters, bool targetLargeShips, bool targetMeteors, bool targetMissiles, bool targetNeutrals, bool targetSmallShips, bool targetStations, float range)
			{
				Enabled = enabled;
				EnableIdleRotation = enableIdleRotation;
				TargetCharacters = targetCharacters;
				TargetLargeShips = targetLargeShips;
				TargetMeteors = targetMeteors;
				TargetMissiles = targetMissiles;
				TargetNeutrals = targetNeutrals;
				TargetSmallShips = targetSmallShips;
				TargetStations = targetStations;
				Range = range;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled} | {EnableIdleRotation} | {TargetCharacters} | {TargetLargeShips} | {TargetMeteors} | {TargetMissiles} | {TargetSmallShips} | {TargetStations} | {Range}";
			}
		}

		private struct GridAirVents
		{
			public readonly IMyAirVent AirVent;
			public readonly AirVentSettings WarTimeSettings;
			public readonly AirVentSettings PeaceTimeSettings;

			public GridAirVents(IMyAirVent airVent, AirVentSettings warTimeSettings, AirVentSettings peaceTimeSettings)
			{
				AirVent = airVent;
				WarTimeSettings = warTimeSettings;
				PeaceTimeSettings = peaceTimeSettings;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{AirVent.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
			}
		}

		private struct GridDoors
		{
			public readonly IMyDoor Door;
			public readonly DoorSettings WarTimeSettings;
			public readonly DoorSettings PeaceTimeSettings;

			public GridDoors(IMyDoor door, DoorSettings warTimeSettings, DoorSettings peaceTimeSettings)
			{
				Door = door;
				WarTimeSettings = warTimeSettings;
				PeaceTimeSettings = peaceTimeSettings;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Door.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
			}
		}

		private struct GridGravityGenerators
		{
			public readonly IMyGravityGenerator GravityGenerator;
			public readonly GravityGeneratorSettings WarTimeSettings;
			public readonly GravityGeneratorSettings PeaceTimeSettings;

			public GridGravityGenerators(IMyGravityGenerator gravityGenerator, GravityGeneratorSettings warTimeSettings, GravityGeneratorSettings peaceTimeSettings)
			{
				GravityGenerator = gravityGenerator;
				WarTimeSettings = warTimeSettings;
				PeaceTimeSettings = peaceTimeSettings;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{GravityGenerator.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
			}
		}

		private struct GridSphericalGravityGenerators
		{
			public readonly IMyGravityGeneratorSphere SphericalGravityGenerator;
			public readonly SphericalGravityGeneratorSettings WarTimeSettings;
			public readonly SphericalGravityGeneratorSettings PeaceTimeSettings;

			public GridSphericalGravityGenerators(IMyGravityGeneratorSphere gravityGenerator, SphericalGravityGeneratorSettings warTimeSettings, SphericalGravityGeneratorSettings peaceTimeSettings)
			{
				SphericalGravityGenerator = gravityGenerator;
				WarTimeSettings = warTimeSettings;
				PeaceTimeSettings = peaceTimeSettings;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{SphericalGravityGenerator.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
			}
		}

		private struct GridTimers
		{
			public readonly IMyTimerBlock TimerBlock;
			public readonly TimerSettings WarTimeSettings;
			public readonly TimerSettings PeaceTimeSettings;

			public GridTimers(IMyTimerBlock timer, TimerSettings warTimeSettings, TimerSettings peaceTimeSettings)
			{
				TimerBlock = timer;
				WarTimeSettings = warTimeSettings;
				PeaceTimeSettings = peaceTimeSettings;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{TimerBlock.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
			}
		}

		private struct GridTurrets
		{
			public readonly IMyLargeTurretBase Turret;
			public readonly TurretSettings WarTimeSettings;
			public readonly TurretSettings PeaceTimeSettings;

			public GridTurrets(IMyLargeTurretBase largeTurretBase, TurretSettings warTimeSettings, TurretSettings peaceTimeSettings)
			{
				Turret = largeTurretBase;
				WarTimeSettings = warTimeSettings;
				PeaceTimeSettings = peaceTimeSettings;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Turret.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
			}
		}

		public void Init()
		{
			int airVents = 0;
			int doors = 0;
			int gravityGenerators = 0;
			int sphericalGravityGenerators = 0;
			int timers = 0;
			int turrets = 0;

			try
			{
				WriteToLog("EmergencyLockDownProtocol", $"Setting up for the long haul! {_thisGrid.GridSizeEnum} {_thisGrid.CubeBlocks.Count} | {_thisGrid.GetFatBlocks().Count}", LogType.General);
				foreach (MyCubeBlock myCubeBlock in _thisGrid.GetFatBlocks())
				{
					IMyLargeTurretBase largeTurretBase = myCubeBlock as IMyLargeTurretBase;
					if (largeTurretBase != null)
					{
						MyObjectBuilder_TurretBase myTurretBase = (MyObjectBuilder_TurretBase)largeTurretBase.GetObjectBuilderCubeBlock();

						TurretSettings archiveSettings = new TurretSettings(
							largeTurretBase.Enabled,
							largeTurretBase.EnableIdleRotation,
							myTurretBase.TargetCharacters,
							myTurretBase.TargetLargeGrids,
							myTurretBase.TargetMeteors,
							myTurretBase.TargetMissiles,
							myTurretBase.TargetNeutrals,
							myTurretBase.TargetSmallGrids, 
							myTurretBase.TargetStations, 
							myTurretBase.Range
							);

						_gridTurretSettings.Add(new GridTurrets(largeTurretBase, (TurretSettings)_warTimeSettings[CubeType.Turret], archiveSettings));

						++turrets;
						StaticMethods.AddGpsLocation($"{CubeType.Turret.ToString()} {turrets}", largeTurretBase.GetPosition());
						continue;
					}


					IMyDoor door = myCubeBlock as IMyDoor;
					if (door != null)
					{
						door.OnDoorStateChanged += delegate(IMyDoor myDoor, bool b)
						{
							if (myDoor.OwnerId != _gridOwnerId) return;
							if (!b && _alertEnabled)
								door.Enabled = false;
						};
						
						_gridDoorSettings.Add(
							new GridDoors(
								door,
								(DoorSettings)_warTimeSettings[CubeType.Door],
								new DoorSettings(door.Enabled, door.IsFullyClosed)
								)
							);
						++doors;
						StaticMethods.AddGpsLocation($"{CubeType.Door.ToString()} {doors}", door.GetPosition());
						continue;
					}

					IMyGravityGenerator generator = myCubeBlock as IMyGravityGenerator;
					if (generator != null)
					{
						_gridGravityGenerators.Add(
							new GridGravityGenerators(
								generator,
								(GravityGeneratorSettings)_warTimeSettings[CubeType.GravityGenerator],
								new GravityGeneratorSettings(
									generator.Enabled,
									generator.FieldSize,
									generator.GravityAcceleration
								)
							));

						++gravityGenerators;
						StaticMethods.AddGpsLocation($"{CubeType.GravityGenerator.ToString()} {gravityGenerators}", generator.GetPosition());
						continue;
					}

					IMyGravityGeneratorSphere generatorSphere = myCubeBlock as IMyGravityGeneratorSphere;
					if (generatorSphere != null)
					{
						_gridSphericalGravityGenerators.Add(
							new GridSphericalGravityGenerators (
								generatorSphere,
								(SphericalGravityGeneratorSettings)_warTimeSettings[CubeType.SphericalGravityGenerator],
								new SphericalGravityGeneratorSettings(
									generatorSphere.Enabled,
									generatorSphere.Radius,
									generatorSphere.GravityAcceleration
								)
							));

						++sphericalGravityGenerators;
						StaticMethods.AddGpsLocation($"{CubeType.SphericalGravityGenerator.ToString()} {sphericalGravityGenerators}", generatorSphere.GetPosition());
						continue;
					}

					IMyTimerBlock myTimer = myCubeBlock as IMyTimerBlock;
					if (myTimer != null)
					{
						_gridTimers.Add(new GridTimers(
							myTimer, (TimerSettings)_warTimeSettings[CubeType.Timer],new TimerSettings(myTimer.Enabled)
							));
						++timers;
						StaticMethods.AddGpsLocation($"{CubeType.Timer.ToString()} {timers}", myTimer.GetPosition());
						continue;
					}
					
					IMyAirVent vent = myCubeBlock as IMyAirVent;
					if (vent == null) continue;
					
					AirVentSettings archivedSettings = new AirVentSettings(
						vent.Enabled,
						vent.Depressurize
					);

					_gridAirVentSettings.Add(new GridAirVents(vent, (AirVentSettings)_warTimeSettings[CubeType.AirVent], archivedSettings));

					++airVents;
					StaticMethods.AddGpsLocation($"{CubeType.AirVent.ToString()} {airVents}", vent.GetPosition());
				}
				
				WriteToLog("EmergencyLockDownProtocol", $"Total Found - Air Vents: {airVents} | Doors: {doors} | Gravity Generators: {gravityGenerators}  | Spherical Gravity Generators: {sphericalGravityGenerators} | Timers: {timers} | Turrets: {turrets} ", LogType.General);
			}
			catch (Exception e)
			{
				WriteToLog("EmergencyLockDownProtocol", $"Exception! {e}", LogType.Exception);
			}
		}

		public void EnableAlert()
		{
			if (_alertEnabled) return;
			WriteToLog("EnableAlert", $"Loading Wartime Settings...", LogType.General);

			SetAirVentSettings(EmergencySetting.Wartime);
			SetDoorSettings(EmergencySetting.Wartime);
			SetGravityGeneratorSettings(EmergencySetting.Wartime);
			SetSphericalGravityGeneratorSettings(EmergencySetting.Wartime);
			SetTimerSettings(EmergencySetting.Wartime);
			SetTurretSettings(EmergencySetting.Wartime);
			
			WriteToLog("EnableAlert", $"Wartime Settings Loaded...", LogType.General);
			_alertEnabled = true;
		}

		public void DisableAlert()
		{
			if (!_alertEnabled) return;
			WriteToLog("DisableAlert", $"Loading Peacetime Settings...", LogType.General);

			SetAirVentSettings(EmergencySetting.PeaceTime);
			SetDoorSettings(EmergencySetting.PeaceTime);
			SetGravityGeneratorSettings(EmergencySetting.PeaceTime);
			SetSphericalGravityGeneratorSettings(EmergencySetting.PeaceTime);
			SetTimerSettings(EmergencySetting.PeaceTime);
			SetTurretSettings(EmergencySetting.PeaceTime);

			WriteToLog("DisableAlert", $"Peacetime Settings Loaded...", LogType.General);
			_alertEnabled = false;
		}

		private void SetAirVentSettings(EmergencySetting emergencySetting)
		{
			for (int index = _gridAirVentSettings.Count - 1; index >= 0; index--)
			{
				if (!_gridAirVentSettings[index].AirVent.InScene || _gridAirVentSettings[index].AirVent.OwnerId != _gridOwnerId)
				{
					_gridAirVentSettings.RemoveAtFast(index);
					continue;
				}

				switch (emergencySetting)
				{
					case EmergencySetting.PeaceTime:
						_gridAirVentSettings[index].AirVent.Enabled = _gridAirVentSettings[index].PeaceTimeSettings.Enabled;
						_gridAirVentSettings[index].AirVent.Depressurize = _gridAirVentSettings[index].PeaceTimeSettings.Depressurize;
						break;
					case EmergencySetting.Wartime:
						_gridAirVentSettings[index].AirVent.Enabled = _gridAirVentSettings[index].WarTimeSettings.Enabled;
						_gridAirVentSettings[index].AirVent.Depressurize = _gridAirVentSettings[index].WarTimeSettings.Depressurize;
						break;
					default:
						return;
				}
			}
		}

		private void SetDoorSettings(EmergencySetting emergencySetting)
		{
			for (int index = _gridDoorSettings.Count - 1; index >= 0; index--)
			{
				if (!_gridDoorSettings[index].Door.InScene || _gridDoorSettings[index].Door.OwnerId != _gridOwnerId)
				{
					_gridDoorSettings.RemoveAtFast(index);
					continue;
				}

				switch (emergencySetting)
				{
					case EmergencySetting.PeaceTime:
						_gridDoorSettings[index].Door.Enabled = _gridDoorSettings[index].PeaceTimeSettings.Enabled;
						if (!_gridDoorSettings[index].PeaceTimeSettings.IsClosed && (_gridDoorSettings[index].Door.Status == DoorStatus.Closed || _gridDoorSettings[index].Door.Status == DoorStatus.Closing))
						{
							_gridDoorSettings[index].Door.Enabled = true;
							_gridDoorSettings[index].Door.OpenDoor();
						}
						break;
					case EmergencySetting.Wartime:
						if (!_gridDoorSettings[index].Door.IsFullyClosed)
						{
							_gridDoorSettings[index].Door.Enabled = true;
							_gridDoorSettings[index].Door.CloseDoor();
						}
						else
							_gridDoorSettings[index].Door.Enabled = _gridDoorSettings[index].WarTimeSettings.Enabled;
						
						break;
					default:
						return;
				}
			}
		}

		private void SetGravityGeneratorSettings(EmergencySetting emergencySetting)
		{
			for (int index = _gridGravityGenerators.Count - 1; index >= 0; index--)
			{
				IMyGravityGenerator gravityGenerator = _gridGravityGenerators[index].GravityGenerator;
				if (!gravityGenerator.InScene || gravityGenerator.OwnerId != _gridOwnerId)
				{
					_gridGravityGenerators.RemoveAtFast(index);
					continue;
				}

				switch (emergencySetting)
				{
					case EmergencySetting.PeaceTime:
						gravityGenerator.Enabled = _gridGravityGenerators[index].PeaceTimeSettings.Enabled;
						gravityGenerator.FieldSize = _gridGravityGenerators[index].PeaceTimeSettings.FieldSize;
						gravityGenerator.GravityAcceleration = _gridGravityGenerators[index].PeaceTimeSettings.FieldStrength;
						break;
					case EmergencySetting.Wartime:
						gravityGenerator.Enabled = _gridGravityGenerators[index].WarTimeSettings.Enabled;
						gravityGenerator.FieldSize = _gridGravityGenerators[index].WarTimeSettings.FieldSize;
						gravityGenerator.GravityAcceleration = _gridGravityGenerators[index].WarTimeSettings.FieldStrength;
						if (_gridGravityGenerators[index].PeaceTimeSettings.FieldStrength >= 0) gravityGenerator.GravityAcceleration *= -1;
						break;
					default:
						return;
				}
			}
		}

		private void SetSphericalGravityGeneratorSettings(EmergencySetting emergencySetting)
		{
			for (int index = _gridSphericalGravityGenerators.Count - 1; index >= 0; index--)
			{
				if (!_gridSphericalGravityGenerators[index].SphericalGravityGenerator.InScene || _gridSphericalGravityGenerators[index].SphericalGravityGenerator.OwnerId != _gridOwnerId)
				{
					_gridSphericalGravityGenerators.RemoveAtFast(index);
					continue;
				}

				switch (emergencySetting)
				{
					case EmergencySetting.PeaceTime:
						_gridSphericalGravityGenerators[index].SphericalGravityGenerator.Enabled = _gridSphericalGravityGenerators[index].PeaceTimeSettings.Enabled;
						_gridSphericalGravityGenerators[index].SphericalGravityGenerator.Radius = _gridSphericalGravityGenerators[index].PeaceTimeSettings.FieldSize;
						_gridSphericalGravityGenerators[index].SphericalGravityGenerator.GravityAcceleration = _gridSphericalGravityGenerators[index].PeaceTimeSettings.FieldStrength;
						break;
					case EmergencySetting.Wartime:
						_gridSphericalGravityGenerators[index].SphericalGravityGenerator.Enabled = _gridSphericalGravityGenerators[index].WarTimeSettings.Enabled;
						_gridSphericalGravityGenerators[index].SphericalGravityGenerator.Radius = _gridSphericalGravityGenerators[index].WarTimeSettings.FieldSize;
						_gridSphericalGravityGenerators[index].SphericalGravityGenerator.GravityAcceleration = _gridSphericalGravityGenerators[index].WarTimeSettings.FieldStrength;
						break;
					default:
						return;
				}
			}
		}

		private void SetTimerSettings(EmergencySetting emergencySetting)
		{
			for (int index = _gridTimers.Count - 1; index >= 0; index--)
			{
				if (!_gridTimers[index].TimerBlock.InScene || _gridTimers[index].TimerBlock.OwnerId != _gridOwnerId)
				{
					_gridTimers.RemoveAtFast(index);
					continue;
				}

				switch (emergencySetting)
				{
					case EmergencySetting.PeaceTime:
						_gridTimers[index].TimerBlock.Enabled = _gridTimers[index].PeaceTimeSettings.Enabled;
						break;
					case EmergencySetting.Wartime:
						_gridTimers[index].TimerBlock.Enabled = _gridTimers[index].WarTimeSettings.Enabled;
						if(_gridTimers[index].TimerBlock.Enabled) _gridTimers[index].TimerBlock.Trigger();
						break;
					default:
						return;
				}
			}
		}

		private void SetTurretSettings(EmergencySetting emergencySetting)
		{
			for (int index = _gridTurretSettings.Count - 1; index >= 0; index--)
			{
				if (!_gridTurretSettings[index].Turret.InScene || _gridTurretSettings[index].Turret.OwnerId != _gridOwnerId)
				{
					_gridTurretSettings.RemoveAtFast(index);
					continue;
				}

				switch (emergencySetting)
				{
					case EmergencySetting.PeaceTime:
						_gridTurretSettings[index].Turret.Enabled = _gridTurretSettings[index].PeaceTimeSettings.Enabled;
						_gridTurretSettings[index].Turret.EnableIdleRotation = _gridTurretSettings[index].PeaceTimeSettings.EnableIdleRotation;
						_gridTurretSettings[index].Turret.SetValueBool("TargetMeteors", _gridTurretSettings[index].PeaceTimeSettings.TargetCharacters);
						_gridTurretSettings[index].Turret.SetValueBool("TargetLargeShips", _gridTurretSettings[index].PeaceTimeSettings.TargetLargeShips);
						_gridTurretSettings[index].Turret.SetValueBool("TargetMeteors", _gridTurretSettings[index].PeaceTimeSettings.TargetMeteors);
						_gridTurretSettings[index].Turret.SetValueBool("TargetMissiles", _gridTurretSettings[index].PeaceTimeSettings.TargetMissiles);
						_gridTurretSettings[index].Turret.SetValueBool("TargetNeutrals", _gridTurretSettings[index].PeaceTimeSettings.TargetNeutrals);
						_gridTurretSettings[index].Turret.SetValueBool("TargetSmallShips", _gridTurretSettings[index].PeaceTimeSettings.TargetSmallShips);
						_gridTurretSettings[index].Turret.SetValueBool("TargetStations", _gridTurretSettings[index].PeaceTimeSettings.TargetStations);
						_gridTurretSettings[index].Turret.SetValueFloat("Range", _gridTurretSettings[index].PeaceTimeSettings.Range);
						break;
					case EmergencySetting.Wartime:
						_gridTurretSettings[index].Turret.Enabled = _gridTurretSettings[index].WarTimeSettings.Enabled;
						_gridTurretSettings[index].Turret.EnableIdleRotation = _gridTurretSettings[index].WarTimeSettings.EnableIdleRotation;
						_gridTurretSettings[index].Turret.SetValueBool("TargetMeteors", _gridTurretSettings[index].WarTimeSettings.TargetCharacters);
						_gridTurretSettings[index].Turret.SetValueBool("TargetLargeShips", _gridTurretSettings[index].WarTimeSettings.TargetLargeShips);
						_gridTurretSettings[index].Turret.SetValueBool("TargetMeteors", _gridTurretSettings[index].WarTimeSettings.TargetMeteors);
						_gridTurretSettings[index].Turret.SetValueBool("TargetMissiles", _gridTurretSettings[index].WarTimeSettings .TargetMissiles);
						_gridTurretSettings[index].Turret.SetValueBool("TargetNeutrals", _gridTurretSettings[index].WarTimeSettings.TargetNeutrals);
						_gridTurretSettings[index].Turret.SetValueBool("TargetSmallShips", _gridTurretSettings[index].WarTimeSettings.TargetSmallShips);
						_gridTurretSettings[index].Turret.SetValueBool("TargetStations", _gridTurretSettings[index].WarTimeSettings.TargetStations);
						_gridTurretSettings[index].Turret.SetValueFloat("Range", _gridTurretSettings[index].WarTimeSettings.Range);
						break;
					default:
						return;
				}
			}
		}

		private readonly Dictionary<CubeType, object> _warTimeSettings = new Dictionary<CubeType, object>
		{
			{CubeType.AirVent, new AirVentSettings(true, true) },
			{CubeType.Door, new DoorSettings(false, true) },
			{CubeType.GravityGenerator, new GravityGeneratorSettings(true, new Vector3(150, 150, 150), 9.81f ) },
			{CubeType.SphericalGravityGenerator,  new SphericalGravityGeneratorSettings(true, 450f, 9.81f )},
			{CubeType.Timer, new TimerSettings(true) },
			{CubeType.Turret, new TurretSettings(true, true, true, true, false, true, false, true, true, 800) }
		};
	}
}
