﻿using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Collections;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Support;
using Eem.Thraxus.Common.Enums;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems
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