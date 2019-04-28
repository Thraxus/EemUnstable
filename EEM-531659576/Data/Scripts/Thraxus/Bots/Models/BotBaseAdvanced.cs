using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.Settings;
using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Common;
using Eem.Thraxus.Extensions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.Models
{
	public class BotBaseAdvanced
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
			SetupBot();
		}

		public void Unload()
		{
			BotMarshal.WriteToLog("BotCore", $"Shutting down -\tId:\t{ThisEntity.EntityId}\tName:\t{ThisEntity.DisplayName}", true);
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
				BotMarshal.WriteToLog("SetFactionOwnership", $"Setting faction ownership of {ThisEntity.DisplayName} to {MyAPIGateway.Session.Factions.TryGetFactionByTag(MyConfig.Faction).Tag}", true);

				ThisCubeGrid.ChangeGridOwnership(OwnerId, Constants.ShareMode);
				foreach (IMyCubeGrid grid in MyAPIGateway.GridGroups.GetGroup(ThisCubeGrid, GridLinkTypeEnum.Mechanical))
					grid.ChangeGridOwnership(OwnerId, Constants.ShareMode);
			}
			catch (Exception e)
			{
				BotMarshal.ExceptionLog("SetFactionOwnership", e.ToString());
			}
		}

		private void OnBlockAdded(IMySlimBlock addedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that added the block

		}

		private void OnBlockRemoved(IMySlimBlock removedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that removed the block; also scan for main RC removal and shut down bot if Single Part
			if (removedBlock.FatBlock == MyShipController)
			{
				BotMarshal.WriteToLog("OnBlockRemoved", $"Triggering shutdown for {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", true);
				BotShutdown?.Invoke();
			}
		}

		private void OnBlockIntegrityChanged(IMySlimBlock block)
		{   // Trigger alert, war, all the fun stuff against the entity owner that triggered the integrity change (probably negative only)
			BotMarshal.WriteToLog("OnBlockIntegrityChanged", $"Block integrity changed for {ThisEntity.DisplayName} with ID {ThisEntity.EntityId} for block {block}", true);
			if (block.FatBlock == MyShipController && !MyShipController.IsFunctional)
			{
				BotMarshal.WriteToLog("OnBlockIntegrityChanged", $"Rc integrity compromised... triggering sleep for {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", true);
				Sleep();
				BotSleep?.Invoke();
			}

			if (block.FatBlock == MyShipController && MyShipController.IsFunctional && _sleeping)
			{
				BotMarshal.WriteToLog("OnBlockIntegrityChanged", $"Rc integrity restored... triggering wakeup for {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", true);
				Wakeup();
				BotWakeup?.Invoke();
			}
		}

		private void Sleep()
		{	// TODO make sure damage handlers are here since the bot is simulating offline here
			BotMarshal.WriteToLog("Sleep", $"Sleeping {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", true);
			_sleeping = true;
			ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
		}

		private void Wakeup()
		{	// TODO make sure damage handlers are here so we can properly wake the bot up
			_sleeping = false;
			BotMarshal.WriteToLog("Wakeup", $"Waking up {ThisEntity.DisplayName} with ID {ThisEntity.EntityId}", true);
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
		}

		private void OnOnBlockOwnershipChanged(IMyCubeGrid cubeGrid)
		{   // Protection for initial spawn with MES, should be disabled after the first few seconds in game (~300 ticks)
			BotMarshal.WriteToLog("OnOnBlockOwnershipChanged", $"Ownership changed for {cubeGrid.DisplayName} with ID {cubeGrid.EntityId}", true);
			if (MyShipController.OwnerId != OwnerId) BotShutdown?.Invoke();
		}
	}
}
