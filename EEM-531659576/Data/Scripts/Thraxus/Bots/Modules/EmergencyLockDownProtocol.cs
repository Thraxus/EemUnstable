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
using VRage.ModAPI;
using IMyDoor = Sandbox.ModAPI.IMyDoor;
using IMyEntityIngame = VRage.Game.ModAPI.Ingame.IMyEntity;
using IMyLargeTurretBase = Sandbox.ModAPI.IMyLargeTurretBase;
using IMyTerminalBlock = Sandbox.ModAPI.IMyTerminalBlock;

namespace Eem.Thraxus.Bots.Modules
{
	public class EmergencyLockDownProtocol : LogBaseEvent, IDisposable
	{
		private enum CubeType
		{
			AirVent, Door, Turret, GravityGenerator
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

			public AirVentSettings(bool depressurize)
			{
				Depressurize = depressurize;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Depressurize}";
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

		private readonly Dictionary<CubeType, object> _warTimeSettings = new Dictionary<CubeType, object>
		{
			{CubeType.Turret, new TurretSettings(true, true, true, false, true, false, true, true, 800) },
			{CubeType.AirVent, new AirVentSettings(true) },
			{CubeType.Door, new DoorSettings(false) }
		};

		
		public void Init()
		{
			int turrets = 0;
			int airvents = 0;
			int doors = 0;
			int gravityGenerators = 0;

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
						//Actions: OnOff | Toggle block On/ Off
						//Actions: OnOff_On | Toggle block On
						//Actions: OnOff_Off | Toggle block Off
						//Actions: ShowOnHUD | Show on HUD On / Off
						//Actions: ShowOnHUD_On | Show on HUD On
						//Actions: ShowOnHUD_Off | Show on HUD Off
						//Actions: Open | Open / Closed
						//Actions: Open_On | Open
						//Actions: Open_Off | Closed
						door.OnDoorStateChanged += delegate(IMyDoor myDoor, bool b)
						{
							if (myDoor.OwnerId != _gridOwnerId) return;
							WriteToLog("OnDoorStateChanged", $"{myDoor.CustomName} {b}", LogType.General);
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
						
						//door.SetValueBool("Open", true);
						//WriteToLog("EmergencyLockDownProtocol", $"{door.EntityId} | {door.CustomName}", LogType.General);
						//PrintTerminalActions(door);		
						++doors;
						//WriteToLog("EmergencyLockDownProtocol", $"Found a door! {myCubeBlock.GetType()} {++doors}", LogType.General);
						StaticMethods.AddGpsLocation($"{CubeType.Door.ToString()} {doors}", door.GetPosition());
					}

					IMyGravityGeneratorBase generatorBase = myCubeBlock as IMyGravityGeneratorBase;
					if (generatorBase != null)
					{
						//Actions: OnOff | Toggle block On/ Off
						//Actions: OnOff_On | Toggle block On
						//Actions: OnOff_Off | Toggle block Off
						//Actions: ShowOnHUD | Show on HUD On / Off
						//Actions: ShowOnHUD_On | Show on HUD On
						//Actions: ShowOnHUD_Off | Show on HUD Off
						//Actions: IncreaseWidth | Increase Field width
						//Actions: DecreaseWidth | Decrease Field width
						//Actions: IncreaseHeight | Increase Field height
						//Actions: DecreaseHeight | Decrease Field height
						//Actions: IncreaseDepth | Increase Field depth
						//Actions: DecreaseDepth | Decrease Field depth
						//Actions: IncreaseGravity | Increase Acceleration
						//Actions: DecreaseGravity | Decrease Acceleration

						WriteToLog("EmergencyLockDownProtocol", $"{generatorBase.EntityId} | {generatorBase.CustomName}", LogType.General);
						PrintTerminalActions(generatorBase);
						++gravityGenerators;
						//WriteToLog("EmergencyLockDownProtocol", $"Found a Gravity Generator! {myCubeBlock.GetType()} {++gravityGenerators}", LogType.General);
						StaticMethods.AddGpsLocation($"{CubeType.GravityGenerator.ToString()} {gravityGenerators}", generatorBase.GetPosition());
					}

					IMyAirVent vent = myCubeBlock as IMyAirVent;
					if (vent == null) continue;
					
					AirVentSettings archivedSettings = new AirVentSettings(
						vent.Depressurize
					);

					_gridAirVentSettings.Add(new GridAirVents(vent, (AirVentSettings)_warTimeSettings[CubeType.AirVent], archivedSettings));

					++airvents;
					StaticMethods.AddGpsLocation($"{CubeType.AirVent.ToString()} {airvents}", vent.GetPosition());
				}
				
				WriteToLog("EmergencyLockDownProtocol", $"Total Found - Turrets: {turrets} | Air Vents: {airvents} | Doors: {doors} | Gravity Generators: {gravityGenerators}", LogType.General);
				foreach (GridTurrets gridTurretSettings in _gridTurretSettings)
					WriteToLog("EmergencyLockDownProtocol", $"{gridTurretSettings}", LogType.General);

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

				airVent.Enabled = _gridAirVentSettings[index].WarTimeSettings.Depressurize;
			}

			_alertEnabled = true;
			WriteToLog("EnableAlert", $"Wartime Settings Loaded...", LogType.General);
		}

		public void DisableAlert()
		{
			if (!_alertEnabled) return;

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

				airVent.Enabled = _gridAirVentSettings[index].PeaceTimeSettings.Depressurize;
			}

			_alertEnabled = false;
		}

		private void PrintTerminalActions(IMyEntity block)
		{
			IMyTerminalBlock myTerminalBlock = block as IMyTerminalBlock;
			if (myTerminalBlock == null) return;
			List<ITerminalAction> results = new List<ITerminalAction>();
			myTerminalBlock.GetActions(results);
			foreach (ITerminalAction terminalAction in results)
			{
				WriteToLog("PrintTerminalActions", $"Actions: {terminalAction.Id} | {terminalAction.Name}", LogType.General);
			}
		}


		private bool _alertEnabled;
		private readonly long _gridOwnerId;


		private readonly MyCubeGrid _thisGrid;
		private readonly List<GridTurrets> _gridTurretSettings;
		private readonly List<GridDoors> _gridDoorSettings;
		private readonly List<GridAirVents> _gridAirVentSettings;

		public EmergencyLockDownProtocol(MyCubeGrid myCubeGrid)
		{
			_thisGrid = myCubeGrid;
			_gridOwnerId = myCubeGrid.BigOwners[0];

			_gridTurretSettings = new List<GridTurrets>();
			_gridAirVentSettings = new List<GridAirVents>();
			_gridDoorSettings = new List<GridDoors>();
			//_archivedTurretSettings = new Dictionary<IMyLargeTurretBase, TurretSettings>();
			//_turretList = new List<IMyLargeTurretBase>();

			//MyCubeGrid myCube = new MyCubeGrid();
			//myCube.getf
			//List<IMySlimBlock> blocks = new List<IMySlimBlock>();
			//myCubeGrid.blocks  .GetBlocks(blocks);




			//myLargeTurretBase = new MyLargeGatlingTurret();
			//myLargeTurretBase.

			//IMyTerminalActionsHelper myTerminalActionsHelper = new MyTerminalControlFactoryHelper();

			//IMyTerminalControls myTerminalControls = new MyTerminalControls();

			//MyObjectBuilder_TurretBase myObjectBuilderTurretBase = new MyObjectBuilder_TurretBase();

			//myObjectBuilderTurretBase.TargetLargeGrids = false;

			//myLargeTurretBase.tar


			//_myLargeInteriorTurretList.Enabled;
			//_myLargeInteriorTurretList.tar

			//_thisGrid = myCubeGrid;
			//_myLargeInteriorTurretList = new List<IMyLargeInteriorTurret>();
			//_myLargeMissileTurretList = new List<IMyLargeMissileTurret>();
			//_myLargeGatlingTurretList = new List<IMyLargeGatlingTurret>();
			//_myAirVentList = new List<IMyAirVent>();
			//_myGravityGeneratorList = new List<IMyGravityGenerator>();
			//_myDoorList = new List<IMyDoor>();

			//ParseCubes();
		}

		public void Unload()
		{
			//_myLargeInteriorTurretList.Clear();
			//_myLargeMissileTurretList.Clear();
			//_myLargeGatlingTurretList.Clear();
			//_myAirVentList.Clear();
			//_myGravityGeneratorList.Clear();
			//_myDoorList.Clear();
		}
		private void ParseCubes()
		{
			//List<IMySlimBlock> slimBlocks = new List<IMySlimBlock>();
			//_thisGrid.GetBlocks(slimBlocks, block => block.FatBlock != null);
			//foreach (IMySlimBlock cubeBlock in slimBlocks)
			//{
			//	if(cubeBlock.GetType() == typeof(IMyLargeGatlingTurret))
			//	{

			//	}
			//}
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
