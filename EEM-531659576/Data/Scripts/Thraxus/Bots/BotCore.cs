using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Eem.Thraxus.Bots.Models;
using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Utilities;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using IMyEntity = VRage.ModAPI.IMyEntity;

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

		private bool _setupApproved;
		private bool _setupComplete;

		private MultiPartBot _multiPartBot;

		private static BotCore _instance;
		private IMyCubeGrid _thisCubeGrid;

		private List<IMyShipController> _myShipControllers;

		private MyEntityUpdateEnum _originalUpdateEnum;

		/// <inheritdoc />
		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			base.Init(objectBuilder);
			_originalUpdateEnum = NeedsUpdate;
			NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;
			_setupApproved = true;
		}
		
		/// <inheritdoc />
		public override void UpdateBeforeSimulation()
		{
			base.UpdateBeforeSimulation();
			if (_setupComplete) return;
			PreApproveSetup();
			if (_setupApproved) ProceedWithSetup();
			else SetupDenied();
		}

		private void PreApproveSetup()
		{
			if (Entity.Physics == null)
			{
				_setupApproved = false;
				return;
			}
			_myShipControllers = new List<IMyShipController>();
			GetControllers();
			if (_myShipControllers.Count == 0)
			{
				_setupApproved = false;
				return;
			}
			_setupApproved = true;
		}

		/// <inheritdoc />
		public override void Close()
		{
			if (_setupApproved) Unload();
			_instance = null;
			base.Close();
		}
		private void SetupDenied()
		{
			_setupComplete = true;
			_myShipControllers.Clear();
			NeedsUpdate = _originalUpdateEnum;
		}
		private void ProceedWithSetup()
		{ // Base bot choice here (single or multi)
			_setupComplete = true;
			_instance = this;
			_thisCubeGrid = ((IMyCubeGrid) Entity);
			_thisCubeGrid.OnBlockRemoved += OnBlockRemoved;
			_multiPartBot = new MultiPartBot(Entity, _myShipControllers);
			_multiPartBot.Shutdown += delegate
			{
				Close();
				SetupDenied();
				_multiPartBot.Unload();
			};
		}

		private void Unload()
		{
			Marshall.WriteToLog("BotCore", $"Shutting down -\tId:\t{Entity.EntityId}\tName:\t{Entity.DisplayName}", true);
			_myShipControllers.Clear();
		}
		private void OnBlockRemoved(IMySlimBlock removedBlock)
		{
			if (!(removedBlock.FatBlock is IMyShipController)) return;
			GetControllers();
			Marshall.WriteToLog("OnBlockRemoved", $"\tId:\t{Entity.EntityId}\tName:\t{Entity.DisplayName}\tController Count:\t{_myShipControllers.Count}", true);
			if (_myShipControllers.Count != 0) return;
			Unload();
		}

		public static void Shutdown()
		{
			
		}

		/// <inheritdoc />
		public override void UpdateOnceBeforeFrame()
		{
			base.UpdateOnceBeforeFrame();
		}

		/// <inheritdoc />
		public override void OnAddedToScene()
		{
			base.OnAddedToScene();
		}

		private void GetControllers()
		{
			try
			{
				MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid((IMyCubeGrid)Entity).GetBlocksOfType(_myShipControllers);
				for (int i = _myShipControllers.Count - 1; i >= 0; i--)
				{
					if (!_myShipControllers[i].CanControlShip)
						_myShipControllers.RemoveAtFast(i);
				}
			}
			catch (Exception e)
			{
				Marshall.ExceptionLog("GetShipControllers",$"Exception!\t{e}");
			}
		}
	}
}
