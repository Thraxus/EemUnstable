using Eem.Thraxus.Bots.Modules.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Support;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Types;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Reporting.Systems.Collections
{
	internal class NavigationCollection : EemFunctionalBlockCollection
	{
		public NavigationCollection(BotSystemType type) : base(type) { }

		public override void AddBlock(IMyFunctionalBlock block)
		{
			ThisSystem.Add(new Navigator(Type, block));
			base.AddBlock(block);
		}
	}
}
