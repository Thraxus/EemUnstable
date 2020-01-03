using System;
using System.CodeDom;
using System.Collections.Generic;
using Eem.Thraxus.Bots.Modules.Support;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Microsoft.Xml.Serialization.GeneratedAssembly;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
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
		private readonly List<Antenna> _antennae = new List<Antenna>();
		private readonly List<Door> _doors = new List<Door>();
		private readonly List<GravityGenerator> _gravityGenerators = new List<GravityGenerator>();
		private readonly List<Sensor> _sensors = new List<Sensor>();
		private readonly List<SphericalGravityGenerator> _sphericalGravityGenerators = new List<SphericalGravityGenerator>();
		private readonly List<Timer> _timers = new List<Timer>();
		private readonly List<Turret> _turrets = new List<Turret>();

		private readonly List<ISetAlert> _setAlerts = new List<ISetAlert>();

		private readonly Dictionary<Type, Action<ISetAlert>> _listManager;

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
			_listManager = new Dictionary<Type, Action<ISetAlert>>()
			{
				{ typeof(AirVent), x => _airVents.Remove(x as AirVent) },
				{ typeof(Antenna), x => _antennae.Remove(x as Antenna) },
				{ typeof(Door), x => _doors.Remove(x as Door) },
				{ typeof(GravityGenerator), x => _gravityGenerators.Remove(x as GravityGenerator) },
				{ typeof(Sensor), x => _sensors.Remove(x as Sensor) },
				{ typeof(SphericalGravityGenerator), x => _sphericalGravityGenerators.Remove(x as SphericalGravityGenerator) },
				{ typeof(Timer), x => _timers.Remove(x as Timer) },
				{ typeof(Turret), x => _turrets.Remove(x as Turret) },
			};
		}
		
		public void Init()
		{
			int airVents = 0;
			int antennae = 0;
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
					IMyRadioAntenna radioAntenna = myCubeBlock as IMyRadioAntenna;
					if (radioAntenna != null && radioAntenna.OwnerId == _gridOwnerId)
					{
						Antenna x = new Antenna(radioAntenna);
						_antennae.Add(x);
						_setAlerts.Add(x);
						antennae++;
						continue;
					}

					IMyLargeTurretBase largeTurretBase = myCubeBlock as IMyLargeTurretBase;
					if (largeTurretBase != null && largeTurretBase.OwnerId == _gridOwnerId)
					{
						Turret x = new Turret(largeTurretBase);
						_turrets.Add(x);
						_setAlerts.Add(x);
						turrets++;
						continue;
					}


					IMyDoor door = myCubeBlock as IMyDoor;
					if (door != null && door.OwnerId == _gridOwnerId)
					{
						Door x = new Door(door);
						_doors.Add(x);
						_setAlerts.Add(x);
						doors++;
						continue;
					}

					IMyGravityGenerator generator = myCubeBlock as IMyGravityGenerator;
					if (generator != null && generator.OwnerId == _gridOwnerId)
					{
						GravityGenerator x = new GravityGenerator(generator);
						_gravityGenerators.Add(x);
						_setAlerts.Add(x);
						gravityGenerators++;
						continue;
					}

					IMyGravityGeneratorSphere generatorSphere = myCubeBlock as IMyGravityGeneratorSphere;
					if (generatorSphere != null && generatorSphere.OwnerId == _gridOwnerId)
					{
						SphericalGravityGenerator x = new SphericalGravityGenerator(generatorSphere);
						_sphericalGravityGenerators.Add(x);
						_setAlerts.Add(x);
						sphericalGravityGenerators++;
						continue;
					}

					IMySensorBlock mySensor = myCubeBlock as IMySensorBlock;
					if (mySensor != null && mySensor.OwnerId == _gridOwnerId)
					{
						Sensor x = new Sensor(mySensor);
						_sensors.Add(x);
						_setAlerts.Add(x);
						sensors++;
						continue;
					}
					
					IMyTimerBlock myTimer = myCubeBlock as IMyTimerBlock;
					if (myTimer != null && myTimer.OwnerId == _gridOwnerId)
					{
						Timer x = new Timer(myTimer);
						_timers.Add(x);
						_setAlerts.Add(x);
						timers++;
						continue;
					}
					
					IMyAirVent vent = myCubeBlock as IMyAirVent;
					if (vent != null && vent.OwnerId == _gridOwnerId)
					{
						AirVent x = new AirVent(vent);
						_airVents.Add(x);
						_setAlerts.Add(x);
						airVents++;
						continue;
					}
				}
				
				WriteToLog("EmergencyLockDownProtocol", $"Total Found - Air Vents: {airVents} | Antennas: {antennae} | Doors: {doors} | Gravity Generators: {gravityGenerators} | Sensors: {sensors} |  Spherical Gravity Generators: {sphericalGravityGenerators} | Timers: {timers} | Turrets: {turrets} ", LogType.General);
			}
			catch (Exception e)
			{
				WriteToLog("EmergencyLockDownProtocol", $"Exception! {e}", LogType.Exception);
			}
		}
		
		public void Alert(AlertSetting alertSetting)
		{
			if (_alertEnabled && alertSetting == AlertSetting.Wartime) return;
			WriteToLog("Alert", $"Loading {alertSetting}...", LogType.General);

			for (int i = _setAlerts.Count - 1; i >= 0; i--)
			{
				if (_setAlerts[i].SetAlert(alertSetting)) continue;
				Action<ISetAlert> action;
				_listManager.TryGetValue(typeof(AirVent), out action);
				action?.Invoke(_setAlerts[i]);
				_setAlerts.RemoveAtFast(i);
			}

			_alertEnabled = alertSetting != AlertSetting.Peacetime;
			WriteToLog("EnableAlert", $"{alertSetting} settings Loaded...", LogType.General);
		}
	}
}
