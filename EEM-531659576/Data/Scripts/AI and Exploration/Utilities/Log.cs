using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EemRdx.Helpers;
using EemRdx.Networking;
using Sandbox.Game;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens;
using Sandbox.Game.SessionComponents.Clipboard;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Definitions.SessionComponents;
using VRage.Game.ModAPI;
using VRage.Library.Utils;
using VRage.Utils;
using VRageRender;

namespace EemRdx.Utilities
{
	public class Log
	{
		private string LogName { get; set; }
		
		private TextWriter TextWriter { get; set; }

		private static string TimeStamp => DateTime.Now.ToString("MMddyy-HH:mm:ss:ffff");

		private readonly Queue<string> _messageQueue = new Queue<string>(20);

		private const int DefaultIndent = 4;

		private static string Indent { get; } = new string(' ', DefaultIndent);

		public Log(string logName)
		{
			LogName = logName + ".log";
			Init();
		}

		private void Init()
		{
			if (TextWriter != null) return;
			TextWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(LogName, typeof(Log));
		}

		public void Close()
		{
			TextWriter?.Flush();
			TextWriter?.Close();
			TextWriter = null;
		}

		public void WriteToLog(string caller, string message, bool showOnHud = false, int duration = Constants.DefaultLocalMessageDisplayTime, string color = MyFontEnum.Green)
		{
			BuildLogLine(caller, message);
			if (!showOnHud) return;
			BuildHudNotification(caller, message, duration, color);
		}

		public void GetTailMessages()
		{
			//TextReader fileInLocalStorage = MyAPIGateway.Utilities.ReadFileInLocalStorage(LogName, typeof(Logger));
			MyAPIGateway.Utilities.ShowMissionScreen(LogName, "", "", string.Join($"{Environment.NewLine}{Environment.NewLine}", _messageQueue.GetQueue()));
			//IMyHudObjectiveLine debugLines = new MyHudObjectiveLine
			//{
			//	Title = LogName,
			//	Objectives = MessageQueue.GetQueue().ToList()
			//};
			//debugLines.Show();
			//MyAPIGateway.Utilities.GetObjectiveLine();
		}

		private static void BuildHudNotification(string caller, string message, int duration, string color)
		{
			Messaging.ShowLocalNotification($"{caller}{Indent}{message}", duration, color);
		}

		private void BuildLogLine(string caller, string message)
		{
			WriteLine($"{TimeStamp}{Indent}{caller}{Indent}{message}");
		}
		
		private void WriteLine(string line)
		{
			_messageQueue.Enqueue(line);
		 	TextWriter.WriteLine(line);
			TextWriter.Flush();
		}
	}
}
