using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Collections;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Support;
using Eem.Thraxus.Common.Enums;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems
{
	internal class Turrets : ShipSystemBase
	{
		// TODO: Ideally they also need to be expanded to cover internal (position agnostic) vs external positional

		public Turrets(BotSystemsQuestLog questScreen) : base(questScreen)
		{
			NewSystem(BotSystemType.Turret);
		}

		protected sealed override void NewSystem(BotSystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			TurretCollection collection = new TurretCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
	}
}