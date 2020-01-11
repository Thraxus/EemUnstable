using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Integrity
{
	internal class Thruster
	{
		private readonly IMyThrust _thruster;
		private readonly MyCubeBlock _thrusterCube;
		private readonly IMySlimBlock _thrusterSlim;

		private bool IsFunctional => _thruster.IsFunctional;

		private float CurrentIntegrity => _thrusterSlim.Integrity;

		private readonly float _functionalIntegrity;

		private float RemainingIntegrity => IsFunctional ? _functionalIntegrity - (_maxIntegrity - CurrentIntegrity) : 0;
		
		private readonly float _maxIntegrity;

		public Thruster(IMyThrust thruster)
		{
			_thruster = thruster;
			_thrusterCube = (MyCubeBlock) _thruster;
			_thrusterSlim = thruster.SlimBlock;
			_maxIntegrity = _thrusterSlim.MaxIntegrity;
			_functionalIntegrity = _maxIntegrity * (1f - _thrusterCube.BlockDefinition.CriticalIntegrityRatio);
		}

		public void DebugTheWorld()
		{
			StaticLog.WriteToLog($"DebugTheWorld",$"{_thruster.EntityId} | {RemainingIntegrity} | {_functionalIntegrity} | {CurrentIntegrity} | {_maxIntegrity} | {_thrusterCube.BlockDefinition.CriticalIntegrityRatio}", LogType.General);
		}
	}
}
