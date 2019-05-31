using System;
using Sandbox.ModAPI;

namespace Settings Eem.Thraxus.Common.
{
	internal static class Settings
	{
		#region Constant Values

		public const bool ForcedDebugMode = false;

		public const string ChatCommandPrefix = "chatCommand";
		public const string SettingsFileName = "ModName-UserConfig.xml";
		public const string StaticDebugLogName = "StaticLog-Debug";
		public const string ExceptionLogName = "Exception";
		public const string StaticGeneralLogName = "StaticLog-General";
		public const string ProflingLogName = "Profiler";

		public const ushort NetworkId = 12345;

		#endregion

		#region Reference Values

		public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;

		public const int DefaultLocalMessageDisplayTime = 5000;
		public const int DefaultServerMessageDisplayTime = 10000;
		public const int TicksPerMinute = TicksPerSecond * 60;
		public const int TicksPerSecond = 60;

		public static Random Random { get; } = new Random();

		#endregion


		#region User Configuration

		public static bool DebugMode = false;
		public static bool ProfilingEnabled = false;


		#endregion
	}
}
