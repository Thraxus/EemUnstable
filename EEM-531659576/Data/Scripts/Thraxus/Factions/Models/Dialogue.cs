using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Utilities.Messages;

namespace Eem.Thraxus.Factions.Models
{
	internal static class Dialogue
	{
		private static readonly Dictionary<string, Func<string>> FactionFirstPeaceAcceptedDialog =
			new Dictionary<string, Func<string>>()
			{
				{Amph.Tag, Amph.FirstPeaceAccepted}, 
				{Civl.Tag, Civl.FirstPeaceAccepted},
				{Exmc.Tag, Exmc.FirstPeaceAccepted},
				{Hs.Tag, Hs.FirstPeaceAccepted}, 
				{Imdc.Tag, Imdc.FirstPeaceAccepted},
				{Istg.Tag, Istg.FirstPeaceAccepted},
				{Kuss.Tag, Kuss.FirstPeaceAccepted}, 
				{Mai.Tag, Mai.FirstPeaceAccepted},
				{Mmec.Tag, Mmec.FirstPeaceAccepted},
				{Sepd.Tag, Sepd.FirstPeaceAccepted}, 
				{Sprt.Tag, Sprt.FirstPeaceAccepted},
				{Ucmf.Tag, Ucmf.FirstPeaceAccepted}
			};

		private static readonly Dictionary<string, Func<string>> FactionPeaceAcceptedDialog =
			new Dictionary<string, Func<string>>()
			{
				{Amph.Tag, Amph.PeaceAccepted}, 
				{Civl.Tag, Civl.PeaceAccepted}, 
				{Exmc.Tag, Exmc.PeaceAccepted},
				{Hs.Tag, Hs.PeaceAccepted},
				{Imdc.Tag, Imdc.PeaceAccepted}, 
				{Istg.Tag, Istg.PeaceAccepted},
				{Kuss.Tag, Kuss.PeaceAccepted}, 
				{Mai.Tag, Mai.PeaceAccepted},
				{Mmec.Tag, Mmec.PeaceAccepted},
				{Sepd.Tag, Sepd.PeaceAccepted}, 
				{Sprt.Tag, Sprt.PeaceAccepted}, 
				{Ucmf.Tag, Ucmf.PeaceAccepted}
			};

		private static readonly Dictionary<string, Func<string>> FactionPeaceConsideredDialog =
			new Dictionary<string, Func<string>>()
			{
				{Amph.Tag, Amph.PeaceConsidered}, 
				{Civl.Tag, Civl.PeaceConsidered},
				{Exmc.Tag, Exmc.PeaceConsidered},
				{Hs.Tag, Hs.PeaceConsidered}, 
				{Imdc.Tag, Imdc.PeaceConsidered}, 
				{Istg.Tag, Istg.PeaceConsidered},
				{Kuss.Tag, Kuss.PeaceConsidered},
				{Mai.Tag, Mai.PeaceConsidered},
				{Mmec.Tag, Mmec.PeaceConsidered},
				{Sepd.Tag, Sepd.PeaceConsidered},
				{Sprt.Tag, Sprt.PeaceConsidered}, 
				{Ucmf.Tag, Ucmf.PeaceConsidered}
			};

		private static readonly Dictionary<string, Func<string>> FactionPeaceProposedDialog =
			new Dictionary<string, Func<string>>()
			{
				{Amph.Tag, Amph.PeaceProposed}, 
				{Civl.Tag, Civl.PeaceProposed}, 
				{Exmc.Tag, Exmc.PeaceProposed},
				{Hs.Tag, Hs.PeaceProposed}, 
				{Imdc.Tag, Imdc.PeaceProposed}, 
				{Istg.Tag, Istg.PeaceProposed},
				{Kuss.Tag, Kuss.PeaceProposed}, 
				{Mai.Tag, Mai.PeaceProposed}, 
				{Mmec.Tag, Mmec.PeaceProposed},
				{Sepd.Tag, Sepd.PeaceProposed}, 
				{Sprt.Tag, Sprt.PeaceProposed}, 
				{Ucmf.Tag, Ucmf.PeaceProposed}
			};
		
		private static readonly Dictionary<string, Func<string>> FactionPeaceRejectedDialog =
			new Dictionary<string, Func<string>>()
			{
				{Amph.Tag, Amph.PeaceRejected}, 
				{Civl.Tag, Civl.PeaceRejected}, 
				{Exmc.Tag, Exmc.PeaceRejected},
				{Hs.Tag, Hs.PeaceRejected},
				{Imdc.Tag, Imdc.PeaceRejected}, 
				{Istg.Tag, Istg.PeaceRejected},
				{Kuss.Tag, Kuss.PeaceRejected}, 
				{Mai.Tag, Mai.PeaceRejected},
				{Mmec.Tag, Mmec.PeaceRejected},
				{Sepd.Tag, Sepd.PeaceRejected}, 
				{Sprt.Tag, Sprt.PeaceRejected},
				{Ucmf.Tag, Ucmf.PeaceRejected}
			};
		
		private static readonly Dictionary<string, Func<string>> FactionWarDeclaredDialog =
			new Dictionary<string, Func<string>>()
			{
				{Amph.Tag, Amph.WarDeclared}, 
				{Civl.Tag, Civl.WarDeclared}, 
				{Exmc.Tag, Exmc.WarDeclared}, 
				{Hs.Tag, Hs.WarDeclared},
				{Imdc.Tag, Imdc.WarDeclared}, 
				{Istg.Tag, Istg.WarDeclared}, 
				{Kuss.Tag, Kuss.WarDeclared}, 
				{Mai.Tag, Mai.WarDeclared},
				{Mmec.Tag, Mmec.WarDeclared}, 
				{Sepd.Tag, Sepd.WarDeclared}, 
				{Sprt.Tag, Sprt.WarDeclared}, 
				{Ucmf.Tag, Ucmf.WarDeclared}
			};
		
		private static readonly Dictionary<string, Func<string>> FactionWarReceivedDialog =
			new Dictionary<string, Func<string>>()
			{
				{Amph.Tag, Amph.WarReceived}, 
				{Civl.Tag, Civl.WarReceived}, 
				{Exmc.Tag, Exmc.WarReceived}, 
				{Hs.Tag, Hs.WarReceived},
				{Imdc.Tag, Imdc.WarReceived}, 
				{Istg.Tag, Istg.WarReceived}, 
				{Kuss.Tag, Kuss.WarReceived}, 
				{Mai.Tag, Mai.WarReceived},
				{Mmec.Tag, Mmec.WarReceived}, 
				{Sepd.Tag, Sepd.WarReceived}, 
				{Sprt.Tag, Sprt.WarReceived}, 
				{Ucmf.Tag, Ucmf.WarReceived}
			};
		
		internal static Func<string> RequestDialog(string tag, DialogType type)
		{
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
					if (FactionFirstPeaceAcceptedDialog.TryGetValue(tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.PeaceAccepted:
					if (FactionPeaceAcceptedDialog.TryGetValue(tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.PeaceConsidered:
					if (FactionPeaceConsideredDialog.TryGetValue(tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.PeaceProposed:
					if (FactionPeaceProposedDialog.TryGetValue(tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.PeaceRejected:
					if (FactionPeaceRejectedDialog.TryGetValue(tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.WarDeclared:
					if (FactionWarDeclaredDialog.TryGetValue(tag, out tmpMessage))
						message = tmpMessage;
					break;
				case DialogType.WarReceived:
					if (FactionWarReceivedDialog.TryGetValue(tag, out tmpMessage))
						message = tmpMessage;
					break;
				default:
					StaticLog.WriteToLog("RequestDialog", $"Dialog Type not found!", LogType.Exception);
					break;
			}
			return message;
		}
	}
}
