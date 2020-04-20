using Eem.Thraxus.Common.BaseClasses;
using Sandbox.Game.Entities;

namespace Eem.Thraxus.Bots.Modules.Targeting
{
	internal class PassiveScanner : LogBaseEvent
	{
		private MyCubeGrid _thisGrid;
		private long _gridOwnerId;

		public PassiveScanner(MyCubeGrid myCubeGrid, long ownerId)
		{
			_thisGrid = myCubeGrid;
			_gridOwnerId = ownerId;
		}
	}
}
