using Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Systems.Types;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems.Collections
{
	internal class ThrusterCollection : EemFunctionalBlockCollection
	{
		public float LastReportedMaxEffectiveThrust { get; private set; }

		public float LastReportedMaxThrust { get; private set; }

		public ThrusterCollection(SystemType type) : base(type) {}
		
		public override void AddBlock(IMyFunctionalBlock block)
		{
			ShipSystems.Add(new Thruster(Type, (IMyThrust) block));
		}

		public override void UpdateCurrentFunctionalIntegrityRatio()
		{
			base.UpdateCurrentFunctionalIntegrityRatio();
			LastReportedMaxEffectiveThrust = 0;
			LastReportedMaxThrust = 0;
			if (ShipSystems.Count == 0) return;
			foreach (EemFunctionalBlock system in ShipSystems)
			{
				LastReportedMaxEffectiveThrust += ((Thruster)system).GetMaxEffectiveThrust();
				LastReportedMaxThrust += ((Thruster)system).GetMaxThrust();
			}
		}
	}
}
