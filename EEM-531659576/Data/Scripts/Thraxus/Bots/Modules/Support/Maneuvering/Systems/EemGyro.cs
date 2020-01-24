using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Maneuvering.Systems
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
