using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Types;
using Eem.Thraxus.Common.Enums;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Collections
{
	internal class PowerProducerCollection : EemFunctionalBlockCollection
	{
		public PowerProducerCollection(BotSystemType type) : base(type)  { }

		public override void AddBlock(IMyFunctionalBlock block)
		{
			ThisSystem.Add(new PowerProducer(Type, block));
			base.AddBlock(block);
		}
	}
}