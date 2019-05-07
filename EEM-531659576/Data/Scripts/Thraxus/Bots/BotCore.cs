using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.Models;
using Eem.Thraxus.Bots.Settings;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace Eem.Thraxus.Bots
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeGrid), false)]
	internal class BotCore : BaseServerGameLogicComp
	{
		/*
		 * TODO Damage Handler
		 * TODO Bot Setup
		 * TODO Faction Monitor to ensure MES isn't stealing us again on init
		 * TODO Bot Classification (drone, fighter, station, trader, etc)
		 * TODO Replace bot scripts with ModAPI (traders mostly)
		 * TODO Target Identifier
		 * TODO Alert Conditions
		 * TODO Fight or Flight Conditions
		 * TODO Kamikaze Conditions
		 * TODO Reinforcement Conditions / calls (antenna drones)
		 *
		 * TODO Convert existing code to new code base modules, including logs
		 * TODO Upcoming: Factions, AiSessionCore, DamageHandler
		 * TODO Finished: EntityManager, BotCore, BotBaseAdvanced, BotMarshall
		 * TODO Ignored: MissileMaster
		 */

		//private EemPrefabConfig prefabConfig;

		//private bool _setupApproved;
		private bool _setupComplete;
		private bool _multiPart;
		
		private BotBaseAdvanced _bot;

		private List<IMyShipController> _myShipControllers;
		private IMyShipController _myShipController;

		private MyEntityUpdateEnum _originalUpdateEnum;

		/// <inheritdoc />
		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			base.Init(objectBuilder);
			if (!Helpers.Constants.IsServer) return;
			_originalUpdateEnum = NeedsUpdate;
			NeedsUpdate |= Constants.CoreUpdateSchedule;
			//if (!_setupComplete) Setup();
			//PreApproveSetup();
			//if (_setupApproved) ProceedWithSetup();
			//else Shutdown();
		}

		public override void UpdateOnceBeforeFrame()
		{
			base.UpdateOnceBeforeFrame();
			if (!Helpers.Constants.IsServer) return;
			if (!_setupComplete) Setup();
			//if (_setupComplete) return;
			//PreApproveSetup();
			//if (_setupApproved) ProceedWithSetup();
			//else Shutdown();
		}

		/// <inheritdoc />
		public override void UpdateBeforeSimulation()
		{   // Basic tick timer on the ship level
			base.UpdateBeforeSimulation();
			if (!Helpers.Constants.IsServer) return;
			//if (!_setupComplete) Setup();
		}

		private void Setup()
		{
			_setupComplete = true;
			if (!PreApproveSetup())
			{
				Shutdown();
				return;
			}
			WriteToLog("Setup", $"Approved.", LogType.General);
			_bot = new BotBaseAdvanced(Entity, _myShipController, _multiPart);
			_bot.WriteToStaticLog += WriteToLog;
			_bot.BotShutdown += Shutdown;
			_bot.BotSleep += Sleep;
			_bot.BotWakeup += WakeUp;
			_bot.Run();
		}

		private void Shutdown()
		{
			WriteToLog("Shutdown", $"Shutdown triggered for {Entity.DisplayName} with ID {Entity.EntityId}", LogType.General);
			_setupComplete = true;
			NeedsUpdate = _originalUpdateEnum;
			_myShipControllers?.Clear();
			if (_bot != null)
			{
				_bot.BotShutdown -= Shutdown;
				_bot.BotSleep -= Sleep;
				_bot.BotWakeup -= WakeUp;
				_bot.WriteToStaticLog -= WriteToLog;
			}
			_bot?.Unload();
		}

		private void Sleep()
		{
			NeedsUpdate = _originalUpdateEnum;
		}

		private void WakeUp()
		{
			NeedsUpdate |= Constants.CoreUpdateSchedule;
		}

		//private void ProceedWithSetup()
		//{ // Base bot choice here (single or multi)
 	//		WriteToLog("ProceedWithSetup", $"Setup approved.", LogType.General);
		//	_setupComplete = true;
		//	_bot = new BotBaseAdvanced();
		//	_bot.Run(Entity, _myShipController);
		//	_bot.WriteToLog += WriteToLog;
		//	_bot.BotShutdown += Shutdown;
		//	_bot.BotSleep += Sleep;
		//	_bot.BotWakeup += WakeUp;
		//}

		/// <inheritdoc />
		public override void OnAddedToScene()
		{
			// TODO: Potential use for warp in effect?  
			base.OnAddedToScene();
		}

		private bool PreApproveSetup()
		{
			try
			{
				EntityName = Entity.DisplayName;
				EntityId = Entity.EntityId;

				if (Entity.Physics == null) return false;

				_myShipControllers = new List<IMyShipController>();
				GetControllers();

				if (_myShipControllers.Count == 0) return false;

				if (BotMarshal.BotOrphans.ContainsKey(Entity.EntityId))
				{
					// TODO: Placeholder for initializing a multipart bot; we already know the setup, and this grid has a functioning control center, so no need to go further
					_multiPart = true;
				}

				foreach (IMyShipController myShipController in _myShipControllers)
					if (myShipController.CustomData.Contains(Constants.EemAiPrefix)) _myShipController = myShipController;

				return _myShipController != null;
			}
			catch (Exception e)
			{
				WriteToLog("PreApproveSetup", $"Exception! {e}", LogType.Exception);
			}
			return false;
		}

		/// <inheritdoc />
		public override void Close()
		{
			if (!Helpers.Constants.IsServer) return;
			Shutdown();
			base.Close();
		}
		
		private void GetControllers()
		{
			try
			{
				MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid((IMyCubeGrid)Entity).GetBlocksOfType(_myShipControllers);
				for (int i = _myShipControllers.Count - 1; i >= 0; i--)
				{
					if (!_myShipControllers[i].CanControlShip || !_myShipControllers[i].IsFunctional)
						_myShipControllers.RemoveAtFast(i);
				}
			}
			catch (Exception e)
			{
				WriteToLog("GetShipControllers",$"Exception!\t{e}", LogType.Exception);
			}
		}
	}
}
