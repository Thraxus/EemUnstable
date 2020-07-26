using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Types;
using Eem.Thraxus.Common.Enums;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Collections
{
	internal class ThrusterCollection : EemFunctionalBlockCollection
	{
		public ThrusterCollection(BotSystemType type) : base(type) {}
		
		public override void AddBlock(IMyFunctionalBlock block)
		{
			ThisSystem.Add(new Thruster(Type, (IMyThrust) block));
			base.AddBlock(block);
		}
	}
}
