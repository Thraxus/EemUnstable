using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace EemRdx
{
	public static class Log // v1.4
	{
		public static string ModName = "UNNAMED";
		public static string ModFolder = "UNNAMED";
		public static ulong WorkshopId;
		public const string LogFile = "info.log";

		private static TextWriter _writer;
		private static IMyHudNotification _notify;
		private static readonly StringBuilder Cache = new StringBuilder(64);
		private static readonly List<string> PreInitMessages = new List<string>(0);
		private static int _indent;

		public static void SetUp(string modName, ulong workshopId, string modFolder = null)
		{
			ModName = modName;
			ModFolder = modFolder ?? modName;
			WorkshopId = workshopId;
		}

		public static void Init()
		{
			if(MyAPIGateway.Utilities == null)
			{
				MyLog.Default.WriteLineAndConsole(ModName + " Log.Init() called before API was ready!");
				return;
			}

			if(_writer != null)
				Close();

			_writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(LogFile, typeof(Log));

			if(PreInitMessages.Count > 0)
			{
				foreach(string msg in PreInitMessages)
				{
					Error(msg);
				}

				PreInitMessages.Clear();
			}

			Info("Initialized");

			Cache.Clear();
			Cache.Append("DS=").Append(MyAPIGateway.Utilities.IsDedicated);
			Cache.Append("; defined=");

			Cache.Append("DEBUG, ");

			Info(Cache.ToString());
			Cache.Clear();
		}

		public static void Close()
		{
			if(_writer != null)
			{
				Info("Unloaded.");

				_writer.Flush();
				_writer.Close();
				_writer = null;
			}

			_indent = 0;
			Cache.Clear();
		}

		public static void IncreaseIndent()
		{
			_indent++;
		}

		public static void DecreaseIndent()
		{
			if(_indent > 0)
				_indent--;
		}

		public static void ResetIndent()
		{
			_indent = 0;
		}

		public static void Error(Exception e)
		{
			Error(e.ToString());
		}

		public static void Error(Exception e, string printText)
		{
			Error(e.ToString(), printText);
		}

		public static void Error(string msg)
		{
			Error(msg, ModName + " error - open %AppData%/SpaceEngineers/Storage/" + WorkshopId + "_" + ModFolder + "/" + LogFile + " for details");
		}

		public static void Error(string msg, string printText)
		{
			Info("ERROR: " + msg);

			try
			{
				MyLog.Default.WriteLineAndConsole(ModName + " error/exception: " + msg);

				if (MyAPIGateway.Session == null) return;
				if(_notify == null)
				{
					_notify = MyAPIGateway.Utilities.CreateNotification(printText, 10000, MyFontEnum.Red);
				}
				else
				{
					_notify.Text = printText;
					_notify.ResetAliveTime();
				}

				_notify.Show();
			}
			catch(Exception e)
			{
				Info("ERROR: Could not send notification to local client: " + e);
				MyLog.Default.WriteLineAndConsole(ModName + " error/exception: Could not send notification to local client: " + e);
			}
		}

		public static void Info(string msg)
		{
			Write(msg);
		}

		private static void Write(string msg)
		{
			try
			{
				Cache.Clear();
				Cache.Append(DateTime.Now.ToString("[HH:mm:ss] "));

				if(_writer == null)
					Cache.Append("(PRE-INIT) ");

				for(int i = 0; i < _indent; i++)
				{
					Cache.Append("\t");
				}

				Cache.Append(msg);

				if(_writer == null)
				{
					PreInitMessages.Add(Cache.ToString());
				}
				else
				{
					_writer.WriteLine(Cache);
					_writer.Flush();
				}

				Cache.Clear();
			}
			catch(Exception e)
			{
				MyLog.Default.WriteLineAndConsole(ModName + " had an error while logging message='" + msg + "'\nLogger error: " + e.Message + "\n" + e.StackTrace);
			}
		}
	}
}