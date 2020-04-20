using Eem.Thraxus.Common.BaseClasses;
using Sandbox.Game.Entities;

namespace Eem.Thraxus.Bots.Modules.Targeting
{
	internal class ThreatAssessment : LogBaseEvent
	{
		private MyCubeGrid _thisGrid;
		private long _gridOwnerId;

		public ThreatAssessment(MyCubeGrid myCubeGrid, long ownerId)
		{
			_thisGrid = myCubeGrid;
			_gridOwnerId = ownerId;
		}
	}
}
