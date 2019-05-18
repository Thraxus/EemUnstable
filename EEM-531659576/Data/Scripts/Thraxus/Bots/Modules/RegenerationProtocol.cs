using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Eem.Thraxus.Bots.Modules
{
	public class RegenerationProtocol : LogEventBase
	{
		private readonly MyObjectBuilder_CubeGrid _myOb;
		private readonly IMyEntity _myEntity;
		private readonly List<Vector3I> _myObMins;

		public RegenerationProtocol(IMyEntity myEntity)
		{
			_myOb = (MyObjectBuilder_CubeGrid) ((MyCubeGrid) myEntity).GetObjectBuilder().Clone();
			_myEntity = myEntity;
			_myObMins = new List<Vector3I>();
			foreach (MyObjectBuilder_CubeBlock myObCubeBlock in _myOb.CubeBlocks)
				_myObMins.Add(myObCubeBlock.Min);
			
		}

		public void HealExisting()
		{

		}

		public void ReplaceMissing()
		{
			
		}

		public void ReportBlockState()
		{
			try
			{
				WriteToLog("ReportBlockState",$"Grid: {_myOb.CubeBlocks.Count} - Entity: {((MyCubeGrid)_myEntity).CubeBlocks.Count}", LogType.General);
				List<Vector3I> vector3Is = _myObMins.Except(((MyCubeGrid) _myEntity).CubeBlocks.Select(x => ((IMySlimBlock) x).Min)).ToList();
				List<MyObjectBuilder_CubeBlock> missingBlocks = new List<MyObjectBuilder_CubeBlock>();
				if (vector3Is.Count <= 0) return;
				foreach (Vector3I missingMin in vector3Is)
				{
					foreach (MyObjectBuilder_CubeBlock cubeBlock in _myOb.CubeBlocks)
					{
						if ((Vector3I)cubeBlock.Min != missingMin) continue;
						missingBlocks.Add(cubeBlock);
					}
				}

				if (missingBlocks.Count <= 5) return;
				foreach (MyObjectBuilder_CubeBlock myObCubeBlock in missingBlocks)
				{
					myObCubeBlock.BuildPercent = 0.1f;
					myObCubeBlock.ConstructionInventory = new MyObjectBuilder_Inventory();
					((IMyCubeGrid) _myEntity).AddBlock(myObCubeBlock, false);
				}
			}
			catch (Exception e)
			{
				WriteToLog("ReportBlockState",$"Exception! {e}", LogType.Exception);
			}

			//WriteToLog("ReportBlockState", $"Missing: {myObCubeBlock.SubtypeName} {((MyCubeGrid)_myEntity).CanPlaceBlock(myObCubeBlock.Min, Vector3I.Zero, myObCubeBlock.BlockOrientation, MyDefinitionManager.Static.GetCubeBlockDefinition(myObCubeBlock))}", LogType.General);

			//foreach (MyObjectBuilder_CubeBlock myObCubeBlock in missingBlocks)
			//	((MyCubeGrid) _myEntity).CanPlaceBlock(myObCubeBlock.Min, Vector3I.Zero, myObCubeBlock.BlockOrientation, MyDefinitionManager.Static.GetCubeBlockDefinition(myObCubeBlock));


			//WriteToLog("ReportBlockState", $"Missing: {vector3Is.Count}", LogType.General);
			//foreach (MyObjectBuilder_CubeBlock myObCubeBlock in _myOb.CubeBlocks)
			//{
			//	foreach (IMySlimBlock mySlimBlock in ((MyCubeGrid)_myEntity).CubeBlocks)
			//	{
			//		if (mySlimBlock.Min == (Vector3I)myObCubeBlock.Min) continue;
			//		WriteToLog("ReportBlockState", $"Block Missing: {mySlimBlock.GetType()}", LogType.General);
			//	}
			//}
		}
	}
}
