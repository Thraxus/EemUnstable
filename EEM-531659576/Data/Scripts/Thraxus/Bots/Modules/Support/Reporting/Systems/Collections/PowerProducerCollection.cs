using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Types;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Collections
{
	internal class PowerProducerCollection : EemFunctionalBlockCollection
	{
		public PowerProducerCollection(SystemType type) : base(type)  { }

		public override void AddBlock(IMyFunctionalBlock block)
		{
			ThisSystem.Add(new PowerProducer(Type, block));
			base.AddBlock(block);
		}
	}
}