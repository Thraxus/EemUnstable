using Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Systems.Types;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems.Collections
{
	internal class NavigationCollection : EemFunctionalBlockCollection
	{
		public NavigationCollection(SystemType type) : base(type) { }

		public override void AddBlock(IMyFunctionalBlock block)
		{
			ShipSystems.Add(new Navigator(Type, block));
		}
	}
}
