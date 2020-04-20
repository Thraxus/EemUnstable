using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Maneuvering.Systems
{
	internal class EemGyro
	{
		private IMyShipController _thisController;

		private IMyGyro _thisGyro;

		public EemGyro(IMyShipController thisController, IMyGyro thisGyro)
		{
			_thisController = thisController;
			_thisGyro = thisGyro;
		}


	}
}
