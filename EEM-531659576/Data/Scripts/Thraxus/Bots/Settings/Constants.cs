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
	}
}
