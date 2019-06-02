using System;
using System.Collections.Generic;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Settings
{
	public static class Settings
	{

		#region Constant Values

		public const bool ForcedDebugMode = false;

		public const string ChatCommandPrefix = "chatCommand";
		public const string SettingsFileName = "ModName-UserConfig.xml";
		public const string StaticDebugLogName = "StaticLog-Debug";
		public const string ExceptionLogName = "Exception";
		public const string StaticGeneralLogName = "StaticLog-General";
		public const string ProflingLogName = "Profiler";

		public const ushort NetworkId = 16759;
		
		#endregion


		#region Reference Values

		public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;

		public const int DefaultLocalMessageDisplayTime = 5000;
		public const int DefaultServerMessageDisplayTime = 10000;
		public const int TicksPerMinute = TicksPerSecond * 60;
		public const int TicksPerSecond = 60;

		public static Random Random { get; } = new Random();

		#endregion


		#region Factions

		/// <summary>
		/// Faction War cooldown period
		///		15 minute default cooldown, 2 minute in Debug Mode
		/// </summary>
		public static int FactionNegativeRelationshipCooldown => DebugMode ? (TicksPerMinute * 2) : (TicksPerMinute * 15);
		public const int FactionNegativeRelationshipAssessment = TicksPerSecond;
		public const int FactionMendingRelationshipAssessment = TicksPerMinute + 20;  // Don't really want these assessments firing at the same time

		/// <summary>
		/// These factions are considered lawful. When they go hostile towards someone,
		/// they also make the police (SEPD) and army (UCMF) go hostile.
		/// </summary>
		public static List<string> LawfulFactionsTags { get; } = new List<string>
		{
			"UCMF", "SEPD", "CIVL", "ISTG", "MA-I", "EXMC", "KUSS", "HS", "AMPH", "IMDC" };

		/// <summary>
		/// 
		/// </summary>
		public static List<string> AllNpcFactions { get; } = new List<string>
		{
			"SPRT", "CIVL", "UCMF", "SEPD", "ISTG", "AMPH", "KUSS", "HS", "MMEC", "MA-I", "EXMC", "IMDC"
		};

		/// <summary>
		/// 
		/// </summary>
		public static List<string> EnforcementFactionsTags { get; } = new List<string>
		{
			"SEPD", "UCMF"
		};

		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<string> PlayerFactionExclusionList { get; } = new List<string>
		{
			"Pirate", "Rogue", "Outlaw", "Bandit"
		};

		#endregion


		#region Cleanup

		// CleanUp.cs's constants:
		public static bool DisableCleanup { get; } = false;

		// text required in the RC's CustomData to be even considrered for removal
		public static string CleanupRcTag { get; } = "[EEM_AI]";

		// any of these is required to be in RC's CustomData for the grid to be removed.
		public static string[] CleanupRcExtraTags { get; } = { "Type:Fighter", "Type:Freighter" };

		// clamp the world's view range to this minimum value, which is used for removing distant ships
		public static int CleanupMinRange { get; } = 40000;

		// world setting of max drones
		public static int ForceMaxDrones { get; } = 20;

		// BuyShip.cs's constants:

		// the particle effect name (particles.sbc) to spawn after the ship is spawned
		public static string SpawnEffectName { get; } = "EEMWarpIn";

		// the ship's bounding sphere radius is used for the particle effect scale, this value scales that radius.
		public static float SpawnEffectScale { get; } = 0.2f;

		// radius of the area around the spawn zone to check for foreign objects
		public static float SpawnAreaRadius { get; } = 15f;

		// the argument that gets used by the mod to call the PB when the buy ship fails due to the position being blocked
		public static string PbargFailPositionblocked { get; } = "fail-positionblocked";

		// in case the PB doesn't work, notify the players within this radius of the failed spawn ship
		public static float SpawnFailNotifyDistance { get; } = 50;

		#endregion


		#region User Configuration

		public static bool DebugMode = false;
		public static bool ProfilingEnabled = false;


		#endregion
	}
}
