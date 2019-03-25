﻿using System;
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

		private static Log _profilingLog;
		private static Log _debugLog;
		private static Log _generalLog;

		private RelationshipManager _relationshipManager;

		// Properties
		

		// Init Methods

		/// <summary>
		/// Runs before the game is ready, safe for some initializations, not safe for others
		/// </summary>
		public override void BeforeStart()
		{
			base.BeforeStart();
			if (!Constants.IsServer || _registerEarly) return;
			RegisterEarly();
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
			if (!Constants.IsServer || _registerEarly) return;
			_generalLog = new Log(Settings.Constants.GeneralLogName);
			_profilingLog = new Log(Settings.Constants.ProfilingLogName);
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
			_relationshipManager = new RelationshipManager();
			_relationshipManager.EnableTimer += TurnUpdatesOn;
			_relationshipManager.DisableTimer += TurnUpdatesOff;
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
			Close();
		}

		/// <summary>
		/// 
		/// </summary>
		private void Close()
		{
			if (!Constants.IsServer) return;
			_relationshipManager.EnableTimer -= TurnUpdatesOn;
			_relationshipManager.DisableTimer -= TurnUpdatesOff;
			_relationshipManager?.Unload();
			_generalLog?.Close();
			_profilingLog?.Close();
			_debugLog?.Close();
			WriteToLog("FactionCore", $"I'm out!... {UpdateOrder}");
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
				_relationshipManager.CheckNegativeRelationships();
			if (_tickTimer % Constants.FactionMendingRelationshipAssessment == 0)
				_relationshipManager.CheckMendingRelationships();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void TurnUpdatesOn(object sender, EventArgs eventArgs)
		{
			if (UpdateOrder == MyUpdateOrder.BeforeSimulation) return;
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.BeforeSimulation));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void TurnUpdatesOff(object sender, EventArgs eventArgs)
		{
			if (UpdateOrder == MyUpdateOrder.NoUpdate) return;
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			_tickTimer = 0;
		}


		// Non-Core logic below this point

		/// <summary>
		/// 
		/// </summary>
		/// <param name="caller"></param>
		/// <param name="message"></param>
		/// <param name="general"></param>
		/// <param name="debug"></param>
		/// <param name="profiler"></param>
		protected static void WriteToLog(string caller, string message, bool general = true, bool debug = false, bool profiler = false)
		{
			if (general) _generalLog.WriteToLog(caller, message);
			if (debug) _debugLog.WriteToLog(caller, message);
			if (profiler) _profilingLog.WriteToLog(caller, message);
		}
	}
}
