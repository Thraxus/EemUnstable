using System;
using System.Collections.Generic;
using Eem.Thraxus.Factions.Messages;
using Eem.Thraxus.Factions.Utilities.Messages;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.Models
{
	internal class Dialogue
	{
		public Dialogue()
		{
			_factionFirstPeaceAcceptedDialog = new Dictionary<string, Func<string>>();
			_factionPeaceAcceptedDialog = new Dictionary<string, Func<string>>();
			_factionPeaceConsideredDialog = new Dictionary<string, Func<string>>();
			_factionPeaceProposedDialog = new Dictionary<string, Func<string>>();
			_factionPeaceRejectedDialog = new Dictionary<string, Func<string>>();
			_factionWarDeclaredDialog = new Dictionary<string, Func<string>>();
			_factionWarReceivedDialog = new Dictionary<string, Func<string>>();
			InitMessageDictionaries();
		}

		public void Unload()
		{
			_factionFirstPeaceAcceptedDialog.Clear();
			_factionPeaceAcceptedDialog.Clear();
			_factionPeaceConsideredDialog.Clear();
			_factionPeaceProposedDialog.Clear();
			_factionPeaceRejectedDialog.Clear();
			_factionWarDeclaredDialog.Clear();
			_factionWarReceivedDialog.Clear();
		}

		internal enum DialogType
		{
			CollectiveDisappointment, CollectiveReprieve, CollectiveWelcome, FirstPeaceAccepted, PeaceAccepted, PeaceConsidered, PeaceProposed, PeaceRejected, WarDeclared, WarReceived
		}

		private Dictionary<string, Func<string>> _factionFirstPeaceAcceptedDialog;
		private Dictionary<string, Func<string>> _factionPeaceAcceptedDialog;
		private Dictionary<string, Func<string>> _factionPeaceConsideredDialog;
		private Dictionary<string, Func<string>> _factionPeaceProposedDialog;
		private Dictionary<string, Func<string>> _factionPeaceRejectedDialog;
		private Dictionary<string, Func<string>> _factionWarDeclaredDialog;
		private Dictionary<string, Func<string>> _factionWarReceivedDialog;

		private void InitMessageDictionaries()
		{
			_factionFirstPeaceAcceptedDialog = new Dictionary<string, Func<string>>
			{
				{Amph.Tag, Amph.FirstPeaceAccepted}, {Civl.Tag, Civl.FirstPeaceAccepted}, {Exmc.Tag, Exmc.FirstPeaceAccepted},
				{Hs.Tag, Hs.FirstPeaceAccepted}, {Imdc.Tag, Imdc.FirstPeaceAccepted}, {Istg.Tag, Istg.FirstPeaceAccepted},
				{Kuss.Tag, Kuss.FirstPeaceAccepted}, {Mai.Tag, Mai.FirstPeaceAccepted}, {Mmec.Tag, Mmec.FirstPeaceAccepted},
				{Sepd.Tag, Sepd.FirstPeaceAccepted}, {Sprt.Tag, Sprt.FirstPeaceAccepted}, {Ucmf.Tag, Ucmf.FirstPeaceAccepted}
			};
			_factionPeaceAcceptedDialog = new Dictionary<string, Func<string>>
			{
				{Amph.Tag, Amph.PeaceAccepted}, {Civl.Tag, Civl.PeaceAccepted}, {Exmc.Tag, Exmc.PeaceAccepted},
				{Hs.Tag, Hs.PeaceAccepted}, {Imdc.Tag, Imdc.PeaceAccepted}, {Istg.Tag, Istg.PeaceAccepted},
				{Kuss.Tag, Kuss.PeaceAccepted}, {Mai.Tag, Mai.PeaceAccepted}, {Mmec.Tag, Mmec.PeaceAccepted},
				{Sepd.Tag, Sepd.PeaceAccepted}, {Sprt.Tag, Sprt.PeaceAccepted}, {Ucmf.Tag, Ucmf.PeaceAccepted}
			};
			_factionPeaceConsideredDialog = new Dictionary<string, Func<string>>
			{
				{Amph.Tag, Amph.PeaceConsidered}, {Civl.Tag, Civl.PeaceConsidered}, {Exmc.Tag, Exmc.PeaceConsidered},
				{Hs.Tag, Hs.PeaceConsidered}, {Imdc.Tag, Imdc.PeaceConsidered}, {Istg.Tag, Istg.PeaceConsidered},
				{Kuss.Tag, Kuss.PeaceConsidered}, {Mai.Tag, Mai.PeaceConsidered}, {Mmec.Tag, Mmec.PeaceConsidered},
				{Sepd.Tag, Sepd.PeaceConsidered}, {Sprt.Tag, Sprt.PeaceConsidered}, {Ucmf.Tag, Ucmf.PeaceConsidered}
			};
			_factionPeaceProposedDialog = new Dictionary<string, Func<string>>
			{
				{Amph.Tag, Amph.PeaceProposed}, {Civl.Tag, Civl.PeaceProposed}, {Exmc.Tag, Exmc.PeaceProposed},
				{Hs.Tag, Hs.PeaceProposed}, {Imdc.Tag, Imdc.PeaceProposed}, {Istg.Tag, Istg.PeaceProposed},
				{Kuss.Tag, Kuss.PeaceProposed}, {Mai.Tag, Mai.PeaceProposed}, {Mmec.Tag, Mmec.PeaceProposed},
				{Sepd.Tag, Sepd.PeaceProposed}, {Sprt.Tag, Sprt.PeaceProposed}, {Ucmf.Tag, Ucmf.PeaceProposed}
			};
			_factionPeaceRejectedDialog = new Dictionary<string, Func<string>>
			{
				{Amph.Tag, Amph.PeaceRejected}, {Civl.Tag, Civl.PeaceRejected}, {Exmc.Tag, Exmc.PeaceRejected},
				{Hs.Tag, Hs.PeaceRejected}, {Imdc.Tag, Imdc.PeaceRejected}, {Istg.Tag, Istg.PeaceRejected},
				{Kuss.Tag, Kuss.PeaceRejected}, {Mai.Tag, Mai.PeaceRejected}, {Mmec.Tag, Mmec.PeaceRejected},
				{Sepd.Tag, Sepd.PeaceRejected}, {Sprt.Tag, Sprt.PeaceRejected}, {Ucmf.Tag, Ucmf.PeaceRejected}
			};
			_factionWarDeclaredDialog = new Dictionary<string, Func<string>>
			{
				{Amph.Tag, Amph.WarDeclared}, {Civl.Tag, Civl.WarDeclared}, {Exmc.Tag, Exmc.WarDeclared}, {Hs.Tag, Hs.WarDeclared},
				{Imdc.Tag, Imdc.WarDeclared}, {Istg.Tag, Istg.WarDeclared}, {Kuss.Tag, Kuss.WarDeclared}, {Mai.Tag, Mai.WarDeclared},
				{Mmec.Tag, Mmec.WarDeclared}, {Sepd.Tag, Sepd.WarDeclared}, {Sprt.Tag, Sprt.WarDeclared}, {Ucmf.Tag, Ucmf.WarDeclared}
			};
			_factionWarReceivedDialog = new Dictionary<string, Func<string>>
			{
				{Amph.Tag, Amph.WarReceived}, {Civl.Tag, Civl.WarReceived}, {Exmc.Tag, Exmc.WarReceived}, {Hs.Tag, Hs.WarReceived},
				{Imdc.Tag, Imdc.WarReceived}, {Istg.Tag, Istg.WarReceived}, {Kuss.Tag, Kuss.WarReceived}, {Mai.Tag, Mai.WarReceived},
				{Mmec.Tag, Mmec.WarReceived}, {Sepd.Tag, Sepd.WarReceived}, {Sprt.Tag, Sprt.WarReceived}, {Ucmf.Tag, Ucmf.WarReceived}
			};
		}

		internal Func<string> RequestDialog(IMyFaction npcFaction, DialogType type)
		{
			AiSessionCore.DebugLog?.WriteToLog("RequestDialog", $"npcFaction:\t{npcFaction?.Tag}\tDialogType:\t{type}");
			Func<string> message = DefaultDialogs.CatchAll;
			Func<string> tmpMessage;
			switch (type)
			{
				case DialogType.CollectiveDisappointment:
					message = DefaultDialogs.CollectiveDisappointment;
					break;
				case DialogType.CollectiveReprieve:
					message = DefaultDialogs.CollectiveReprieve;
					break;
				case DialogType.CollectiveWelcome:
					message = DefaultDialogs.CollectiveWelcome;
					break;
				case DialogType.FirstPeaceAccepted:
					if (npcFaction != null && _factionFirstPeaceAcceptedDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.PeaceAccepted:
					if (npcFaction != null && _factionPeaceAcceptedDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.PeaceConsidered:
					if (npcFaction != null && _factionPeaceConsideredDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.PeaceProposed:
					if (npcFaction != null && _factionPeaceProposedDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.PeaceRejected:
					if (npcFaction != null && _factionPeaceRejectedDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.WarDeclared:
					if (npcFaction != null && _factionWarDeclaredDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.WarReceived:
					if (npcFaction != null && _factionWarReceivedDialog.TryGetValue(npcFaction.Tag, out tmpMessage))
						message = tmpMessage;
					break;
				// ReSharper disable once RedundantEmptySwitchSection
				default:
					break;
			}
			return message;
		}
	}
}
