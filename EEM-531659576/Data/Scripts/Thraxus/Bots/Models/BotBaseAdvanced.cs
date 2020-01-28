using System;
using Eem.Thraxus.Bots.Modules;
using Eem.Thraxus.Bots.Modules.Support.Maneuvering;
using Eem.Thraxus.Bots.Modules.Support.Reporting;
using Eem.Thraxus.Bots.SessionComps;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Debug;
using Eem.Thraxus.Factions;
using Eem.Thraxus.Factions.DataTypes;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Models
{
	public class BotBaseAdvanced : LogBaseEvent
	{
		internal readonly IMyEntity ThisEntity;
		internal readonly MyCubeGrid ThisCubeGrid;
		internal readonly IMyCubeGrid ThisMyCubeGrid;

		private IMyFaction _myFaction;
		private readonly IMyShipController _myShipController;
		private readonly EemPrefabConfig _myConfig;

		private AlertSetting _alertStatus;

		private long _ownerId;
		private bool _sleeping;
		
		private readonly MyConcurrentHashSet<long> _warHash = new MyConcurrentHashSet<long>();
		private readonly MyConcurrentDictionary<IMySlimBlock, float> _integrityDictionary = new MyConcurrentDictionary<IMySlimBlock, float>();
		private readonly ConcurrentCachingList<ValidTarget> _validTargets = new ConcurrentCachingList<ValidTarget>();

		private long _lastAttacked;
		private long _ticks;
		
		public event ShutdownRequest BotShutdown;
		public delegate void ShutdownRequest();

		public event SleepRequest BotSleep;
		public delegate void SleepRequest();

		public event WakeupRequest BotWakeup;
		public delegate void WakeupRequest();

		private bool _barsActive;

		// TODO Remove the below later to their proper bot type
		//private RegenerationProtocol _regenerationProtocol;
		private AlertConditions _emergencyLockDownProtocol;
		private ShipSystems _shipSystems;
		private ShipControl _shipControl;
		private TargetIdentification _targetIdentification;


		public BotBaseAdvanced(IMyEntity passedEntity, IMyShipController controller, bool isMultipart = false)
		{
			ThisEntity = passedEntity;
			ThisCubeGrid = ((MyCubeGrid) ThisEntity);
			ThisMyCubeGrid = (IMyCubeGrid) ThisEntity;
			_myShipController = controller;
			controller.IsMainCockpit = true;
			_myConfig = !isMultipart ? new EemPrefabConfig(controller.CustomData) : BotMarshal.BotOrphans[passedEntity.EntityId].MyLegacyConfig;
			
			// TODO Remove the below later to their proper bot type
			//_regenerationProtocol = new RegenerationProtocol(passedEntity);
			//_regenerationProtocol.OnWriteToLog += WriteToLog;
		}

		internal void Run()
		{
			//WriteToLog("Run", $"{_myConfig.ToStringVerbose()}", LogType.General);
			WriteToLog("BotCore", $"BotBaseAdvanced powering up.", LogType.General);
			_myFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(_myConfig.Faction);
			_ownerId = _myFaction?.FounderId ?? 0;
			//_ownerId = _myConfig.Faction == "Nobody" ? 0 : MyAPIGateway.Session.Factions.TryGetFactionByTag(_myConfig.Faction).FounderId;

			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
			ThisCubeGrid.OnBlockRemoved += OnBlockRemoved;
			ThisCubeGrid.OnBlockOwnershipChanged += OnOnBlockOwnershipChanged;
			ThisCubeGrid.OnBlockIntegrityChanged += OnBlockIntegrityChanged;
			BotMarshal.RegisterNewEntity(ThisEntity.EntityId);
			Damage.TriggerAlert += DamageHandlerOnTriggerAlert;
			//DamageHandler.TriggerIntegrityCheck += DamageHandlerOnTriggerIntegrityCheck;
			BotMarshal.ModDictionary.TryGetValue(BotSettings.BarsModId, out _barsActive);
			SetupBot();

			foreach (IMySlimBlock mySlimBlock in ThisCubeGrid.CubeBlocks)
				_integrityDictionary.Add(mySlimBlock, mySlimBlock.Integrity);

			// TODO Remove the below later to their proper bot type
			_emergencyLockDownProtocol = new AlertConditions(ThisCubeGrid, _ownerId);
			_emergencyLockDownProtocol.OnWriteToLog += WriteToLog;
			_emergencyLockDownProtocol.Init();

			_shipSystems = new ShipSystems(ThisCubeGrid, _myShipController);
			_shipControl = new ShipControl(_myShipController, ThisCubeGrid, ThisMyCubeGrid, ThisEntity);
			_targetIdentification = new TargetIdentification(ThisCubeGrid, _ownerId, _validTargets);
			_targetIdentification.OnWriteToLog += WriteToLog;

			WriteToLog("BotCore", $"BotBaseAdvanced ready to rock!", LogType.General);
		}
		
		public void Unload()
		{
			try
			{
				WriteToLog("BotCore", $"Shutting down.", LogType.General);
				BotMarshal.RemoveDeadEntity(ThisEntity.EntityId);
				Damage.TriggerAlert -= DamageHandlerOnTriggerAlert;
				ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
				ThisCubeGrid.OnBlockRemoved -= OnBlockRemoved;
				ThisCubeGrid.OnBlockOwnershipChanged -= OnOnBlockOwnershipChanged;
				ThisCubeGrid.OnBlockIntegrityChanged -= OnBlockIntegrityChanged;
				_warHash.Clear();
				_integrityDictionary.Clear();
				_shipSystems.Close();
				_shipControl.Close();
				_validTargets.ClearList();
				_validTargets.ApplyChanges();
				_targetIdentification.Close();
				_targetIdentification.OnWriteToLog -= WriteToLog;
				// TODO Remove the below later to their proper bot type
				_emergencyLockDownProtocol.OnWriteToLog -= WriteToLog;
				//_regenerationProtocol.OnWriteToLog -= WriteToLog;
				//_regenerationProtocol = null;
			}
			catch (Exception e)
			{
				WriteToLog("Unload", $"Exception! {e}", LogType.Exception);
			}
		}

		private void UpdateCollections(long blockId)
		{
			//if (_ticks - _lastShipIntegrityUpdate < 10) return;
			//_lastShipIntegrityUpdate = _ticks;
			// TODO: Non-threaded this can tank sim.  Need to investigate areas for improvement
			// TODO: Still an issue...
			//if (!IntegrityNeedsUpdate) return;
			//MyAPIGateway.Parallel.StartBackground(() =>
			_shipSystems.UpdateIntegrity(blockId);
			//);

			//MyAPIGateway.Parallel.StartBackground(() =>
			_shipControl.UpdateBlocks();
			//);
			//WriteToLog($"UpdateShipIntegrity", $"UpdateCalled", LogType.General);
			//IntegrityNeedsUpdate = false;
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

		private enum CheckType
		{
			Integrity, Ownership, Removed
		}

		private void HandleBars(IMySlimBlock block, CheckType check)
		{
			//if (!_barsActive) return;
			// TODO: Make sure this still works.  updated when i change away from timespan
			// TODO: Restore BaRS Protection; disabled with the new damage handler
			if ((_ticks - _lastAttacked) < (GeneralSettings.TicksPerSecond / 2)) return;
			
			switch (check)
			{
				case CheckType.Removed:
				case CheckType.Ownership:
					//WriteToLog("HandleBars", $"Block removed or owner changed, alerting the damage handler... {inMs}", LogType.General);
					
					//DamageHandler.BarsSuspected(ThisEntity);
					break;
				case CheckType.Integrity:
					{
						float oldIntegrity;
						if (!_integrityDictionary.TryGetValue(block, out oldIntegrity))
							return;
						if (oldIntegrity <= block.Integrity)
						{
							//WriteToLog("HandleBars", $"Block integrity improved; setting check value higher", LogType.General);
							_integrityDictionary[block] = block.Integrity;
							return;
						}

						//WriteToLog("HandleBars", $"Block integrity lowered, alerting the damage handler... {inMs}", LogType.General);
						//DamageHandler.BarsSuspected(ThisEntity);

						break;
					}
				default:
					return;
			}

			//WriteToLog("HandleBars", $"Bars Suspected... {_lastAttacked} -- {DateTime.Now}", LogType.General);
			//DamageHandler.BarsSuspected(ThisEntity);
		}

		public void ScheduledUpdates(long ticks)
		{
			_ticks = ticks;

			if (_alertStatus == AlertSetting.Wartime && _ticks % GeneralSettings.TicksPerSecond == 0)
			{	
				_targetIdentification.GetAllEnemiesInRange();
			}

			if (_lastAttacked == 0) return;
			if (_ticks - _lastAttacked <= GeneralSettings.TicksPerMinute * GeneralSettings.AlertCooldown) return;
			WriteToLog("EvaluateAlerts", $"{ticks} | {_lastAttacked} | {_ticks - _lastAttacked >= GeneralSettings.TicksPerMinute * GeneralSettings.AlertCooldown}", LogType.General);
			_warHash.Clear();
			_lastAttacked = 0;
			SetAlertStatus(AlertSetting.Peacetime);
		}

		private void SetAlertStatus(AlertSetting alert)
		{
			_alertStatus = alert;
			_emergencyLockDownProtocol.Alert(alert);
		}

		private void DamageHandlerOnTriggerAlert(long shipId, long blockId, long playerId)
		{
			if (ThisEntity.EntityId != shipId) return;
			UpdateCollections(blockId);
			if ( _ownerId == 0 || playerId == _ownerId || playerId == _myFaction.FactionId) return;

			_lastAttacked = _ticks;
			if (_warHash.Contains(playerId))
				return;
			_warHash.Add(playerId);

			//_myShipController.IsMainCockpit = true;
			//_myShipController.MoveAndRotate(new Vector3(50, 50, 120), new Vector2(60, 120), 90f);

			//_shipControl.ActivateManeuvering();
			//_shipControl.DebugRotate();

			FactionCore.FactionCoreStaticInstance.RegisterWar(new PendingWar(playerId, _myFaction.FactionId));
			SetAlertStatus(AlertSetting.Wartime);
			BotWakeup?.Invoke();
		}
		
		private void OnBlockAdded(IMySlimBlock addedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that added the block
			//WriteToLog("OnBlockAdded", $"Triggering alert to Damage Handler", LogType.General);
			DamageHandlerOnTriggerAlert(ThisEntity.EntityId, 0, addedBlock.GetObjectBuilder().BuiltBy);
			//DamageHandler.ErrantBlockPlaced(ThisEntity.EntityId, addedBlock);
			WriteToLog("OnBlockAdded", $"Block added.", LogType.General);
		}

		private void OnBlockRemoved(IMySlimBlock removedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that removed the block; also scan for main RC removal and shut down bot if Single Part
			// TODO Remove the below later to their proper bot type
			//_regenerationProtocol.ReportBlockState();
			
			if (_barsActive)
				HandleBars(removedBlock, CheckType.Removed);
			if (removedBlock.FatBlock != _myShipController) return;
			//WriteToLog("OnBlockRemoved", $"Triggering shutdown.", LogType.General);
			BotShutdown?.Invoke();
		}

		private void OnBlockIntegrityChanged(IMySlimBlock block)
		{   // Trigger alert, war, all the fun stuff against the entity owner that triggered the integrity change (probably negative only)
			IMyCubeBlock x = block.FatBlock;
			if (x != null)
				UpdateCollections(x.EntityId);

			if (_barsActive)
				HandleBars(block, CheckType.Integrity);
			//WriteToLog("OnBlockIntegrityChanged", $"Block integrity changed for block {block} {_lastAttacked.AddSeconds(1) < DateTime.Now} {_lastAttacked.AddSeconds(1)} {DateTime.Now}", LogType.General);
			if (block.FatBlock == _myShipController && !_myShipController.IsFunctional)
			{
				//WriteToLog("OnBlockIntegrityChanged", $"Rc integrity compromised... triggering sleep.", LogType.General);
				Sleep();
				BotSleep?.Invoke();
			}

			if (block.FatBlock == _myShipController && _myShipController.IsFunctional && _sleeping)
			{
				//WriteToLog("OnBlockIntegrityChanged", $"Rc integrity restored... triggering wakeup.", LogType.General);
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
				HandleBars(null, CheckType.Ownership);
			//WriteToLog("OnBlockOwnershipChanged", $"Ownership changed. {_lastAttacked.AddSeconds(1) < DateTime.Now} {_lastAttacked.AddSeconds(1)} {DateTime.Now}", LogType.General);
			if (_myShipController.OwnerId != _ownerId) BotShutdown?.Invoke();
		}

		private void Sleep()
		{   // TODO make sure damage handlers are here since the bot is simulating offline
			WriteToLog("Sleep", $"Going to sleep.", LogType.General);
			_sleeping = true;
			ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
		}

		private void Wakeup()
		{   // TODO make sure damage handlers are here so we can properly wake the bot up
			_sleeping = false;
			WriteToLog("Wakeup", $"Waking up.", LogType.General);
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
		}
	}
}
