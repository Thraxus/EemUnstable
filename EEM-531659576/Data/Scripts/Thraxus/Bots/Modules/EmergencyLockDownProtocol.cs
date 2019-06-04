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
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using IMyEntity = VRage.Game.ModAPI.Ingame.IMyEntity;

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

		private readonly Dictionary<CubeType, object> _warTimeSettings = new Dictionary<CubeType, object>
		{
			{CubeType.Turret, new TurretSettings(true, true, true, false, true, false, true, true, 800) },
			{CubeType.AirVent,new AirVentSettings(true, false) }
		};


		private readonly Dictionary<IMyLargeTurretBase, TurretSettings> _archivedTurretSettings;
		private readonly List<IMyLargeTurretBase> _turretList;

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

						_archivedTurretSettings.Add(largeTurretBase, archiveSettings);
						_turretList.Add(largeTurretBase);
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
				foreach (KeyValuePair<IMyLargeTurretBase, TurretSettings> archivedTurretSetting in _archivedTurretSettings)
					WriteToLog("EmergencyLockDownProtocol", $"{archivedTurretSetting.Key} | {archivedTurretSetting.Value}", LogType.General);

			}
			catch (Exception e)
			{
				WriteToLog("EmergencyLockDownProtocol", $"Exception! {e}", LogType.Exception);
			}
		}

		public void EnableAlert()
		{
			if (AlertEnabled) return;
			foreach (IMyLargeTurretBase turretBase in _turretList)
			{
				if (turretBase == null) continue;
				WriteToLog("EnableAlert", $"{turretBase.EntityId} - Loading Wartime Settings...", LogType.General);
				turretBase.Enabled = ((TurretSettings)_warTimeSettings[CubeType.Turret]).Enabled;

				turretBase.SetValueBool("TargetMeteors", ((TurretSettings)_warTimeSettings[CubeType.Turret]).TargetCharacters);
				turretBase.SetValueBool("TargetLargeShips", ((TurretSettings)_warTimeSettings[CubeType.Turret]).TargetLargeShips);
				turretBase.SetValueBool("TargetMeteors", ((TurretSettings)_warTimeSettings[CubeType.Turret]).TargetMeteors);
				turretBase.SetValueBool("TargetMissiles", ((TurretSettings)_warTimeSettings[CubeType.Turret]).TargetMissiles);
				turretBase.SetValueBool("TargetNeutrals", ((TurretSettings)_warTimeSettings[CubeType.Turret]).TargetNeutrals);
				turretBase.SetValueBool("TargetSmallShips", ((TurretSettings)_warTimeSettings[CubeType.Turret]).TargetSmallShips);
				turretBase.SetValueBool("TargetStations", ((TurretSettings)_warTimeSettings[CubeType.Turret]).TargetStations);

				turretBase.SetValueFloat("Range", ((TurretSettings)_warTimeSettings[CubeType.Turret]).Range);
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

		//using Sandbox.ModAPI.Interfaces;


		//block.SetValueFloat("Range", 800);
		//block.SetValueBool("TargetMeteors", true);
		//block.SetValueBool("TargetMissiles", true);
		//block.SetValueBool("TargetSmallShips", true);
		//block.SetValueBool("TargetLargeShips", true);
		//block.SetValueBool("TargetCharacters", true);
		//block.SetValueBool("TargetStations", true);
		//block.SetValueBool("TargetNeutrals", true);

		public void DisableAlert()
		{
			if (!AlertEnabled) return;
			foreach (KeyValuePair<IMyLargeTurretBase, TurretSettings> archivedTurretSetting in _archivedTurretSettings)
			{
				if (archivedTurretSetting.Key == null) continue;
				WriteToLog("DisableAlert", $"{archivedTurretSetting.Key.EntityId} - Loading Default Settings...", LogType.General);
				archivedTurretSetting.Key.Enabled = archivedTurretSetting.Value.Enabled;

				archivedTurretSetting.Key.SetValueBool("TargetMeteors", archivedTurretSetting.Value.TargetCharacters);
				archivedTurretSetting.Key.SetValueBool("TargetLargeShips", archivedTurretSetting.Value.TargetLargeShips);
				archivedTurretSetting.Key.SetValueBool("TargetMeteors", archivedTurretSetting.Value.TargetMeteors);
				archivedTurretSetting.Key.SetValueBool("TargetMissiles", archivedTurretSetting.Value.TargetMissiles);
				archivedTurretSetting.Key.SetValueBool("TargetNeutrals", archivedTurretSetting.Value.TargetNeutrals);
				archivedTurretSetting.Key.SetValueBool("TargetSmallShips", archivedTurretSetting.Value.TargetSmallShips);
				archivedTurretSetting.Key.SetValueBool("TargetStations", archivedTurretSetting.Value.TargetStations);
				
				archivedTurretSetting.Key.SetValueFloat("Range", archivedTurretSetting.Value.Range);
			}

			AlertEnabled = false;
		}

		public EmergencyLockDownProtocol(MyCubeGrid myCubeGrid)
		{
			_thisGrid = myCubeGrid;
			_archivedTurretSettings = new Dictionary<IMyLargeTurretBase, TurretSettings>();
			_turretList = new List<IMyLargeTurretBase>();

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
