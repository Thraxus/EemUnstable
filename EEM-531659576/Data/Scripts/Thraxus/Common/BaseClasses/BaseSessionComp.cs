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

		protected BaseSessionComp(string generalLogName, string debugLogName)
		{
			_instance = this;
			_baseGeneralLogName = generalLogName;
			_baseDebugLogName = debugLogName;
		}

		private static BaseSessionComp _instance;
		
		public static Log GeneralLog;
		public static Log DebugLog;

		private bool _earlySetupComplete;
		private bool _lateSetupComplete;

		/// <inheritdoc />
		public override bool UpdatedBeforeInit()
		{
			return base.UpdatedBeforeInit();
		}

		/// <inheritdoc />
		public override void BeforeStart()
		{
			base.BeforeStart();
		
		}

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
			GeneralLog = new Log(_baseGeneralLogName);
			DebugLog = new Log(_baseDebugLogName);
		}

		protected virtual void LateSetup()
		{
			_lateSetupComplete = true;
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
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

		public virtual void Unload()
		{
			DebugLog?.Close();
			GeneralLog?.Close();
			_instance = null;
		}
		
		private static readonly object WriteLocker = new object();
		
		public static void WriteToLog(string caller, string message, bool general = false)
		{
			lock (WriteLocker)
			{
				if (general) GeneralLog?.WriteToLog(caller, message);
				DebugLog?.WriteToLog(caller, message);
			}
		}

		public static void ExceptionLog(string caller, string message)
		{
			WriteToLog(caller, $"Exception! {message}", true);
		}
	}
}
