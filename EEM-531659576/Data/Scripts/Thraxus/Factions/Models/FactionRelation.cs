using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.Tools.Networking;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Interfaces;

namespace Eem.Thraxus.Factions.Models
{
	public class FactionRelation : IRepControl
	{
		public readonly IMyFaction FromFaction;

		public readonly HashSet<long> ToFactions;

		private readonly List<IMyPlayer> _players = new List<IMyPlayer>();

		private IEnumerable<IMyPlayer> Players
		{
			get
			{
				_players.Clear();
				MyAPIGateway.Players.GetPlayers(_players);
				return _players;
			}
		}

		public FactionRelation(IMyFaction fromFaction)
		{
			FromFaction = fromFaction;
			ToFactions = new HashSet<long>();
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

		public void AddNewRelation(long id, int? rep = null)
		{
			if (!MyAPIGateway.Session.Factions.Factions.ContainsKey(id)) return;
			ToFactions.Add(id);
			if (rep != null) 
				SetReputation(id, (int) rep);
		}

		public bool RelationExists(long id)
		{
			return ToFactions.Contains(id);
		}
		
		public int GetReputation(long id)
		{
			return MyAPIGateway.Session.Factions.GetReputationBetweenFactions(FromFaction.FactionId, id);
		}

		public void SetReputation(long id, int rep)
		{
			MyAPIGateway.Session.Factions.SetReputation(FromFaction.FactionId, id, rep);
			foreach (KeyValuePair<long, MyFactionMember> member in FromFaction.Members)
				MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(member.Key, id, rep);
		}
		
		public void DecayReputation()
		{
			foreach (long toFaction in ToFactions)
			{
				int rep = GetReputation(toFaction);
				if (rep > GeneralSettings.DefaultNeutralRep)
					SetReputation(toFaction, rep + GeneralSettings.RepDecay / 2);
				if (rep < GeneralSettings.DefaultNegativeRep)
					SetReputation(toFaction, rep + GeneralSettings.RepDecay);
			}
		}

		public void TriggerWar(long against)
		{
			int rep = GetReputation(against);

			if (rep > GeneralSettings.DefaultWarRep - GeneralSettings.AdditionalWarRepPenalty)
				SetReputation(against, GeneralSettings.DefaultWarRep);
			else
				SetReputation(against, rep - GeneralSettings.AdditionalWarRepPenalty);

			IsDialogRequired(rep, against);	// Dialog may be required.  However, dialog is not given if TriggerWar pushes rep further into the war zone
		}

		private void IsDialogRequired(int oldRep, long against)
		{
			int newRep = GetReputation(against);
			if (oldRep > GeneralSettings.DefaultNeutralRep && newRep < GeneralSettings.DefaultNeutralRep)
			{	// Was neutral, is hostile
				DialogRequest(DialogType.WarDeclared, MyAPIGateway.Session.Factions.Factions[against].Tag);
				return;
			}

			if (oldRep < GeneralSettings.DefaultNeutralRep && newRep > GeneralSettings.DefaultNeutralRep)
			{	// Was hostile, is neutral
				DialogRequest(DialogType.PeaceAccepted, MyAPIGateway.Session.Factions.Factions[against].Tag);
				return;
			}
		}

		public void DialogRequest(DialogType dialog, string sender)
		{
			Func<string> message = Dialogue.RequestDialog(sender, dialog);
			if (message != null)
				SendMessageToMyFaction(message.Invoke(), sender);
		}

		private void SendMessageToMyFaction(string message, string sender)
		{
			foreach (IMyPlayer player in Players)
			{
				if (!player.IsBot && FromFaction.Members.ContainsKey(player.IdentityId))
					Messaging.SendMessageToPlayer($"{message}", sender, player.IdentityId, MyFontEnum.DarkBlue);
			}
			_players.Clear();
		}

		public List<FactionRelationSave> GetSaveState()
		{
			List<FactionRelationSave> save = new List<FactionRelationSave>();
			foreach (long toFaction in ToFactions)
			{
				save.Add(new FactionRelationSave(FromFaction.FactionId, toFaction, GetReputation(toFaction)));
			}
			return save;
		}

		public override string ToString()
		{
			return $"FromFactionTag: {FromFaction.Tag} | FromFactionId: {FromFaction.FactionId} | RelationCounts: {ToFactions.Count}";
		}

		public string ToStringExtended()
		{
			StringBuilder returnString = new StringBuilder();
			foreach (long toFaction in ToFactions)
			{
				returnString.Append($"FromFactionTag: {FromFaction.Tag} | FromFactionId: {FromFaction.FactionId} | FromFactionTag: {FromFaction.Tag} | ToFactionTag: {toFaction} | Reputation: {GetReputation(toFaction)}");
			}
			return returnString.ToString();
		}
	}
}