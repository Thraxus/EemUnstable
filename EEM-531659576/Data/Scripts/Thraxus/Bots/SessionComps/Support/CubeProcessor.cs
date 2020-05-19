using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots.SessionComps.Support
{
	public static class CubeProcessor
	{
		public static ProcessedCube ProcessCube(IMySlimBlock block)
		{
			StaticLog.WriteToLog("CubeProcessor.ProcessCube", $"{block.BlockDefinition.Id.TypeId} | {block.BlockDefinition.Id.SubtypeId}", LogType.General);

			if (block.BlockDefinition.Context.ModName == "")
			{
				// this is where vanilla blocks go for processing
			}
			else
			{
				// this is where modded blocks go for processing
				
			}

			return new ProcessedCube();
		}
	}
}
