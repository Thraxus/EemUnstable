using Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Systems.Collections;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Navigation : ShipSystemBase
	{
		public Navigation(BotSystemsQuestLog questScreen) : base(questScreen)
		{
			NewSystem(SystemType.Navigation);
		}

		protected sealed override void NewSystem(SystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			NavigationCollection collection = new NavigationCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
	}
}
