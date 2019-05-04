using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;

namespace Eem.Thraxus.Bots.Utilities
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MinValue)]
	// ReSharper disable once ClassNeverInstantiated.Global
	internal class BotMarshal : BaseSessionComp
	{
		private const string GeneralLogName = "BotGeneral";
		private const string DebugLogName = "BotDebug";

		public BotMarshal() : base(GeneralLogName, DebugLogName) {  } // Do nothing else

		public static ConcurrentCachingList<long> ActiveShipRegistry;
		public static ConcurrentDictionary<long, long> PlayerShipControllerHistory;
		public static ConcurrentCachingList<long> WarRegistry;
		
		public static ConcurrentDictionary<long, BotOrphan> BotOrphans;
		
		/// <inheritdoc />
		protected override void EarlySetup()
		{
			base.EarlySetup();
			BotOrphans = new ConcurrentDictionary<long, BotOrphan>();
			ActiveShipRegistry = new ConcurrentCachingList<long>();
			PlayerShipControllerHistory = new ConcurrentDictionary<long, long>();
			WarRegistry = new ConcurrentCachingList<long>();
			DamageHandler.Run();
		}

		/// <inheritdoc />
		protected override void LateSetup()
		{
			base.LateSetup();
			MyAPIGateway.Session.Player.Controller.ControlledEntityChanged += ControlAcquired;
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
		
		public override void Unload()
		{
			base.Unload();
			DamageHandler.Unload();
			MyAPIGateway.Session.Player.Controller.ControlledEntityChanged -= ControlAcquired;
			ActiveShipRegistry?.ClearList();
			PlayerShipControllerHistory?.Clear();
			WarRegistry?.ClearList();
			BotOrphans?.Clear();
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
	}
}
