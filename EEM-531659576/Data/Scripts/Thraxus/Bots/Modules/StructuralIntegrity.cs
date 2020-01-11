using System.Collections.Generic;
using Eem.Thraxus.Bots.Modules.Support.Integrity;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules
{
	internal class StructuralIntegrity
	{
		private readonly MyCubeGrid _thisGrid;
		private readonly List<Thruster> _thrusters = new List<Thruster>();
		//private List<IMyThrust> _forwardThrusters;
		//private List<IMyThrust> _backwardThrusters;
		//private List<IMyThrust> _leftThrusters;
		//private List<IMyThrust> _rightThrusters;
		//private List<IMyThrust> _upThrusters;
		//private List<IMyThrust> _downThrusters;


		public StructuralIntegrity(MyCubeGrid thisGrid)
		{
			_thisGrid = thisGrid;
			StaticLog.WriteToLog("StructuralIntegrity", $"Made it captain!", LogType.General);
			foreach (MyCubeBlock block in _thisGrid.GetFatBlocks())
			{
				
				IMyThrust myThrust = block as IMyThrust;
				if (myThrust == null) continue;
				_thrusters.Add(new Thruster(myThrust));
				StaticLog.WriteToLog("StructuralIntegrity", $"Adding Thruster {myThrust.EntityId}", LogType.General);
			}
		}


		public void IntegrityChanged()
		{
			foreach (Thruster thruster in _thrusters)
			{
				thruster.DebugTheWorld();
			}
		}

	}
}
