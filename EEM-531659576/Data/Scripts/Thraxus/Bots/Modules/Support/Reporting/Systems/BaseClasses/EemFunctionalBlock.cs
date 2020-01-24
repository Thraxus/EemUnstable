using System.Text;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses
{
	internal abstract class EemFunctionalBlock : IReportIntegrity
	{
		private IMyFunctionalBlock _myFunctionalBlock;

		private MyCubeBlock _myCubeBlock;

		private IMySlimBlock _mySlimBlock;

		private readonly long _originalOwner;

		public readonly long _myId;

		public SystemType Type { get; }

		public bool IsClosed { get; private set; }

		protected bool IsFunctional => _myFunctionalBlock.IsFunctional;

		private float MaxIntegrity { get; }

		private float CurrentIntegrity => _mySlimBlock.Integrity;
		
		private float MaxFunctionalIntegrity { get; }

		private float CurrentFunctionalIntegrity()
		{
			return (MaxFunctionalIntegrity - (MaxIntegrity - CurrentIntegrity));
		}

		public int CurrentFunctionalIntegrityRatio()
		{
			if (IsClosed) return 0;
			if (IsDestroyed()) return 0;
			if (IsDetached()) return 0;
			if (!IsFunctional) return 0;
			return (int) ((CurrentFunctionalIntegrity() / MaxFunctionalIntegrity)* 100);
		}

		protected bool IsDestroyed()
		{
			if (_myFunctionalBlock.InScene || _myFunctionalBlock.MarkedForClose) return false;
			Close();
			return true;
		}

		protected bool IsDetached()
		{
			if (_myCubeBlock.CubeGrid.EntityId == _originalOwner) return false;
			Close();
			return true;
		}

		protected EemFunctionalBlock(SystemType type, IMyFunctionalBlock myFunctionalBlock)
		{
			Type = type;
			_myFunctionalBlock = myFunctionalBlock;
			_myCubeBlock = (MyCubeBlock)_myFunctionalBlock;
			_mySlimBlock = _myFunctionalBlock.SlimBlock;
			_originalOwner = _myCubeBlock.CubeGrid.EntityId;
			_myId = myFunctionalBlock.EntityId;
			MaxIntegrity = _mySlimBlock.MaxIntegrity;
			MaxFunctionalIntegrity = MaxIntegrity * (1f - _myCubeBlock.BlockDefinition.CriticalIntegrityRatio);
		}

		public virtual void Close()
		{
			if (IsClosed) return;
			IsClosed = true;
			_myFunctionalBlock = null;
			_myCubeBlock = null;
			_mySlimBlock = null;
		}

		public override string ToString()
		{
			return new StringBuilder($"{_myFunctionalBlock.EntityId} | {MaxFunctionalIntegrity} | {CurrentFunctionalIntegrity()} | {CurrentFunctionalIntegrityRatio()}%").ToString();
		}
	}
}
