using Eem.Thraxus.Common.Interfaces;
using Eem.Thraxus.Common.Utilities;
using Eem.Thraxus.Utilities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace Eem.Thraxus.Common.BaseClasses
{
	public abstract class BaseServerSessionComp : MySessionComponentBase, ILogEntry
	{
		private readonly string _baseGeneralLogName;
		private readonly string _baseDebugLogName;
		private readonly string _baseType;

		private readonly bool _noUpdate;

		private Log _generalLog;
		private Log _debugLog;

		private bool _earlySetupComplete;
		private bool _lateSetupComplete;

		protected BaseServerSessionComp(string generalLogName, string debugLogName, string baseType, bool noUpdate = true)
		{
			_baseGeneralLogName = generalLogName;
			_baseDebugLogName = debugLogName;
			_baseType = baseType;
			_noUpdate = noUpdate;
		}

		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			base.Init(sessionComponent);
			if (!_earlySetupComplete) EarlySetup();
		}

		protected virtual void EarlySetup()
		{
			if (!Helpers.Constants.IsServer) return;
			_earlySetupComplete = true;
			_generalLog = new Log(_baseGeneralLogName);
			if (Helpers.Constants.DebugMode) _debugLog = new Log(_baseDebugLogName);
			WriteToLog("EarlySetup", $"Waking up.", LogType.General);
		}
		
		public override void UpdateBeforeSimulation()
		{
			base.UpdateBeforeSimulation();
			if (!_lateSetupComplete) LateSetup();
		}

		protected virtual void LateSetup()
		{
			if (!Helpers.Constants.IsServer) return;
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
			if (!Helpers.Constants.IsServer) return;
			WriteToLog("Unload", $"Retired.", LogType.General);
			_debugLog?.Close();
			_generalLog?.Close();
		}
		
		/// <inheritdoc />
		public void WriteToLog(string caller, string message, LogType logType)
		{
			switch (logType)
			{
				case LogType.General:
					GeneralLog(caller, message);
					return;
				case LogType.Debug:
					if (Helpers.Constants.DebugMode) DebugLog(caller, message);
					return;
				case LogType.Exception:
					ExceptionLog(caller, message);
					return;
				default:
					return;
			}
		}

		private readonly object _writeLocker = new object();

		private void GeneralLog(string caller, string message)
		{
			lock (_writeLocker)
			{
				_generalLog?.WriteToLog($"{_baseType}: {caller}", message);
			}
		}

		private void DebugLog(string caller, string message)
		{
			lock (_writeLocker)
			{
				_debugLog?.WriteToLog($"{_baseType}: {caller}", message);
			}
		}

		private void ExceptionLog(string caller, string message)
		{
			StaticExceptionLog($"{_baseType}: {caller}", message);
		}

		protected static void StaticExceptionLog(string caller, string message)
		{
			StaticLogger.WriteToLog(caller, $"Exception! {message}", LogType.Exception);
		}
	}
}
