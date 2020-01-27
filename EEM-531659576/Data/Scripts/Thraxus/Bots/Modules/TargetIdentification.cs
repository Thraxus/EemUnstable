using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Statics;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageRender;

namespace Eem.Thraxus.Bots.Modules
{
	internal class TargetIdentification : LogBaseEvent
	{
		private MyCubeGrid _thisGrid;

		private IMyCubeGrid _thisCubeGrid;

		private readonly long _gridOwnerId;

		private bool _isClosed;

		private const float ActiveTargetingRange = 2000;

		private const float ActivePollingRange = 4000;

		private const float PassiveScanningRange = 15000;

		private readonly ConcurrentCachingList<MyEntity> _allTargets = new ConcurrentCachingList<MyEntity>();

		public TargetIdentification(MyCubeGrid myCubeGrid, long ownerId)
		{
			_thisGrid = myCubeGrid;
			_thisCubeGrid = myCubeGrid;
			_gridOwnerId = ownerId;
		}

		public void GetAllEnemiesInRange()
		{
			foreach (IMyEntity potentialTarget in Statics.DetectTopMostEntitiesInSphere(_thisCubeGrid.GetPosition(), ActiveTargetingRange))
			{
				IMyCubeGrid targetGrid = potentialTarget as IMyCubeGrid;
				if (targetGrid != null)
				{
					if (targetGrid == _thisCubeGrid) continue;
					if (targetGrid.BigOwners.Contains(_gridOwnerId)) continue;
					Statics.AddGpsLocation($"PotentialTarget-Grid - {Statics.CalculateGridThreat((MyCubeGrid) targetGrid)}", potentialTarget.GetPosition());
					continue;
				}
				
				IMyCharacter targetCharacter = potentialTarget as IMyCharacter;
				if (targetCharacter != null)
				{
					Statics.AddGpsLocation($"PotentialTarget-Player - {Statics.CalculatePlayerThreat(targetCharacter, _thisCubeGrid.GetPosition())}", potentialTarget.GetPosition());
					continue;
				}
				
				//WriteToLog("GetAllEnemiesInRange", $"Filters failed to detect: {potentialTarget.EntityId} | {potentialTarget.GetType()}", LogType.General);
			}
			// Notes: Check grid for fat blocks to rule out garbage grids
		}

		public void Close()
		{
			if (_isClosed) return;
			_isClosed = true;
			_allTargets.ClearList();
			_allTargets.ApplyChanges();
			_thisCubeGrid = null;
			_thisGrid = null;
		}
	}
}
