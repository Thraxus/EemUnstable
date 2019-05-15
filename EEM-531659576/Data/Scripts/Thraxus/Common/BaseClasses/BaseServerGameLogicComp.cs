using Eem.Thraxus.Common.Interfaces;
using Eem.Thraxus.Common.SessionComps;
using VRage.Game.Components;

namespace Eem.Thraxus.Common.BaseClasses
{
	internal abstract class BaseServerGameLogicComp : MyGameLogicComponent, ILogEntry
	{
		protected string EntityName = "PlaceholderName";
		protected long EntityId = 0L;
		
		public void WriteToLog(string caller, string message, LogType logType)
		{
			switch (logType)
			{
				case LogType.General:
					GeneralLog(caller, message);
					return;
				case LogType.Debug:
					DebugLog(caller, message);
					return;
				case LogType.Exception:
					ExceptionLog(caller, message);
					return;
				default:
					return;
			}
		}
		
		private void GeneralLog(string caller, string message)
		{
			StaticLogger.WriteToLog($"{EntityName} ({EntityId}): {caller}", message, LogType.General);
		}

		private void DebugLog(string caller, string message)
		{
			StaticLogger.WriteToLog($"{EntityName} ({EntityId}): {caller}", message, LogType.Debug);
		}

		private void ExceptionLog(string caller, string message)
		{
			StaticLogger.WriteToLog($"{EntityName} ({EntityId}): {caller}", $"Exception! {message}", LogType.Exception);
		}
	}
}
