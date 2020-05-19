using Eem.Thraxus.Bots.Modules.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Collections;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Support;

namespace Eem.Thraxus.Bots.Modules.Reporting.Systems
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
