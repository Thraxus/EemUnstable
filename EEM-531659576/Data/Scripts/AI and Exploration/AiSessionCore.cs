using System;
using System.Collections.Generic;
using EemRdx.Helpers;
using EemRdx.Models;
using EemRdx.Networking;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
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

		private bool _inited;

		public override void UpdateBeforeSimulation()
		{
			if (!_inited) Initialize();
			if (!IsServer) return;
			TickTimer();
		}

		private void Initialize()
		{
			if (Constants.DebugMode) Messaging.ShowLocalNotification($"Initialize - {IsServer}", 20000);
			if (!Factions.SetupComplete) Factions.ManageFactions();
			Messaging.Register();
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageRefHandler);
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, GenericDamageHandler);
			MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, GenericDamageHandler);
			_inited = true;
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

		}

		/// <summary>
		/// Unloads the handlers
		/// </summary>
		protected override void UnloadData()
		{
			Messaging.Unregister();
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
			if (_tickTimer % Constants.WasAssessmentCounterLimit == 0) Factions.AssessFactionWar();
			if (_tickTimer % Constants.FactionAssessmentCounterLimit == 0) Factions.ManageFactions();
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