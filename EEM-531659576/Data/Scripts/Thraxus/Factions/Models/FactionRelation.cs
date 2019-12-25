using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.Tools.Networking;
using Eem.Thraxus.Factions.BaseClasses;

namespace Eem.Thraxus.Factions.Models
{
	public class FactionRelation : RepControl
	{
		public readonly IMyFaction FromFaction;

		public FactionRelation(IMyFaction fromFaction)
		{
			FromFaction = fromFaction;
			FromRelationId = fromFaction.FactionId;
		}

		public void AddNewMember(long newMemberId)
		{
			foreach (long toFaction in ToFactions)
			{
				int newRep = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(newMemberId, toFaction);
				if (newRep < GeneralSettings.DefaultNeutralRep)
				{
					SetReputation(toFaction, newRep);
					return;
				}
				SetReputation(toFaction, ((GetReputation(toFaction) * (FromFaction.Members.Count - 1)) + newRep) / FromFaction.Members.Count);
			}
		}

		protected override int GetReputation(long id)
		{
			return MyAPIGateway.Session.Factions.GetReputationBetweenFactions(FromFaction.FactionId, id);
		}

		protected override void SetReputation(long id, int rep)
		{
			MyAPIGateway.Session.Factions.SetReputation(FromFaction.FactionId, id, rep);
			foreach (KeyValuePair<long, MyFactionMember> member in FromFaction.Members)
				MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(member.Key, id, rep);
		}
		
		protected override void SendMessage(string message, string sender)
		{
			foreach (IMyPlayer player in Players)
			{
				if (!player.IsBot && FromFaction.Members.ContainsKey(player.IdentityId))
					Messaging.SendMessageToPlayer($"{message}", sender, player.IdentityId, MyFontEnum.DarkBlue);
			}
			Players.Clear();
		}
	}
}