using Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Systems.Collections;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Power : ShipSystemBase
	{
		public Power(BotSystemsQuestLog questScreen) : base(questScreen)
		{
			NewSystem(SystemType.PowerProducer);
		}

		protected sealed override void NewSystem(SystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			PowerProducerCollection collection = new PowerProducerCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
	}
}
