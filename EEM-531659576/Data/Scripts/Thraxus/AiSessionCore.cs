using System;
using System.Collections.Generic;
using Eem.Thraxus.Debug;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Networking;
using Eem.Thraxus.Utilities;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.ModAPI;
using VRage.Utils;

namespace Eem.Thraxus
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class AiSessionCore : MySessionComponentBase
	{
		private static readonly Dictionary<long, BotBase.OnDamageTaken> DamageHandlers = new Dictionary<long, BotBase.OnDamageTaken>();

		public static void AddDamageHandler(long gridId, BotBase.OnDamageTaken handler)
		{
			DamageHandlers.Add(gridId, handler);
		}

		public static void AddDamageHandler(IMyCubeGrid grid, BotBase.OnDamageTaken handler)
		{
			AddDamageHandler(grid.GetTopMostParent().EntityId, handler);
		}

		public static void RemoveDamageHandler(long gridId)
		{
			DamageHandlers.Remove(gridId);
		}

		public static void RemoveDamageHandler(IMyCubeGrid grid)
		{
			RemoveDamageHandler(grid.GetTopMostParent().EntityId);
		}

		public static bool HasDamageHandler(long gridId)
		{
			return DamageHandlers.ContainsKey(gridId);
		}

		public static bool HasDamageHandler(IMyCubeGrid grid)
		{
			return HasDamageHandler(grid.GetTopMostParent().EntityId);
		}

		public static bool LogSetupComplete;
		public static Log ProfilingLog;
		public static Log DebugLog;
		public static Log GeneralLog;

		private bool _initialized;
		private bool _debugInitialized;

		public override void UpdateBeforeSimulation()
		{
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			if (!Constants.IsServer) return;
			if (Constants.DebugMode && !_debugInitialized) DebugInit();
			if (!_initialized) Initialize();
		}

		private void DebugInit()
		{
			_debugInitialized = true;
			DebugLog = new Log(Constants.DebugLogName);
			InformationExporter.Run();
			MyAPIGateway.Entities.OnEntityAdd += delegate (IMyEntity entity)
			{
				GeneralLog.WriteToLog("Core", $"Entity Added\t{entity.EntityId}\t{entity.DisplayName}");
			};
			MyAPIGateway.Entities.OnEntityRemove += delegate (IMyEntity entity)
			{
				GeneralLog.WriteToLog("Core", $"Entity Removed\t{entity.EntityId}\t{entity.DisplayName}");
			};
		}

		private void Initialize()
		{
			if (Constants.DebugMode) DebugLog.WriteToLog("Initialize", $"Debug Active - IsServer: {Constants.IsServer}", true, 20000);
			Messaging.Register();
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageRefHandler);
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, GenericDamageHandler);
			MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, GenericDamageHandler);
			_initialized = true;
		}

		private static void InitLogs()
		{
			if (Constants.EnableProfilingLog) ProfilingLog = new Log(Constants.ProfilingLogName);
			if (Constants.EnableGeneralLog) GeneralLog = new Log(Constants.GeneralLogName);
			LogSetupComplete = true;
			GeneralLog.WriteToLog("Core", $"Cargo: {MyAPIGateway.Session.SessionSettings.CargoShipsEnabled}");
			GeneralLog.WriteToLog("Core", $"Encounters: {MyAPIGateway.Session.SessionSettings.EnableEncounters}");
			GeneralLog.WriteToLog("Core", $"Drones: {MyAPIGateway.Session.SessionSettings.EnableDrones}");
			GeneralLog.WriteToLog("Core", $"Scripts: {MyAPIGateway.Session.SessionSettings.EnableIngameScripts}");
			GeneralLog.WriteToLog("Core", $"Sync: {MyAPIGateway.Session.SessionSettings.SyncDistance}");
			GeneralLog.WriteToLog("Core", $"View: {MyAPIGateway.Session.SessionSettings.ViewDistance}");
			GeneralLog.WriteToLog("Core", $"PiratePCU: {MyAPIGateway.Session.SessionSettings.PiratePCU}");
			GeneralLog.WriteToLog("Core", $"TotalPCU: {MyAPIGateway.Session.SessionSettings.TotalPCU}");
		}

		private static void CloseLogs()
		{
			if (Constants.DebugMode) DebugLog?.Close();
			if (Constants.EnableProfilingLog) ProfilingLog?.Close();
			if (Constants.EnableGeneralLog) GeneralLog?.Close();
		}

		

		///// <summary>
		///// Initial setup
		///// </summary>
		///// <param name="sessionComponent"></param>
		//public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		//{
		//	base.Init(sessionComponent);
		//}

		public override void BeforeStart()
		{
			InitLogs();
		}

		/// <summary>
		/// Unloads the handlers
		/// </summary>
		protected override void UnloadData()
		{
			base.UnloadData();
			Messaging.Unregister();
			CloseLogs();
		}

		public void DamageRefHandler(object damagedObject, ref MyDamageInformation damage)
		{
			GenericDamageHandler(damagedObject, damage);
		}

		public void GenericDamageHandler(object damagedObject, MyDamageInformation damage)
		{
			try
			{
				if (damage.AttackerId == 0 || !(damagedObject is IMySlimBlock)) return;
				IMySlimBlock damagedBlock = (IMySlimBlock)damagedObject;
				IMyCubeGrid damagedGrid = damagedBlock.CubeGrid;
				long gridId = damagedGrid.GetTopMostParent().EntityId;
				if (!DamageHandlers.ContainsKey(gridId)) return;
				DamageHandlers[gridId].Invoke(damagedBlock, damage);
			}
			catch (Exception scrap)
			{
				LogError("GenericDamageHandler", scrap);
			}
		}

		public static void LogError(string source, Exception scrap, string debugPrefix = "SessionCore.")
		{
			MyLog.Default.WriteLine($"Core Crash - {scrap.StackTrace}");
			DebugHelper.Print("Core Crash - Please reload the game", $"Fatal error in '{debugPrefix + source}': {scrap.Message}. {(scrap.InnerException != null ? scrap.InnerException.Message : "No additional info was given by the game :(")}");
		}

		public static void DebugWrite(string source, string message, bool antiSpam = true)
		{
			if (Constants.DebugMode) DebugHelper.Print($"{source}", $"{message}", antiSpam);
		}
	}
}