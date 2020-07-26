using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Collections;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Support;
using Eem.Thraxus.Common.Enums;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems
{
	internal class Propulsion : ShipSystemBase
	{
		public Propulsion(BotSystemsQuestLog questScreen) : base(questScreen)
		{
			NewSystem(BotSystemType.ForwardPropulsion);
			NewSystem(BotSystemType.ReversePropulsion);
			NewSystem(BotSystemType.LeftPropulsion);
			NewSystem(BotSystemType.RightPropulsion);
			NewSystem(BotSystemType.UpPropulsion);
			NewSystem(BotSystemType.DownPropulsion);
		}

		protected sealed override void NewSystem(BotSystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			ThrusterCollection collection = new ThrusterCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
	}
}
