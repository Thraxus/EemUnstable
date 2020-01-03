using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
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
		private readonly List<ISetAlert> _setAlerts = new List<ISetAlert>();
		
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
						_setAlerts.Add(new Antenna(radioAntenna));
						antennae++;
						continue;
					}

					IMyLargeTurretBase largeTurretBase = myCubeBlock as IMyLargeTurretBase;
					if (largeTurretBase != null && largeTurretBase.OwnerId == _gridOwnerId)
					{
						_setAlerts.Add(new Turret(largeTurretBase));
						turrets++;
						continue;
					}


					IMyDoor door = myCubeBlock as IMyDoor;
					if (door != null && door.OwnerId == _gridOwnerId)
					{
						Door x = new Door(door);
						_setAlerts.Add(x);
						doors++;
						continue;
					}

					IMyGravityGenerator generator = myCubeBlock as IMyGravityGenerator;
					if (generator != null && generator.OwnerId == _gridOwnerId)
					{
						_setAlerts.Add(new GravityGenerator(generator));
						gravityGenerators++;
						continue;
					}

					IMyGravityGeneratorSphere generatorSphere = myCubeBlock as IMyGravityGeneratorSphere;
					if (generatorSphere != null && generatorSphere.OwnerId == _gridOwnerId)
					{
						_setAlerts.Add(new SphericalGravityGenerator(generatorSphere));
						sphericalGravityGenerators++;
						continue;
					}

					IMySensorBlock mySensor = myCubeBlock as IMySensorBlock;
					if (mySensor != null && mySensor.OwnerId == _gridOwnerId)
					{
						_setAlerts.Add(new Sensor(mySensor));
						sensors++;
						continue;
					}
					
					IMyTimerBlock myTimer = myCubeBlock as IMyTimerBlock;
					if (myTimer != null && myTimer.OwnerId == _gridOwnerId)
					{
						_setAlerts.Add(new Timer(myTimer));
						timers++;
						continue;
					}
					
					IMyAirVent vent = myCubeBlock as IMyAirVent;
					if (vent != null && vent.OwnerId == _gridOwnerId)
					{
						_setAlerts.Add(new AirVent(vent));
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
				_setAlerts[i].Close();
				_setAlerts.RemoveAtFast(i);
			}

			_alertEnabled = alertSetting != AlertSetting.Peacetime;
			WriteToLog("EnableAlert", $"{alertSetting} settings Loaded...", LogType.General);
		}
	}
}
