using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Models;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.BaseClasses
{
	public abstract class RepControl
	{
		public readonly HashSet<long> ToFactions = new HashSet<long>();

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
		
		public abstract void AddNewMember(long newId);

		public void AddNewRelation(long id, int? rep = null)
		{
			if (!MyAPIGateway.Session.Factions.Factions.ContainsKey(id)) return;
			ToFactions.Add(id);
			if (rep != null)
				SetReputation(id, (int)rep);
		}

		public bool RelationExists(long id)
		{
			return ToFactions.Contains(id);
		}

		public abstract int GetReputation(long id);

		protected abstract void SetReputation(long id, int rep);

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

			IsDialogRequired(rep, against); // Dialog may be required.  However, dialog is not given if TriggerWar pushes rep further into the war zone
		}

		private void IsDialogRequired(int oldRep, long against)
		{
			int newRep = GetReputation(against);
			if (oldRep > GeneralSettings.DefaultNeutralRep && newRep < GeneralSettings.DefaultNeutralRep)
			{   // Was neutral, is hostile
				DialogRequest(DialogType.WarDeclared, MyAPIGateway.Session.Factions.Factions[against].Tag);
				return;
			}

			if (oldRep < GeneralSettings.DefaultNeutralRep && newRep > GeneralSettings.DefaultNeutralRep)
			{   // Was hostile, is neutral
				DialogRequest(DialogType.PeaceAccepted, MyAPIGateway.Session.Factions.Factions[against].Tag);
				return;
			}
		}

		private void DialogRequest(DialogType dialog, string sender)
		{
			Func<string> message = Dialogue.RequestDialog(sender, dialog);
			if (message != null)
				SendMessage(message.Invoke(), sender);
		}

		protected abstract void SendMessage(string message, string sender);

		public abstract List<T> GetSaveState<T>();

		public abstract override string ToString();

		public abstract string ToStringExtended();
	}
}
