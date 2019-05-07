using System;
using System.Collections.Concurrent;
using Eem.Thraxus.Bots.Settings;
using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.Models
{
	public class BotBaseAdvanced : LogBase
	{
		internal IMyEntity ThisEntity;
		internal IMyCubeGrid ThisCubeGrid;
		internal IMyShipController MyShipController;
		internal EemPrefabConfig MyConfig;
		internal long OwnerId;
		private bool _sleeping;
		private ConcurrentCachingList<long> _warList;

		public event ShutdownRequest BotShutdown;
		public delegate void ShutdownRequest();

		public event SleepRequest BotSleep;
		public delegate void SleepRequest();

		public event WakeupRequest BotWakeup;
		public delegate void WakeupRequest();

		//public BotBaseAdvanced(IMyEntity passedEntity, IMyShipController controller, bool isMultipart = false)
		//{
		//	OnWriteToLog("BotCore", $"Powering up.", LogType.General);
		//	ThisEntity = passedEntity;
		//	MyShipController = controller;
		//	MyConfig = !isMultipart ? new EemPrefabConfig(controller.CustomData) : BotMarshal.BotOrphans[passedEntity.EntityId].MyLegacyConfig;
		//	OwnerId = MyAPIGateway.Session.Factions.TryGetFactionByTag(MyConfig.Faction).FounderId;
		//	ThisCubeGrid = ((IMyCubeGrid)passedEntity);
		//	ThisCubeGrid.OnBlockAdded += OnBlockAdded;
		//	ThisCubeGrid.OnBlockRemoved += OnBlockRemoved;
		//	ThisCubeGrid.OnBlockOwnershipChanged += OnOnBlockOwnershipChanged;
		//	ThisCubeGrid.OnBlockIntegrityChanged += OnBlockIntegrityChanged;
		//	BotMarshal.RegisterNewEntity(ThisEntity.EntityId);
		//	DamageHandler.TriggerAlert += DamageHandlerOnTriggerAlert;
		//	SetupBot();
		//}

		public void Run(IMyEntity passedEntity, IMyShipController controller, bool isMultipart = false)
		{
			OnWriteToLog("BotCore", $"Powering up.", LogType.General);
			ThisEntity = passedEntity;
			MyShipController = controller;
			MyConfig = !isMultipart ? new EemPrefabConfig(controller.CustomData) : BotMarshal.BotOrphans[passedEntity.EntityId].MyLegacyConfig;
			OwnerId = string.IsNullOrEmpty(MyConfig.Faction) ? MyAPIGateway.Session.Factions.TryGetFactionByTag("SPRT").FounderId : MyAPIGateway.Session.Factions.TryGetFactionByTag(MyConfig.Faction).FounderId;
			_warList = new ConcurrentCachingList<long>();
			ThisCubeGrid = ((IMyCubeGrid)passedEntity);
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
			ThisCubeGrid.OnBlockRemoved += OnBlockRemoved;
			ThisCubeGrid.OnBlockOwnershipChanged += OnOnBlockOwnershipChanged;
			ThisCubeGrid.OnBlockIntegrityChanged += OnBlockIntegrityChanged;
			BotMarshal.RegisterNewEntity(ThisEntity.EntityId);
			DamageHandler.TriggerAlert += DamageHandlerOnTriggerAlert;
			SetupBot();
		}

		private void DamageHandlerOnTriggerAlert(long shipId, long playerId)
		{
			if (ThisEntity.EntityId == shipId)
				TriggerAlertConditions(shipId, playerId);
		}

		private void TriggerAlertConditions(long shipId, long playerId)
		{
			OnWriteToLog("TriggerAlertConditions", $"Alert conditions triggered against {playerId}", LogType.General);
			_warList.Add(playerId);
			// TODO Add alert conditions
		}

		public void Unload()
		{
			OnWriteToLog("BotCore", $"Shutting down.", LogType.General);
			_warList.ClearList();
			BotMarshal.RemoveDeadEntity(ThisEntity.EntityId);
			ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
			ThisCubeGrid.OnBlockRemoved -= OnBlockRemoved;
			ThisCubeGrid.OnBlockOwnershipChanged -= OnOnBlockOwnershipChanged;
			ThisCubeGrid.OnBlockIntegrityChanged -= OnBlockIntegrityChanged;
		}

		internal void SetupBot()
		{
			SetFactionOwnership();
		}

		private void SetFactionOwnership()
		{
			try
			{
				OnWriteToLog("SetFactionOwnership", $"Setting faction ownership to {MyAPIGateway.Session.Factions.TryGetFactionByTag(MyConfig.Faction).Tag}", LogType.General);
				ThisCubeGrid.ChangeGridOwnership(OwnerId, Constants.ShareMode);
				//foreach (IMyCubeGrid grid in MyAPIGateway.GridGroups.GetGroup(ThisCubeGrid, GridLinkTypeEnum.Mechanical).Where(grid => grid.BigOwners != ThisCubeGrid.BigOwners))
				foreach (IMyCubeGrid grid in MyAPIGateway.GridGroups.GetGroup(ThisCubeGrid, GridLinkTypeEnum.Mechanical))
					grid.ChangeGridOwnership(OwnerId, Constants.ShareMode);
			}
			catch (Exception e)
			{
				OnWriteToLog("SetFactionOwnership", e.ToString(), LogType.Exception);
			}
		}

		private void OnBlockAdded(IMySlimBlock addedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that added the block

		}

		private void OnBlockRemoved(IMySlimBlock removedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that removed the block; also scan for main RC removal and shut down bot if Single Part
			if (removedBlock.FatBlock == MyShipController)
			{
				OnWriteToLog("OnBlockRemoved", $"Triggering shutdown.", LogType.General);
				BotShutdown?.Invoke();
			}
		}

		private void OnBlockIntegrityChanged(IMySlimBlock block)
		{   // Trigger alert, war, all the fun stuff against the entity owner that triggered the integrity change (probably negative only)
			OnWriteToLog("OnBlockIntegrityChanged", $"Block integrity changed for block {block}", LogType.General);
			if (block.FatBlock == MyShipController && !MyShipController.IsFunctional)
			{
				OnWriteToLog("OnBlockIntegrityChanged", $"Rc integrity compromised... triggering sleep.", LogType.General);
				Sleep();
				BotSleep?.Invoke();
			}

			if (block.FatBlock == MyShipController && MyShipController.IsFunctional && _sleeping)
			{
				OnWriteToLog("OnBlockIntegrityChanged", $"Rc integrity restored... triggering wakeup.", LogType.General);
				Wakeup();
				BotWakeup?.Invoke();
			}

		}
		private void OnOnBlockOwnershipChanged(IMyCubeGrid cubeGrid)
		{   // Protection for initial spawn with MES, should be disabled after the first few seconds in game (~300 ticks)
			OnWriteToLog("OnBlockOwnershipChanged", $"Ownership changed.", LogType.General);
			if (MyShipController.OwnerId != OwnerId) BotShutdown?.Invoke();
		}

		private void Sleep()
		{   // TODO make sure damage handlers are here since the bot is simulating offline here
			OnWriteToLog("Sleep", $"Going to sleep.", LogType.General);
			_sleeping = true;
			ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
		}

		private void Wakeup()
		{	// TODO make sure damage handlers are here so we can properly wake the bot up
			_sleeping = false;
			OnWriteToLog("Wakeup", $"Waking up.", LogType.General);
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
		}
	}
}
