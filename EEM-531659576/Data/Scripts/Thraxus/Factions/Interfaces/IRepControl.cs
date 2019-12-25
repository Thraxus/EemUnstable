using System;
using Eem.Thraxus.Factions.DataTypes;

namespace Eem.Thraxus.Factions.Interfaces
{
	internal interface IRepControl
	{
		void AddNewRelation(long id, int? rep = null);

		bool RelationExists(long id);

		int GetReputation(long id);

		void SetReputation(long id, int rep);

		void DecayReputation();

		void TriggerWar(long against);

		void IsDialogRequired(int oldRep, long against);

		void DialogRequest(DialogType dialog, string sender);

		string ToString();

		string ToStringExtended();
	}
}
