using System;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace Eem.Thraxus.Bots.Modules.Support.Systems.Integrity
{
	internal class CustomFunctionalBlock : IReportDamage
	{
		public event Action<SystemType, float> SystemDamaged;

		private IMyFunctionalBlock _myFunctionalBlock;

		private MyCubeBlock _myCubeBlock;

		private IMySlimBlock _mySlimBlock;

		private readonly SystemType _type;

		public CustomFunctionalBlock(SystemType type, IMyFunctionalBlock block)
		{
			_myFunctionalBlock = block;
			_type = type;
			_myCubeBlock = (MyCubeBlock)_myFunctionalBlock;
			_mySlimBlock = _myFunctionalBlock.SlimBlock;
			MaxIntegrity = _mySlimBlock.MaxIntegrity;
			FunctionalIntegrity = MaxIntegrity * (1f - _myCubeBlock.BlockDefinition.CriticalIntegrityRatio);
			LastUpdatedIntegrity = CurrentIntegrity;
			//LastUpdatedIntegrity = RemainingFunctionalIntegrity;
		}

		public bool IsFunctional => _myFunctionalBlock.IsFunctional;

		public bool IsDestroyed => !_myFunctionalBlock.InScene;

		public float MaxIntegrity { get; }

		public float AccumulatedDamage => _mySlimBlock.AccumulatedDamage;

		public float FunctionalIntegrity { get; }

		public float CurrentIntegrity => _mySlimBlock.Integrity;

		public float RemainingFunctionalIntegrity => IsDestroyed ? 0 : IsFunctional ? FunctionalIntegrity - (MaxIntegrity - CurrentIntegrity) : 0;

		public float RemainingIntegrityRatio => RemainingFunctionalIntegrity / FunctionalIntegrity;
		
		public float LastUpdatedIntegrity { get; private set; }

		public bool IsClosed { get; private set; }

		public void RunUpdate()
		{
			//if (!MyUtils.IsZero(CurrentIntegrity, LastUpdatedIntegrity)) return;
			if (!(Math.Abs(CurrentIntegrity - LastUpdatedIntegrity) > 0.01)) return;
			SystemDamaged?.Invoke(_type, RemainingIntegrityRatio);
			LastUpdatedIntegrity = CurrentIntegrity;
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
			return $"{_myFunctionalBlock.EntityId} | {FunctionalIntegrity} | {RemainingFunctionalIntegrity}";
		}
	}
}
