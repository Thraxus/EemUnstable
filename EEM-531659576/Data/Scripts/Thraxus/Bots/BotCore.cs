using System.Collections.Generic;
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

		private BotOrphan _myOldParentInfo;

		private IMyCubeGrid _thisEntity;

		private MyEntityUpdateEnum _originalUpdateEnum;

		private List<IMyShipController> myShipControllers;

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
			_thisEntity = (IMyCubeGrid)Entity;
			myShipControllers = new List<IMyShipController>();
			MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(_thisEntity).GetBlocksOfType(myShipControllers);
			if (myShipControllers.Count == 0)
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
			base.Close();
		}
		private void SetupDenied()
		{
			_setupComplete = true;
			NeedsUpdate = _originalUpdateEnum;
		}
		private void ProceedWithSetup()
		{
			((IMyCubeGrid)Entity).OnGridSplit += OnGridSplit;
			((IMyCubeGrid)Entity).OnBlockAdded += OnBlockAdded;
			((IMyCubeGrid)Entity).OnBlockRemoved += OnBlockRemoved;
			((IMyCubeGrid)Entity).OnBlockOwnershipChanged += OnOnBlockOwnershipChanged;
			SetupBot();
		}

		private void Unload()
		{
			Marshall.WriteToLog("BotCore", $"Shutting down -\tId:\t{Entity.EntityId}\tName:\t{Entity.DisplayName}", true);
			_thisEntity.OnGridSplit -= OnGridSplit;
			_thisEntity.OnBlockAdded -= OnBlockAdded;
			_thisEntity.OnBlockRemoved -= OnBlockRemoved;
			_thisEntity.OnBlockOwnershipChanged -= OnOnBlockOwnershipChanged;
		}

		/// <inheritdoc />
		public override void UpdateOnceBeforeFrame()
		{
			base.UpdateOnceBeforeFrame();
		}

		private void SetupBot()
		{
			_setupComplete = true;
			Marshall.WriteToLog("SetupBot", $"New Entity -\tId:\t{Entity.EntityId}\tName:\t{Entity.DisplayName}\tController Count:\t{myShipControllers.Count}", true);
			if (Marshall.BotOrphans.TryGetValue(Entity.EntityId, out _myOldParentInfo))
				Marshall.WriteToLog("SetupBot", $"I'm a new Entity with Id: {Entity.EntityId} and {myShipControllers.Count} controllers.  My parent was an entity with Id: {_myOldParentInfo.MyParentId}.  My grandparent was an entity with Id: {_myOldParentInfo.MyGrandParentId}", true);

		}

		/// <inheritdoc />
		public override void OnAddedToScene()
		{
			base.OnAddedToScene();
		}

		private void OnGridSplit(IMyCubeGrid originalGrid, IMyCubeGrid newGrid)
		{
			Marshall.BotOrphans.Add(newGrid.EntityId, new BotOrphan(originalGrid.EntityId, _myOldParentInfo.MyParentId));
			Marshall.WriteToLog("OnGridSplit", $"Parent:\t{originalGrid.EntityId}\tChild:\t{newGrid.EntityId}", true);
		}

		private void OnBlockAdded(IMySlimBlock addedBlock)
		{

		}

		private void OnBlockRemoved(IMySlimBlock removedBlock)
		{
			if (!(removedBlock.FatBlock is IMyShipController)) return;
			MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(_thisEntity).GetBlocksOfType(myShipControllers);
			Marshall.WriteToLog("OnBlockRemoved", $"\tId:\t{Entity.EntityId}\tName:\t{Entity.DisplayName}\tController Count:\t{myShipControllers.Count}", true);
			if (myShipControllers.Count != 0) return;
			_setupApproved = false;
			SetupDenied();
			Unload();
		}

		private void OnOnBlockOwnershipChanged(IMyCubeGrid cubeGrid)
		{
			
		}

		
	}
}
