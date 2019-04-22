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

		public static bool DisableAi { get; } = false;

		public static bool EnableProfilingLog { get; } = true;

		public static bool EnableGeneralLog { get; } = true;

		public static bool DisableCleanup { get; } = false;
		
		public static string DebugLogName { get; } = "EEM_Debug"; 

		public static string ProfilingLogName { get; } = "EEM_Profiling";

		public static string GeneralLogName { get; } = "EEM_General";

		/// <summary>
		/// This permits certain operations to throw custom exceptions in order to
		/// provide detailed descriptions of what gone wrong, over call stack.<para />
		/// BEWARE, every exception thrown must be explicitly provided with a catcher, or it will crash the entire game!
		/// </summary>
		public static bool AllowThrowingErrors { get; } = true;

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

		public const string ServerCommsPrefix = "EEMServerMessage";

		public const string DeclareWarMessagePrefix = "DeclareWar";

		public const string DeclarePeaceMessagePrefix = "DeclarePeace";

		public const string AcceptPeaceMessagePrefix = "AcceptPeace";

		public const string RejectPeaceMessagePrefix = "RejectPeace";

		public const string InitFactionsMessagePrefix = "InitFactions";

		#endregion

		// CleanUp.cs's constants:

		// verbose debug log output
		public static bool CleanupDebug { get; } = false;

		// text required in the RC's CustomData to be even considrered for removal
		public static string CleanupRcTag { get; } = "[EEM_AI]";

		// any of these is required to be in RC's CustomData for the grid to be removed.
		public static string[] CleanupRcExtraTags { get; } = { "Type:Fighter", "Type:Freighter" };

		// clamp the world's view range to this minimum value, which is used for removing distant ships
		public static int CleanupMinRange { get; } = 40000;

		// remove connector-connected ships too?
		public static bool CleanupConnectorConnected { get; } = false;

		// world setting of max drones
		public static int ForceMaxDrones { get; } = 20;


		// BuyShip.cs's constants:

		// time in seconds to wait after spawning before spawning again
		public static int TradeDelaySeconds { get; } = 15;

		// prefix and suffix words that encapsulate the prefab name
		public static string TradeEchoPrefix { get; } = "Ship bought:";

		public static string TradeEchoSuffix { get; } = "\n";

		// relative spawn position to the PB, use negative for the opposite direction
		public static double SpawnRelativeOffsetUp { get; } = 10.0;

		public static double SpawnRelativeOffsetLeft { get; } = 0.0;

		public static double SpawnRelativeOffsetForward { get; } = 0.0;

		// after the ship is spawned, all blocks with inventory in the PB's grid with this word in their name and owned by the trader faction will have their contents purged
		public static string PostspawnEmptyinventoryTag { get; } = "(purge)";

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