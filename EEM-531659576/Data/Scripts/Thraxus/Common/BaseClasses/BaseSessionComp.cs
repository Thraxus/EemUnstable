using Eem.Thraxus.Utilities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace Eem.Thraxus.Common.BaseClasses
{
	abstract class BaseSessionComp : MySessionComponentBase
	{
		private readonly string _baseGeneralLogName;
		private readonly string _baseDebugLogName;
		private readonly string _baseType;

		protected BaseSessionComp(string generalLogName, string debugLogName, string baseType)
		{
			_instance = this;
			_baseGeneralLogName = generalLogName;
			_baseDebugLogName = debugLogName;
			_baseType = baseType;
		}

		private static BaseSessionComp _instance;

		private static Log _generalLog;
		private static Log _debugLog;

		private bool _earlySetupComplete;
		private bool _lateSetupComplete;

		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			base.Init(sessionComponent);
			if (!Helpers.Constants.IsServer || _earlySetupComplete) return;
			if (!_earlySetupComplete) EarlySetup();
		}

		protected virtual void EarlySetup()
		{
			_earlySetupComplete = true;
			_instance = this;
			_generalLog = new Log(_baseGeneralLogName);
			_debugLog = new Log(_baseDebugLogName);
			WriteToLog("EarlySetup", $"{_baseType} waking up.", true);
		}

		protected virtual void LateSetup()
		{
			_lateSetupComplete = true;
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			WriteToLog("LateSetup", $"{_baseType} fully online.", true);
		}
		
		public override void UpdateBeforeSimulation()
		{
			base.UpdateBeforeSimulation();
			if (!Helpers.Constants.IsServer || _lateSetupComplete) return;
			LateSetup();
		}
		
		protected override void UnloadData()
		{
			base.UnloadData();
			if (!Helpers.Constants.IsServer) return;
			Unload();
		}

		protected virtual void Unload()
		{
			WriteToLog("Unload", $"{_baseType} retired.", true);
			_debugLog?.Close();
			_generalLog?.Close();
			_instance = null;
		}
		
		private static readonly object WriteLocker = new object();
		
		public static void WriteToLog(string caller, string message, bool general = false)
		{
			lock (WriteLocker)
			{
				if (general) _generalLog?.WriteToLog(caller, message);
				_debugLog?.WriteToLog(caller, message);
			}
		}

		public static void ExceptionLog(string caller, string message)
		{
			WriteToLog(caller, $"Exception! {message}", true);
		}
	}
}
