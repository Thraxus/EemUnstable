using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRageMath;
using IMyDoor = Sandbox.ModAPI.IMyDoor;
using IMyEntityIngame = VRage.Game.ModAPI.Ingame.IMyEntity;
using IMyLargeTurretBase = Sandbox.ModAPI.IMyLargeTurretBase;

namespace Eem.Thraxus.Bots.Modules
{
	public class EmergencyLockDownProtocol : LogBaseEvent, IDisposable
	{
		private enum CubeType
		{
			AirVent, Door, Turret, GravityGenerator, GravityGeneratorReversed, SphericalGravityGenerator
		}

		private struct TurretSettings
		{
			public readonly bool Enabled;
			public readonly bool TargetCharacters;
			public readonly bool TargetLargeShips;
			public readonly bool TargetMeteors;
			public readonly bool TargetMissiles;
			public readonly bool TargetNeutrals;
			public readonly bool TargetSmallShips;
			public readonly bool TargetStations;

			public readonly float Range;

			public TurretSettings(bool enabled, bool targetCharacters, bool targetLargeShips, bool targetMeteors, bool targetMissiles, bool targetNeutrals, bool targetSmallShips, bool targetStations, float range)
			{
				Enabled = enabled;
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
				return $"{Enabled} | {TargetCharacters} | {TargetLargeShips} | {TargetMeteors} | {TargetMissiles} | {TargetSmallShips} | {TargetStations} | {Range}";
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
				return $"{Turret.EntityId} | {PeaceTimeSettings} | {WarTimeSettings}";
			}
		}


		private struct DoorSettings
		{
			public readonly bool Enabled;

			public DoorSettings(bool enabled)
			{
				Enabled = enabled;
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
				return $"{AirVent.EntityId} | {PeaceTimeSettings} | {WarTimeSettings}";
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
						if (generator.GravityAcceleration > 0)
							_gridGravityGenerators.Add(
								new GridGravityGenerators(
								generator, 
								(GravityGeneratorSettings)_warTimeSettings[CubeType.GravityGeneratorReversed],
								new GravityGeneratorSettings(
									generator.Enabled,
									generator.FieldSize, 
									generator.GravityAcceleration
									)
								));
						else
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
								(SphericalGravityGeneratorSettings)_warTimeSettings[CubeType.GravityGeneratorReversed],
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

			for (int index = _gridDoorSettings.Count - 1; index >= 0; index--)
			{
				IMyDoor gridDoor = _gridDoorSettings[index].Door;
				if (!gridDoor.InScene || gridDoor.OwnerId != _gridOwnerId)
				{
					_gridDoorSettings.RemoveAtFast(index);
					continue;
				}
				
				gridDoor.SetValueBool("Open", false);
				if (gridDoor.Status == DoorStatus.Closed)
					gridDoor.Enabled = _gridDoorSettings[index].WarTimeSettings.Enabled;
			}

			for (int index = _gridTurretSettings.Count - 1; index >= 0; index--)
			{
				IMyLargeTurretBase turretBase = _gridTurretSettings[index].Turret;
				if (!turretBase.InScene || turretBase.OwnerId != _gridOwnerId)
				{
					_gridTurretSettings.RemoveAtFast(index);
					continue;
				}
				
				turretBase.Enabled = _gridTurretSettings[index].WarTimeSettings.Enabled;
				turretBase.SetValueBool("TargetMeteors", _gridTurretSettings[index].WarTimeSettings.TargetCharacters);
				turretBase.SetValueBool("TargetLargeShips", _gridTurretSettings[index].WarTimeSettings.TargetLargeShips);
				turretBase.SetValueBool("TargetMeteors", _gridTurretSettings[index].WarTimeSettings.TargetMeteors);
				turretBase.SetValueBool("TargetMissiles", _gridTurretSettings[index].WarTimeSettings.TargetMissiles);
				turretBase.SetValueBool("TargetNeutrals", _gridTurretSettings[index].WarTimeSettings.TargetNeutrals);
				turretBase.SetValueBool("TargetSmallShips", _gridTurretSettings[index].WarTimeSettings.TargetSmallShips);
				turretBase.SetValueBool("TargetStations", _gridTurretSettings[index].WarTimeSettings.TargetStations);
				turretBase.SetValueFloat("Range", _gridTurretSettings[index].WarTimeSettings.Range);
			}


			for (int index = _gridAirVentSettings.Count - 1; index >= 0; index--)
			{
				IMyAirVent airVent = _gridAirVentSettings[index].AirVent;
				if (!airVent.InScene || airVent.OwnerId != _gridOwnerId)
				{
					_gridAirVentSettings.RemoveAtFast(index);
					continue;
				}

				airVent.Enabled = _gridAirVentSettings[index].WarTimeSettings.Enabled;
				airVent.Depressurize = _gridAirVentSettings[index].WarTimeSettings.Depressurize;
			}

			for (int index = _gridGravityGenerators.Count - 1; index >= 0; index--)
			{
				IMyGravityGenerator gravityGenerator = _gridGravityGenerators[index].GravityGenerator;
				if (!gravityGenerator.InScene || gravityGenerator.OwnerId != _gridOwnerId)
				{
					_gridGravityGenerators.RemoveAtFast(index);
					continue;
				}

				gravityGenerator.Enabled = _gridGravityGenerators[index].WarTimeSettings.Enabled;
				gravityGenerator.FieldSize = _gridGravityGenerators[index].WarTimeSettings.FieldSize;
				gravityGenerator.GravityAcceleration = _gridGravityGenerators[index].WarTimeSettings.FieldStrength;
			}

			for (int index = _gridSphericalGravityGenerators.Count - 1; index >= 0; index--)
			{
				IMyGravityGeneratorSphere sphericalGravityGenerator = _gridSphericalGravityGenerators[index].SphericalGravityGenerator;
				if (!sphericalGravityGenerator.InScene || sphericalGravityGenerator.OwnerId != _gridOwnerId)
				{
					_gridSphericalGravityGenerators.RemoveAtFast(index);
					continue;
				}

				sphericalGravityGenerator.Enabled = _gridSphericalGravityGenerators[index].WarTimeSettings.Enabled;
				sphericalGravityGenerator.Radius = _gridSphericalGravityGenerators[index].WarTimeSettings.FieldSize;
				sphericalGravityGenerator.GravityAcceleration = _gridSphericalGravityGenerators[index].WarTimeSettings.FieldStrength;
			}

			_alertEnabled = true;
			WriteToLog("EnableAlert", $"Wartime Settings Loaded...", LogType.General);
		}

		public void DisableAlert()
		{
			if (!_alertEnabled) return;
			WriteToLog("DisableAlert", $"Loading Peacetime Settings...", LogType.General);

			for (int index = _gridDoorSettings.Count - 1; index >= 0; index--)
			{
				IMyDoor gridDoor = _gridDoorSettings[index].Door;
				if (!gridDoor.InScene || gridDoor.OwnerId != _gridOwnerId)
				{
					_gridDoorSettings.RemoveAtFast(index);
					continue;
				}

				gridDoor.Enabled = _gridDoorSettings[index].PeaceTimeSettings.Enabled;
			}

			for (int index = _gridTurretSettings.Count - 1; index >= 0; index--)
			{
				IMyLargeTurretBase turretBase = _gridTurretSettings[index].Turret;
				if (!turretBase.InScene || turretBase.OwnerId != _gridOwnerId)
				{
					_gridTurretSettings.RemoveAtFast(index);
					continue;
				}

				WriteToLog("DisableAlert", $"{turretBase.EntityId} - Loading Default Settings...", LogType.General);
				turretBase.Enabled = _gridTurretSettings[index].PeaceTimeSettings.Enabled;
				turretBase.SetValueBool("TargetMeteors", _gridTurretSettings[index].PeaceTimeSettings.TargetCharacters);
				turretBase.SetValueBool("TargetLargeShips", _gridTurretSettings[index].PeaceTimeSettings.TargetLargeShips);
				turretBase.SetValueBool("TargetMeteors", _gridTurretSettings[index].PeaceTimeSettings.TargetMeteors);
				turretBase.SetValueBool("TargetMissiles", _gridTurretSettings[index].PeaceTimeSettings.TargetMissiles);
				turretBase.SetValueBool("TargetNeutrals", _gridTurretSettings[index].PeaceTimeSettings.TargetNeutrals);
				turretBase.SetValueBool("TargetSmallShips", _gridTurretSettings[index].PeaceTimeSettings.TargetSmallShips);
				turretBase.SetValueBool("TargetStations", _gridTurretSettings[index].PeaceTimeSettings.TargetStations);
				turretBase.SetValueFloat("Range", _gridTurretSettings[index].PeaceTimeSettings.Range);
			}

			for (int index = _gridAirVentSettings.Count - 1; index >= 0; index--)
			{
				IMyAirVent airVent = _gridAirVentSettings[index].AirVent;
				if (!airVent.InScene || airVent.OwnerId != _gridOwnerId)
				{
					_gridAirVentSettings.RemoveAtFast(index);
					continue;
				}

				airVent.Enabled = _gridAirVentSettings[index].PeaceTimeSettings.Enabled;
				airVent.Depressurize = _gridAirVentSettings[index].PeaceTimeSettings.Depressurize;
			}

			for (int index = _gridGravityGenerators.Count - 1; index >= 0; index--)
			{
				IMyGravityGenerator gravityGenerator = _gridGravityGenerators[index].GravityGenerator;
				if (!gravityGenerator.InScene || gravityGenerator.OwnerId != _gridOwnerId)
				{
					_gridGravityGenerators.RemoveAtFast(index);
					continue;
				}

				gravityGenerator.Enabled = _gridGravityGenerators[index].PeaceTimeSettings.Enabled;
				gravityGenerator.FieldSize = _gridGravityGenerators[index].PeaceTimeSettings.FieldSize;
				gravityGenerator.GravityAcceleration = _gridGravityGenerators[index].PeaceTimeSettings.FieldStrength;
			}

			for (int index = _gridSphericalGravityGenerators.Count - 1; index >= 0; index--)
			{
				IMyGravityGeneratorSphere sphericalGravityGenerator = _gridSphericalGravityGenerators[index].SphericalGravityGenerator;
				if (!sphericalGravityGenerator.InScene || sphericalGravityGenerator.OwnerId != _gridOwnerId)
				{
					_gridSphericalGravityGenerators.RemoveAtFast(index);
					continue;
				}

				sphericalGravityGenerator.Enabled = _gridSphericalGravityGenerators[index].PeaceTimeSettings.Enabled;
				sphericalGravityGenerator.Radius = _gridSphericalGravityGenerators[index].PeaceTimeSettings.FieldSize;
				sphericalGravityGenerator.GravityAcceleration = _gridSphericalGravityGenerators[index].PeaceTimeSettings.FieldStrength;
			}

			WriteToLog("DisableAlert", $"Peacetime Settings Loaded...", LogType.General);
			_alertEnabled = false;
		}

		private readonly Dictionary<CubeType, object> _warTimeSettings = new Dictionary<CubeType, object>
		{
			{CubeType.Turret, new TurretSettings(true, true, true, false, true, false, true, true, 800) },
			{CubeType.AirVent, new AirVentSettings(true, true) },
			{CubeType.Door, new DoorSettings(false) },
			{CubeType.GravityGenerator, new GravityGeneratorSettings(true, new Vector3(150, 150, 150), 9.81f ) },
			{CubeType.GravityGeneratorReversed, new GravityGeneratorSettings(true, new Vector3(150, 150, 150), -9.81f ) },
			{CubeType.SphericalGravityGenerator,  new SphericalGravityGeneratorSettings(true, 450f, 9.81f )},
		};

		private bool _alertEnabled;

		private readonly long _gridOwnerId;
		
		private readonly MyCubeGrid _thisGrid;
		private readonly List<GridTurrets> _gridTurretSettings;
		private readonly List<GridDoors> _gridDoorSettings;
		private readonly List<GridAirVents> _gridAirVentSettings;
		private readonly List<GridGravityGenerators> _gridGravityGenerators;
		private readonly List<GridSphericalGravityGenerators> _gridSphericalGravityGenerators;

		public EmergencyLockDownProtocol(MyCubeGrid myCubeGrid)
		{
			// TODO Timers for existing flavor emergency conditions (possibly scan for settings related to what this code controls now and remove it)
			//		Example from Helios: [HELIOS|Tme] [Alert_On] Timer Alarm is the alert timer, trigger for on, trigger for off?  need to research.
			// TODO Antenna for custom drone spawning for special conditions / alert if enemy seen within some range - may move this idea to a separate module though

			_thisGrid = myCubeGrid;
			_gridOwnerId = myCubeGrid.BigOwners[0];

			_gridTurretSettings = new List<GridTurrets>();
			_gridAirVentSettings = new List<GridAirVents>();
			_gridDoorSettings = new List<GridDoors>();
			_gridGravityGenerators = new List<GridGravityGenerators>();
			_gridSphericalGravityGenerators = new List<GridSphericalGravityGenerators>();
		}
		private bool _unloaded = false;

		~EmergencyLockDownProtocol()
		{
			Dispose();
		}
		/// <inheritdoc />
		public void Dispose()
		{
			if (!_unloaded)
			{

			}
			_unloaded = true;
		}
	}
}
