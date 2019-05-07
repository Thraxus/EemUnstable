using Eem.Thraxus.Common.Interfaces;

namespace Eem.Thraxus.Common.BaseClasses
{
	public abstract class LogBase : ILogEntry
	{
		public event TriggerLog WriteToStaticLog;
		public delegate void TriggerLog(string caller, string message, LogType logType);

		/// <inheritdoc />
		public void WriteToLog(string caller, string message, LogType logType)
		{
			WriteToStaticLog?.Invoke(caller, message, logType);
		}
	}
}
