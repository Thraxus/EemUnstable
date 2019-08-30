using Eem.Thraxus.Common.BaseClasses;
using Sandbox.Game.Entities;

namespace Eem.Thraxus.Bots.Modules
{
	class RetreatProtocol : LogBaseEvent
	{
		private MyCubeGrid _thisGrid;
		private long _gridOwnerId;

		public RetreatProtocol(MyCubeGrid myCubeGrid, long ownerId)
		{
			_thisGrid = myCubeGrid;
			_gridOwnerId = ownerId;
		}
	}
}
