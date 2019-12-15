using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;

namespace Eem.Thraxus.Bots.SessionComps
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MinValue + 1)]
	internal class BotMarshal : BaseServerSessionComp
	{
		private const string GeneralLogName = "BotMarshalGeneral";
		private const string DebugLogName = "BotMarshalDebug";
		private const string SessionCompName = "BotMarshal";

		public BotMarshal() : base(GeneralLogName, DebugLogName, SessionCompName) {  } // Do nothing else
		
		public static readonly ConcurrentCachingList<long> ActiveShipRegistry = new ConcurrentCachingList<long>();
		
		public static ConcurrentDictionary<long, long> PlayerShipControllerHistory;
		public static ConcurrentDictionary<long, BotOrphan> BotOrphans;
		public static ConcurrentDictionary<long, long> WarRegistry;
		public static ConcurrentDictionary<ulong, bool> ModDictionary;
		public static ConcurrentDictionary<long, ConcurrentCachingHashSet<TargetEntity>> PriorityTargetDictionary;

		/// <inheritdoc />
		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			base.Init(sessionComponent);
		}

		/// <inheritdoc />
		protected override void SuperEarlySetup()
		{
			base.SuperEarlySetup();
			ModDictionary = new ConcurrentDictionary<ulong, bool>();
			BotOrphans = new ConcurrentDictionary<long, BotOrphan>();
			PlayerShipControllerHistory = new ConcurrentDictionary<long, long>();
			WarRegistry = new ConcurrentDictionary<long, long>();
			PriorityTargetDictionary = new ConcurrentDictionary<long, ConcurrentCachingHashSet<TargetEntity>>();
		}

		/// <inheritdoc />
		protected override void EarlySetup()
		{
			base.EarlySetup();
			DamageHandler.TriggerAlert += RegisterNewWar;
		}
		
		/// <inheritdoc />
		protected override void LateSetup()
		{
			base.LateSetup();
			MyAPIGateway.Session.Player.Controller.ControlledEntityChanged += ControlAcquired;
			foreach (ulong mod in BotSettings.ModsToWatch)
				ModDictionary.TryAdd(mod, ModDetection.DetectMod(mod));
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
			DamageHandler.TriggerAlert -= RegisterNewWar;
			MyAPIGateway.Session.Player.Controller.ControlledEntityChanged -= ControlAcquired;
			//ActiveShipRegistry?.ClearList();
			//PlayerShipControllerHistory?.Clear();
			//WarRegistry?.Clear();
			//PriorityTargetDictionary.Clear();
			//BotOrphans?.Clear();
			base.Unload();
		}

		public static void RegisterNewEntity(long entityId)
		{
			try
			{
				WriteToStaticLog("RegisterNewEntity", $"New Entity: {entityId}", LogType.General);
				ActiveShipRegistry.Add(entityId);
				ActiveShipRegistry.ApplyAdditions();
			}
			catch (Exception e)
			{
				WriteToStaticLog("RegisterNewEntity", e.ToString(), LogType.Exception);
			}
		}

		public static void RemoveDeadEntity(long entityId)
		{
			try
			{
				ActiveShipRegistry.Remove(entityId);
				ActiveShipRegistry.ApplyRemovals();
			}
			catch (Exception e)
			{
				WriteToStaticLog("RemoveDeadEntity", e.ToString(), LogType.Exception);
			}
		}

		public static void RegisterNewWar(long entityId, long playerId)
		{ // Use this to trigger Factions; all war filters through here.
			try
			{
				WarRegistry.TryAdd(entityId, playerId);
			}
			catch (Exception e)
			{
				WriteToStaticLog("RegisterNewWar", e.ToString(), LogType.Exception);
			}
		}

		public static void RemoveOldWar(long entityId, long playerId)
		{
			try
			{
				WarRegistry.Remove(entityId);
			}
			catch (Exception e)
			{
				WriteToStaticLog("RemoveOldWar", e.ToString(), LogType.Exception);
			}
		}

		public static void RegisterNewPriorityTarget(long shipId, TargetEntity target)
		{ // Use this to trigger Factions; all war filters through here.
			try
			{
				if (!PriorityTargetDictionary.TryAdd(shipId, new ConcurrentCachingHashSet<TargetEntity> {target}))
					PriorityTargetDictionary[shipId].Add(target);
			}
			catch (Exception e)
			{
				WriteToStaticLog("RegisterNewPriorityTarget", e.ToString(), LogType.Exception);
			}
		}

		public static void RemoveOldPriorityTarget(long shipId, TargetEntity target)
		{
			try
			{
				PriorityTargetDictionary[shipId].Remove(target);
				if (PriorityTargetDictionary[shipId].Count == 0)
					PriorityTargetDictionary.Remove(shipId);
			}
			catch (Exception e)
			{
				WriteToStaticLog("RemoveOldPriorityTarget", e.ToString(), LogType.Exception);
			}
		}
	}
}
