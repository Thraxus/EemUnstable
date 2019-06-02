using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules
{
	public class EmergencyLockDownProtocol : LogBaseEvent, IDisposable
	{
		private enum CubeType
		{
			AirVent, Door, Turret, GravityGenerator
		}

		struct TurretSettings
		{
			public readonly bool Enabled;
			public readonly bool TargetCharacters;
			public readonly bool TargetLargeGrids;
			public readonly bool TargetMeteors;
			public readonly bool TargetMissiles;
			public readonly bool TargetSmallGrids;
			public readonly bool TargetStations;

			public readonly float Range;

			public TurretSettings(bool enabled, bool targetCharacters, bool targetLargeGrids, bool targetMeteors, bool targetMissiles, bool targetSmallGrids, bool targetStations, float range)
			{
				Enabled = enabled;
				TargetCharacters = targetCharacters;
				TargetLargeGrids = targetLargeGrids;
				TargetMeteors = targetMeteors;
				TargetMissiles = targetMissiles;
				TargetSmallGrids = targetSmallGrids;
				TargetStations = targetStations;
				Range = range;
			}
		}

		private readonly Dictionary<CubeType, TurretSettings> WarTimeSettings = new Dictionary<CubeType, TurretSettings>
		{
			{CubeType.Turret, new TurretSettings(true, true, true, true, true, true, true, 1000f) }
		};


		private Dictionary<long, TurretSettings> ArchivedSettings;

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

		private IMyLargeTurretBase myLargeTurretBase;
		IMyLargeInteriorTurret _myLargeInteriorTurretList;
		IMyLargeMissileTurret _myLargeMissileTurretList;
		IMyLargeGatlingTurret _myLargeGatlingTurretList;
		IMyAirVent _myAirVentList;
		IMyGravityGenerator _myGravityGeneratorList;
		IMyDoor _myDoorList;

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
						++turrets;
						//WriteToLog("EmergencyLockDownProtocol", $"Found a turret! {myCubeBlock.GetType()} {++turrets}", LogType.General);
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

			}
			catch (Exception e)
			{
				WriteToLog("EmergencyLockDownProtocol", $"Exception! {e}", LogType.Exception);
			}
		}

		public EmergencyLockDownProtocol(MyCubeGrid myCubeGrid)
		{
			_thisGrid = myCubeGrid;
			
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
