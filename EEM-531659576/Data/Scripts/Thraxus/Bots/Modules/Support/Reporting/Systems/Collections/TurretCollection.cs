using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Types;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Collections
{
	internal class TurretCollection : EemFunctionalBlockCollection
	{
		public TurretCollection(SystemType type) : base(type) { }

		public override void AddBlock(IMyFunctionalBlock block)
		{
			ThisSystem.Add(new Turret(Type, block));
			base.AddBlock(block);
		}
	}
}
