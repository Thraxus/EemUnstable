using System.Collections.Generic;
using VRage.Game;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.Settings
{
	internal static class Constants
	{
		public const string EemAiPrefix = "[EEM_AI]";
		public const MyOwnershipShareModeEnum ShareMode = MyOwnershipShareModeEnum.Faction;
		public const MyEntityUpdateEnum CoreUpdateSchedule = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;
		public const int UnownedGridDetectionRange = 250;
		
		#region ModId's for mods we need protection from

		public static readonly List<ulong> ModsToWatch = new List<ulong>()
		{
			BarsModId
		};

		public const ulong BarsModId = 857053359;

		#endregion
	}
}
