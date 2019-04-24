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

		public event ShutdownRequest Shutdown;
		public delegate void ShutdownRequest();

		public BotBaseAdvanced(IMyEntity passedEntity, IMyShipController controller, bool isMultipart = false)
		{
			ThisEntity = passedEntity;
			MyShipController = controller;
			ThisCubeGrid = ((IMyCubeGrid)passedEntity);
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
			ThisCubeGrid.OnBlockRemoved += OnBlockRemoved;
			ThisCubeGrid.OnBlockOwnershipChanged += OnOnBlockOwnershipChanged;
			ThisCubeGrid.OnBlockIntegrityChanged += OnBlockIntegrityChanged;
			MyConfig = !isMultipart ? new EemPrefabConfig(controller.CustomData) : Marshall.BotOrphans[passedEntity.EntityId].MyLegacyConfig;
			SetupBot();
		}

		public void Unload()
		{
			Marshall.WriteToLog("BotCore", $"Shutting down -\tId:\t{ThisEntity.EntityId}\tName:\t{ThisEntity.DisplayName}", true);
			ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
			ThisCubeGrid.OnBlockRemoved -= OnBlockRemoved;
			ThisCubeGrid.OnBlockOwnershipChanged -= OnOnBlockOwnershipChanged;
			ThisCubeGrid.OnBlockIntegrityChanged -= OnBlockIntegrityChanged;
		}

		internal virtual void SetupBot()
		{
			SetFactionOwnership();
		}

		private void SetFactionOwnership()
		{
			try
			{
				long factionFounderId = MyAPIGateway.Session.Factions.TryGetFactionByTag(MyConfig.Faction).FounderId;
				
				ThisCubeGrid.ChangeGridOwnership(factionFounderId, Constants.ShareMode);
				foreach (IMyCubeGrid grid in MyAPIGateway.GridGroups.GetGroup(ThisCubeGrid, GridLinkTypeEnum.Mechanical))
					grid.ChangeGridOwnership(factionFounderId, Constants.ShareMode);
			}
			catch (Exception e)
			{
				Marshall.ExceptionLog("SetFactionOwnership", e.ToString());
			}
		}

		private void OnBlockAdded(IMySlimBlock addedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that added the block

		}

		private void OnBlockRemoved(IMySlimBlock removedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that removed the block; also scan for main RC removal and shut down bot if Single Part
			if (removedBlock.FatBlock == MyShipController) Shutdown?.Invoke();
		}

		private void OnBlockIntegrityChanged(IMySlimBlock obj)
		{   // Trigger alert, war, all the fun stuff against the entity owner that triggered the integrity change (probably negative only)

		}

		private void OnOnBlockOwnershipChanged(IMyCubeGrid cubeGrid)
		{   // Protection for initial spawn with MES, should be disabled after the first few seconds in game (~300 ticks)

		}
	}
}
