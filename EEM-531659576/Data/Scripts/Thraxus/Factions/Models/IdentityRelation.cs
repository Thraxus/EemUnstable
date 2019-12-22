using System.Collections.Generic;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Interfaces;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.Models
{
	public class IdentityRelation : IRepControl
	{
		public readonly IMyIdentity FromIdentity;
		public readonly HashSet<long> ToFactions;

		public IdentityRelation(IMyIdentity fromIdentity, IEnumerable<long> toFactions)
		{
			FromIdentity = fromIdentity;
			ToFactions = (HashSet<long>) toFactions;
		}

		public int GetReputation(long factionId)
		{
			return MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(FromIdentity.IdentityId, factionId);
		}

		public void SetReputation(long id, int rep)
		{
			if (!RelationExists(id)) return;
			MyAPIGateway.Session.Factions.SetReputation(FromIdentity.IdentityId, id, rep);
		}

		public void DecayReputation()
		{
			foreach (long factionRelationship in ToFactions)
			{   // This will loop past if the rep is default negative, since it's assumed at that point the relationship is either a pirate or un-salvageable
				int rep = GetReputation(factionRelationship);
				if (rep > GeneralSettings.DefaultNeutralRep)
					SetReputation(factionRelationship, rep + GeneralSettings.RepDecay / 2);
				if (rep < GeneralSettings.DefaultNegativeRep)
					SetReputation(factionRelationship, rep + GeneralSettings.RepDecay);
			}
		}

		public bool RelationExists(long factionId)
		{
			return ToFactions.Contains(factionId);
		}

		
		public void AddNewFaction(long factionId, int? rep = null)
		{
			ToFactions.Add(factionId);
			if (rep != null)
				SetReputation(factionId, (int) rep);
		}
		
		public void TriggerWar(long againstFaction)
		{
			int rep = GetReputation(againstFaction);

			if (rep > GeneralSettings.DefaultWarRep - GeneralSettings.AdditionalWarRepPenalty)
				SetReputation(againstFaction, GeneralSettings.DefaultWarRep);
			else 
				SetReputation(againstFaction, rep - GeneralSettings.AdditionalWarRepPenalty);
		}

		public IdentityRelationSave GetSaveState()
		{
			return new IdentityRelationSave(FromIdentity.IdentityId, ToFactions);
		}

		public override string ToString()
		{
			return $"";
			//return $"FromFactionId: {FromFaction.FactionId} | FromFactionTag: {FromFaction.Tag} | ToFactionTag: {Factions.FactionId} | ToFactionTag: {Factions.Tag} | Reputation: {Reputation}";
		}
	}
}
