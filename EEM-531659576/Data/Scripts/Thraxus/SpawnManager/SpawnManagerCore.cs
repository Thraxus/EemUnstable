using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eem.Thraxus.Factions.Models;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.SpawnManager.Models;
using Eem.Thraxus.Utilities;
using VRage.Game.Components;

namespace Eem.Thraxus.SpawnManager
{
	[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class SpawnManagerCore : MySessionComponentBase
	{
		// Constants

		private const string DebugLogName = "SpawnManagerDebug";
		private const string GeneralLogName = "SpawnManagerGeneral";

		// Fields

		private bool _registerEarly;
		private bool _initialized;

		private static Log _debugLog;
		private static Log _generalLog;

		private EntityTracker _entityTracker;

		/// <inheritdoc />
		public override void LoadData()
		{
			base.LoadData();
		}

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
			_entityTracker = new EntityTracker();
			TickTimer();
		}

		/// <summary>
		/// 
		/// </summary>
		private void RegisterEarly()
		{
			if (Constants.DebugMode) _debugLog = new Log(DebugLogName);
			_generalLog = new Log(GeneralLogName);
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.BeforeSimulation));
			WriteToLog("SpawnManagerCore", $"RegisterEarly Complete... {UpdateOrder}", true);
			_registerEarly = true;
		}

		/// <summary>
		/// 
		/// </summary>
		private void Initialize()
		{
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			WriteToLog("SpawnManagerCore", $"Initialized... {UpdateOrder}", true);
			_initialized = true;
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void UnloadData()
		{
			base.UnloadData();
			if (!Constants.IsServer) return;
			_entityTracker.Close();
			WriteToLog("SpawnManagerCore", $"I'm out!... {UpdateOrder}", true);
			_debugLog?.Close();
			_generalLog?.Close();
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

		}

		// Non-Core logic below this point

		/// <summary>
		/// 
		/// </summary>
		/// <param name="caller"></param>
		/// <param name="message"></param>
		/// <param name="general"></param>
		public static void WriteToLog(string caller, string message, bool general = false)
		{
			_debugLog?.WriteToLog(caller, message);
			if (general) _generalLog?.WriteToLog(caller, message);
		}
	}
}
