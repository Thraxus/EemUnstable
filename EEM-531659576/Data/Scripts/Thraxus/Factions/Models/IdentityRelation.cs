using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Factions.DataTypes;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.Models
{
	public class IdentityRelation : LogBaseEvent
	{
		public readonly IMyIdentity FromIdentity;
		public readonly Dictionary<long, int> ToFactions;

		public IdentityRelation(IMyIdentity fromIdentity, Dictionary<long, int> toFactions)
		{
			FromIdentity = fromIdentity;
			ToFactions = toFactions;
		}

		public int GetReputation(long factionId)
		{
			return MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(FromIdentity.IdentityId, factionId);
		}

		public bool RelationExists(long factionId)
		{
			return ToFactions.ContainsKey(factionId);
		}

		public bool SetReputation(long factionId, int rep)
		{
			if (!RelationExists(factionId)) return false;
			ToFactions[factionId] = rep;
			MyAPIGateway.Session.Factions.SetReputation(FromIdentity.IdentityId, factionId, rep);
			return true;
		}
		
		public void AddNewFaction(long factionId, bool hostile, int? rep = null)
		{
			if (hostile)
				ToFactions.Add(factionId, rep ?? GeneralSettings.DefaultNegativeRep);
			else ToFactions.Add(factionId, rep ?? GeneralSettings.DefaultNeutralRep);
		}

		public void ReputationDecay()
		{
			foreach (KeyValuePair<long, int> factionRelationship in ToFactions.Where(factionRelationship => factionRelationship.Value != GeneralSettings.DefaultNegativeRep))
			{	// This will loop past if the rep is default negative, since it's assumed at that point the relationship is either a pirate or un-salvageable
				if (factionRelationship.Value > GeneralSettings.DefaultNeutralRep)
					SetReputation(factionRelationship.Key, factionRelationship.Value + GeneralSettings.RepDecay / 2);
				if (factionRelationship.Value < GeneralSettings.DefaultNegativeRep)
					SetReputation(factionRelationship.Key, factionRelationship.Value + GeneralSettings.RepDecay);
				WriteToLog($"ReputationDecay-{FromIdentity.DisplayName} || {FromIdentity.IdentityId}", $"Successfully decayed rep with {factionRelationship.Key}.  New rep is: {GetReputation(factionRelationship.Key)}", LogType.General);
			}
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
