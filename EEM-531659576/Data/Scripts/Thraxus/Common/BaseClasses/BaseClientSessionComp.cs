using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace Eem.Thraxus.Common.BaseClasses
{
	class BaseClientSessionComp : MySessionComponentBase
	{
		private readonly string _baseGeneralLogName;
		private readonly string _baseDebugLogName;
		private readonly string _baseType;

		private readonly bool _noUpdate;

		private Log _generalLog;
		private Log _debugLog;

		private bool _earlySetupComplete;
		private bool _lateSetupComplete;

		protected BaseClientSessionComp(string generalLogName, string debugLogName, string baseType, bool noUpdate = true)
		{
			_baseGeneralLogName = generalLogName;
			_baseDebugLogName = debugLogName;
			_baseType = baseType;
			_noUpdate = noUpdate;
		}

		public override void BeforeStart()
		{
			if (Settings.GeneralSettings.IsServer) return;
			base.BeforeStart();
		}

		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			if (Settings.GeneralSettings.IsServer) return;
			base.Init(sessionComponent);
			if (!_earlySetupComplete) EarlySetup();
		}

		protected virtual void EarlySetup()
		{
			_earlySetupComplete = true;
			_generalLog = new Log(_baseGeneralLogName);
			if (Settings.GeneralSettings.DebugMode) _debugLog = new Log(_baseDebugLogName);
			WriteToLog("EarlySetup", $"Waking up.", LogType.General);
		}

		public override void UpdateBeforeSimulation()
		{
			if (Settings.GeneralSettings.IsServer) return;
			base.UpdateBeforeSimulation();
			if (!_lateSetupComplete) LateSetup();
		}

		public override void UpdateAfterSimulation()
		{
			if (Settings.GeneralSettings.IsServer) return;
			base.UpdateAfterSimulation();
		}

		protected virtual void LateSetup()
		{
			_lateSetupComplete = true;
			if (_noUpdate) MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			WriteToLog("LateSetup", $"Fully online.", LogType.General);
		}

		protected override void UnloadData()
		{
			Unload();
			base.UnloadData();
		}

		protected virtual void Unload()
		{
			if (Settings.GeneralSettings.IsServer) return;
			WriteToLog("Unload", $"Retired.", LogType.General);
			_debugLog?.Close();
			_generalLog?.Close();
		}

		/// <inheritdoc />
		public void WriteToLog(string caller, string message, LogType logType)
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

		private readonly object _writeLocker = new object();

		private void WriteDebug(string caller, string message)
		{
			lock (_writeLocker)
			{
				_debugLog?.WriteToLog($"{_baseType}: {caller}", message);
			}
		}

		private void WriteException(string caller, string message)
		{
			StaticLog.WriteToLog($"{_baseType}: {caller}", $"Exception! {message}", LogType.Exception);
		}

		private void WriteGeneral(string caller, string message)
		{
			lock (_writeLocker)
			{
				_generalLog?.WriteToLog($"{_baseType}: {caller}", message);
			}
		}

		private void WriteProfiler(string caller, string message)
		{
			lock (_writeLocker)
			{
				StaticLog.WriteToLog(caller, message, LogType.Profiling);
			}
		}

		public static void WriteToStaticLog(string caller, string message, LogType logType)
		{
			StaticLog.WriteToLog(caller, message, logType);
		}
	}
}
