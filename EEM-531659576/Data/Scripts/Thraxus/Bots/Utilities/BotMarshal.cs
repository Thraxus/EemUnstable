using System;
using System.Collections.Concurrent;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Utilities;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;

namespace Eem.Thraxus.Bots.Utilities
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MinValue + 1)]
	// ReSharper disable once ClassNeverInstantiated.Global
	internal class BotMarshal : BaseServerSessionComp
	{
		private const string GeneralLogName = "BotMarshalGeneral";
		private const string DebugLogName = "BotMarshalDebug";
		private const string SessionCompName = "BotMarshal";

		public BotMarshal() : base(GeneralLogName, DebugLogName, SessionCompName) {  } // Do nothing else
		
		public static ConcurrentCachingList<long> ActiveShipRegistry;
		public static ConcurrentCachingList<long> WarRegistry;

		public static ConcurrentDictionary<long, long> PlayerShipControllerHistory;
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
				WriteToLog("ControlAcquired", $"Exception! {e}", LogType.Exception);
			}
		}

		protected override void Unload()
		{
			DamageHandler.Unload();
			MyAPIGateway.Session.Player.Controller.ControlledEntityChanged -= ControlAcquired;
			ActiveShipRegistry?.ClearList();
			PlayerShipControllerHistory?.Clear();
			WarRegistry?.ClearList();
			BotOrphans?.Clear();
			base.Unload();
		}

		public static void RegisterNewEntity(long entityId)
		{
			try
			{
				ActiveShipRegistry.Add(entityId);
			}
			catch (Exception e)
			{
				StaticExceptionLog("RegisterNewEntity", e.ToString());
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
				StaticExceptionLog("RemoveDeadEntity", e.ToString());
			}
		}
	}
}
