namespace Eem.Thraxus.Factions.Interfaces
{
	internal interface IRepControl
	{
		bool RelationExists(long id);

		int GetReputation(long id);

		void SetReputation(long id, int rep);

		void DecayReputation();

		void TriggerWar(long against);

		string ToString();
	}
}
