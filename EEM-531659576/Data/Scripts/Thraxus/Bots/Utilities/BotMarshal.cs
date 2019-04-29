using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Eem.Thraxus.Common;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Utilities;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Screens.Helpers;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Utilities
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MaxValue)]
	// ReSharper disable once ClassNeverInstantiated.Global
	internal class BotMarshal : MySessionComponentBase
	{
		private static BotMarshal _instance;

		public static MyConcurrentList<long> ActiveShipRegistry;
		public static ConcurrentDictionary<long, long> PlayerShipControllerHistory;

		private const string GeneralLogName = "BotGeneral";
		private const string DebugLogName = "BotDebug";

		private static Log _botGeneralLog;
		private static Log _botDebugLog;

		private bool _setupComplete;
		private bool _lateSetupComplete;

		public static Dictionary<long, BotOrphan> BotOrphans;

		/// <inheritdoc />
		public override void LoadData()
		{
			_instance = this;
			base.LoadData();
		}

		/// <inheritdoc />
		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			base.Init(sessionComponent);
			if (!Helpers.Constants.IsServer || _setupComplete) return;
			if (!_setupComplete) Setup();
		}

		private void Setup()
		{
			_setupComplete = true;
			_botGeneralLog = new Log(GeneralLogName);
			if (Helpers.Constants.DebugMode) _botDebugLog = new Log(DebugLogName);
			BotOrphans = new Dictionary<long, BotOrphan>();
			ActiveShipRegistry = new MyConcurrentList<long>();
			PlayerShipControllerHistory = new ConcurrentDictionary<long, long>();
			DamageHandler.Run();

		}

		/// <inheritdoc />
		public override void BeforeStart()
		{
			base.BeforeStart();
		}

		/// <inheritdoc />
		public override void UpdateBeforeSimulation()
		{
			base.UpdateBeforeSimulation();
			if (!Helpers.Constants.IsServer || _lateSetupComplete) return;
			_lateSetupComplete = true;
			MyAPIGateway.Session.Player.Controller.ControlledEntityChanged += ControlAcquired;
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
		}

		private void ControlAcquired(IMyControllableEntity player, IMyControllableEntity controlled)
		{
			try
			{
				if (player == null || controlled == null) return;
				IMyPlayer myPlayer = MyAPIGateway.Players.GetPlayerById(controlled.ControllerInfo.ControllingIdentityId);
				IMyCubeGrid myCubeGrid = controlled.Entity.GetTopMostParent() as IMyCubeGrid;
				if (myPlayer == null || myCubeGrid == null) return;
				if (!PlayerShipControllerHistory.TryAdd(myCubeGrid.EntityId, myPlayer.IdentityId))
					PlayerShipControllerHistory[myCubeGrid.EntityId] = myPlayer.IdentityId;
			}
			catch (Exception e)
			{
				ExceptionLog("ControlAcquired", $"Exception! {e}");
			}
		}

		/// <inheritdoc />
		protected override void UnloadData()
		{
			base.UnloadData();
			if (!Helpers.Constants.IsServer) return;
			Unload();
			_instance = null;
		}

		private void Unload()
		{
			DamageHandler.Unload();
			MyAPIGateway.Session.Player.Controller.ControlledEntityChanged -= ControlAcquired;
			ActiveShipRegistry?.Clear();
			PlayerShipControllerHistory?.Clear();
			BotOrphans?.Clear();
			_botDebugLog?.Close();
			_botGeneralLog?.Close();
		}

		public static void RegisterNewEntity(long entityId)
		{
			try
			{
				ActiveShipRegistry.Add(entityId);
			}
			catch (Exception e)
			{
				ExceptionLog("RegisterNewEntity", e.ToString());
			}
		}

		public static void RemoveDeadEntity(long entityId)
		{
			try
			{
				ActiveShipRegistry.Remove(entityId);
			}
			catch (Exception e)
			{
				ExceptionLog("RemoveDeadEntity", e.ToString());
			}
		}

		private static readonly object WriteLocker = new object();

		public static void WriteToLog(string caller, string message, bool general = false)
		{
			lock (WriteLocker)
			{
				if (general) _botGeneralLog?.WriteToLog(caller, message);
				_botDebugLog?.WriteToLog(caller, message);
			}
		}

		public static void ExceptionLog(string caller, string message)
		{
			WriteToLog(caller, $"Exception! {message}", true);
		}
	}
}
