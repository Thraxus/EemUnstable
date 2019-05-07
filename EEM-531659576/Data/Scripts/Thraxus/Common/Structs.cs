using System;
using System.Collections.Generic;
using VRageMath;

namespace Eem.Thraxus.Common
{
	public struct ShipControllerHistory
	{
		public readonly long Controller;
		public readonly long ControlledEntity;
		public readonly DateTime TimeStamp;

		public ShipControllerHistory(long controller, long controlledEntity, DateTime timeStamp)
		{
			Controller = controller;
			ControlledEntity = controlledEntity;
			TimeStamp = timeStamp;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Controller: {Controller} - ControllerEntity: {ControlledEntity} - TimeStamp: {TimeStamp}";
		}
	}

	public struct MissileHistory
	{
		public readonly long OwnerId;
		public readonly long LauncherId;
		public readonly Vector3D Location;
		public readonly DateTime TimeStamp;

		public MissileHistory(long launcherId, long ownerId, Vector3D location, DateTime timeStamp)
		{
			LauncherId = launcherId;
			OwnerId = ownerId;
			Location = location;
			TimeStamp = timeStamp;
		}
	}

	public struct BotOrphan
	{
		public readonly long MyParentId;
		public readonly List<long> MyAncestors;
		public readonly EemPrefabConfig MyLegacyConfig;

		public BotOrphan(long myParentId, List<long> myAncestors, EemPrefabConfig myLegacyConfig)
		{
			MyParentId = myParentId;
			MyAncestors = myAncestors;
			MyLegacyConfig = myLegacyConfig;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Parent: {MyParentId} - MyAncestors: {MyAncestors?.ToArray()} - MyConfig: {MyLegacyConfig}";
		}
	}

	public struct EemPrefabConfig
	{
		public string PrefabType;
		public string Preset;
		public string CallForHelpProbability;
		public string SeekDistance;
		public string Faction;
		public string FleeOnlyWhenDamaged;
		public string FleeTriggerDistance;
		public string FleeSpeedCap;
		public string AmbushMode;
		public string DelayedAi;
		public string ActivationDistance;
		public string PlayerPriority;
		public string MultiBot;

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

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Prefab Config:\t{Faction}\t{PrefabType}\t{Preset}\t{CallForHelpProbability}\t{DelayedAi}\t{SeekDistance}\t{AmbushMode}\t{ActivationDistance}\t{FleeOnlyWhenDamaged}\t{FleeTriggerDistance}\t{FleeSpeedCap}\t{PlayerPriority}\t{MultiBot}";
		}

		public EemPrefabConfig(string config) : this()
		{
			ParseConfigEntry(config);
		}
	}
}
