using Eem.Thraxus.Common.BaseClasses;
using Sandbox.Game.Entities;

namespace Eem.Thraxus.Bots.Modules
{
	class FightOrFlight : LogBaseEvent
	{
		private MyCubeGrid _thisGrid;
		private long _gridOwnerId;

		public FightOrFlight(MyCubeGrid myCubeGrid, long ownerId)
		{
			_thisGrid = myCubeGrid;
			_gridOwnerId = ownerId;
		}
	}
}
