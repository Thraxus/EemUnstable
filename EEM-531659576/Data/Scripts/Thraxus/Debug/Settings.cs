using System;
using System.Collections.Generic;
using Eem.Thraxus.Utilities;
using Sandbox.Definitions;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace Eem.Thraxus.Debug
{
	internal static class Settings
	{
		public static void Run()
		{
			//TODO add any debug session settings here that matter. Global Events (all the garbage below) aren't available, so don't bother with them.

			if (MyAPIGateway.Session.SessionSettings.PiratePCU <= 50000)
				MyAPIGateway.Session.SessionSettings.PiratePCU = 100000;

			if (MyAPIGateway.Session.SessionSettings.TotalPCU <= 100000 && MyAPIGateway.Session.SessionSettings.TotalPCU != 0)
				MyAPIGateway.Session.SessionSettings.TotalPCU = 200000;

			//List<MyObjectBuilder_GlobalEventBase> spawnEvents = new List<MyObjectBuilder_GlobalEventBase>()
			//{
			//	new MyObjectBuilder_GlobalEventBase
			//	{
			//		EventType = MyGlobalEventTypeEnum.SpawnCargoShip, 
			//	}
			//};
			//MyGlobalEventDefinition myGlobalEventDefinition = new MyGlobalEventDefinition
			//{
			//	FirstActivationTime = TimeSpan.FromMilliseconds(60000),
			//	MinActivationTime = TimeSpan.FromMilliseconds(60000),
			//	MaxActivationTime = TimeSpan.FromMilliseconds(60000),
			//};

			//MyObjectBuilder_GlobalEventDefinition myObjectBuilderGlobalEventDefinition = new MyObjectBuilder_GlobalEventDefinition
			//{
			//	SubtypeName = "SpawnCargoShip",
			//	FirstActivationTimeMs = 60000,
			//	MinActivationTimeMs = 60000,
			//	MaxActivationTimeMs = 60000
			//};

			//Id = new MyDefinitionId(MyObjectBuilderType.Parse("MyObjectBuilder_GlobalEventBase/SpawnCargoShip"))
			//Id = new MyDefinitionId(MyObjectBuilderType.Parse("MyObjectBuilder_GlobalEventBase"), "SpawnCargoShip")
			//Id = MyDefinitionId.Parse("MyObjectBuilder_GlobalEventBase/SpawnCargoShip")

			//MyDefinitionId myDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventDefinition), "SpawnCargoShip");
			//AiSessionCore.GeneralLog.WriteToLog("UpdateCargoShipSpawn", $"{myDefinitionId.TypeId}");

			//foreach (KeyValuePair<Type, Dictionary<MyStringHash, MyDefinitionBase>> keyValuePair in MyDefinitionManager.Static.Definitions.Definitions)
			//{
			//	AiSessionCore.GeneralLog.WriteToLog("def",$"{keyValuePair.Key}\t{keyValuePair.Value}\t{keyValuePair.Value.Count}");
			//}

			//MyDefinitionBase myDefinition = new MyGlobalEventDefinition()
			//{
			//	Id = MyDefinitionId.Parse("MyObjectBuilder_GlobalEventBase/SpawnCargoShip"),
			//	FirstActivationTime = TimeSpan.FromMinutes(1),
			//	MinActivationTime = TimeSpan.FromMinutes(1),
			//	MaxActivationTime = TimeSpan.FromMinutes(1)
			//};
			//MyDefinitionManager.Static.Definitions.AddOrRelaceDefinition(myDefinition);

			//AiSessionCore.GeneralLog.WriteToLog("UpdateCargoShipSpawn", $"{myDefinition.Id.TypeId}");
			//AiSessionCore.GeneralLog.WriteToLog("UpdateCargoShipSpawn", $"{(Type) myDefinition.Id.TypeId}");
			//Dictionary<MyStringHash, MyDefinitionBase> testDict;
			//MyDefinitionManager.Static.Definitions.Definitions.TryGetValue(myDefinition.Id.TypeId, out testDict);
			//AiSessionCore.GeneralLog.WriteToLog("UpdateCargoShipSpawn", $"{testDict?.Count}");
			//AiSessionCore.GeneralLog.WriteToLog("UpdateCargoShipSpawn",$"{MyDefinitionManager.Static.Definitions.AddOrRelaceDefinition(myDefinition)}");
			////MyDefinitionManager.Static.Definitions.AddDefinition(myDefinition);
		}
	}
}
