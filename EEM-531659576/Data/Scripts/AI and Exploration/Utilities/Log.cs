using System;
using System.IO;
using EemRdx.Helpers;
using EemRdx.Networking;
using Sandbox.ModAPI;
using VRage.Game;

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
			MyAPIGateway.Utilities.ShowMissionScreen(LogName, "", "", string.Join($"{Environment.NewLine}{Environment.NewLine}", _messageQueue.GetQueue()));
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
