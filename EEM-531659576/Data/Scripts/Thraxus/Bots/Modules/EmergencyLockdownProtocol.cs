using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage.ModAPI;
using IMyEntityIngame = VRage.Game.ModAPI.Ingame.IMyEntity;

namespace Eem.Thraxus.Bots.Modules
{
	public class EmergencyLockDownProtocol : LogBaseEvent, IDisposable
	{
		private bool AlertEnabled;

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

		private readonly List<GridTurrets> _gridTurretSettings;

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

		private readonly Dictionary<CubeType, object> _warTimeSettings = new Dictionary<CubeType, object>
		{
			{CubeType.Turret, new TurretSettings(true, true, true, false, true, false, true, true, 800) },
			{CubeType.AirVent, new AirVentSettings(true, false) }
		};

		//private readonly Dictionary<IMyLargeTurretBase, TurretSettings> _archivedTurretSettings;
		//private readonly List<IMyLargeTurretBase> _turretList;

		private struct AirVentSettings
		{
			public readonly bool Depressurize;
			public readonly bool PressurizationEnabled;

			public AirVentSettings(bool depressurize, bool pressurizationEnabled)
			{
				Depressurize = depressurize;
				PressurizationEnabled = pressurizationEnabled;
			}
		}

		private readonly MyCubeGrid _thisGrid;

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
						++doors;
						//WriteToLog("EmergencyLockDownProtocol", $"Found a door! {myCubeBlock.GetType()} {++doors}", LogType.General);
						StaticMethods.AddGpsLocation($"{CubeType.Door.ToString()} {doors}", door.GetPosition());
					}

					IMyGravityGeneratorBase generatorBase = myCubeBlock as IMyGravityGeneratorBase;
					if (generatorBase != null)
					{
						++gravityGenerators;
						//WriteToLog("EmergencyLockDownProtocol", $"Found a Gravity Generator! {myCubeBlock.GetType()} {++gravityGenerators}", LogType.General);
						StaticMethods.AddGpsLocation($"{CubeType.GravityGenerator.ToString()} {gravityGenerators}", generatorBase.GetPosition());
					}

					IMyAirVent vent = myCubeBlock as IMyAirVent;
					if (vent == null) continue;
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
			if (AlertEnabled) return;
			for (int index = _gridTurretSettings.Count - 1; index >= 0; index--)
			{
				IMyLargeTurretBase turretBase = _gridTurretSettings[index].Turret;
				if (!turretBase.InScene)
				{
					_gridTurretSettings.RemoveAtFast(index);
					continue;
				}

				WriteToLog("EnableAlert", $"{turretBase.EntityId} - Loading Wartime Settings...", LogType.General);
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

			AlertEnabled = true;
		}

		private void PrintTerminalActions(IMyEntity block)
		{
			IMyTerminalBlock myTerminalBlock = block as IMyTerminalBlock;
			if (myTerminalBlock == null) return;
			List<ITerminalAction> results = new List<ITerminalAction>();
			myTerminalBlock.GetActions(results);
			foreach (ITerminalAction terminalAction in results)
			{
				WriteToLog("TurretControls", $"Actions: {terminalAction.Id} | {terminalAction.Name}", LogType.General);
			}
		}

		public void DisableAlert()
		{
			if (!AlertEnabled) return;
			for (int index = _gridTurretSettings.Count - 1; index >= 0; index--)
			{
				IMyLargeTurretBase turretBase = _gridTurretSettings[index].Turret;
				if (!turretBase.InScene)
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
			AlertEnabled = false;
		}

		public EmergencyLockDownProtocol(MyCubeGrid myCubeGrid)
		{
			_thisGrid = myCubeGrid;
			_gridTurretSettings = new List<GridTurrets>();
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
