using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Debug;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Factions.DataTypes;

namespace Eem.Thraxus.Factions.Models
{
	public class FactionRelation : LogBaseEvent
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
		
		public void ReputationDecay()
		{
			int rep = Reputation;

			if (rep > GeneralSettings.DefaultNeutralRep)
				SetReputation(rep + GeneralSettings.RepDecay/2);
			if(rep < GeneralSettings.DefaultNegativeRep)
				SetReputation(rep + GeneralSettings.RepDecay);

			WriteToLog($"ReputationDecay-{FromFaction.Tag} || {FromFaction.FactionId}",$"Successfully decayed rep with {ToFaction.Tag} || {ToFaction.FactionId}.  New rep is: {Reputation}", LogType.General);
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