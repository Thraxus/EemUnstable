using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Types;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Collections
{
	internal class ThrusterCollection : EemFunctionalBlockCollection
	{
		public float LastReportedMaxEffectiveThrust { get; private set; }

		public float LastReportedMaxThrust { get; private set; }

		public ThrusterCollection(SystemType type) : base(type) {}
		
		public override void AddBlock(IMyFunctionalBlock block)
		{
			ThisSystem.Add(new Thruster(Type, (IMyThrust) block));
			base.AddBlock(block);
		}

		public override bool UpdateCurrentFunctionalIntegrityRatio(long blockId)
		{
			if(!base.UpdateCurrentFunctionalIntegrityRatio(blockId)) return false;
			LastReportedMaxEffectiveThrust = 0;
			LastReportedMaxThrust = 0;
			if (ThisSystem.Count == 0) return false;
			foreach (EemFunctionalBlock system in ThisSystem)
			{
				LastReportedMaxEffectiveThrust += ((Thruster)system).GetMaxEffectiveThrust();
				LastReportedMaxThrust += ((Thruster)system).GetMaxThrust();
			}
			return true;
		}
	}
}
