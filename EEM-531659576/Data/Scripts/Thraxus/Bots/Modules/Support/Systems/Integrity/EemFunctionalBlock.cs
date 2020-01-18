using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems.Integrity
{
	internal class EemFunctionalBlock : IReportIntegrity
	{
		private IMyFunctionalBlock _myFunctionalBlock;

		private MyCubeBlock _myCubeBlock;

		private IMySlimBlock _mySlimBlock;

		public SystemType Type { get; }

		public bool IsClosed { get; private set; }

		private bool IsFunctional => _myFunctionalBlock.IsFunctional;

		private float MaxIntegrity { get; }

		private float CurrentIntegrity => _mySlimBlock.Integrity;

		private float AccumulatedDamage => _mySlimBlock.AccumulatedDamage;

		private float MaxFunctionalIntegrity { get; }

		private float CurrentFunctionalIntegrity()
		{
			if (IsClosed) return 0;
			if (IsDestroyed()) return 0;
			if (!IsFunctional) return 0;
			return (MaxFunctionalIntegrity - (MaxIntegrity - CurrentIntegrity) - AccumulatedDamage);
		}

		public int CurrentFunctionalIntegrityRatio()
		{
			return (int) ((CurrentFunctionalIntegrity() / MaxFunctionalIntegrity)* 100);
		}

		private bool IsDestroyed()
		{
			if (_myFunctionalBlock.InScene) return false;
			Close();
			return true;
		}

		public EemFunctionalBlock(SystemType type, IMyFunctionalBlock myFunctionalBlock)
		{
			Type = type;
			_myFunctionalBlock = myFunctionalBlock;
			_myCubeBlock = (MyCubeBlock)_myFunctionalBlock;
			_mySlimBlock = _myFunctionalBlock.SlimBlock;
			MaxIntegrity = _mySlimBlock.MaxIntegrity;
			MaxFunctionalIntegrity = MaxIntegrity * (1f - _myCubeBlock.BlockDefinition.CriticalIntegrityRatio);
		}

		public void Close()
		{
			if (IsClosed) return;
			_myFunctionalBlock = null;
			_myCubeBlock = null;
			_mySlimBlock = null;
			IsClosed = true;
		}

		public override string ToString()
		{
			return $"{_myFunctionalBlock.EntityId} | {MaxFunctionalIntegrity} | {CurrentFunctionalIntegrity()} | {CurrentFunctionalIntegrityRatio()}%";
		}
	}
}
