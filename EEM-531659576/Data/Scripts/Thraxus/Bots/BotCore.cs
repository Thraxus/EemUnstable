using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.Models;
using Eem.Thraxus.Bots.SessionComps;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace Eem.Thraxus.Bots
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeGrid), false)]
	// ReSharper disable once ClassNeverInstantiated.Global
	// ReSharper disable once UnusedMember.Global
	internal class BotCore : BaseGameLogicComp
	{
		/*
		 * TODO Damage Handler -- done
		 * TODO Bot Setup -- done		 
		 * TODO Bot Classification (drone, fighter, station, trader, etc) -- done
		 * TODO Replace bot scripts with ModAPI (traders mostly) -- ignored
		 * TODO Target Identifier
		 * TODO Alert Conditions -- done
		 * TODO Fight or Flight Conditions  
		 * TODO Kamikaze Conditions
		 * TODO Reinforcement Conditions / calls (antenna drones)
		 * TODO Faction Monitor to ensure MES isn't stealing us again on init
		 *
		 * Modules and their use:
		 *	Alert Conditions -- Sets wartime conditions on a ship.  Includes, but is not limited to setting up turrets, closing / locking doors, venting atmosphere
		 *	Fight or Flight -- Determines whether the NPC is going to engage the player or run
		 *	Passive Scanner -- Used when just flying around on it's normal path, will keep an eye out for enemies and respond appropriately (call police, keep range, prepare to run, etc.)
		 *	Regeneration -- Concept only, but will repair a ship or station back to it's original configuration
		 *	Reinforcements -- Responsible for triggering the antenna and calling for help (drones).  added here for more control / customization of the reinforcements called
		 *	Retreat -- Controls the method for retreating from the enemy
		 *	Suicide -- When all else fails, the NPC will attempt to use himself as a weapon
		 *	Target Identification -- Looks for targets and chooses which to engage
		 *	Threat Assessment -- Uses targets identified in Target Identification to assess the most dangerous and reports back
		 *
		 * TODO Convert existing code to new code base modules, including logs
		 * TODO Upcoming: Factions
		 * TODO Finished: EntityManager, BotCore, BotBaseAdvanced, BotMarshall, EemCore, DamageHandler
		 * TODO Ignored: MissileMaster
		 */

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
			if (!GeneralSettings.IsServer) return;
			_originalUpdateEnum = NeedsUpdate;
			NeedsUpdate |= BotSettings.CoreUpdateSchedule;
		}

		public override void UpdateOnceBeforeFrame()
		{
			base.UpdateOnceBeforeFrame();
			if (!GeneralSettings.IsServer) return;
			if (!_setupComplete) Setup();
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
			_bot.OnWriteToLog += WriteToLog;
			_bot.BotShutdown += Shutdown;
			_bot.BotSleep += Sleep;
			_bot.BotWakeup += WakeUp;
			_bot.Run();
		}

		private void Shutdown()
		{
			_setupComplete = true;
			NeedsUpdate = _originalUpdateEnum;

			if (_bot == null) return;
			WriteToLog("Shutdown", $"Shutdown triggered for {Entity.DisplayName} with ID {Entity.EntityId}", LogType.General);
			_bot.BotShutdown -= Shutdown;
			_bot.BotSleep -= Sleep;
			_bot.BotWakeup -= WakeUp;
			_bot.OnWriteToLog -= WriteToLog;
			_bot.Unload();
		}

		private void Sleep()
		{
			NeedsUpdate = _originalUpdateEnum;
		}

		private void WakeUp()
		{
			NeedsUpdate |= BotSettings.CoreUpdateSchedule;
		}

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
					if (myShipController.CustomData.Contains(BotSettings.EemAiPrefix)) _myShipController = myShipController;

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
			if (!GeneralSettings.IsServer) return;
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
