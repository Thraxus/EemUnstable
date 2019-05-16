using System;
using Eem.Thraxus.Bots.SessionComps;
using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.Models
{
	public class BotBaseAdvanced : StaticLogBase
	{
		internal readonly IMyEntity ThisEntity;
		internal readonly IMyCubeGrid ThisCubeGrid;
		private readonly IMyShipController _myShipController;
		private readonly EemPrefabConfig _myConfig;
		private long _ownerId;
		private bool _sleeping;
		private MyConcurrentDictionary<long, DateTime> _warDictionary;
		private DateTime _lastAttacked;

		public event ShutdownRequest BotShutdown;
		public delegate void ShutdownRequest();

		public event SleepRequest BotSleep;
		public delegate void SleepRequest();

		public event WakeupRequest BotWakeup;
		public delegate void WakeupRequest();

		private bool _barsActive;

		public BotBaseAdvanced(IMyEntity passedEntity, IMyShipController controller, bool isMultipart = false)
		{
			ThisEntity = passedEntity;
			ThisCubeGrid = ((IMyCubeGrid)ThisEntity);
			_myShipController = controller;
			_myConfig = !isMultipart ? new EemPrefabConfig(controller.CustomData) : BotMarshal.BotOrphans[passedEntity.EntityId].MyLegacyConfig;
		}

		internal void Run()
		{
			WriteToLog("Run", $"{_myConfig.ToStringVerbose()}", LogType.General);
			WriteToLog("BotCore", $"BotBaseAdvanced powering up.", LogType.General);
			_lastAttacked = DateTime.Now;
			_ownerId = _myConfig.Faction == "Nobody" ? 0 : MyAPIGateway.Session.Factions.TryGetFactionByTag(_myConfig.Faction).FounderId;
			_warDictionary = new MyConcurrentDictionary<long, DateTime>();
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
			ThisCubeGrid.OnBlockRemoved += OnBlockRemoved;
			ThisCubeGrid.OnBlockOwnershipChanged += OnOnBlockOwnershipChanged;
			ThisCubeGrid.OnBlockIntegrityChanged += OnBlockIntegrityChanged;
			BotMarshal.RegisterNewEntity(ThisEntity.EntityId);
			DamageHandler.TriggerAlert += DamageHandlerOnTriggerAlert;
			BotMarshal.ModDictionary.TryGetValue(BotSettings.BarsModId, out _barsActive);
			SetupBot();
		}

		public void Unload()
		{
			WriteToLog("BotCore", $"Shutting down.", LogType.General);
			_warDictionary.Clear();
			BotMarshal.RemoveDeadEntity(ThisEntity.EntityId);
			ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
			ThisCubeGrid.OnBlockRemoved -= OnBlockRemoved;
			ThisCubeGrid.OnBlockOwnershipChanged -= OnOnBlockOwnershipChanged;
			ThisCubeGrid.OnBlockIntegrityChanged -= OnBlockIntegrityChanged;
		}

		internal void SetupBot()
		{
			SetFactionOwnership();
			WriteToLog("SetupBot", $"BotBaseAdvanced online.  BaRS Detected: {_barsActive}", LogType.General);
		}

		private void SetFactionOwnership()
		{
			try
			{
				string factionTag = _ownerId == 0 ? "Nobody" : MyAPIGateway.Session.Factions.TryGetFactionByTag(_myConfig.Faction).Tag;
				WriteToLog("SetFactionOwnership", $"Setting faction ownership to {factionTag}", LogType.General);
				ThisCubeGrid.ChangeGridOwnership(_ownerId, BotSettings.ShareMode);
				foreach (IMyCubeGrid grid in MyAPIGateway.GridGroups.GetGroup(ThisCubeGrid, GridLinkTypeEnum.Mechanical))
					grid.ChangeGridOwnership(_ownerId, BotSettings.ShareMode);
			}
			catch (Exception e)
			{
				WriteToLog("SetFactionOwnership", $"{_myConfig.Faction} {e}", LogType.Exception);
			}
		}

		private void HandleBars()
		{
			if (!_barsActive) return;
			WriteToLog("HandleBars", $"Bars Suspected... {_lastAttacked} -- {DateTime.Now}", LogType.General);
			if (_lastAttacked.AddSeconds(1) > DateTime.Now) return;
			WriteToLog("HandleBars", $"Time trigger passed, alerting the damage handler...", LogType.General);
			DamageHandler.BarsSuspected(ThisEntity);
		}
		
		private void DamageHandlerOnTriggerAlert(long shipId, long playerId)
		{
			if (ThisEntity.EntityId == shipId)
				TriggerAlertConditions(playerId);
		}

		private void TriggerAlertConditions(long playerId)
		{
			WriteToLog("TriggerAlertConditions", $"Alert conditions triggered against {playerId}", LogType.General);
			if (!_warDictionary.TryAdd(playerId, DateTime.Now))
				_warDictionary[playerId] = DateTime.Now;
			_lastAttacked = DateTime.Now;
			// TODO Add alert conditions
		}

		private void OnBlockAdded(IMySlimBlock addedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that added the block
			WriteToLog("OnBlockAdded", $"Triggering alert to Damage Handler", LogType.General);
			DamageHandler.ErrantBlockPlaced(ThisEntity.EntityId, addedBlock);
		}

		private void OnBlockRemoved(IMySlimBlock removedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that removed the block; also scan for main RC removal and shut down bot if Single Part
			if (_barsActive)
				HandleBars();
			if (removedBlock.FatBlock != _myShipController) return;
			WriteToLog("OnBlockRemoved", $"Triggering shutdown.", LogType.General);
			BotShutdown?.Invoke();
		}

		private void OnBlockIntegrityChanged(IMySlimBlock block)
		{   // Trigger alert, war, all the fun stuff against the entity owner that triggered the integrity change (probably negative only)
			if (_barsActive)
				HandleBars();
			WriteToLog("OnBlockIntegrityChanged", $"Block integrity changed for block {block}", LogType.General);
			if (block.FatBlock == _myShipController && !_myShipController.IsFunctional)
			{
				WriteToLog("OnBlockIntegrityChanged", $"Rc integrity compromised... triggering sleep.", LogType.General);
				Sleep();
				BotSleep?.Invoke();
			}

			if (block.FatBlock == _myShipController && _myShipController.IsFunctional && _sleeping)
			{
				WriteToLog("OnBlockIntegrityChanged", $"Rc integrity restored... triggering wakeup.", LogType.General);
				Wakeup();
				BotWakeup?.Invoke();
			}
		}

		private void RestoreControllerIntegrity()
		{
			_myShipController.SlimBlock.IncreaseMountLevel(_myShipController.SlimBlock.MaxIntegrity - _myShipController.SlimBlock.Integrity, _ownerId);
			_myShipController.SlimBlock.FixBones(0f, 0f);
		}

		private void OnOnBlockOwnershipChanged(IMyCubeGrid cubeGrid)
		{   // Protection for initial spawn with MES, should be disabled after the first few seconds in game (~300 ticks)
			if (_barsActive)
				HandleBars();
			WriteToLog("OnBlockOwnershipChanged", $"Ownership changed.", LogType.General);
			if (_myShipController.OwnerId != _ownerId) BotShutdown?.Invoke();
		}

		private void Sleep()
		{   // TODO make sure damage handlers are here since the bot is simulating offline
			WriteToLog("Sleep", $"Going to sleep.", LogType.General);
			_sleeping = true;
			ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
		}

		private void Wakeup()
		{	// TODO make sure damage handlers are here so we can properly wake the bot up
			_sleeping = false;
			WriteToLog("Wakeup", $"Waking up.", LogType.General);
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
		}
	}
}
