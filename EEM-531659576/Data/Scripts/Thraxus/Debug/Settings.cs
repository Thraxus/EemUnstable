using System;
using System.Collections.Generic;
using Eem.Thraxus.Utilities;
using Sandbox.Definitions;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.ObjectBuilders;

namespace Eem.Thraxus.Debug
{
	internal static class Settings
	{
		public static void Run()
		{
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

			MyDefinitionBase myDefinition = new MyGlobalEventDefinition()
			{
				FirstActivationTime = TimeSpan.FromMilliseconds(60000),
				MinActivationTime = TimeSpan.FromMilliseconds(60000),
				MaxActivationTime = TimeSpan.FromMilliseconds(60000),
				Id = new MyDefinitionId(MyObjectBuilderType.Parse("MyObjectBuilder_GlobalEventBase"), "SpawnCargoShip")
			};

			MyDefinitionManager.Static.Definitions.AddOrRelaceDefinition(myDefinition);
		}
	}
}
