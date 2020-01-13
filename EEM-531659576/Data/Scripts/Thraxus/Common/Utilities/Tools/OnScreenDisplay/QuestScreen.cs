using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Utilities.Tools.OnScreenDisplay
{
	public class QuestScreen
	{
		private readonly string _questName;

		private readonly Dictionary<string, int> _questLog = new Dictionary<string, int>();

		private readonly long _sendTo = MyAPIGateway.Session.Player.IdentityId;

		public QuestScreen(string questName)
		{
			_questName = questName;
			MyVisualScriptLogicProvider.SetQuestlog(true, questName, _sendTo);
		}

		public void NewQuest(string quest)
		{
			_questLog.Add(quest, MyVisualScriptLogicProvider.AddQuestlogDetail(quest, false, false, _sendTo));
		}

		public void UpdateQuest(string oldQuest, string newQuest)
		{
			int old;
			if (!_questLog.TryGetValue(oldQuest, out old)) return;
			_questLog.Remove(oldQuest);
			_questLog.Add(newQuest, old);
			MyVisualScriptLogicProvider.ReplaceQuestlogDetail(old, newQuest, false, _sendTo);
		}

		public void Close()
		{
			MyVisualScriptLogicProvider.RemoveQuestlogDetails(_sendTo);
		}
	}
}
