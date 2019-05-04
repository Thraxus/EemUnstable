using System;
using Eem.Thraxus.Bots.Settings;
using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.Models
{
	public class BotBaseAdvanced : LogBase
	{
		internal readonly IMyEntity ThisEntity;
		internal readonly IMyCubeGrid ThisCubeGrid;
		internal readonly IMyShipController MyShipController;
		internal readonly EemPrefabConfig MyConfig;
		internal readonly long OwnerId;
		private bool _sleeping;

		public event ShutdownRequest BotShutdown;
		public delegate void ShutdownRequest();

		public event SleepRequest BotSleep;
		public delegate void SleepRequest();

		public event WakeupRequest BotWakeup;
		public delegate void WakeupRequest();

		public BotBaseAdvanced(IMyEntity passedEntity, IMyShipController controller, bool isMultipart = false)
		{
			ThisEntity = passedEntity;
			MyShipController = controller;
			MyConfig = !isMultipart ? new EemPrefabConfig(controller.CustomData) : BotMarshal.BotOrphans[passedEntity.EntityId].MyLegacyConfig;
			OwnerId = MyAPIGateway.Session.Factions.TryGetFactionByTag(MyConfig.Faction).FounderId;
			ThisCubeGrid = ((IMyCubeGrid)passedEntity);
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
			ThisCubeGrid.OnBlockRemoved += OnBlockRemoved;
			ThisCubeGrid.OnBlockOwnershipChanged += OnOnBlockOwnershipChanged;
			ThisCubeGrid.OnBlockIntegrityChanged += OnBlockIntegrityChanged;
			BotMarshal.RegisterNewEntity(ThisEntity.EntityId);
			DamageHandler.TriggerAlert += DamageHandlerOnTriggerAlert;
			SetupBot();
		}

		private void DamageHandlerOnTriggerAlert(long shipId)
		{
			if (ThisEntity.EntityId == shipId)
				TriggerAlertConditions();
		}

		private void TriggerAlertConditions()
		{
			// TODO Add alert conditions
		}

		public void Unload()
		{
			OnWriteToLog("BotCore", $"Shutting down -\tId:\t{ThisEntity.EntityId}\tName:\t{ThisEntity.DisplayName}", LogType.General);
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
				OnWriteToLog("SetFactionOwnership", $"Setting faction ownership of {ThisEntity.DisplayName} to {MyAPIGateway.Session.Factions.TryGetFactionByTag(MyConfig.Faction).Tag}", LogType.General);

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
				OnWriteToLog("OnBlockRemoved", $"Triggering shutdown for {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", LogType.General);
				BotShutdown?.Invoke();
			}
		}

		private void OnBlockIntegrityChanged(IMySlimBlock block)
		{   // Trigger alert, war, all the fun stuff against the entity owner that triggered the integrity change (probably negative only)
			OnWriteToLog("OnBlockIntegrityChanged", $"Block integrity changed for {ThisEntity.DisplayName} with ID {ThisEntity.EntityId} for block {block}", LogType.General);
			if (block.FatBlock == MyShipController && !MyShipController.IsFunctional)
			{
				OnWriteToLog("OnBlockIntegrityChanged", $"Rc integrity compromised... triggering sleep for {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", LogType.General);
				Sleep();
				BotSleep?.Invoke();
			}

			if (block.FatBlock == MyShipController && MyShipController.IsFunctional && _sleeping)
			{
				OnWriteToLog("OnBlockIntegrityChanged", $"Rc integrity restored... triggering wakeup for {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", LogType.General);
				Wakeup();
				BotWakeup?.Invoke();
			}
		}

		private void Sleep()
		{   // TODO make sure damage handlers are here since the bot is simulating offline here
			OnWriteToLog("Sleep", $"Sleeping {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", LogType.General);
			_sleeping = true;
			ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
		}

		private void Wakeup()
		{	// TODO make sure damage handlers are here so we can properly wake the bot up
			_sleeping = false;
			OnWriteToLog("Wakeup", $"Waking up {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", LogType.General);
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
		}

		private void OnOnBlockOwnershipChanged(IMyCubeGrid cubeGrid)
		{   // Protection for initial spawn with MES, should be disabled after the first few seconds in game (~300 ticks)
			OnWriteToLog("OnOnBlockOwnershipChanged", $"Ownership changed for {cubeGrid.DisplayName} with ID {cubeGrid.EntityId}", LogType.General);
			if (MyShipController.OwnerId != OwnerId) BotShutdown?.Invoke();
		}
	}
}
