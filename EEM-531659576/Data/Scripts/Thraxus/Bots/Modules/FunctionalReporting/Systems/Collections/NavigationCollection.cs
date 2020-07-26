using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Types;
using Eem.Thraxus.Common.Enums;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Collections
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
