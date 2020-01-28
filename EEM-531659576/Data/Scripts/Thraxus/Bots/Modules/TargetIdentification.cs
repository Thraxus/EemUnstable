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

		private readonly ConcurrentCachingList<ValidTarget> _validTargets;

		public TargetIdentification(MyCubeGrid myCubeGrid, long ownerId, ConcurrentCachingList<ValidTarget> validTargets)
		{
			_thisGrid = myCubeGrid;
			_thisCubeGrid = myCubeGrid;
			_gridOwnerId = ownerId;
			_validTargets = validTargets;
		}

		public void GetAllEnemiesInRange()
		{
			// TODO: Need to make this list self-pruning, or need to somehow figure out how to remove duplicate ID's
			foreach (IMyEntity potentialTarget in Statics.DetectTopMostEntitiesInSphere(_thisCubeGrid.GetPosition(), ActiveTargetingRange))
			{
				IMyCubeGrid targetGrid = potentialTarget as IMyCubeGrid;
				if (targetGrid != null)
				{
					if (targetGrid == _thisCubeGrid) continue;
					if (targetGrid.BigOwners.Contains(_gridOwnerId)) continue;
					if (Statics.GetRelationBetweenGrids(_thisCubeGrid, targetGrid) == FactionRelationships.Friends) continue;
					ValidTarget newTarget = new ValidTarget(Statics.CalculateGridThreat((MyCubeGrid)targetGrid), targetGrid, null);
					if (_validTargets.Contains(newTarget)) continue;
					_validTargets.Add(newTarget);
					_validTargets.ApplyAdditions();
					Statics.AddGpsLocation($"PotentialTarget-Grid - {newTarget.Threat}", potentialTarget.GetPosition());
					continue;
				}
				
				IMyCharacter targetCharacter = potentialTarget as IMyCharacter;
				if (targetCharacter != null)
				{
					if (Statics.GetRelationBetweenGridAndCharacter(_thisCubeGrid, targetCharacter) == FactionRelationships.Friends) continue;
					ValidTarget newTarget = new ValidTarget(Statics.CalculatePlayerThreat(targetCharacter, _thisCubeGrid.GetPosition()), null, targetCharacter);
					if (_validTargets.Contains(newTarget)) continue;
					_validTargets.Add(newTarget);
					_validTargets.ApplyAdditions();
					//_validTargets.Sort();
					Statics.AddGpsLocation($"PotentialTarget-Player - {newTarget.Threat}", potentialTarget.GetPosition());
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
			_thisCubeGrid = null;
			_thisGrid = null;
		}
	}
}
