using Eem.Thraxus.Common.DataTypes;

namespace Eem.Thraxus.Common.BaseClasses
{
	public abstract class LogBaseStatic
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
