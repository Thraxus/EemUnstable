using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Types;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Collections
{
	internal class WeaponCollection : EemFunctionalBlockCollection
	{
		public WeaponCollection(SystemType type) : base(type) { }

		public override void AddBlock(IMyFunctionalBlock block)
		{
			ThisSystem.Add(new Weapon(Type, block));
			base.AddBlock(block);
		}
	}
}
