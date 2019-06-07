using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRageMath;
using IMyDoor = Sandbox.ModAPI.IMyDoor;
using IMyLargeTurretBase = Sandbox.ModAPI.IMyLargeTurretBase;

namespace Eem.Thraxus.Bots.Modules
{
	public class EmergencyLockdownProtocol : LogBaseEvent
	{
		private bool _alertEnabled;

		private readonly long _gridOwnerId;

		private readonly MyCubeGrid _thisGrid;

		private readonly List<GridAirVents> _gridAirVentSettings;
		private readonly List<GridDoors> _gridDoorSettings;
		private readonly List<GridGravityGenerators> _gridGravityGenerators;
		private readonly List<GridSphericalGravityGenerators> _gridSphericalGravityGenerators;
		private readonly List<IMyTimerBlock> _gridTimers;
		private readonly List<GridTurrets> _gridTurretSettings;

		public EmergencyLockdownProtocol(MyCubeGrid myCubeGrid, long ownerId)
		{
			// TODO Timers for existing flavor emergency conditions (possibly scan for settings related to what this code controls now and remove it)
			//		Example from Helios: [HELIOS|Tme] [Alert_On] Timer Alarm is the alert timer, trigger for on, trigger for off?  need to research.
			// TODO Antenna for custom drone spawning for special conditions / alert if enemy seen within some range - may move this idea to a separate module though

			_thisGrid = myCubeGrid;
			_gridOwnerId = ownerId;
			
			_gridAirVentSettings = new List<GridAirVents>();
			_gridDoorSettings = new List<GridDoors>();
			_gridGravityGenerators = new List<GridGravityGenerators>();
			_gridSphericalGravityGenerators = new List<GridSphericalGravityGenerators>();
			_gridTimers = new List<IMyTimerBlock>();
			_gridTurretSettings = new List<GridTurrets>();
		}

		private enum CubeType
		{
			AirVent, Door, GravityGenerator, SphericalGravityGenerator, Timer, Turret
		}

		enum EmergencySetting
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

			public DoorSettings(bool enabled)
			{
				Enabled = enabled;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled}";
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
			int turrets = 0;
			int airvents = 0;
			int doors = 0;
			int gravityGenerators = 0;
			int sphericalGravityGenerators = 0;

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
								new DoorSettings(door.Enabled)
								)
							);
						++doors;
						StaticMethods.AddGpsLocation($"{CubeType.Door.ToString()} {doors}", door.GetPosition());
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
					}

					IMyAirVent vent = myCubeBlock as IMyAirVent;
					if (vent == null) continue;
					
					AirVentSettings archivedSettings = new AirVentSettings(
						vent.Enabled,
						vent.Depressurize
					);

					_gridAirVentSettings.Add(new GridAirVents(vent, (AirVentSettings)_warTimeSettings[CubeType.AirVent], archivedSettings));

					++airvents;
					StaticMethods.AddGpsLocation($"{CubeType.AirVent.ToString()} {airvents}", vent.GetPosition());
				}
				
				WriteToLog("EmergencyLockDownProtocol", $"Total Found - Turrets: {turrets} | Air Vents: {airvents} | Doors: {doors} | Gravity Generators: {gravityGenerators}  | Spherical Gravity Generators: {sphericalGravityGenerators}", LogType.General);
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
						break;
					case EmergencySetting.Wartime:
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
			{CubeType.Turret, new TurretSettings(true, true, true, true, false, true, false, true, true, 800) },
			{CubeType.AirVent, new AirVentSettings(true, true) },
			{CubeType.Door, new DoorSettings(false) },
			{CubeType.GravityGenerator, new GravityGeneratorSettings(true, new Vector3(150, 150, 150), 9.81f ) },
			{CubeType.SphericalGravityGenerator,  new SphericalGravityGeneratorSettings(true, 450f, 9.81f )},
		};
	}
}
