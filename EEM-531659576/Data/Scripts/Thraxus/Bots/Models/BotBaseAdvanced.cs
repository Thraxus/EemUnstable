using System;
using Eem.Thraxus.Bots.Modules;
using Eem.Thraxus.Bots.SessionComps;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.Models
{
	public class BotBaseAdvanced : LogBaseEvent
	{
		internal readonly IMyEntity ThisEntity;
		internal readonly MyCubeGrid ThisCubeGrid;
		private readonly IMyShipController _myShipController;
		private readonly EemPrefabConfig _myConfig;
		private long _ownerId;
		private bool _sleeping;
		private MyConcurrentDictionary<long, DateTime> _warDictionary;
		private MyConcurrentDictionary<IMySlimBlock, float> _integrityDictionary;
		private DateTime _lastAttacked;
		
		public event ShutdownRequest BotShutdown;
		public delegate void ShutdownRequest();

		public event SleepRequest BotSleep;
		public delegate void SleepRequest();

		public event WakeupRequest BotWakeup;
		public delegate void WakeupRequest();

		private bool _barsActive;

		// TODO Remove the below later to their proper bot type
		//private RegenerationProtocol _regenerationProtocol;
		private EmergencyLockDownProtocol _emergencyLockDownProtocol;

		public BotBaseAdvanced(IMyEntity passedEntity, IMyShipController controller, bool isMultipart = false)
		{
			ThisEntity = passedEntity;
			ThisCubeGrid = ((MyCubeGrid)ThisEntity);
			_myShipController = controller;
			_myConfig = !isMultipart ? new EemPrefabConfig(controller.CustomData) : BotMarshal.BotOrphans[passedEntity.EntityId].MyLegacyConfig;

			// TODO Remove the below later to their proper bot type
			//_regenerationProtocol = new RegenerationProtocol(passedEntity);
			//_regenerationProtocol.OnWriteToLog += WriteToLog;
		}

		internal void Run()
		{
			//WriteToLog("Run", $"{_myConfig.ToStringVerbose()}", LogType.General);
			WriteToLog("BotCore", $"BotBaseAdvanced powering up.", LogType.General);
			_lastAttacked = DateTime.Now;
			_ownerId = _myConfig.Faction == "Nobody" ? 0 : MyAPIGateway.Session.Factions.TryGetFactionByTag(_myConfig.Faction).FounderId;
			_warDictionary = new MyConcurrentDictionary<long, DateTime>();
			_integrityDictionary = new MyConcurrentDictionary<IMySlimBlock, float>();
			ThisCubeGrid.OnBlockAdded += OnBlockAdded;
			ThisCubeGrid.OnBlockRemoved += OnBlockRemoved;
			ThisCubeGrid.OnBlockOwnershipChanged += OnOnBlockOwnershipChanged;
			ThisCubeGrid.OnBlockIntegrityChanged += OnBlockIntegrityChanged;
			BotMarshal.RegisterNewEntity(ThisEntity.EntityId);
			DamageHandler.TriggerAlert += DamageHandlerOnTriggerAlert;
			BotMarshal.ModDictionary.TryGetValue(BotSettings.BarsModId, out _barsActive);
			SetupBot();

			foreach (IMySlimBlock mySlimBlock in ThisCubeGrid.CubeBlocks)
				_integrityDictionary.Add(mySlimBlock, mySlimBlock.Integrity);

			// TODO Remove the below later to their proper bot type
			_emergencyLockDownProtocol = new EmergencyLockDownProtocol(ThisCubeGrid);
			_emergencyLockDownProtocol.OnWriteToLog += WriteToLog;
			_emergencyLockDownProtocol.Init();

			WriteToLog("BotCore", $"BotBaseAdvanced ready to rock!", LogType.General);
		}

		public void Unload()
		{
			try
			{
				WriteToLog("BotCore", $"Shutting down.", LogType.General);
				BotMarshal.RemoveDeadEntity(ThisEntity.EntityId);
				DamageHandler.TriggerAlert -= DamageHandlerOnTriggerAlert;
				ThisCubeGrid.OnBlockAdded -= OnBlockAdded;
				ThisCubeGrid.OnBlockRemoved -= OnBlockRemoved;
				ThisCubeGrid.OnBlockOwnershipChanged -= OnOnBlockOwnershipChanged;
				ThisCubeGrid.OnBlockIntegrityChanged -= OnBlockIntegrityChanged;


				// TODO Remove the below later to their proper bot type
				_emergencyLockDownProtocol.OnWriteToLog -= WriteToLog;
				//_regenerationProtocol.OnWriteToLog -= WriteToLog;
				//_regenerationProtocol = null;
			}
			catch (Exception e)
			{
				WriteToLog("Unload", $"Exception!", LogType.Exception);
			}
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

		enum CheckType
		{
			Integrity, Ownership, Removed
		}

		private void HandleBars(IMySlimBlock block, CheckType check)
		{
			//if (!_barsActive) return;
			TimeSpan timeSinceLastAttack = DateTime.Now - _lastAttacked;
			if (timeSinceLastAttack.TotalMilliseconds < 500) return;
			
			switch (check)
			{
				case CheckType.Removed:
				case CheckType.Ownership:
					//WriteToLog("HandleBars", $"Block removed or owner changed, alerting the damage handler... {inMs}", LogType.General);
					DamageHandler.BarsSuspected(ThisEntity);
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
						DamageHandler.BarsSuspected(ThisEntity);

						break;
					}
				default:
					return;
			}

			//WriteToLog("HandleBars", $"Bars Suspected... {_lastAttacked} -- {DateTime.Now}", LogType.General);
			DamageHandler.BarsSuspected(ThisEntity);
		}

		private void DamageHandlerOnTriggerAlert(long shipId, long playerId)
		{
			if (ThisEntity.EntityId == shipId)
				TriggerAlertConditions(playerId);


		}

		private bool debugSwitch;
		private void TriggerAlertConditions(long identityId)
		{
			try
			{
				WriteToLog("TriggerAlertConditions", $"Alert conditions triggered against {identityId}", LogType.General);
				if (!_warDictionary.TryAdd(identityId, DateTime.Now))
					_warDictionary[identityId] = DateTime.Now;
				_lastAttacked = DateTime.Now;
				// TODO Add alert conditions
				if (debugSwitch)
				{
					_emergencyLockDownProtocol.DisableAlert();
					debugSwitch = false;
				}
				else
				{
					_emergencyLockDownProtocol.EnableAlert();
					debugSwitch = true;
				}
			}
			catch (Exception e)
			{
				WriteToLog("TriggerAlertConditions", $"Exception! {e}", LogType.Exception);
			}
		}

		private void OnBlockAdded(IMySlimBlock addedBlock)
		{   // Trigger alert, war, all the fun stuff against the player / faction that added the block
			//WriteToLog("OnBlockAdded", $"Triggering alert to Damage Handler", LogType.General);
			DamageHandler.ErrantBlockPlaced(ThisEntity.EntityId, addedBlock);
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
