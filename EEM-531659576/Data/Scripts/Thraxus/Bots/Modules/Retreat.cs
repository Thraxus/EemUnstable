using Eem.Thraxus.Common.BaseClasses;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules
{
	internal class Retreat : LogBaseEvent
	{
		private MyCubeGrid _thisGrid;
		private long _gridOwnerId;
		private IMyRemoteControl _myRemoteControl;

		public Retreat(MyCubeGrid myCubeGrid, IMyShipController myShipController)
		{
			_thisGrid = myCubeGrid;
			_gridOwnerId = myShipController.OwnerId;
			_myRemoteControl = (IMyRemoteControl) myShipController;
		}

		void foo(double? x = null)
		{
			//_myRemoteControl.
			double y;

			y = x ?? 1;
		}
	}
}
