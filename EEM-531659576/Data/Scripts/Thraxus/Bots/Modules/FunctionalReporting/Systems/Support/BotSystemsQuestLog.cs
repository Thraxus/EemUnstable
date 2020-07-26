using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Utilities.Tools.OnScreenDisplay;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Support
{
	internal class BotSystemsQuestLog
	{
		private readonly QuestScreen _questScreen;
		private readonly Dictionary<BotSystemType, QuestLogDetail> _questLogDetails = new Dictionary<BotSystemType, QuestLogDetail>();

		public BotSystemsQuestLog(string name)
		{
			_questScreen = new QuestScreen(name);
		}

		public void UpdateQuest(BotSystemType system, int currentFunctionalIntegrityRatio)
		{
			if (!_questLogDetails.ContainsKey(system)) return;
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {currentFunctionalIntegrityRatio}%");
			_questLogDetails[system].UpdateQuest(newQuest);
			_questScreen.UpdateQuest(_questLogDetails[system]);
		}

		public void NewQuest(BotSystemType system, int integrityRatio)
		{
			if (_questLogDetails.ContainsKey(system)) return;
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {integrityRatio}%");
			_questLogDetails.Add(system, new QuestLogDetail(newQuest));
			_questScreen.NewQuest(_questLogDetails[system]);
		}

		public void Close()
		{
			_questLogDetails.Clear();
			_questScreen.Close();
		}
	}
}
