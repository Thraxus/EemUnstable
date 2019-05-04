using Eem.Thraxus.Utilities;
using VRage.Game;
using VRage.Game.Components;

namespace Eem.Thraxus.Common.Utilities
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MinValue)]
	// ReSharper disable once ClassNeverInstantiated.Global
	internal class StaticLogger : MySessionComponentBase
	{
		private const string GeneralLogName = "StaticGeneral";
		private const string DebugLogName = "StaticDebug";
		private const string ExceptionLogName = "Exception";

		private static Log _generalLog;
		private static Log _debugLog;
		private static Log _exceptionLog;

		private static readonly object GeneralWriteLocker = new object();
		private static readonly object DebugWriteLocker = new object();
		private static readonly object ExceptionWriteLocker = new object();

		/// <inheritdoc />
		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			base.Init(sessionComponent);
			_generalLog = new Log(GeneralLogName);
			_debugLog = new Log(DebugLogName);
			_exceptionLog = new Log(ExceptionLogName);
			WriteToLog("StaticLogger", "Static logs loaded.",LogType.General);
		}

		/// <inheritdoc />
		protected override void UnloadData()
		{
			WriteToLog("StaticLogger", "Closing static logs.", LogType.General);
			lock (GeneralWriteLocker)
			{
				_generalLog?.Close();
			}
			lock (DebugWriteLocker)
			{
				_debugLog?.Close();
			}
			lock (ExceptionWriteLocker)
			{
				_exceptionLog?.Close();
			}
			base.UnloadData();
		}

		public static void WriteToLog(string caller, string message, LogType logType)
		{
			switch (logType)
			{
				case LogType.General:
					WriteGeneral(caller, message);
					return;
				case LogType.Debug:
					WriteDebug(caller, message);
					return;
				case LogType.Exception:
					WriteException(caller, message);
					return;
				default:
					return;
			}
		}

		private static void WriteGeneral(string caller, string message)
		{
			lock (GeneralWriteLocker)
			{
				_generalLog?.WriteToLog(caller, message);
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
	}
}
