using System;
using System.Collections.Generic;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Settings
{
	public static class GeneralSettings
	{
		public const string ConfigFileName = "config.xml";
		public const string SaveFileName = "save.eem";
		public const string SandboxVariableName = "EemConfigData";

		#region User Configuration

		public static bool DebugMode = false;
		public static bool ProfilingEnabled = false;


		#endregion


		#region Bot Settings

		public const string TimerAlertOn = "Alert_On";
		public const string TimerAlertOff = "Alert_Off";
		public const string AntennaAlert = "Alert";
		public const string BackupAntennaAlert = "Alert_Backup";

		#endregion


		#region Constant Values

		public const bool ForcedDebugMode = false;

		public const string ChatCommandPrefix = "chatCommand";
		public const string StaticDebugLogName = "StaticLog-Debug";
		public const string ExceptionLogName = "Exception";
		public const string StaticGeneralLogName = "StaticLog-General";
		public const string ProfilingLogName = "Profiler";

		public const ushort NetworkId = 16759;
		
		#endregion


		#region Reference Values

		public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;

		public const int DefaultLocalMessageDisplayTime = 5000;
		public const int DefaultServerMessageDisplayTime = 10000;
		public const int TicksPerMinute = TicksPerSecond * 60;
		public const int TicksPerSecond = 60;

		public static Random Random { get; } = new Random();

		#endregion


		#region Factions

		/// <summary>
		/// Value all hostile relationships start out at
		/// </summary>
		public const int DefaultNegativeRep = -1500;

		/// <summary>
		/// Value all neutral relationships start out at
		/// </summary>
		public const int DefaultNeutralRep = -500;

		/// <summary>
		/// Value all neutral relationships start out at
		/// </summary>
		public const int DefaultWarRep = -550;

		/// <summary>
		/// Value all neutral relationships start out at
		/// </summary>
		public const int AdditionalWarRepPenalty = 20;

		/// <summary>
		/// The amount of rep to change every minute from hostile -> neutral
		/// From above neutral back to neutral should be some fraction of this; perhaps 1/2
		/// If this doesn't work out, then will need to change rep war conditions
		/// Value must be an even number!
		/// </summary>
		public const int RepDecay = 2;

		/// <summary>
		/// Faction War cooldown period
		///		15 minute default cooldown, 2 minute in Debug Mode
		/// </summary>
		public static int FactionNegativeRelationshipCooldown => DebugMode ? (TicksPerMinute * 2) : (TicksPerMinute * 15);
		public const int FactionNegativeRelationshipAssessment = TicksPerSecond;
		public const int FactionMendingRelationshipAssessment = TicksPerMinute + 20;  // Don't really want these assessments firing at the same time

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

		public static List<string> NpcFirstNames { get; } = new List<string>
		{
			"Rosae", "Davith", "Soaph", "Elrin", "Svjetlana", "Zan", "Riya", "Kasdy", "Betrice", "Jaycobe", "Crayg",
			"Emilyse", "Edan", "Brialeagh", "Stanka", "Asan", "Dragoslav", "Vena", "Flyx", "Svetoslav", "Zaid",
			"Timoth", "Katlina", "Kimly", "Jenzen", "Megn", "Juith", "Cayedn", "Jaenelle", "Jayedn", "Alestra", "Madn",
			"Cayelyn", "Rayelyn", "Naethan", "Jaromil", "Laeila", "Aleigha", "Balee", "Kurson", "Kalina", "Allan",
			"Iskren", "Alexi", "Malax", "Baelleigh", "Harp", "Haelee", "Tijan", "Klatko", "Vojta", "Tasya", "Maslinka",
			"Ljupka", "Aubriena", "Danuella", "Jastin", "Idania", "Xandr", "Koba", "Roemary", "Dlilah", "Tanr",
			"Sobeska", "Zaiyah", "Lubka", "Bogomila", "Roderock", "Dayne", "Pribuska", "Kyel", "Svilena", "Laylah",
			"Tray", "Bobbyx", "Kaence", "Rade", "Gojslav", "Tugomir", "Drahomir", "Aldon", "Gyanna", "Jezzy", "Roseya",
			"Zand", "Saria", "Own", "Adriyel", "Ayana", "Spasena", "Vlade", "Kimbr", "Billix", "Landn", "Ylena",
			"Canning", "Slavka", "Gayge", "Dobroslaw", "Jasemine", "Jaden", "Ayna", "Slavomir", "Milaia", "Koale",
			"Elriot", "Ondrea", "Viliana", "Emex", "Ashir", "Yce", "Lyuboslav", "Makenna", "Senka", "Radacek", "Lilea",
			"Wilm", "Burian", "Randis", "Bentom", "Olver", "Charliza", "Vjera", "Caera", "Yasen", "Roselyna", "Venka",
			"Lana", "Nayla", "Ayaan", "Ryliea", "Nicholya", "Adriaenne", "Armanix", "Jazon", "Sulvan", "Roys", "Liyam",
			"Aebby", "Alextra", "Bogomil", "Kole", "Desree", "Zyre", "Haral", "Aerav", "Doriyan", "Rayely", "Helna",
			"Arman", "Zavyr", "Xavis", "Winson", "Arihan", "Adrihan", "Walkr", "Laera", "Victr", "Dobroniega", "Yan",
			"Maianna", "Leshi", "Niklas", "Rebexa", "Renaya", "Jaelyne", "Catlea", "Zdik", "Sereya", "Barba", "Desmon",
			"Arjun", "Boleslava", "Jaxson", "Thalira", "Leslaw", "Aevangelina", "Kade", "Jaro", "Charlise", "Loriya",
			"Ljubica", "Rober", "Iveanne", "Slavena", "Maikle", "Vladica", "Zdiska", "Berivoj", "Shaene", "Brencis",
			"Karina", "Yavor", "Darilan", "Aellana", "Landan", "Adit", "Jazzly", "Ozren", "Nyala", "Azarea", "Sveta",
			"Jaessa", "Aedyn", "Maecey", "Braeylee", "Julyen", "Vela", "Amelise", "Benjam", "Vierka", "Aibram"
		};

		public static List<string> NpcLastNames { get; } = new List<string>
		{
			"Fusepelt", "Andichanteau", "Aubemont", "Kantorovich", "Lomafort", "Borisov", "Wyverneyes", "Abaleilles",
			"Snowreaver", "Litvinchuk", "Vigny", "Vinet", "Milenkovic", "Lamassac", "Masterflower", "Holyblaze",
			"Boberel", "Deathcaller", "Saintimeur", "Châtissac", "Marblemane", "Calic", "Golitsyn", "Aboret",
			"Hardstalker", "Humblevalor", "Sergeyev", "Rameur", "Grassfire", "Forestrock", "Snowsteel", "Chaykovskiy",
			"Smartwoods", "Lightningeyes", "Vassemeur", "Proksch", "Saurriver", "Albignes", "Clarifort", "Pridemaul",
			"Deathhelm", "Vinogradov", "Châtiffet", "Wolinsk", "Limoze", "Chananas", "Hanak", "Popovic", "Noblearm",
			"Belemond", "Runemane", "Chamidras", "Chamigné", "Mildlight", "Kergatillon", "Truedreamer", "Slivkin",
			"Frostbone", "Greatthorne", "Woodtaker", "Nerevilliers", "Abavau", "Stamenkovikj", "Hardlight",
			"Roughsworn", "Nobleroot", "Chaunteau", "Lomages", "Vichanteau", "Laurelet", "Brichagnon", "Shieldsnout",
			"Nozac", "Burningwalker", "Peaceseeker", "Kavka", "Mistseeker", "Sugné", "Sedlak", "Firemore", "Prokesch",
			"Sendula", "Perlich", "Bricharel", "Morningwhisk", "Keenwoods", "Sublirac", "Vilart", "Raunas", "Dewheart",
			"Balaban", "Ravenpike", "Snowcreek", "Sarrarel", "Yellen", "Rochevès", "Croivès", "Chauvetelet", "Polyakov",
			"Mourningroar", "Rambunac", "Woodensworn", "Chabastel", "Fogshaper", "Fistbranch", "Chauthier", "Crerel",
			"Springhand", "Bougaiffet", "Angestel", "Stojanovska", "Bladekeeper", "Heartgloom", "Vajda", "Bloodwound",
			"Mucibaba", "Lhotsky", "Pinekeeper", "Abitillon", "Spiderarm", "Limolot", "Ragnac", "Chaustel", "Croille",
			"Michalek", "Cloudtoe", "Cressier", "Regalshadow", "Cabarkapa", "Snowchewer", "Twerski", "Voronov",
			"Shieldbane", "Gaibannes", "Roquemont", "Gaiffet", "Lamodieu", "Silentwhirl", "Fuseforce", "Farwood",
			"Bouldershade", "Rochedras", "Smolensky", "Bougairelli", "Graysnout", "Korda", "Lonebraid", "Agueleilles",
			"Chanaron", "Chanagnes", "Barassac", "Hnilo", "Popov", "Grayhair", "Younghorn", "Volinsky", "Boberon",
			"Topolski", "Kergassec", "Humblewhisk", "Longbend", "Whitrage", "Pyredrifter", "Wyvernflow", "Vernissier",
			"Dudar", "Chamiveron", "Carlowitz", "Waterbough", "Commonmight", "Raullane", "Boyko", "Wyvernhair",
			"Kovalevsky", "Astateuil", "Bonnetillon", "Dawnleaf", "Laurenteau", "Aguefelon", "Bonnemoux", "Baragre",
			"Kergallane", "Warvale", "Chanaffet", "Polyak", "Kohout", "Wach", "Dolezal", "Doomsprinter", "Malenkov",
			"Woodgazer", "Janowitz", "Golovin", "Milosevic", "Mourningkiller", "Novak", "Barleycrag", "Rabinowicz",
			"Bizelle", "Bohatec", "Rockstrider", "Snowore", "Chauvelet", "Andimtal", "Bonespirit", "Nerelle",
			"Ostrovsky", "Heavystriker", "Cindercutter", "Grasslance", "Baraffet", "Svehla"
		};

		#endregion


		#region Cleanup

		// CleanUp.cs's constants:
		public static bool DisableCleanup { get; } = false;

		// text required in the RC's CustomData to be even considrered for removal
		public static string CleanupRcTag { get; } = "[EEM_AI]";

		// any of these is required to be in RC's CustomData for the grid to be removed.
		public static string[] CleanupRcExtraTags { get; } = { "Type:Fighter", "Type:Freighter" };

		// clamp the world's view range to this minimum value, which is used for removing distant ships
		public static int CleanupMinRange { get; } = 40000;

		// world setting of max drones
		public static int ForceMaxDrones { get; } = 20;

		// BuyShip.cs's constants:

		// the particle effect name (particles.sbc) to spawn after the ship is spawned
		public static string SpawnEffectName { get; } = "EEMWarpIn";

		// the ship's bounding sphere radius is used for the particle effect scale, this value scales that radius.
		public static float SpawnEffectScale { get; } = 0.2f;

		// radius of the area around the spawn zone to check for foreign objects
		public static float SpawnAreaRadius { get; } = 15f;

		// the argument that gets used by the mod to call the PB when the buy ship fails due to the position being blocked
		public static string PbargFailPositionblocked { get; } = "fail-positionblocked";

		// in case the PB doesn't work, notify the players within this radius of the failed spawn ship
		public static float SpawnFailNotifyDistance { get; } = 50;

		#endregion
	}
}
