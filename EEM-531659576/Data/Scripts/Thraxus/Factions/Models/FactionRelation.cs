using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.Tools.Networking;
using Eem.Thraxus.Factions.BaseClasses;
using Eem.Thraxus.Factions.DataTypes;

namespace Eem.Thraxus.Factions.Models
{
	public class FactionRelation : RepControl
	{
		public readonly IMyFaction FromFaction;

		public FactionRelation(IMyFaction fromFaction)
		{
			FromFaction = fromFaction;
			FromRelationId = fromFaction.FactionId;
			RelationType = RelationType.Faction;
		}

		/// <summary>
		/// Adds a new member to the faction.  This member must have their faction set before adding else default SE reps will override
		/// </summary>
		/// <param name="newMemberId"></param>
		public void AddNewFactionMember(long newMemberId)
		{
			foreach (KeyValuePair<long, int> toFaction in ToFactions)
			{
				int newRep = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(newMemberId, toFaction.Key);
				if (newRep != GeneralSettings.DefaultNeutralRep)
				{
					SetReputation(toFaction.Key, newRep);
					return;
				}
				SetReputation(toFaction.Key, ((GetReputation(toFaction.Key) * (FromFaction.Members.Count - 1)) + newRep) / FromFaction.Members.Count);
			}
		}

		public override int GetSeReputation(long id)
		{
			return MyAPIGateway.Session.Factions.GetReputationBetweenFactions(FromFaction.FactionId, id);
		}

		protected override void SetSeReputation(long id, int rep)
		{
			MyAPIGateway.Session.Factions.SetReputation(FromFaction.FactionId, id, rep);
			foreach (KeyValuePair<long, MyFactionMember> member in FromFaction.Members)
				MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(member.Key, id, rep);
		}

		protected override void SendMessage(string message, string sender)
		{
			//StaticLog.WriteToLog("SendMessage", $"From: {sender} | To: {FromRelationId} | Message: {message}", LogType.General);
			foreach (IMyPlayer player in GetPlayers().ToList().Where(player => !player.IsBot && FromFaction.Members.ContainsKey(player.IdentityId)))
				Messaging.SendMessageToPlayer($"{message}", sender, player.IdentityId, MyFontEnum.DarkBlue);
		}
	}
}