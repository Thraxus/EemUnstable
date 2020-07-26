using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Collections;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Support;
using Eem.Thraxus.Common.Enums;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems
{
	internal class Power : ShipSystemBase
	{
		// TODO: Power needs to be expanded to cover % produced by whatever object is considered a power producer
		//			Problem right now is if you have 20 solar panels and 1 large reactor, hp numbers are identical
		//			Expected outcome is HP numbers are weighted for the reactor since it's output far exceeds 20 solar panels

		public Power(BotSystemsQuestLog questScreen) : base(questScreen)
		{
			NewSystem(BotSystemType.PowerProducer);
		}

		protected sealed override void NewSystem(BotSystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			PowerProducerCollection collection = new PowerProducerCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
	}
}
