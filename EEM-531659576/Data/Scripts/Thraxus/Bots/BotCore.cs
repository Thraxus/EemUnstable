using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.Models;
using Eem.Thraxus.Bots.Settings;
using Eem.Thraxus.Bots.Utilities;
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
	internal class BotCore : MyGameLogicComponent
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
		 */

		//private EemPrefabConfig prefabConfig;

		private bool _setupApproved;
		private bool _setupComplete;

		private static BotCore _instance;

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
			//PreApproveSetup();
			//if (_setupApproved) ProceedWithSetup();
			//else Shutdown();
		}

		private void Shutdown()
		{
			_setupComplete = true;
			NeedsUpdate = _originalUpdateEnum;
			_myShipControllers?.Clear();
			_instance = null;
			if (_bot != null)
			{
				_bot.BotShutdown -= Shutdown;
				_bot.BotSleep -= Sleep;
				_bot.BotWakeup -= WakeUp;
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

		/// <inheritdoc />
		public override void UpdateBeforeSimulation()
		{	// Basic tick timer on the ship level
			base.UpdateBeforeSimulation();
			if (!Helpers.Constants.IsServer) return;
			if (_setupComplete) return;
			PreApproveSetup();
			if (_setupApproved) ProceedWithSetup();
			else
			{
				BaseSessionComp.WriteToLog("UpdateBeforeSimulation", $"Shutdown triggered for {Entity.DisplayName} with ID {Entity.EntityId}", true);
				Shutdown();
			}
		}

		/// <inheritdoc />
		public override void UpdateOnceBeforeFrame()
		{
			base.UpdateOnceBeforeFrame();
			//if (!Helpers.Constants.IsServer) return;
			//if (_setupComplete) return;
			//PreApproveSetup();
			//if (_setupApproved) ProceedWithSetup();
			//else Shutdown();
		}

		private void ProceedWithSetup()
		{ // Base bot choice here (single or multi)
			BaseSessionComp.WriteToLog("ProceedWithSetup", $"Setup approved for {Entity.DisplayName} with ID {Entity.EntityId}", true);
			_setupComplete = true;
			_instance = this;
			_bot = new BotBaseAdvanced(Entity, _myShipController);
			_bot.BotShutdown += Shutdown;
			_bot.BotSleep += Sleep;
			_bot.BotWakeup += WakeUp;
		}

		/// <inheritdoc />
		public override void OnAddedToScene()
		{
			// TODO: Potential use for warp in effect?  
			base.OnAddedToScene();
		}

		private void PreApproveSetup()
		{
			try
			{
				if (Entity.Physics == null) return;

				_myShipControllers = new List<IMyShipController>();
				GetControllers();

				if (_myShipControllers.Count == 0) return;

				if (BotMarshal.BotOrphans.ContainsKey(Entity.EntityId))
				{
					// TODO: Placeholder for initializing a multipart bot; we already know the setup, and this grid has a functioning control center, so no need to go further
					//_setupApproved = true;
					//return;
				}

				foreach (IMyShipController myShipController in _myShipControllers)
					if (myShipController.CustomData.Contains(Constants.EemAiPrefix)) _myShipController = myShipController;

				if (_myShipController == null) return;

				_setupApproved = true;
			}
			catch (Exception e)
			{
				BaseSessionComp.ExceptionLog("PreApproveSetup", $"Exception! {e}");
			}
		}

		/// <inheritdoc />
		public override void Close()
		{
			if (!Helpers.Constants.IsServer) return;
			BaseSessionComp.WriteToLog("Close", $"Shutdown triggered for {Entity.DisplayName} with ID {Entity.EntityId}", true);
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
				BaseSessionComp.ExceptionLog("GetShipControllers",$"Exception!\t{e}");
			}
		}
	}
}
