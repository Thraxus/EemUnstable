using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Bots.Utilities;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Modules.ModManagers
{
	public static class BuildAndRepairSystem
	{
		private static IEnumerable<string> NanobotBuildAndRepairDefinitions { get; } = new List<string>()
		{
			"SELtdNanobotBuildAndRepairSystem",
			"SELtdLargeNanobotBuildAndRepairSystem",
			"SELtdSmallNanobotBuildAndRepairSystem"
		};

		public static List<IMyEntity> DetectAllBars(Vector3D detectionCenter, double range)
		{
			StaticMethods.AddGpsLocation($"Detecting BaRS {range}", detectionCenter);

			List<IMyEntity> barsList = new List<IMyEntity>();
			foreach (MyEntity ent in StaticMethods.DetectAllEntitiesInSphere(detectionCenter, range))
			{
				MyCubeGrid myGrid = ent as MyCubeGrid;
				if (myGrid == null) return barsList;
				foreach (MyCubeBlock block in myGrid.GetFatBlocks())
				{
					if (!(block is IMyShipWelder)) continue;
					if (!NanobotBuildAndRepairDefinitions.Any(x => block.BlockDefinition.BlockPairName.Contains(x))) continue;
					(block as IMyShipWelder).Enabled = false; //Disables the BaRS
					(block as IMyShipWelder).Render.ColorMaskHsv = new Vector3(0, 0, 0.05f); //Colors it red
					barsList.Add(block);
				}
			}
			return barsList;
		}
	}
}
