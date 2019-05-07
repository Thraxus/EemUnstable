using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRageMath;

namespace Eem.Thraxus.Bots.Modules.ModManagers
{
	class BuildAndRepairSystem
	{
		public static List<string> NanobotBuildAndRepairDefinitions { get; } = new List<string>()
		{
			"SELtdNanobotBuildAndRepairSystem",
			"SELtdLargeNanobotBuildAndRepairSystem",
			"SELtdSmallNanobotBuildAndRepairSystem"
		};

		public static List<long> DetectNanobots(Vector3D detectionCenter, double range)
		{
			List<long> barsList = new List<long>();
			foreach (MyEntity ent in Utilities.StaticMethods.DetectAllEntitiesInSphere(detectionCenter, range))
			{
				MyCubeGrid myGrid = ent as MyCubeGrid;
				if (myGrid == null) return barsList;
				foreach (MyCubeBlock block in myGrid.GetFatBlocks())
				{
					if (!(block is IMyShipWelder)) continue;
					if (!NanobotBuildAndRepairDefinitions.Any(x => block.BlockDefinition.BlockPairName.Contains(x))) continue;
					(block as IMyShipWelder).Enabled = false; //Disables the BaRS
					(block as IMyShipWelder).Render.ColorMaskHsv = new Vector3(0, 0, 0.05f); //Colors it red
					barsList.Add((block as IMyShipWelder).EntityId);
				}
			}
			return barsList;
		}
	}
}
