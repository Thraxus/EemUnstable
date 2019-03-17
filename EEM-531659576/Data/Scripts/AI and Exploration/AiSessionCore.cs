using System;
using System.Collections.Generic;
using EemRdx.Factions;
using EemRdx.Helpers;
using EemRdx.Networking;
using EemRdx.Utilities;
using Sandbox.Definitions;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace EemRdx
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class AiSessionCore : MySessionComponentBase
	{
		public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;// { get; private set; }

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
			if (Constants.DebugMode && !_debugInitialized) DebugInit();
			if (!_initialized) Initialize();
			if (!IsServer) return;
			if (MyAPIGateway.Multiplayer.Players.Count > 0 && !Factions.Factions.PlayerFactionInitComplete) { Factions.Factions.PlayerInitFactions(); }
			TickTimer();
		}

		private void DebugInit()
		{
			DebugLog = new Log(Constants.DebugLogName);
			MyAPIGateway.Session.Factions.FactionStateChanged += DebugFactions;
			_debugInitialized = true;
		}

		private static void DebugFactions(MyFactionStateChange action, long fromFaction, long toFaction, long playerId, long senderId)
		{
			DebugLog?.WriteToLog("FactionStateChanged-AiSession", $"OnServer:\t{IsServer}\tAction:\t{action} fromFaction:\t{fromFaction}\tfromFactionTag:\t{fromFaction.GetFactionById()?.Tag}");
			DebugLog?.WriteToLog("FactionStateChanged-AiSession", $"OnServer:\t{IsServer}\tAction:\t{action}\ttoFaction:\t{toFaction}\ttoFactionTag:\t{toFaction.GetFactionById()?.Tag}");
		}

		private void DebugClose()
		{
			MyAPIGateway.Session.Factions.FactionStateChanged -= DebugFactions;
		}

		private void Initialize()
		{
			if (Constants.DebugMode) DebugLog.WriteToLog("Initialize",$"Debug Active - IsServer: {IsServer}", true, 20000);
			Factions.Factions.Initialize();
			Messaging.Register();
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageRefHandler);
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, GenericDamageHandler);
			MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, GenericDamageHandler);
			MyAPIGateway.Entities.OnEntityAdd += delegate(IMyEntity entity) { GeneralLog.WriteToLog("Core", $"Entity Added: {entity.EntityId}: {entity.DisplayName}"); };
			MyAPIGateway.Entities.OnEntityRemove += delegate(IMyEntity entity) { GeneralLog.WriteToLog("Core", $"Entity Removed: {entity.EntityId}: {entity.DisplayName}"); };
			_initialized = true;
		}

		private static void InitLogs()
		{
			if(Constants.EnableProfilingLog) ProfilingLog = new Log(Constants.ProfilingLogName);
			if(Constants.EnableGeneralLog) GeneralLog = new Log(Constants.GeneralLogName);
			LogSetupComplete = true;
			GeneralLog.WriteToLog("Core", $"Cargo: {MyAPIGateway.Session.SessionSettings.CargoShipsEnabled}");
			GeneralLog.WriteToLog("Core", $"Encounters: {MyAPIGateway.Session.SessionSettings.EnableEncounters}");
			GeneralLog.WriteToLog("Core", $"Drones: {MyAPIGateway.Session.SessionSettings.EnableDrones}");
			GeneralLog.WriteToLog("Core", $"Scripts: {MyAPIGateway.Session.SessionSettings.EnableIngameScripts}");
			GeneralLog.WriteToLog("Core", $"Sync: {MyAPIGateway.Session.SessionSettings.SyncDistance}");

			//foreach (MySpawnGroupDefinition spawnGroupDefinition in MyDefinitionManager.Static.GetSpawnGroupDefinitions())
			//{
			//	if (!spawnGroupDefinition.Public || !spawnGroupDefinition.Context.IsBaseGame) continue;
			//	GeneralLog.WriteToLog("Core", $@"IsBaseGame:	{spawnGroupDefinition.Context.IsBaseGame}	SubtypeName:	{spawnGroupDefinition.Id.SubtypeName}	Frequency:	{spawnGroupDefinition.Frequency}	IsPirate:	{spawnGroupDefinition.IsPirate}	IsEncounter:	{spawnGroupDefinition.IsEncounter}	IsCargoShip:	{spawnGroupDefinition.IsCargoShip}");
			//}

		}

		private static void CloseLogs()
		{
			if (Constants.DebugMode) DebugLog?.Close();
			if (Constants.EnableProfilingLog) ProfilingLog?.Close();
			if (Constants.EnableGeneralLog) GeneralLog?.Close();
		}

		/// <summary>
		/// Initial setup
		/// </summary>
		/// <param name="sessionComponent"></param>
		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{

		}

		public override void BeforeStart()
		{
			InitLogs();
		}

		/// <summary>
		/// Unloads the handlers
		/// </summary>
		protected override void UnloadData()
		{
			Factions.Factions.Unload();
			Messaging.Unregister();
			DebugClose();
			CloseLogs();
		}

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
			if (_tickTimer % Constants.WarAssessmentCounterLimit == 0) Factions.Factions.AssessFactionWar();
			//if (_tickTimer % Constants.FactionAssessmentCounterLimit == 0) Factions.FactionAssessment();
			if (_tickTimer % (Constants.TicksPerSecond * 30) == 0) Factions.Factions.FactionAssessment();
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