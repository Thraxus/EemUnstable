using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Types;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Collections
{
	internal class ThrusterCollection : EemFunctionalBlockCollection
	{
		public ThrusterCollection(SystemType type) : base(type) {}
		
		public override void AddBlock(IMyFunctionalBlock block)
		{
			ThisSystem.Add(new Thruster(Type, (IMyThrust) block));
			base.AddBlock(block);
		}
	}
}
