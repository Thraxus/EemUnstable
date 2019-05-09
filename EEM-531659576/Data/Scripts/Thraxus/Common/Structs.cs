using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
