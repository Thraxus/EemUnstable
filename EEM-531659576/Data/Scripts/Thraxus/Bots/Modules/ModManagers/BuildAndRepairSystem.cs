using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Modules.ModManagers
{
	public static class BuildAndRepairSystem
	{
		private static readonly Type BarsType = typeof(MyObjectBuilder_ShipWelder);

		private static IEnumerable<string> BuildAndRepairDefinitions { get; } = new List<string>()
		{
			"SELtdNanobotBuildAndRepairSystem",
			"SELtdLargeNanobotBuildAndRepairSystem",
			"SELtdSmallNanobotBuildAndRepairSystem"
		};

		public static List<IMyEntity> DetectAllBars(Vector3D detectionCenter, double range)
		{
			StaticMethods.AddGpsLocation($"Detecting BaRS {range}", detectionCenter);

			// Linq, but possibly slower.  Need to profile.
			//return StaticMethods.DetectAllEntitiesInSphere(detectionCenter, range)
			//	.OfType<MyCubeGrid>().SelectMany(myGrid => myGrid.GetFatBlocks())
			//	.OfType<IMyShipWelder>().Where(block => BuildAndRepairDefinitions.Any(x => block.BlockDefinition.SubtypeId.Contains(x)))
			//	.Cast<IMyEntity>().ToList();

			List<IMyEntity> barsList = new List<IMyEntity>();
			foreach (MyEntity ent in StaticMethods.DetectAllEntitiesInSphere(detectionCenter, range))
			{
				MyCubeGrid myGrid = ent as MyCubeGrid;
				if (myGrid == null) continue;// return barsList;
				foreach (MyCubeBlock block in myGrid.GetFatBlocks())
				{
					if (!(block is IMyShipWelder)) continue;
					if (!BuildAndRepairDefinitions.Any(x => block.BlockDefinition.BlockPairName.Contains(x))) continue;
					barsList.Add(block);
				}
			}
			return barsList;
		}
	}
}
