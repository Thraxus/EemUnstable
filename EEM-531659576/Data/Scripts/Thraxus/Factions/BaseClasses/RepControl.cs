using System;
using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Eem.Thraxus.Common.Utilities.Tools.Networking;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Models;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.BaseClasses
{
	public abstract class RepControl
	{
		public long FromRelationId;
		protected string RelationTag;

		public bool IsPirate { get; protected set; }

		public readonly HashSet<long> ToFactions = new HashSet<long>();
		
		// TODO: Must change the ToFactions list to include rep; probably need a custom class

		protected readonly List<IMyPlayer> Players = new List<IMyPlayer>();

		protected List<IMyPlayer> GetPlayers()
		{
			Players.Clear();
			MyAPIGateway.Players.GetPlayers(Players);
			return Players;
		}
		
		public void AddNewRelation(long id, int? rep = null)
		{
			if (!MyAPIGateway.Session.Factions.Factions.ContainsKey(id)) return;
			ToFactions.Add(id);
			if (rep != null)
				SetReputation(id, (int)rep);
			StaticLog.WriteToLog("AddNewRelation", $"Type: {RelationTag} - Id: {id} - Rep: {rep.ToString()}", LogType.General);
		}

		public void RemoveRelation(long id)
		{
			if (RelationExists(id))
				ToFactions.Remove(id);
		}

		public bool RelationExists(long id)
		{
			return ToFactions.Contains(id);
		}

		public int RelationCount()
		{
			return ToFactions.Count;
		}

		public abstract int GetReputation(long id);

		protected abstract void SetReputation(long id, int rep);

		public void SetAsPirate()
		{
			IsPirate = true;
			foreach (long toFaction in ToFactions)
				SetReputation(toFaction, GeneralSettings.DefaultNegativeRep);
		}

		public abstract void ResetReputation();

		public void NoLongerPirate()
		{
			IsPirate = false;
		}

		public void DecayReputation()
		{
			foreach (long toFaction in ToFactions)
			{
				int rep = GetReputation(toFaction);
				//if (rep == GeneralSettings.DefaultNeutralRep || rep == GeneralSettings.DefaultNegativeRep)
				//{
				//	StaticLog.WriteToLog("DecayReputation", $"Type: {RelationTag} - Rep Decayed not required between {FromRelationId} and {toFaction} - Currently: {GetReputation(toFaction)}", LogType.General);
				//	continue;
				//}
				if (rep > GeneralSettings.DefaultNeutralRep)
					SetReputation(toFaction, rep - GeneralSettings.RepDecay / 2);
				//if (rep < GeneralSettings.DefaultNeutralRep && rep > GeneralSettings.DefaultNegativeRep)
				if (rep < GeneralSettings.DefaultNeutralRep)
					SetReputation(toFaction, rep + GeneralSettings.RepDecay);
				StaticLog.WriteToLog("DecayReputation", $"Type: {RelationTag} - Rep Decayed for {FromRelationId} against {toFaction} - Original: {rep} | New: {GetReputation(toFaction)}", LogType.General);
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

		protected virtual void SendMessage(string message, string sender)
		{
			Messaging.SendMessageToPlayer($"{message}", sender, FromRelationId, MyFontEnum.DarkBlue);
		}

		public RelationSave GetSaveState()
		{
			HashSet<Relation> relations = new HashSet<Relation>();
			foreach (long toFaction in ToFactions)
			{
				relations.Add(new Relation(toFaction, GetReputation(toFaction)));
			}
			return new RelationSave(FromRelationId, relations);
		}

		public override string ToString()
		{
			return $"FromId: {FromRelationId} | RelationCounts: {ToFactions.Count}";
		}

		public string ToStringExtended()
		{
			StringBuilder returnString = new StringBuilder();
			foreach (long toFaction in ToFactions)
			{
				returnString.Append($"FromId: {FromRelationId} | ToFactionTag: {toFaction} | Reputation: {GetReputation(toFaction)}\n");
			}
			return returnString.ToString();
		}
	}
}
