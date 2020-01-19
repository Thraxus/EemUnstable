using System.Text;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses
{
	internal abstract class EemFunctionalBlock : IReportIntegrity
	{
		private IMyFunctionalBlock _myFunctionalBlock;

		private MyCubeBlock _myCubeBlock;

		private IMySlimBlock _mySlimBlock;

		public SystemType Type { get; }

		public bool IsClosed { get; private set; }

		protected bool IsFunctional => _myFunctionalBlock.IsFunctional;

		private float MaxIntegrity { get; }

		private float CurrentIntegrity => _mySlimBlock.Integrity;

		private float AccumulatedDamage => _mySlimBlock.AccumulatedDamage;

		private float MaxFunctionalIntegrity { get; }

		private float CurrentFunctionalIntegrity()
		{
			return (MaxFunctionalIntegrity - (MaxIntegrity - CurrentIntegrity) - AccumulatedDamage);
		}

		public int CurrentFunctionalIntegrityRatio()
		{
			if (IsClosed) return 0;
			if (IsDestroyed()) return 0;
			if (!IsFunctional) return 0;
			StaticLog.WriteToLog("CurrentFunctionalIntegrityRatio", $"{IsFunctional} | {IsClosed} | {CurrentFunctionalIntegrity()}", LogType.General);
			return (int) ((CurrentFunctionalIntegrity() / MaxFunctionalIntegrity)* 100);
		}

		protected bool IsDestroyed()
		{
			if (_myFunctionalBlock.InScene || _myFunctionalBlock.MarkedForClose) return false;
			Close();
			return true;
		}

		protected EemFunctionalBlock(SystemType type, IMyFunctionalBlock myFunctionalBlock)
		{
			Type = type;
			_myFunctionalBlock = myFunctionalBlock;
			_myCubeBlock = (MyCubeBlock)_myFunctionalBlock;
			_mySlimBlock = _myFunctionalBlock.SlimBlock;
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
