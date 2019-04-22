using System;
using System.Collections.Generic;
using Eem.Thraxus.Utilities;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ObjectBuilders.ComponentSystem;

namespace Eem.Thraxus.Debug
{
	internal static class InformationExporter
	{
		private static Log _shipLog;
		private static Log _spawnGroupLog;

		public static void Run()
		{
			PrintAllShipInfo();
			PrintAllSpawnGroupInfo();
		}
		
		private static void PrintAllSpawnGroupInfo()
		{
			MyAPIGateway.Parallel.Start(delegate
			{
				_spawnGroupLog = new Log("SpawnGroup");
				foreach (MySpawnGroupDefinition spawnGroupDefinition in MyDefinitionManager.Static.GetSpawnGroupDefinitions())
				{
					if (!spawnGroupDefinition.Public) continue;
					string spawnGroupInfo = $"{spawnGroupDefinition.IsCargoShip}\t{spawnGroupDefinition.IsEncounter}\t{spawnGroupDefinition.IsPirate}\t{spawnGroupDefinition.Prefabs.Count}\t{spawnGroupDefinition.Frequency}\t{spawnGroupDefinition.SpawnRadius}\t{spawnGroupDefinition.Context.IsBaseGame}\t{spawnGroupDefinition.Context.ModId}";
					foreach (MySpawnGroupDefinition.SpawnGroupPrefab prefab in spawnGroupDefinition.Prefabs)
					{
						_spawnGroupLog.WriteToLog($"SpawnGroupLog", $"{spawnGroupDefinition.Id.SubtypeName}\t{spawnGroupInfo}\t{prefab.SubtypeId}\t{prefab.BeaconText}\t{prefab.Behaviour}\t{prefab.BehaviourActivationDistance}");
					}
				}
				_spawnGroupLog?.WriteToLog("SpawnGroupLog", "Complete");
				_spawnGroupLog?.Close();
			});
		}

		private static void PrintAllShipInfo()
		{
			MyAPIGateway.Parallel.Start(delegate
			{
				List<string> eemRcSubtypes = new List<string>()
				{
					"EEMPilotedFighterCockpit", "EEMPilotedFlightSeat", "EEMPilotedLargeBlockCockpitSeat", "EEMPilotedSmallBlockCockpit", "EEMPilotedPassengerSeat", "LargeBlockRemoteControl", "SmallBlockRemoteControl"
				};
				_shipLog = new Log("Ship");
				foreach (KeyValuePair<string, MyPrefabDefinition> prefabDefinition in MyDefinitionManager.Static.GetPrefabDefinitions())
				{
					if (!prefabDefinition.Value.Public) continue;
					List<EemPrefabConfig> eemPrefabConfigs = new List<EemPrefabConfig>();
					foreach (MyObjectBuilder_CubeGrid cubeGrid in prefabDefinition.Value.CubeGrids)
					{
						if (cubeGrid?.CubeBlocks == null) continue;
						foreach (MyObjectBuilder_CubeBlock cubeBlock in cubeGrid.CubeBlocks)
						{
							if (cubeBlock == null) continue;
							if (cubeBlock.TypeId != typeof(MyObjectBuilder_RemoteControl)) continue;
							if (cubeBlock.SubtypeName == null) continue;
							if (!eemRcSubtypes.Contains(cubeBlock.SubtypeName)) continue;
							if (cubeBlock.ComponentContainer?.Components == null) continue;
							foreach (MyObjectBuilder_ComponentContainer.ComponentData componentContainer in cubeBlock.ComponentContainer.Components)
							{
								if (componentContainer?.TypeId == null) continue;
								if (componentContainer.Component == null) continue;
								if (componentContainer.Component.TypeId != typeof(MyObjectBuilder_ModStorageComponent)) continue;
								if (((MyObjectBuilder_ModStorageComponent)componentContainer.Component).Storage.Dictionary == null) continue;
								foreach (KeyValuePair<Guid, string> componentKeyValuePair in ((MyObjectBuilder_ModStorageComponent)componentContainer.Component).Storage.Dictionary)
								{
									if (string.IsNullOrEmpty(componentKeyValuePair.Value)) continue;
									eemPrefabConfigs.Add(new EemPrefabConfig(componentKeyValuePair.Value));
								}
							}
						}
					}
					if (eemPrefabConfigs.Count > 0)
					{
						foreach (EemPrefabConfig eemPrefabConfig in eemPrefabConfigs)
						{
							_shipLog.WriteToLog($"ShipLog", $"{prefabDefinition.Key}\t{prefabDefinition.Value.Context.IsBaseGame}\t{prefabDefinition.Value.Context.ModId}\t{prefabDefinition.Value.CubeGrids[0].IsStatic}\t{prefabDefinition.Value.CubeGrids[0].IsRespawnGrid}\t{prefabDefinition.Value.CubeGrids[0].IsUnsupportedStation}\t{prefabDefinition.Value.CubeGrids[0].IsPowered}\t{prefabDefinition.Value.CubeGrids[0].GridSizeEnum}\t{eemPrefabConfig}");
						}
					}
					else _shipLog.WriteToLog($"ShipLog", $"{prefabDefinition.Key}\t{prefabDefinition.Value.Context.IsBaseGame}\t{prefabDefinition.Value.Context.ModId}\t{prefabDefinition.Value.CubeGrids[0].IsStatic}\t{prefabDefinition.Value.CubeGrids[0].IsRespawnGrid}\t{prefabDefinition.Value.CubeGrids[0].IsUnsupportedStation}\t{prefabDefinition.Value.CubeGrids[0].IsPowered}\t{prefabDefinition.Value.CubeGrids[0].GridSizeEnum}");
				}
				_shipLog?.WriteToLog("ShipLog", "Complete");
				_shipLog?.Close();
			});
		}

		private struct EemPrefabConfig
		{
			private string PrefabType;
			private string Preset;
			private string CallForHelpProbability;
			private string SeekDistance;
			private string Faction;
			private string FleeOnlyWhenDamaged;
			private string FleeTriggerDistance;
			private string FleeSpeedCap;
			private string AmbushMode;
			private string DelayedAi;
			private string ActivationDistance;
			private string PlayerPriority;

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
						default: break;
					}
				}
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Faction}\t{PrefabType}\t{Preset}\t{CallForHelpProbability}\t{DelayedAi}\t{SeekDistance}\t{AmbushMode}\t{ActivationDistance}\t{FleeOnlyWhenDamaged}\t{FleeTriggerDistance}\t{FleeSpeedCap}\t{PlayerPriority}";
			}

			public EemPrefabConfig(string config) : this()
			{
				ParseConfigEntry(config);
			}
		}
	}
}
