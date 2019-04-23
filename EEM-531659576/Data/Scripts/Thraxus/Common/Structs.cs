namespace Eem.Thraxus.Common
{
	public struct BotOrphan
	{
		public readonly long MyParentId;
		public readonly long MyGrandParentId;

		public BotOrphan(long myParentId, long myGrandParentId)
		{
			MyParentId = myParentId;
			MyGrandParentId = myGrandParentId;
		}
	}

	public struct EemPrefabConfig
	{
		private string _prefabType;
		private string _preset;
		private string _callForHelpProbability;
		private string _seekDistance;
		private string _faction;
		private string _fleeOnlyWhenDamaged;
		private string _fleeTriggerDistance;
		private string _fleeSpeedCap;
		private string _ambushMode;
		private string _delayedAi;
		private string _activationDistance;
		private string _playerPriority;

		private void ParseConfigEntry(string config)
		{
			foreach (string cfg in config.Trim().Replace("\r\n", "\n").Split('\n'))
			{
				if (cfg == null) continue;
				string[] x = cfg.Trim().Replace("\r\n", "\n").Split(':');
				if (x.Length < 2) continue;
				switch (x[0].ToLower())
				{
					case "type":
						_prefabType = x[1];
						break;
					case "preset":
						_preset = x[1];
						break;
					case "callforhelprobability":
						_callForHelpProbability = x[1];
						break;
					case "seekdistance":
						_seekDistance = x[1];
						break;
					case "faction":
						_faction = x[1];
						break;
					case "fleeonlywhendamaged":
						_fleeOnlyWhenDamaged = x[1];
						break;
					case "fleetriggerdistance":
						_fleeTriggerDistance = x[1];
						break;
					case "fleespeedcap":
						_fleeSpeedCap = x[1];
						break;
					case "ambushmode":
						_ambushMode = x[1];
						break;
					case "delayedai":
						_delayedAi = x[1];
						break;
					case "activationdistance":
						_activationDistance = x[1];
						break;
					case "playerpriority":
						_playerPriority = x[1];
						break;
					default: break;
				}
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{_faction}\t{_prefabType}\t{_preset}\t{_callForHelpProbability}\t{_delayedAi}\t{_seekDistance}\t{_ambushMode}\t{_activationDistance}\t{_fleeOnlyWhenDamaged}\t{_fleeTriggerDistance}\t{_fleeSpeedCap}\t{_playerPriority}";
		}

		public EemPrefabConfig(string config) : this()
		{
			ParseConfigEntry(config);
		}
	}
}
