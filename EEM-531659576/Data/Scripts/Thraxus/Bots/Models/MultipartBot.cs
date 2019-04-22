using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.Utilities;
using Sandbox.ModAPI;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;
using IMySlimBlock = VRage.Game.ModAPI.IMySlimBlock;
using IMyEntity = VRage.ModAPI.IMyEntity;

namespace Eem.Thraxus.Bots.Models
{
	internal class MultiPartBot
	{
		private readonly IMyEntity _thisEntity;
		private readonly IMyCubeGrid _thisCubeGrid;
		private readonly List<IMyShipController> _myShipControllers;

		private BotOrphan _myOldParentInfo;

		public event ShutdownRequest Shutdown;
		public delegate void ShutdownRequest();

		public MultiPartBot(IMyEntity passedEntity, List<IMyShipController> controllers)
		{
			_thisEntity = passedEntity;
			_myShipControllers = controllers;
			_thisCubeGrid = ((IMyCubeGrid) passedEntity);
			_thisCubeGrid.OnGridSplit += OnGridSplit;
			_thisCubeGrid.OnBlockAdded += OnBlockAdded;
			_thisCubeGrid.OnBlockRemoved += OnBlockRemoved;
			_thisCubeGrid.OnBlockOwnershipChanged += OnOnBlockOwnershipChanged;
			SetupBot();
		}

		public void Unload()
		{
			Marshall.WriteToLog("BotCore", $"Shutting down -\tId:\t{_thisEntity.EntityId}\tName:\t{_thisEntity.DisplayName}", true);
			_myShipControllers.Clear();
			_thisCubeGrid.OnGridSplit -= OnGridSplit;
			_thisCubeGrid.OnBlockAdded -= OnBlockAdded;
			_thisCubeGrid.OnBlockRemoved -= OnBlockRemoved;
			_thisCubeGrid.OnBlockOwnershipChanged -= OnOnBlockOwnershipChanged;
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
			Shutdown?.Invoke();
		}

		private void SetupBot()
		{
			Marshall.WriteToLog("SetupBot", $"New Entity -\tId:\t{_thisEntity.EntityId}\tName:\t{_thisEntity.DisplayName}\tController Count:\t{_myShipControllers.Count}", true);
			if (Marshall.BotOrphans.TryGetValue(_thisEntity.EntityId, out _myOldParentInfo))
				Marshall.WriteToLog("SetupBot", $"I'm a new Entity with Id: {_thisEntity.EntityId} and {_myShipControllers.Count} controllers.  My parent was an entity with Id: {_myOldParentInfo.MyParentId}.  My grandparent was an entity with Id: {_myOldParentInfo.MyGrandParentId}", true);
		}

		private void OnOnBlockOwnershipChanged(IMyCubeGrid cubeGrid)
		{

		}
	}
}
