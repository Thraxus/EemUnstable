using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace Eem.Thraxus.Common.BaseClasses
{
	public abstract class BaseServerSessionComp : MySessionComponentBase
	{
		private readonly string _baseGeneralLogName;
		private readonly string _baseDebugLogName;
		private readonly string _baseType;

		private readonly bool _noUpdate;

		private Log _generalLog;
		private Log _debugLog;

		private bool _superEarlySetupComplete;
		private bool _earlySetupComplete;
		private bool _lateSetupComplete;

		protected BaseServerSessionComp(string generalLogName, string debugLogName, string baseType, bool noUpdate = true)
		{
			_baseGeneralLogName = generalLogName;
			_baseDebugLogName = debugLogName;
			_baseType = baseType;
			_noUpdate = noUpdate;
		}

		/// <inheritdoc />
		public override void LoadData()
		{
			if (!Settings.Settings.IsServer) return;
			base.LoadData();
			if (!_superEarlySetupComplete) SuperEarlySetup();
		}

		protected virtual void SuperEarlySetup()
		{
			_superEarlySetupComplete = true;
			_generalLog = new Log(_baseGeneralLogName);
			if (Settings.Settings.DebugMode) _debugLog = new Log(_baseDebugLogName);
		}

		public override void BeforeStart()
		{
			if (!Settings.Settings.IsServer) return;
			base.BeforeStart();
		}

		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			if (!Settings.Settings.IsServer) return;
			base.Init(sessionComponent);
			if (!_earlySetupComplete) EarlySetup();
		}

		protected virtual void EarlySetup()
		{
			_earlySetupComplete = true;
			WriteToLog("EarlySetup", $"Waking up.", LogType.General);
		}

		public override void UpdateBeforeSimulation()
		{
			if (!Settings.Settings.IsServer) return;
			base.UpdateBeforeSimulation();
			if (!_lateSetupComplete) LateSetup();
		}

		protected virtual void LateSetup()
		{
			_lateSetupComplete = true;
			if (_noUpdate) MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			WriteToLog("LateSetup", $"Fully online.", LogType.General);
		}


		public override void UpdateAfterSimulation()
		{
			if (!Settings.Settings.IsServer) return;
			base.UpdateAfterSimulation();
		}

		protected override void UnloadData()
		{
			Unload();
			base.UnloadData();
		}

		protected virtual void Unload()
		{
			if (!Settings.Settings.IsServer) return;
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
