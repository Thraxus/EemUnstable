using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Utilities.Tools.OnScreenDisplay
{
	/// <summary>
	/// This is designed for local player only; intended for debug purposes.
	/// </summary>
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

		public void NewQuest(QuestLogDetail quest)
		{
			_questLog.Add(quest.NewQuest.ToString(), MyVisualScriptLogicProvider.AddQuestlogDetail(quest.NewQuest.ToString(), false, false, _sendTo));
		}

		public void UpdateQuest(QuestLogDetail quest)
		{
			int old;
			if (!_questLog.TryGetValue(quest.OldQuest.ToString(), out old)) return;
			_questLog.Remove(quest.OldQuest.ToString());
			_questLog.Add(quest.NewQuest.ToString(), old);
			MyAPIGateway.Utilities.InvokeOnGameThread(() =>
				MyVisualScriptLogicProvider.ReplaceQuestlogDetail(old, quest.NewQuest.ToString(), false, _sendTo)
			);
		}

		public void Close()
		{
			MyVisualScriptLogicProvider.RemoveQuestlogDetails(_sendTo);
		}
	}
}
