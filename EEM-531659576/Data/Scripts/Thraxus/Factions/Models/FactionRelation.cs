using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Interfaces;

namespace Eem.Thraxus.Factions.Models
{
	public class FactionRelation : IRepControl
	{
		public readonly IMyFaction FromFaction;
		public readonly IMyFaction ToFaction;

		private int Reputation => MyAPIGateway.Session.Factions.GetReputationBetweenFactions(FromFaction.FactionId, ToFaction.FactionId);

		public IEnumerable<long> MemberList => FromFaction.Members.Keys;

		public FactionRelation(IMyFaction fromFaction, IMyFaction toFaction, int reputation)
		{
			FromFaction = fromFaction;
			ToFaction = toFaction;
			SetReputation(reputation);
		}

		public void SetReputation(int rep)
		{
			MyAPIGateway.Session.Factions.SetReputation(FromFaction.FactionId, ToFaction.FactionId, rep);
			foreach (KeyValuePair<long, MyFactionMember> member in FromFaction.Members)
				MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(member.Key, ToFaction.FactionId, rep);
		}

		public void AddNewMember(long newMemberId)
		{
			int newRep = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(newMemberId, ToFaction.FactionId);
			if (newRep < GeneralSettings.DefaultNeutralRep)
			{
				SetReputation(newRep);
				return;
			}
			SetReputation(((Reputation * (FromFaction.Members.Count - 1)) + newRep) / FromFaction.Members.Count);
		}
		
		public bool RelationExists(long id)
		{
			return ToFaction.FactionId == id;
		}

		public int GetReputation(long id)
		{
			return Reputation;
		}

		public void SetReputation(long id, int rep)
		{
			SetReputation(rep);
		}

		public void DecayReputation()
		{
			int rep = Reputation;

			if (rep > GeneralSettings.DefaultNeutralRep)
				SetReputation(rep + GeneralSettings.RepDecay / 2);
			if (rep < GeneralSettings.DefaultNegativeRep)
				SetReputation(rep + GeneralSettings.RepDecay);
		}

		public void TriggerWar(long against)
		{
			int rep = GetReputation(against);

			if (rep > GeneralSettings.DefaultWarRep - GeneralSettings.AdditionalWarRepPenalty)
				SetReputation(against, GeneralSettings.DefaultWarRep);
			else
				SetReputation(against, rep - GeneralSettings.AdditionalWarRepPenalty);
		}
		

		public FactionRelationSave GetSaveState()
		{   
			return new FactionRelationSave(FromFaction.FactionId, ToFaction.FactionId, Reputation);
		}

		public override string ToString()
		{
			return $"FromFactionId: {FromFaction.FactionId} | FromFactionTag: {FromFaction.Tag} | ToFactionTag: {ToFaction.FactionId} | ToFactionTag: {ToFaction.Tag} | Reputation: {Reputation}";
		}
	}
}