using Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Systems.Collections;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Weapons : ShipSystemBase
	{
		// TODO: Weapons needs to be expanded to include fixed point (rocket launchers, gatling guns)
		//			Ideally they also need to be expanded to cover internal (position agnostic) vs external positional

		public Weapons(BotSystemsQuestLog questScreen) : base(questScreen)
		{
			NewSystem(SystemType.Weapon);
		}

		protected sealed override void NewSystem(SystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			WeaponCollection collection = new WeaponCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
	}
}