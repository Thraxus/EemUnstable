namespace Eem.Thraxus.Common.Interfaces
{
	internal interface ILogEntry
	{
		void WriteToLog(string caller, string message, LogType logType);
	}
}
