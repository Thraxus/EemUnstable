namespace Eem.Thraxus.Common.DataTypes
{
	public class EemPrefabConfig
	{
		public readonly string PrefabType;
		public readonly string Preset;
		public readonly string CallForHelpProbability;
		public readonly string SeekDistance;
		public readonly string Faction;
		public readonly string FleeOnlyWhenDamaged;
		public readonly string FleeTriggerDistance;
		public readonly string FleeSpeedCap;
		public readonly string AmbushMode;
		public readonly string DelayedAi;
		public readonly string ActivationDistance;
		public readonly string PlayerPriority;
		public readonly string MultiBot;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Prefab Config:\t{Faction}\t{PrefabType}\t{Preset}\t{CallForHelpProbability}\t{DelayedAi}\t{SeekDistance}\t{AmbushMode}\t{ActivationDistance}\t{FleeOnlyWhenDamaged}\t{FleeTriggerDistance}\t{FleeSpeedCap}\t{PlayerPriority}\t{MultiBot}";
		}

		public string ToStringVerbose()
		{
			return $"Prefab Config - |Faction: {Faction} |Type: {PrefabType} |Preset: {Preset} |CallForHelpProbability: {CallForHelpProbability} |DelayedAi: {DelayedAi} |SeekDistance: {SeekDistance} |AmbushMode: {AmbushMode} |ActivationDistance: {ActivationDistance} |FleeOnlyWhenDamaged: {FleeOnlyWhenDamaged} |FleeTriggerDistance: {FleeTriggerDistance} |FleeSpeedCap: {FleeSpeedCap} |PlayerPriority: {PlayerPriority} |MultiBot: {MultiBot}";
		}

		public EemPrefabConfig(string config)
		{
			foreach (string cfg in config.Trim().Replace("\r\n", "\n").Split('\n'))
			{
				if (cfg == null) continue;
				string[] x = cfg.Trim().Replace("\r\n", "\n").Split(':');
				if (x.Length < 2) continue;
				switch (x[0].ToLower())
				{
					case "type":
						PrefabType = x[1];
						break;
					case "preset":
						Preset = x[1];
						break;
					case "callforhelprobability":
						CallForHelpProbability = x[1];
						break;
					case "seekdistance":
						SeekDistance = x[1];
						break;
					case "faction":
						Faction = x[1];
						break;
					case "fleeonlywhendamaged":
						FleeOnlyWhenDamaged = x[1];
						break;
					case "fleetriggerdistance":
						FleeTriggerDistance = x[1];
						break;
					case "fleespeedcap":
						FleeSpeedCap = x[1];
						break;
					case "ambushmode":
						AmbushMode = x[1];
						break;
					case "delayedai":
						DelayedAi = x[1];
						break;
					case "activationdistance":
						ActivationDistance = x[1];
						break;
					case "playerpriority":
						PlayerPriority = x[1];
						break;
					case "multibot":
						MultiBot = x[1];
						break;
					default: break;
				}
			}

			if (string.IsNullOrEmpty(Faction)) Faction = "SPRT";
		}
	}
}