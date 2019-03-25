using System.Collections.Generic;

namespace Eem.Thraxus.Factions.Settings
{
	public static class Constants
	{
		/// <summary>
		/// These factions are considered lawful. When they go hostile towards someone,
		/// they also make the police (SEPD) and army (UCMF) go hostile.
		/// </summary>
		public static List<string> LawfulFactionsTags { get; } = new List<string>
		{
			"UCMF", "SEPD", "CIVL", "ISTG", "MA-I", "EXMC", "KUSS", "HS", "AMPH", "IMDC" };

		/// <summary>
		/// 
		/// </summary>
		public static List<string> AllNpcFactions { get; } = new List<string>
		{
			"SPRT", "CIVL", "UCMF", "SEPD", "ISTG", "AMPH", "KUSS", "HS", "MMEC", "MA-I", "EXMC", "IMDC"
		};

		/// <summary>
		/// 
		/// </summary>
		public static List<string> EnforcementFactionsTags { get; } = new List<string>
		{
			"SEPD", "UCMF"
		};

		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<string> PlayerFactionExclusionList { get; } = new List<string>
		{
			"Pirate", "Rogue", "Outlaw", "Bandit"
		};

		public const string DebugLogName = "Factions_Debug";
		public const string GeneralLogName = "Factions_General";
		public const string ProfilingLogName = "Factions_Profiling";
		public const ushort FactionsNetworkingId = 35467;
	}
}
