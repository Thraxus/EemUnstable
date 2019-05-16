using Eem.Thraxus.Common.Interfaces;

namespace Eem.Thraxus.Common.BaseClasses
{
	public class LogEventBase : ILogEntry
	{
		public event TriggerLog OnWriteToLog;
		public delegate void TriggerLog(string caller, string message, LogType logType);
		
		public void WriteToLog(string caller, string message, LogType logType)
		{
			OnWriteToLog?.Invoke(caller, message, logType);
		}
	}
}
