using System.Text;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Support;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Reporting.Systems.BaseClasses
{
	internal abstract class EemFunctionalBlock : IReportIntegrity
	{
		private IMyFunctionalBlock _myFunctionalBlock;

		private MyCubeBlock _myCubeBlock;

		private IMySlimBlock _mySlimBlock;

		private readonly long _originalOwner;

		public readonly long MyId; 

		private bool _isDisabled;

		public BotSystemType Type { get; }

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
			if (_isDisabled) return 0;
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

		protected EemFunctionalBlock(BotSystemType type, IMyFunctionalBlock myFunctionalBlock)
		{
			Type = type;
			_myFunctionalBlock = myFunctionalBlock;
			_myCubeBlock = (MyCubeBlock)_myFunctionalBlock;
			_mySlimBlock = _myFunctionalBlock.SlimBlock;
			_originalOwner = _myCubeBlock.CubeGrid.EntityId;
			MyId = myFunctionalBlock.EntityId;
			MaxIntegrity = _mySlimBlock.MaxIntegrity;
			MaxFunctionalIntegrity = MaxIntegrity * (1f - _myCubeBlock.BlockDefinition.CriticalIntegrityRatio);
			_myFunctionalBlock.IsWorkingChanged += WorkingChanged;
			_myFunctionalBlock.OnMarkForClose += MarkedForClose;
			_myFunctionalBlock.OwnershipChanged += OwnershipChanged;
		}

		private void OwnershipChanged(IMyTerminalBlock thisBlock)
		{
			if (thisBlock.EntityId != MyId) return;
			StaticLog.WriteToLog("OwnershipChanged", $"{Type} | {MyId}", LogType.General);
			Close();
		}

		private void MarkedForClose(IMyEntity thisBlock)
		{
			if (thisBlock.EntityId != MyId) return;
			StaticLog.WriteToLog("MarkedForClose", $"{Type} | {MyId}", LogType.General);
			Close();
		}

		private void WorkingChanged(IMyCubeBlock thisBlock)
		{
			if (thisBlock.EntityId != MyId) return;
			_isDisabled = !thisBlock.IsWorking;
			StaticLog.WriteToLog("WorkingChanged", $"{Type} | {MyId} | {_isDisabled}", LogType.General);
		}

		public virtual void Close()
		{
			if (IsClosed) return;
			IsClosed = true;
			StaticLog.WriteToLog("Close", $"{Type} | {MyId}", LogType.General);
			_myFunctionalBlock.IsWorkingChanged -= WorkingChanged;
			_myFunctionalBlock.OnMarkForClose -= MarkedForClose;
			_myFunctionalBlock.OwnershipChanged -= OwnershipChanged;
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
