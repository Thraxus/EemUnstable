using Eem.Thraxus.Factions.Models;
using Eem.Thraxus.Utilities;
using Eem.Thraxus.Helpers;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace Eem.Thraxus.Factions
{
	[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class FactionCore : MySessionComponentBase
	{
		// Fields

		private bool _registerEarly;
		private bool _initialized;

		private static Log _debugLog;

		public RelationshipManager RelationshipManager { get; private set; }

		public static FactionCore FactionCoreStaticInstance;

		/// <inheritdoc />
		public override void LoadData()
		{
			base.LoadData();
			FactionCoreStaticInstance = this;
		}

		// Properties
		

		// Init Methods

		/// <summary>
		/// Runs before the game is ready, safe for some initializations, not safe for others
		/// </summary>
		public override void BeforeStart()
		{
			base.BeforeStart();
			if (!Constants.IsServer) return;
			if (!_registerEarly) RegisterEarly();
		}

		/// <summary>
		/// Runs every tick before the simulation is updated
		/// </summary>
		public override void UpdateBeforeSimulation()
		{
			base.UpdateBeforeSimulation();
			if (!Constants.IsServer) return;
			if (!_initialized) Initialize();
			TickTimer();
		}

		/// <summary>
		/// 
		/// </summary>
		private void RegisterEarly()
		{
			if (_registerEarly) return;
			if (Constants.DebugMode) _debugLog = new Log(Settings.Constants.DebugLogName);
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.BeforeSimulation));
			WriteToLog("FactionCore", $"RegisterEarly Complete... {UpdateOrder}");
			_registerEarly = true;
		}

		/// <summary>
		/// 
		/// </summary>
		private void Initialize()
		{
			RelationshipManager = new RelationshipManager();
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			WriteToLog("FactionCore", $"Initialized... {UpdateOrder}");
			_initialized = true;
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void UnloadData()
		{
			base.UnloadData();
			if (!Constants.IsServer) return;
			RelationshipManager?.Unload();
			WriteToLog("FactionCore", $"I'm out!... {UpdateOrder}");
			_debugLog?.Close();
			FactionCoreStaticInstance = null;
		}
		

		// Core Logic Methods

		/// <summary>
		/// Increments every server tick
		/// </summary>
		private ulong _tickTimer;

		/// <summary>
		/// Processes certain things at set intervals
		/// </summary>
		private void TickTimer()
		{
			_tickTimer++;
			if (_tickTimer % Constants.FactionNegativeRelationshipAssessment == 0)
				RelationshipManager.CheckNegativeRelationships();
			if (_tickTimer % Constants.FactionMendingRelationshipAssessment == 0)
				RelationshipManager.CheckMendingRelationships();
		}

		// Non-Core logic below this point

		/// <summary>
		/// 
		/// </summary>
		/// <param name="caller"></param>
		/// <param name="message"></param>
		public static void WriteToLog(string caller, string message)
		{
			_debugLog?.WriteToLog(caller, message);
		}
	}
}
