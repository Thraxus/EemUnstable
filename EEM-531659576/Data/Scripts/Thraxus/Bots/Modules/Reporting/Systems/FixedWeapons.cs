using Eem.Thraxus.Bots.Modules.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Collections;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Support;

namespace Eem.Thraxus.Bots.Modules.Reporting.Systems
{
	internal class FixedWeapons : ShipSystemBase
	{
		public FixedWeapons(BotSystemsQuestLog questScreen) : base(questScreen)
		{
			NewSystem(SystemType.FixedWeapon);
		}

		protected sealed override void NewSystem(SystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			FixedWeaponCollection collection = new FixedWeaponCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
	}
}