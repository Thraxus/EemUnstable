using Eem.Thraxus.Bots.Modules.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Support;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Types;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Reporting.Systems.Collections
{
	internal class TurretCollection : EemFunctionalBlockCollection
	{
		public TurretCollection(BotSystemType type) : base(type) { }

		public override void AddBlock(IMyFunctionalBlock block)
		{
			ThisSystem.Add(new Turret(Type, block));
			base.AddBlock(block);
		}
	}
}
