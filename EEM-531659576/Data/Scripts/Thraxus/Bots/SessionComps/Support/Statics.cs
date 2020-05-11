using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.DataTypes;
using Sandbox.Common.ObjectBuilders;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace Eem.Thraxus.Bots.SessionComps.Support
{
	public static class Statics
	{
		// Cargo
		private static readonly BlockValue CargoContainer = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue ShipConnector = new BlockValue { Value = 20, Threat = 10 };

		// Communications
		private static readonly BlockValue Beacon = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue LaserAntenna = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue RadioAntenna = new BlockValue { Value = 20, Threat = 10 };

		// Controllers
		private static readonly BlockValue Cockpit = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue RemoteControl = new BlockValue { Value = 20, Threat = 10 };
		
		// Manufacturing
		private static readonly BlockValue Assembler = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue Refinery = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SurvivalKit = new BlockValue { Value = 20, Threat = 10 };

		// Medical
		private static readonly BlockValue CryoChamber = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue MedicalRoom = new BlockValue { Value = 20, Threat = 10 };

		// Power Producers or Providers
		private static readonly BlockValue BatteryBlock = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue HydrogenEngine = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue Reactor = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SolarPanel = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue WindTurbine = new BlockValue { Value = 20, Threat = 10 };

		// Utility
		private static readonly BlockValue GasTank = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue JumpDrive = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue MyProgrammableBlock = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue OxygenGenerator = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue OxygenTank = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue Thrust = new BlockValue { Value = 20, Threat = 10 };

		// Weapons
		private static readonly BlockValue InteriorTurret = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue LargeGatlingTurret = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue LargeMissileTurret = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SmallGatlingGun = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SmallMissileLauncher = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SmallMissileLauncherReload = new BlockValue { Value = 20, Threat = 10 };

		private static readonly Dictionary<MyObjectBuilderType, BlockValue> BlockValues = new Dictionary<MyObjectBuilderType, BlockValue>()
		{
			// Cargo
			{ typeof(MyObjectBuilder_CargoContainer), CargoContainer },
			{ typeof(MyObjectBuilder_ShipConnector), ShipConnector },

			// Communications
			{ typeof(MyObjectBuilder_Beacon), Beacon },
			{ typeof(MyObjectBuilder_LaserAntenna), LaserAntenna },
			{ typeof(MyObjectBuilder_RadioAntenna), RadioAntenna },
		
			// Controllers
			{ typeof(MyObjectBuilder_Cockpit), Cockpit },
			{ typeof(MyObjectBuilder_RemoteControl), RemoteControl},

			// Manufacturing
			{ typeof(MyObjectBuilder_Assembler), Assembler },
			{ typeof(MyObjectBuilder_Refinery), Refinery },
			{ typeof(MyObjectBuilder_SurvivalKit), SurvivalKit },
			
			// Medical 
			{ typeof(MyObjectBuilder_CryoChamber), CryoChamber },
			{ typeof(MyObjectBuilder_MedicalRoom), MedicalRoom },

			// Power Producers or providers
			{ typeof(MyObjectBuilder_BatteryBlock), BatteryBlock },
			{ typeof(MyObjectBuilder_HydrogenEngine), HydrogenEngine },
			{ typeof(MyObjectBuilder_Reactor), Reactor },
			{ typeof(MyObjectBuilder_SolarPanel), SolarPanel },
			{ typeof(MyObjectBuilder_WindTurbine), WindTurbine },

			// Utility
			{ typeof(MyObjectBuilder_GasTank), GasTank },
			{ typeof(MyObjectBuilder_JumpDrive), JumpDrive },
			{ typeof(MyObjectBuilder_MyProgrammableBlock), MyProgrammableBlock },
			{ typeof(MyObjectBuilder_OxygenGenerator), OxygenGenerator },
			{ typeof(MyObjectBuilder_OxygenTank), OxygenTank },
			{ typeof(MyObjectBuilder_Thrust), Thrust },

			// Weapons
			{ typeof(MyObjectBuilder_InteriorTurret), InteriorTurret },
			{ typeof(MyObjectBuilder_LargeGatlingTurret), LargeGatlingTurret },
			{ typeof(MyObjectBuilder_LargeMissileTurret), LargeMissileTurret },
			{ typeof(MyObjectBuilder_SmallGatlingGun), SmallGatlingGun },
			{ typeof(MyObjectBuilder_SmallMissileLauncher), SmallMissileLauncher },
			{ typeof(MyObjectBuilder_SmallMissileLauncherReload), SmallMissileLauncherReload },
		};

		private static HashSet<MyStringHash> DefenseShieldSubtypes = new HashSet<MyStringHash>
		{ // Base Type: MyObjectBuilder_UpgradeModule
			MyStringHash.GetOrCompute("EmitterST"),
			MyStringHash.GetOrCompute("EmitterL"),
			MyStringHash.GetOrCompute("EmitterS"),
			MyStringHash.GetOrCompute("EmitterLA"),
			MyStringHash.GetOrCompute("EmitterSA")
		};

		private static HashSet<MyStringHash> EnergyShieldSubtypes = new HashSet<MyStringHash>
		{ // Base Type: MyObjectBuilder_RefineryDefinition
			MyStringHash.GetOrCompute("LargeShipSmallShieldGeneratorBase"),
			MyStringHash.GetOrCompute("LargeShipLargeShieldGeneratorBase"),
			MyStringHash.GetOrCompute("SmallShipSmallShieldGeneratorBase"),
			MyStringHash.GetOrCompute("SmallShipMicroShieldGeneratorBase"),
			MyStringHash.GetOrCompute("EmitterSA")
		};

		public static BlockValue GetBlockValue(Type type)
		{
			BlockValue returnValue;
			return BlockValues.TryGetValue(type, out returnValue)
				? returnValue
				: new BlockValue() { Value = 1, Threat = 1 }; // return is a slim block or some other unknown or inconsequential block
		}
	}
}