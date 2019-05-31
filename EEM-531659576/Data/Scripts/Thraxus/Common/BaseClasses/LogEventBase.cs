using Eem.Thraxus.Common.DataTypes;

namespace Eem.Thraxus.Common.BaseClasses
{
	public class LogEventBase
	{
		public event TriggerLog OnWriteToLog;
		public delegate void TriggerLog(string caller, string message, LogType logType);
		
		public void WriteToLog(string caller, string message, LogType logType)
		{
			OnWriteToLog?.Invoke(caller, message, logType);
		}
	}
}
