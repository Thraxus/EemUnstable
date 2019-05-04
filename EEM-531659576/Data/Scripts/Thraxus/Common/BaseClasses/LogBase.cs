namespace Eem.Thraxus.Common.BaseClasses
{
	public class LogBase
	{
		public event TriggerLog WriteToLog;
		public delegate void TriggerLog(string caller, string message, LogType logType);

		protected void OnWriteToLog(string caller, string message, LogType logType)
		{
			WriteToLog?.Invoke(caller, message, logType);
		}
	}
}
