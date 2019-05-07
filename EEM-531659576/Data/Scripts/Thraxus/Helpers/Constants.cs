using System;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Helpers
{
	public static class Constants
	{
		#region General

		public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;

		internal static Random Random { get; } = new Random();

		public static bool DebugMode { get; } = false;

		public static bool EnableProfilingLog { get; } = true;

		public static bool EnableGeneralLog { get; } = true;

		public static bool DisableCleanup { get; } = false;
		
		public const int TicksPerSecond = 60;

		public const int TicksPerMinute = TicksPerSecond * 60;

		public const int DefaultLocalMessageDisplayTime = 5000;

		public const int DefaultServerMessageDisplayTime = 10000;

		#endregion

		#region Factions

		/// <summary>
		/// Faction War cooldown period
		///		15 minute default cooldown, 2 minute in Debug Mode
		/// </summary>
		public static int FactionNegativeRelationshipCooldown => DebugMode ? (TicksPerMinute * 2) : (TicksPerMinute * 15);

		public const int FactionNegativeRelationshipAssessment = TicksPerSecond;

		public const int FactionMendingRelationshipAssessment = TicksPerMinute + 20;  // Don't really want these assessments firing at the same time

		#endregion

		#region Networking

		public static ushort EemCoreNetworkId { get; } = 16759;

		public const string DeclareWarMessagePrefix = "DeclareWar";

		public const string DeclarePeaceMessagePrefix = "DeclarePeace";

		public const string AcceptPeaceMessagePrefix = "AcceptPeace";

		public const string RejectPeaceMessagePrefix = "RejectPeace";

		#endregion

		// CleanUp.cs's constants:

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
	}
}