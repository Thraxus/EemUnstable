using System.Collections.Generic;
using VRage.Game;
using VRage.ModAPI;

namespace Eem.Thraxus.Common.Settings
{
	internal static class BotSettings
	{
		public const string EemAiPrefix = "[EEM_AI]";
		public const MyOwnershipShareModeEnum ShareMode = MyOwnershipShareModeEnum.Faction;
		public const MyEntityUpdateEnum CoreUpdateSchedule = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;
		public const int UnownedGridDetectionRange = 250;
		public const int MaxAllowedThrusterDamage = 500;
		
		#region ModId's for mods we need protection from

		public static readonly List<ulong> ModsToWatch = new List<ulong>
		{
			BarsModId, EnergyShieldsModId, DefenseShields, WeaponCore
		};

		public const ulong BarsModId = 857053359;

		public const ulong EnergyShieldsModId = 484504816;

		public const ulong DefenseShields = 1365616918;

		public const ulong WeaponCore = 1918681825;

		#endregion
	}
}
