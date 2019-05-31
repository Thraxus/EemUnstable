using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Utilities;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ObjectBuilders.ComponentSystem;

namespace Eem.Thraxus.Debug
{
	internal static class InformationExporter
	{



		public static void Run()
		{
			PrintAllShipInfo();
			PrintAllSpawnGroupInfo();
			PrintAllDefinitions();
		}

		private static void PrintAllDefinitions()
		{
			MyAPIGateway.Parallel.Start(delegate
			{
				Log allDefinitions = new Log("All_Definitions");
				DictionaryValuesReader<MyDefinitionId, MyDefinitionBase> dictionaryValuesReader = MyDefinitionManager.Static.GetAllDefinitions();
				foreach (MyDefinitionBase definition in dictionaryValuesReader)
				{
					allDefinitions.WriteToLog("Settings", $"{definition.Id}\t{definition.Id.SubtypeName}\t{definition.Id.SubtypeId}\t{definition.Id.TypeId}\t{definition.Context.ModName}\t{definition.Public}");
				}

				allDefinitions.Close();
			});
		}

		private static void PrintAllSpawnGroupInfo()
		{
			MyAPIGateway.Parallel.Start(delegate
			{
				Log spawnGroupLog = new Log("SpawnGroup");
				foreach (MySpawnGroupDefinition spawnGroupDefinition in MyDefinitionManager.Static.GetSpawnGroupDefinitions())
				{
					if (!spawnGroupDefinition.Public) continue;
					string spawnGroupInfo = $"{spawnGroupDefinition.IsCargoShip}\t{spawnGroupDefinition.IsEncounter}\t{spawnGroupDefinition.IsPirate}\t{spawnGroupDefinition.Prefabs.Count}\t{spawnGroupDefinition.Frequency}\t{spawnGroupDefinition.SpawnRadius}\t{spawnGroupDefinition.Context.IsBaseGame}\t{spawnGroupDefinition.Context.ModId}";
					foreach (MySpawnGroupDefinition.SpawnGroupPrefab prefab in spawnGroupDefinition.Prefabs)
					{
						spawnGroupLog.WriteToLog($"SpawnGroupLog", $"{spawnGroupDefinition.Id.SubtypeName}\t{spawnGroupInfo}\t{prefab.SubtypeId}\t{prefab.BeaconText}\t{prefab.Behaviour}\t{prefab.BehaviourActivationDistance}");
					}
				}
				spawnGroupLog?.WriteToLog("SpawnGroupLog", "Complete");
				spawnGroupLog?.Close();
			});
		}

		private static void PrintAllShipInfo()
		{
			MyAPIGateway.Parallel.Start(delegate
			{
				Log shipLog = new Log("Ship");
				List<string> eemRcSubtypes = new List<string>()
				{
					"EEMPilotedFighterCockpit", "EEMPilotedFlightSeat", "EEMPilotedLargeBlockCockpitSeat", "EEMPilotedSmallBlockCockpit", "EEMPilotedPassengerSeat", "LargeBlockRemoteControl", "SmallBlockRemoteControl"
				};
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
							shipLog.WriteToLog($"ShipLog", $"{prefabDefinition.Key}\t{prefabDefinition.Value.Context.IsBaseGame}\t{prefabDefinition.Value.Context.ModId}\t{prefabDefinition.Value.CubeGrids[0].IsStatic}\t{prefabDefinition.Value.CubeGrids[0].IsRespawnGrid}\t{prefabDefinition.Value.CubeGrids[0].IsUnsupportedStation}\t{prefabDefinition.Value.CubeGrids[0].IsPowered}\t{prefabDefinition.Value.CubeGrids[0].GridSizeEnum}\t{eemPrefabConfig}");
						}
					}
					else shipLog.WriteToLog($"ShipLog", $"{prefabDefinition.Key}\t{prefabDefinition.Value.Context.IsBaseGame}\t{prefabDefinition.Value.Context.ModId}\t{prefabDefinition.Value.CubeGrids[0].IsStatic}\t{prefabDefinition.Value.CubeGrids[0].IsRespawnGrid}\t{prefabDefinition.Value.CubeGrids[0].IsUnsupportedStation}\t{prefabDefinition.Value.CubeGrids[0].IsPowered}\t{prefabDefinition.Value.CubeGrids[0].GridSizeEnum}");
				}
				shipLog?.WriteToLog("ShipLog", "Complete");
				shipLog?.Close();
			});
		}


	}
}
