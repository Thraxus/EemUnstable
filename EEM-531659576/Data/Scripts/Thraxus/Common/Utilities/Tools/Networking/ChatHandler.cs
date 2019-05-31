using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Common.Utilities.Tools.Networking
{
	class ChatHandler
	{
		internal const string ChatCommandPrefix = Settings.Settings.ChatCommandPrefix;
		private const string HelpPrefix = "help";

		private static readonly Dictionary<string, Action<string>> ChatAction = new Dictionary<string, Action<string>>
		{
			{HelpPrefix, PrintHelpCommands}
		};

		public static void HandleChatMessage(string message)
		{

			IMyPlayer localPlayer = MyAPIGateway.Session.Player;

			if (localPlayer.PromoteLevel < MyPromoteLevel.Admin)
			{
				Messaging.ShowLocalNotification($"You must be an Administrator to invoke Chat Commands.  Current Rank: {localPlayer.PromoteLevel.ToString()}");
				return;
			}

			string[] chatCommand = message.Split(' ');

			if (chatCommand.Length < 2)
			{
				PrintHelpCommands("");
				return;
			}

			Action<string> action;
			string actionText = null;

			if (chatCommand.Length > 2)
				actionText = chatCommand[2];

			if (ChatAction.TryGetValue(chatCommand[1], out action))
				action?.Invoke(actionText);
			else PrintHelpCommands("");
		}

		/// <summary>
		/// Prints a list of available commands
		/// </summary>  
		private static void PrintHelpCommands(string s)
		{
			Messaging.ShowLocalNotification($"'{ChatCommandPrefix} {HelpPrefix}' will show this message");
			Messaging.ShowLocalNotification($"'SomeOtherPrefix This is an exmaple of a second message");
		}
	}
}
