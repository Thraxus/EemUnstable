using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Eem.Thraxus.Bots.Modules.Support;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Statics;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRageMath;
using IMyDoor = Sandbox.ModAPI.IMyDoor;
using IMyLargeTurretBase = Sandbox.ModAPI.IMyLargeTurretBase;
using IMySensorBlock = Sandbox.ModAPI.IMySensorBlock;

namespace Eem.Thraxus.Bots.Modules
{
	public class AlertConditions : LogBaseEvent
	{
		private bool _alertEnabled;

		private readonly long _gridOwnerId;

		private readonly MyCubeGrid _thisGrid;

		private readonly List<AirVent> _airVents = new List<AirVent>();
		private readonly List<Door> _doors = new List<Door>();
		private readonly List<GravityGenerator> _gravityGenerators = new List<GravityGenerator>();
		private readonly List<Sensor> _sensors = new List<Sensor>();
		private readonly List<SphericalGravityGenerator> _sphericalGravityGenerators = new List<SphericalGravityGenerator>();
		private readonly List<Timer> _timers = new List<Timer>();
		private readonly List<Turret> _turrets = new List<Turret>();
		

		public AlertConditions(MyCubeGrid myCubeGrid, long ownerId)
		{
			// TODO 08.28.2019: Feature complete at this point as far as functionality.  Need to add in custom data scanning to make sure all items are parsed properly.				
			// TODO					An example of this is the term "alert" on a timer telling me it's one i need to track.  No custom data, no timer added to the alert list
			// TODO					Timers may need to come in pairs; one for on, one for off.
			// 
			// TODO Timers for existing flavor emergency conditions (possibly scan for settings related to what this code controls now and remove it)
			//		Example from Helios: [HELIOS|Tme] [Alert_On] Timer Alarm is the alert timer, trigger for on, trigger for off?  need to research.
			// TODO Antenna for custom drone spawning for special conditions / alert if enemy seen within some range - may move this idea to a separate module though
			// TODO MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid

			_thisGrid = myCubeGrid;
			_gridOwnerId = ownerId;
		}
		
		public void Init()
		{
			int airVents = 0;
			int doors = 0;
			int gravityGenerators = 0;
			int sphericalGravityGenerators = 0;
			int sensors = 0;
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
						_turrets.Add(new Turret(largeTurretBase, (TurretSettings)_warTimeSettings[CubeType.Turret], archiveSettings));
						turrets++;
						//Statics.AddGpsLocation($"{CubeType.Turret.ToString()} {turrets}", largeTurretBase.GetPosition());
						continue;
					}


					IMyDoor door = myCubeBlock as IMyDoor;
					if (door != null)
					{
						_doors.Add(new Door(door, 
								(DoorSettings)_warTimeSettings[CubeType.Door],
								new DoorSettings(door.Enabled, door.IsFullyClosed)
								)
							);
						doors++;
						//Statics.AddGpsLocation($"{CubeType.Door.ToString()} {doors}", door.GetPosition());
						continue;
					}

					IMyGravityGenerator generator = myCubeBlock as IMyGravityGenerator;
					if (generator != null)
					{
						_gravityGenerators.Add(
							new GravityGenerator(
								generator,
								(GravityGeneratorSettings)_warTimeSettings[CubeType.GravityGenerator],
								new GravityGeneratorSettings(
									generator.Enabled,
									generator.FieldSize,
									generator.GravityAcceleration
								)
							));

						gravityGenerators++;
						//Statics.AddGpsLocation($"{CubeType.GravityGenerator.ToString()} {gravityGenerators}", generator.GetPosition());
						continue;
					}

					IMyGravityGeneratorSphere generatorSphere = myCubeBlock as IMyGravityGeneratorSphere;
					if (generatorSphere != null)
					{
						_sphericalGravityGenerators.Add(
							new SphericalGravityGenerator (
								generatorSphere,
								(SphericalGravityGeneratorSettings)_warTimeSettings[CubeType.SphericalGravityGenerator],
								new SphericalGravityGeneratorSettings(
									generatorSphere.Enabled,
									generatorSphere.Radius,
									generatorSphere.GravityAcceleration
								)
							));

						sphericalGravityGenerators++;
						//Statics.AddGpsLocation($"{CubeType.SphericalGravityGenerator.ToString()} {sphericalGravityGenerators}", generatorSphere.GetPosition());
						continue;
					}

					IMySensorBlock mySensor = myCubeBlock as IMySensorBlock;
					if (mySensor != null)
					{
						_sensors.Add(new Sensor(
							mySensor, (SensorSettings)_warTimeSettings[CubeType.Sensor], new SensorSettings(mySensor.Enabled)
							));
						sensors++;
						//Statics.AddGpsLocation($"{CubeType.Timer.ToString()} {sensors}", mySensor.GetPosition());
						continue;
					}
					
					IMyTimerBlock myTimer = myCubeBlock as IMyTimerBlock;
					if (myTimer != null)
					{
						_timers.Add(new Timer(
							myTimer, (TimerSettings)_warTimeSettings[CubeType.Timer],new TimerSettings(myTimer.Enabled)
							));
						timers++;
						//Statics.AddGpsLocation($"{CubeType.Timer.ToString()} {timers}", myTimer.GetPosition());
						continue;
					}
					
					IMyAirVent vent = myCubeBlock as IMyAirVent;
					if (vent == null) continue;
					
					AirVentSettings archivedSettings = new AirVentSettings(
						vent.Enabled,
						vent.Depressurize
					);

					_airVents.Add(new AirVent(vent, (AirVentSettings)_warTimeSettings[CubeType.AirVent], archivedSettings));

					airVents++;
					//Statics.AddGpsLocation($"{CubeType.AirVent.ToString()} {airVents}", vent.GetPosition());
				}
				
				WriteToLog("EmergencyLockDownProtocol", $"Total Found - Air Vents: {airVents} | Doors: {doors} | Gravity Generators: {gravityGenerators} | Sensors: {sensors} |  Spherical Gravity Generators: {sphericalGravityGenerators} | Timers: {timers} | Turrets: {turrets} ", LogType.General);
			}
			catch (Exception e)
			{
				WriteToLog("EmergencyLockDownProtocol", $"Exception! {e}", LogType.Exception);
			}
		}

		public void Alert(AlertSetting alertSetting)
		{
			if (_alertEnabled) return;
			WriteToLog("Alert", $"Loading {alertSetting}...", LogType.General);

			SetAirVentSettings(alertSetting);
			SetDoorSettings(alertSetting);
			SetGravityGeneratorSettings(alertSetting);
			SetSphericalGravityGeneratorSettings(alertSetting);
			SetTimerSettings(alertSetting);
			SetTurretSettings(alertSetting);
			SetSensorSettings(alertSetting);
			
			WriteToLog("EnableAlert", $"Wartime Settings Loaded...", LogType.General);
			_alertEnabled = true;
		}

		private void SetAirVentSettings(AlertSetting emergencySetting)
		{
			for (int i = _airVents.Count - 1; i >= 0; i--)
			{
				if (!_airVents[i].SetAlert(emergencySetting))
					_airVents.RemoveAtFast(i);
			}
		}

		private void SetDoorSettings(AlertSetting emergencySetting)
		{
			for (int i = _doors.Count - 1; i >= 0; i--)
			{
				if (_doors[i].SetAlert(emergencySetting)) continue;
				_doors[i].Close();
				_doors.RemoveAtFast(i);
			}
		}

		private void SetGravityGeneratorSettings(AlertSetting emergencySetting)
		{
			for (int i = _gravityGenerators.Count - 1; i >= 0; i--)
			{
				if (!_gravityGenerators[i].SetAlert(emergencySetting))
					_gravityGenerators.RemoveAtFast(i);
			}
		}

		private void SetSensorSettings(AlertSetting emergencySetting)
		{
			for (int i = _sensors.Count - 1; i >= 0; i--)
			{
				if (!_sensors[i].SetAlert(emergencySetting))
					_sensors.RemoveAtFast(i);
			}
		}

		private void SetSphericalGravityGeneratorSettings(AlertSetting emergencySetting)
		{
			for (int i = _sphericalGravityGenerators.Count - 1; i >= 0; i--)
			{
				if (!_sphericalGravityGenerators[i].SetAlert(emergencySetting))
					_sphericalGravityGenerators.RemoveAtFast(i);
			}
		}

		private void SetTimerSettings(AlertSetting emergencySetting)
		{
			for (int i = _timers.Count - 1; i >= 0; i--)
			{
				if (!_timers[i].SetAlert(emergencySetting))
					_timers.RemoveAtFast(i);
			}
		}

		private void SetTurretSettings(AlertSetting emergencySetting)
		{
			for (int i = _turrets.Count - 1; i >= 0; i--)
			{
				if (!_turrets[i].SetAlert(emergencySetting))
					_turrets.RemoveAtFast(i);
			}
		}


		private readonly Dictionary<CubeType, object> _warTimeSettings = new Dictionary<CubeType, object>
		{
			{CubeType.AirVent, new AirVentSettings(true, true) },
			{CubeType.Door, new DoorSettings(false, true) },
			{CubeType.GravityGenerator, new GravityGeneratorSettings(true, new Vector3(150, 150, 150), 9.81f ) },
			{CubeType.SphericalGravityGenerator,  new SphericalGravityGeneratorSettings(true, 450f, 9.81f )},
			{CubeType.Sensor, new SensorSettings(false) },
			{CubeType.Timer, new TimerSettings(true) },
			{CubeType.Turret, new TurretSettings(true, true, true, true, false, true, false, true, true, 800) }
		};
	}
}
