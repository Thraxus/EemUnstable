using Eem.Thraxus.Common.Enums;
using VRage.Game;
using VRage.Game.Components;

namespace Eem.Thraxus.Common.Utilities.Tools.Logging
{
	[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, priority: int.MinValue)]
	// ReSharper disable once ClassNeverInstantiated.Global
	internal class StaticLog : MySessionComponentBase
	{
		private const string DebugLogName = Settings.GeneralSettings.StaticDebugLogName;
		private const string ExceptionLogName = Settings.GeneralSettings.ExceptionLogName;
		private const string GeneralLogName = Settings.GeneralSettings.StaticGeneralLogName;
		private const string ProfilingLogName = Settings.GeneralSettings.ProfilingLogName;

		private static Log _debugLog;
		private static Log _exceptionLog;
		private static Log _generalLog;
		private static Log _profilingLog;

		private static readonly object DebugWriteLocker = new object();
		private static readonly object ExceptionWriteLocker = new object();
		private static readonly object GeneralWriteLocker = new object();
		private static readonly object ProfilerWriteLocker = new object();

		/// <inheritdoc />
		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			base.Init(sessionComponent);
			if (Settings.GeneralSettings.DebugMode) _debugLog = new Log(DebugLogName);
			_exceptionLog = new Log(ExceptionLogName);
			_generalLog = new Log(GeneralLogName);
			if (Settings.GeneralSettings.ProfilingEnabled) _profilingLog = new Log(ProfilingLogName);
			WriteToLog("StaticLogger", "Static logs loaded.", LogType.General);
		}

		/// <inheritdoc />
		protected override void UnloadData()
		{
			WriteToLog("StaticLogger", "Closing static logs.", LogType.General);
			lock (DebugWriteLocker)
			{
				_debugLog?.Close();
			}

			lock (ExceptionWriteLocker)
			{
				_exceptionLog?.Close();
			}
			lock (GeneralWriteLocker)
			{
				_generalLog?.Close();
			}
			lock (ProfilerWriteLocker)
			{
				_profilingLog?.Close();
			}
			base.UnloadData();
		}

		public static void WriteToLog(string caller, string message, LogType logType)
		{
			switch (logType)
			{
				case LogType.Debug:
					WriteDebug(caller, message);
					return;
				case LogType.Exception:
					WriteException(caller, message);
					return;
				case LogType.General:
					WriteGeneral(caller, message);
					return;
				case LogType.Profiling:
					WriteProfiler(caller, message);
					return;
				default:
					return;
			}
		}

		private static void WriteDebug(string caller, string message)
		{
			lock (DebugWriteLocker)
			{
				_debugLog?.WriteToLog(caller, message);
			}
		}

		private static void WriteException(string caller, string message)
		{
			lock (ExceptionWriteLocker)
			{
				_exceptionLog?.WriteToLog(caller, message);
			}
		}

		private static void WriteGeneral(string caller, string message)
		{
			lock (GeneralWriteLocker)
			{
				_generalLog?.WriteToLog(caller, message);
			}
		}

		private static void WriteProfiler(string caller, string message)
		{
			lock (ProfilingLogName)
			{
				_profilingLog?.WriteToLog(caller, message);
			}
		}
	}
}
