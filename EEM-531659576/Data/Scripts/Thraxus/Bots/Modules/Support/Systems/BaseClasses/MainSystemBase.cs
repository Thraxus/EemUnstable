using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.Integrity;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses
{
	internal class MainSystemBase : INeedUpdates
	{
		public event Action<SystemType, float> SystemDamaged;

		protected readonly List<IReportDamage> TrackedBlocks = new List<IReportDamage>();

		private readonly SystemType _type;

		internal MainSystemBase(SystemType type)
		{
			_type = type;
			MaxFunctionalIntegrity = 0;
		}

		public float MaxFunctionalIntegrity { get; private set; }

		public float RemainingFunctionalIntegrity { get; private set; }

		public float RemainingFunctionalIntegrityRatio => RemainingFunctionalIntegrity / MaxFunctionalIntegrity;

		public bool IsClosed { get; private set; }

		public void AddBlock(IMyFunctionalBlock block)
		{
			//StaticLog.WriteToLog($"MainSystemBase - {_type} - AddBlock", $"{block.EntityId}", LogType.General);
			CustomFunctionalBlock newBlock = new CustomFunctionalBlock(_type, block);
			newBlock.SystemDamaged += OnSystemDamaged;
			TrackedBlocks.Add(newBlock);
			UpdateMaxFunctionalIntegrity();
			UpdateRemainingFunctionalIntegrity();
		}

		protected void OnSystemDamaged(SystemType type, float unused)
		{
			UpdateRemainingFunctionalIntegrity();
		}

		protected void UpdateMaxFunctionalIntegrity()
		{
			float x = 0;
			foreach (IReportDamage reporter in TrackedBlocks)
				x += reporter.FunctionalIntegrity;
			MaxFunctionalIntegrity = x;
		}

		protected void UpdateRemainingFunctionalIntegrity()
		{
			float x = 0;
			foreach (IReportDamage reporter in TrackedBlocks)
				x += reporter.RemainingFunctionalIntegrity;
			RemainingFunctionalIntegrity = x;
			SystemDamaged?.Invoke(_type, RemainingFunctionalIntegrityRatio);
		}

		public void RunUpdate()
		{
			//StaticLog.WriteToLog($"RunUpdate {_type}", $"UpdateCalled", LogType.General);
			foreach (IReportDamage reporter in TrackedBlocks)
				reporter.RunUpdate();
		}

		public void Close()
		{
			if (IsClosed) return;
			foreach (IReportDamage reporter in TrackedBlocks)
			{
				reporter.SystemDamaged -= OnSystemDamaged;
				reporter.Close();
			}
			TrackedBlocks.Clear();
			IsClosed = true;
		}

		public override string ToString()
		{
			return $"{_type}: {MaxFunctionalIntegrity} | {MaxFunctionalIntegrity} | {RemainingFunctionalIntegrityRatio}";
		}
	}
}
