﻿using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Common.Utilities.Tools.OnScreenDisplay;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class BotSystemsQuestLog
	{
		private readonly QuestScreen _questScreen;
		private readonly Dictionary<SystemType, QuestLogDetail> _questLogDetails = new Dictionary<SystemType, QuestLogDetail>();

		public BotSystemsQuestLog(string name)
		{
			_questScreen = new QuestScreen(name);
		}

		public void UpdateQuest(SystemType system, int currentFunctionalIntegrityRatio)
		{
			if (!_questLogDetails.ContainsKey(system)) return;
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {currentFunctionalIntegrityRatio}%");
			_questLogDetails[system].UpdateQuest(newQuest);
			_questScreen.UpdateQuest(_questLogDetails[system]);
		}

		public void NewQuest(SystemType system, int integrityRatio)
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
