﻿using Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Systems.Collections;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Propulsion : ShipSystemBase
	{
		public Propulsion(BotSystemsQuestLog questScreen) : base(questScreen)
		{
			NewSystem(SystemType.ForwardPropulsion);
			NewSystem(SystemType.ReversePropulsion);
			NewSystem(SystemType.LeftPropulsion);
			NewSystem(SystemType.RightPropulsion);
			NewSystem(SystemType.UpPropulsion);
			NewSystem(SystemType.DownPropulsion);
		}

		protected sealed override void NewSystem(SystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			ThrusterCollection collection = new ThrusterCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
	}
}
