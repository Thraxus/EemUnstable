using System.Linq;
using Eem.Thraxus.Bots.Modules.Targeting.Support;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Statics;
using Sandbox.Game.Entities;
using VRage.Collections;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Targeting
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
		private readonly TargetCompare _compare = new TargetCompare();

		public TargetIdentification(MyCubeGrid myCubeGrid, long ownerId, ConcurrentCachingList<ValidTarget> validTargets)
		{
			_thisGrid = myCubeGrid;
			_thisCubeGrid = myCubeGrid;
			_gridOwnerId = ownerId;
			_validTargets = validTargets;
		}

		public void GetAllEnemiesInRange()
		{
			int i = 0;
			// TODO: Need to make this list self-pruning, or need to somehow figure out how to remove duplicate ID's
			// TODO: Throw this all out and start fresh. something isn't right, so need to plan better
			// Perhaps a self-managing class for targets.. only use the below to grab targets, use the class to manage them
			foreach (IMyEntity potentialTarget in Statics.DetectTopMostEntitiesInSphere(_thisCubeGrid.GetPosition(), ActiveTargetingRange))
			{
				i++;
				IMyCubeGrid targetGrid = potentialTarget as IMyCubeGrid;
				if (targetGrid != null)
				{
					if (targetGrid == _thisCubeGrid) continue;
					if (targetGrid.BigOwners.Contains(_gridOwnerId)) continue;
					if (Statics.GetRelationBetweenGrids(_thisCubeGrid, targetGrid) == FactionRelationship.Friends) continue;
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
					if (Statics.GetRelationBetweenGridAndCharacter(_thisCubeGrid, targetCharacter) == FactionRelationship.Friends) continue;
					ValidTarget newTarget = new ValidTarget(Statics.CalculatePlayerThreat(targetCharacter, _thisCubeGrid.GetPosition()), null, targetCharacter);
					if (_validTargets.Contains(newTarget)) continue;
					_validTargets.Add(newTarget);
					_validTargets.ApplyAdditions();
					Statics.AddGpsLocation($"PotentialTarget-Player - {newTarget.Threat}", potentialTarget.GetPosition());
					continue;
				}
				
			}
			_validTargets.Sort(_compare);
			WriteToLog("GetAllEnemiesInRange", $"{i} | {_validTargets.Count} | {_validTargets.FirstOrDefault()}", LogType.General);
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
