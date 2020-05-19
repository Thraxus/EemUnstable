using Eem.Thraxus.Bots.Modules.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Collections;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Support;

namespace Eem.Thraxus.Bots.Modules.Reporting.Systems
{
	internal class Navigation : ShipSystemBase
	{
		public Navigation(BotSystemsQuestLog questScreen) : base(questScreen)
		{
			NewSystem(BotSystemType.Navigation);
		}

		protected sealed override void NewSystem(BotSystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			NavigationCollection collection = new NavigationCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
	}
}
