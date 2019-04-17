using System;
using System.Collections.Generic;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Networking;
using Eem.Thraxus.Utilities;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.ModAPI;
using VRage.Utils;

namespace Eem.Thraxus
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class AiSessionCore : MySessionComponentBase
	{
		private static readonly Dictionary<long, BotBase.OnDamageTaken> DamageHandlers = new Dictionary<long, BotBase.OnDamageTaken>();

		public static void AddDamageHandler(long gridId, BotBase.OnDamageTaken handler)
		{
			DamageHandlers.Add(gridId, handler);
		}

		public static void AddDamageHandler(IMyCubeGrid grid, BotBase.OnDamageTaken handler)
		{
			AddDamageHandler(grid.GetTopMostParent().EntityId, handler);
		}

		public static void RemoveDamageHandler(long gridId)
		{
			DamageHandlers.Remove(gridId);
		}

		public static void RemoveDamageHandler(IMyCubeGrid grid)
		{
			RemoveDamageHandler(grid.GetTopMostParent().EntityId);
		}

		public static bool HasDamageHandler(long gridId)
		{
			return DamageHandlers.ContainsKey(gridId);
		}

		public static bool HasDamageHandler(IMyCubeGrid grid)
		{
			return HasDamageHandler(grid.GetTopMostParent().EntityId);
		}

		public static bool LogSetupComplete;
		public static Log ProfilingLog;
		public static Log DebugLog;
		public static Log GeneralLog;
		private Log _shipLog;
		private Log _spawnGroupLog;


		private bool _initialized;
		private bool _debugInitialized;

		public override void UpdateBeforeSimulation()
		{
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			if (!Constants.IsServer) return;
			if (Constants.DebugMode && !_debugInitialized) DebugInit();
			if (!_initialized) Initialize();


			if (!Constants.DebugMode) return;
			PrintAllSpawnGroupInfo();
			PrintAllShipInfo();
		}

		private void PrintAllSpawnGroupInfo()
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
				_spawnGroupLog.WriteToLog("SpawnGroupLog", "Complete");
			});
		}

		private void PrintAllShipInfo()
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
				_shipLog.WriteToLog("ShipLog", "Complete");
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

		private void DebugInit()
		{
			DebugLog = new Log(Constants.DebugLogName);
			_debugInitialized = true;
		}

		private void Initialize()
		{
			if (Constants.DebugMode) DebugLog.WriteToLog("Initialize", $"Debug Active - IsServer: {Constants.IsServer}", true, 20000);
			//if (Constants.IsServer) Factions.Factions.Initialize();
			Messaging.Register();
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, DamageRefHandler);
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, GenericDamageHandler);
			MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, GenericDamageHandler);
			MyAPIGateway.Entities.OnEntityAdd += delegate (IMyEntity entity)
			{
				GeneralLog.WriteToLog("Core", $"Entity Added\t{entity.EntityId}\t{entity.DisplayName}");
			};
			MyAPIGateway.Entities.OnEntityRemove += delegate(IMyEntity entity)
			{
				GeneralLog.WriteToLog("Core", $"Entity Removed\t{entity.EntityId}\t{entity.DisplayName}");
			};
			_initialized = true;
		}

		private static void InitLogs()
		{
			if (Constants.EnableProfilingLog) ProfilingLog = new Log(Constants.ProfilingLogName);
			if (Constants.EnableGeneralLog) GeneralLog = new Log(Constants.GeneralLogName);
			LogSetupComplete = true;
			GeneralLog.WriteToLog("Core", $"Cargo: {MyAPIGateway.Session.SessionSettings.CargoShipsEnabled}");
			GeneralLog.WriteToLog("Core", $"Encounters: {MyAPIGateway.Session.SessionSettings.EnableEncounters}");
			GeneralLog.WriteToLog("Core", $"Drones: {MyAPIGateway.Session.SessionSettings.EnableDrones}");
			GeneralLog.WriteToLog("Core", $"Scripts: {MyAPIGateway.Session.SessionSettings.EnableIngameScripts}");
			GeneralLog.WriteToLog("Core", $"Sync: {MyAPIGateway.Session.SessionSettings.SyncDistance}");
			GeneralLog.WriteToLog("Core", $"PiratePCU: {MyAPIGateway.Session.SessionSettings.PiratePCU}");
			GeneralLog.WriteToLog("Core", $"TotalPCU: {MyAPIGateway.Session.SessionSettings.TotalPCU}");
		}

		private static void CloseLogs()
		{
			if (Constants.DebugMode) DebugLog?.Close();
			if (Constants.EnableProfilingLog) ProfilingLog?.Close();
			if (Constants.EnableGeneralLog) GeneralLog?.Close();
		}

		/// <summary>
		/// Initial setup
		/// </summary>
		/// <param name="sessionComponent"></param>
		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{

		}

		public override void BeforeStart()
		{
			InitLogs();
		}

		/// <summary>
		/// Unloads the handlers
		/// </summary>
		protected override void UnloadData()
		{
			base.UnloadData();
			Messaging.Unregister();
			CloseLogs();
		}

		public void DamageRefHandler(object damagedObject, ref MyDamageInformation damage)
		{
			GenericDamageHandler(damagedObject, damage);
		}

		public void GenericDamageHandler(object damagedObject, MyDamageInformation damage)
		{
			try
			{
				if (damage.AttackerId == 0 || !(damagedObject is IMySlimBlock)) return;
				IMySlimBlock damagedBlock = (IMySlimBlock)damagedObject;
				IMyCubeGrid damagedGrid = damagedBlock.CubeGrid;
				long gridId = damagedGrid.GetTopMostParent().EntityId;
				if (!DamageHandlers.ContainsKey(gridId)) return;
				DamageHandlers[gridId].Invoke(damagedBlock, damage);
			}
			catch (Exception scrap)
			{
				LogError("GenericDamageHandler", scrap);
			}
		}

		public static void LogError(string source, Exception scrap, string debugPrefix = "SessionCore.")
		{
			MyLog.Default.WriteLine($"Core Crash - {scrap.StackTrace}");
			DebugHelper.Print("Core Crash - Please reload the game", $"Fatal error in '{debugPrefix + source}': {scrap.Message}. {(scrap.InnerException != null ? scrap.InnerException.Message : "No additional info was given by the game :(")}");
		}

		public static void DebugWrite(string source, string message, bool antiSpam = true)
		{
			if (Constants.DebugMode) DebugHelper.Print($"{source}", $"{message}", antiSpam);
		}
	}
}