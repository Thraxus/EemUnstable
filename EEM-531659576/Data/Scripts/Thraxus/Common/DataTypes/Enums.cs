namespace Eem.Thraxus.Common.DataTypes
{
	public enum LogType
	{
		Debug, Exception, General, Profiling
	}

	public class BaseTargetPriorities
	{	// yeah, not a true enum, but it's serving the same purpose with some flexibility
		public const int Bars = 0;
	}

	public enum AlertSetting
	{
		Peacetime, Wartime
	}
}
