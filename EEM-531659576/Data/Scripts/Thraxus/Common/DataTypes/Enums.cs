namespace Eem.Thraxus.Common.DataTypes
{
	public enum LogType
	{
		Debug, 
		Exception, 
		General, 
		Profiling
	}

	public class BaseTargetPriorities
	{	// yeah, not a true enum, but it's serving the same purpose with some flexibility
		public const int Bars = 0;
	}

	public enum AlertSetting
	{
		Peacetime, 
		Wartime
	}

	public enum FactionRelationship
	{
		Friends, 
		Enemies
	}


	public enum GridOwnerType
	{
		Player,
		Npc,
		None
	}

	public enum GridType
	{
		Debris,
		Projection,
		Ship,
		Station
	}

	public enum GridSize
	{
		Large,
		Small
	}
}
