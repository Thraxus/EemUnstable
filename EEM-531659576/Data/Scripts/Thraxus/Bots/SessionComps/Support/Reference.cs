using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;
using Sandbox.Common.ObjectBuilders;
using ObjectBuilders.SafeZone;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots.SessionComps.Support
{
	public class MasterObjectBuilderRefernece
	{
		public Dictionary<MyStringHash, BlockValue> VanillaSubtypeIds;
		public Dictionary<MyStringHash, BlockValue> ModdedSubtypeIds;
		private BlockValue DefaultValue = new BlockValue { Value = 1, Threat = 1 };
		public MyObjectBuilderType _type;

		public BlockValue GetValue(MyObjectBuilderType type, MyStringHash subtype)
		{
			_type = type;
			BlockValue returnValue;
			if (VanillaSubtypeIds.TryGetValue(subtype, out returnValue))
				return returnValue;
			if (ModdedSubtypeIds.TryGetValue(subtype, out returnValue))
				return returnValue;
			return GetDefaultValue(subtype);
		}

		private BlockValue GetDefaultValue(MyStringHash subtype)
		{
			StaticLog.WriteToLog("MasterObjectBuilderRefernece", $"Value not found in any dictionaries: {_type} | {subtype}", LogType.General);
			return DefaultValue;
		}
	}

	public static class Reference
	{
		public static BlockValue GetBlockValue(IMySlimBlock block)
		{
			MasterObjectBuilderRefernece refernece;
			if (ReferenceValues.TryGetValue(block.BlockDefinition.Id.TypeId, out refernece))
			{
				return refernece.GetValue(block.BlockDefinition.Id.TypeId, block.BlockDefinition.Id.SubtypeId);
			}
			return GetDefaultValue(block.BlockDefinition.Id.TypeId, block.BlockDefinition.Id.SubtypeId);
		}

		private static BlockValue GetDefaultValue(MyObjectBuilderType type, MyStringHash subtype)
		{
			StaticLog.WriteToLog("Reference", $"Value not found in any dictionaries: {type} | {subtype}", LogType.General);
			return defaultValue;
		}

		private static readonly Dictionary<MyObjectBuilderType, MasterObjectBuilderRefernece> ReferenceValues = new Dictionary<MyObjectBuilderType, MasterObjectBuilderRefernece>
		{
			{ typeof(MyObjectBuilder_AirtightHangarDoor), AirtightHangarDoor },
			{ typeof(MyObjectBuilder_AirtightSlideDoor), AirtightSlideDoor },
			{ typeof(MyObjectBuilder_AirVent), AirVent },
			{ typeof(MyObjectBuilder_Assembler), Assembler },
			{ typeof(MyObjectBuilder_BatteryBlock), BatteryBlock },
			{ typeof(MyObjectBuilder_Beacon), Beacon },
			{ typeof(MyObjectBuilder_ButtonPanel), ButtonPanel },
			{ typeof(MyObjectBuilder_CameraBlock), CameraBlock },
			{ typeof(MyObjectBuilder_CargoContainer), CargoContainer },
			{ typeof(MyObjectBuilder_Cockpit), Cockpit },
			{ typeof(MyObjectBuilder_Collector), Collector },
			{ typeof(MyObjectBuilder_ContractBlock), ContractBlock },
			{ typeof(MyObjectBuilder_Conveyor), Conveyor },
			{ typeof(MyObjectBuilder_ConveyorConnector), ConveyorConnector },
			{ typeof(MyObjectBuilder_ConveyorSorter), ConveyorSorter },
			{ typeof(MyObjectBuilder_CryoChamber), CryoChamber },
			{ typeof(MyObjectBuilder_CubeBlock), CubeBlock },
			{ typeof(MyObjectBuilder_Decoy), Decoy },
			{ typeof(MyObjectBuilder_Door), Door },
			{ typeof(MyObjectBuilder_Drill), Drill },
			{ typeof(MyObjectBuilder_GravityGenerator), GravityGenerator },
			{ typeof(MyObjectBuilder_GravityGeneratorSphere), GravityGeneratorSphere },
			{ typeof(MyObjectBuilder_Gyro), Gyro },
			{ typeof(MyObjectBuilder_HydrogenEngine), HydrogenEngine },
			{ typeof(MyObjectBuilder_InteriorLight), InteriorLight },
			{ typeof(MyObjectBuilder_InteriorTurret), InteriorTurret },
			{ typeof(MyObjectBuilder_Jukebox), Jukebox },
			{ typeof(MyObjectBuilder_JumpDrive), JumpDrive },
			{ typeof(MyObjectBuilder_Kitchen), Kitchen },
			{ typeof(MyObjectBuilder_Ladder2), Ladder2 },
			{ typeof(MyObjectBuilder_LandingGear), LandingGear },
			{ typeof(MyObjectBuilder_LargeGatlingTurret), LargeGatlingTurret },
			{ typeof(MyObjectBuilder_LargeMissileTurret), LargeMissileTurret },
			{ typeof(MyObjectBuilder_LaserAntenna), LaserAntenna },
			{ typeof(MyObjectBuilder_LCDPanelsBlock), LCDPanelsBlock },
			{ typeof(MyObjectBuilder_MedicalRoom), MedicalRoom },
			{ typeof(MyObjectBuilder_MergeBlock), MergeBlock },
			{ typeof(MyObjectBuilder_MotorAdvancedRotor), MotorAdvancedRotor },
			{ typeof(MyObjectBuilder_MotorAdvancedStator), MotorAdvancedStator },
			{ typeof(MyObjectBuilder_MotorRotor), MotorRotor },
			{ typeof(MyObjectBuilder_MotorStator), MotorStator },
			{ typeof(MyObjectBuilder_MotorSuspension), MotorSuspension },
			{ typeof(MyObjectBuilder_MyProgrammableBlock), MyProgrammableBlock },
			{ typeof(MyObjectBuilder_OreDetector), OreDetector },
			{ typeof(MyObjectBuilder_OxygenFarm), OxygenFarm },
			{ typeof(MyObjectBuilder_OxygenGenerator), OxygenGenerator },
			{ typeof(MyObjectBuilder_OxygenTank), OxygenTank },
			{ typeof(MyObjectBuilder_Parachute), Parachute },
			{ typeof(MyObjectBuilder_Passage), Passage },
			{ typeof(MyObjectBuilder_PistonBase), PistonBase },
			{ typeof(MyObjectBuilder_PistonTop), PistonTop },
			{ typeof(MyObjectBuilder_Planter), Planter },
			{ typeof(MyObjectBuilder_Projector), Projector },
			{ typeof(MyObjectBuilder_RadioAntenna), RadioAntenna },
			{ typeof(MyObjectBuilder_Reactor), Reactor },
			{ typeof(MyObjectBuilder_Refinery), Refinery },
			{ typeof(MyObjectBuilder_RemoteControl), RemoteControl },
			{ typeof(MyObjectBuilder_SafeZoneBlock), SafeZoneBlock },
			{ typeof(MyObjectBuilder_SensorBlock), SensorBlock },
			{ typeof(MyObjectBuilder_ShipConnector), ShipConnector },
			{ typeof(MyObjectBuilder_ShipGrinder), ShipGrinder },
			{ typeof(MyObjectBuilder_ShipWelder), ShipWelder },
			{ typeof(MyObjectBuilder_SmallGatlingGun), SmallGatlingGun },
			{ typeof(MyObjectBuilder_SmallMissileLauncher), SmallMissileLauncher },
			{ typeof(MyObjectBuilder_SmallMissileLauncherReload), SmallMissileLauncherReload },
			{ typeof(MyObjectBuilder_SolarPanel), SolarPanel },
			{ typeof(MyObjectBuilder_SoundBlock), SoundBlock },
			{ typeof(MyObjectBuilder_SpaceBall), SpaceBall },
			{ typeof(MyObjectBuilder_StoreBlock), StoreBlock },
			{ typeof(MyObjectBuilder_SurvivalKit), SurvivalKit },
			{ typeof(MyObjectBuilder_TerminalBlock), TerminalBlock },
			{ typeof(MyObjectBuilder_TextPanel), TextPanel },
			{ typeof(MyObjectBuilder_Thrust), Thrust },
			{ typeof(MyObjectBuilder_TimerBlock), TimerBlock },
			{ typeof(MyObjectBuilder_UpgradeModule), UpgradeModule },
			{ typeof(MyObjectBuilder_VendingMachine), VendingMachine },
			{ typeof(MyObjectBuilder_VirtualMass), VirtualMass },
			{ typeof(MyObjectBuilder_Warhead), Warhead },
			{ typeof(MyObjectBuilder_Wheel), Wheel },
			{ typeof(MyObjectBuilder_WindTurbine), WindTurbine }
		};


		private static readonly BlockValue defaultValue = new BlockValue { Value = 1, Threat = 1 };
		private static readonly BlockValue enhancedtValue = new BlockValue { Value = 5, Threat = 5 };

		// Cargo
		private static readonly BlockValue cargoContainerValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue ShipConnectorValue = new BlockValue { Value = 20, Threat = 10 };

		// Communications
		private static readonly BlockValue BeaconValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue LaserAntennaValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue RadioAntennaValue = new BlockValue { Value = 20, Threat = 10 };

		// Controllers
		private static readonly BlockValue CockpitValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue RemoteControlValue = new BlockValue { Value = 20, Threat = 10 };

		// Manufacturing
		private static readonly BlockValue AssemblerValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue RefineryValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SurvivalKitValue = new BlockValue { Value = 20, Threat = 10 };

		// Medical
		private static readonly BlockValue CryoChamberValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue MedicalRoomValue = new BlockValue { Value = 20, Threat = 10 };

		// Power Producers or Providers
		private static readonly BlockValue BatteryBlockValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue HydrogenEngineValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue ReactorValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SolarPanelValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue WindTurbineValue = new BlockValue { Value = 20, Threat = 10 };

		// Utility
		private static readonly BlockValue GasTankValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue JumpDriveValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue MyProgrammableBlockValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue OxygenGeneratorValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue OxygenTankValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue ThrustValue = new BlockValue { Value = 20, Threat = 10 };

		// Weapons
		private static readonly BlockValue InteriorTurretValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue LargeGatlingTurretValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue LargeMissileTurretValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SmallGatlingGunValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SmallMissileLauncherValue = new BlockValue { Value = 20, Threat = 10 };
		private static readonly BlockValue SmallMissileLauncherReloadValue = new BlockValue { Value = 20, Threat = 10 };

		// Modded Blocks
		private static readonly BlockValue DefenseShieldsValue = new BlockValue { Value = 200, Threat = 300 };
		private static readonly BlockValue EnergyShieldsValue = new BlockValue { Value = 200, Threat = 300 };


		private static readonly MasterObjectBuilderRefernece AirtightHangarDoor = new MasterObjectBuilderRefernece
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute(""), defaultValue },
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_AirtightHangarDoor)
		};

		private static readonly MasterObjectBuilderRefernece AirtightSlideDoor = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockSlideDoor"), defaultValue },
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_AirtightSlideDoor)
		};

		private static readonly MasterObjectBuilderRefernece AirVent = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("SmallAirVent"), defaultValue },
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_AirVent)
		};

		private static readonly MasterObjectBuilderRefernece Assembler = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("BasicAssembler"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeAssembler"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Assembler)
		};

		private static readonly MasterObjectBuilderRefernece BatteryBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockBatteryBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockBatteryBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallBatteryBlock"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_BatteryBlock)
		};

		private static readonly MasterObjectBuilderRefernece Beacon = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockBeacon"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockBeacon"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Beacon)
		};

		private static readonly MasterObjectBuilderRefernece ButtonPanel = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("ButtonPanelLarge"), defaultValue },
				{ MyStringHash.GetOrCompute("ButtonPanelSmall"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_ButtonPanel)
		};

		private static readonly MasterObjectBuilderRefernece CameraBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeCameraBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallCameraBlock"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_CameraBlock)
		};

		private static readonly MasterObjectBuilderRefernece CargoContainer = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLargeContainer"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockLockerRoom"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockLockerRoomCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockLockers"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallContainer"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeContainer"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockMediumContainer"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallContainer"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_CargoContainer)
		};

		private static readonly MasterObjectBuilderRefernece Cockpit = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("CockpitOpen"), defaultValue },
				{ MyStringHash.GetOrCompute("DBSmallBlockFighterCockpit"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockBathroom"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockBathroomOpen"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockCockpit"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockCockpitIndustrial"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockCockpitSeat"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockCouch"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockCouchCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockDesk"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockDeskCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockToilet"), defaultValue },
				{ MyStringHash.GetOrCompute("OpenCockpitLarge"), defaultValue },
				{ MyStringHash.GetOrCompute("OpenCockpitSmall"), defaultValue },
				{ MyStringHash.GetOrCompute("PassengerSeatLarge"), defaultValue },
				{ MyStringHash.GetOrCompute("PassengerSeatSmall"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockCockpit"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockCockpitIndustrial"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Cockpit)
		};

		private static readonly MasterObjectBuilderRefernece Collector = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("Collector"), defaultValue },
				{ MyStringHash.GetOrCompute("CollectorSmall"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Collector)
		};

		private static readonly MasterObjectBuilderRefernece ContractBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("ContractBlock"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_ContractBlock)
		};

		private static readonly MasterObjectBuilderRefernece Conveyor = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockConveyor"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockConveyor"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallShipConveyorHub"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Conveyor)
		};

		private static readonly MasterObjectBuilderRefernece ConveyorConnector = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("ConveyorFrameMedium"), defaultValue },
				{ MyStringHash.GetOrCompute("ConveyorTube"), defaultValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeCurved"), defaultValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeCurvedMedium"), defaultValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeMedium"), defaultValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeSmall"), defaultValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeSmallCurved"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_ConveyorConnector)
		};

		private static readonly MasterObjectBuilderRefernece ConveyorSorter = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockConveyorSorter"), defaultValue },
				{ MyStringHash.GetOrCompute("MediumBlockConveyorSorter"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockConveyorSorter"), defaultValue },
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_ConveyorSorter)
		};

		private static readonly MasterObjectBuilderRefernece CryoChamber = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockBed"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockCryoChamber"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockCryoChamber"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_CryoChamber)
		};

		private static readonly MasterObjectBuilderRefernece CubeBlock = new MasterObjectBuilderRefernece
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("DeadAstronaut"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeDeadAstronaut"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeRailStraight"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeRoundArmor_Corner"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeRoundArmor_CornerInv"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeRoundArmor_Slope"), defaultValue },
				{ MyStringHash.GetOrCompute("Monolith"), defaultValue },
				{ MyStringHash.GetOrCompute("Stereolith"), defaultValue },
				{ MyStringHash.GetOrCompute("ArmorCenter"), defaultValue },
				{ MyStringHash.GetOrCompute("ArmorCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("ArmorInvCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("ArmorSide"), defaultValue },
				{ MyStringHash.GetOrCompute("Catwalk"), defaultValue },
				{ MyStringHash.GetOrCompute("CatwalkCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("CatwalkRailingEnd"), defaultValue },
				{ MyStringHash.GetOrCompute("CatwalkRailingHalfLeft"), defaultValue },
				{ MyStringHash.GetOrCompute("CatwalkRailingHalfRight"), defaultValue },
				{ MyStringHash.GetOrCompute("CatwalkStraight"), defaultValue },
				{ MyStringHash.GetOrCompute("CatwalkWall"), defaultValue },
				{ MyStringHash.GetOrCompute("DeadBody01"), defaultValue },
				{ MyStringHash.GetOrCompute("DeadBody02"), defaultValue },
				{ MyStringHash.GetOrCompute("DeadBody03"), defaultValue },
				{ MyStringHash.GetOrCompute("DeadBody04"), defaultValue },
				{ MyStringHash.GetOrCompute("DeadBody05"), defaultValue },
				{ MyStringHash.GetOrCompute("DeadBody06"), defaultValue },
				{ MyStringHash.GetOrCompute("Freight1"), defaultValue },
				{ MyStringHash.GetOrCompute("Freight2"), defaultValue },
				{ MyStringHash.GetOrCompute("Freight3"), defaultValue },
				{ MyStringHash.GetOrCompute("GratedHalfStairs"), defaultValue },
				{ MyStringHash.GetOrCompute("GratedHalfStairsMirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("GratedStairs"), defaultValue },
				{ MyStringHash.GetOrCompute("HalfArmorBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("HalfSlopeArmorBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("HeavyHalfArmorBlock"), enhancedtValue },
				{ MyStringHash.GetOrCompute("HeavyHalfSlopeArmorBlock"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorCorner2Base"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorCorner2Tip"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorCornerInv"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorInvCorner2Base"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorInvCorner2Tip"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorRoundCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorRoundCornerInv"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorRoundSlope"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorSlope"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorSlope2Base"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorSlope2Tip"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockDeskChairless"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockDeskChairlessCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockInteriorWall"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeCoverWall"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeCoverWallHalf"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeHalfArmorBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeHalfSlopeArmorBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorBlock"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorCorner"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorCorner2Base"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorCorner2Tip"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorCornerInv"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorInvCorner2Base"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorInvCorner2Tip"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorRoundCorner"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorRoundCornerInv"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorRoundSlope"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorSlope"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorSlope2Base"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorSlope2Tip"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyHalfArmorBlock"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeHeavyHalfSlopeArmorBlock"), enhancedtValue },
				{ MyStringHash.GetOrCompute("LargeInteriorPillar"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeRamp"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeStairs"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeSteelCatwalk"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeSteelCatwalk2Sides"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeSteelCatwalkCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeSteelCatwalkPlate"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeWindowEdge"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeWindowSquare"), defaultValue },
				{ MyStringHash.GetOrCompute("RailingCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("RailingDiagonal"), defaultValue },
				{ MyStringHash.GetOrCompute("RailingDouble"), defaultValue },
				{ MyStringHash.GetOrCompute("RailingHalfLeft"), defaultValue },
				{ MyStringHash.GetOrCompute("RailingHalfRight"), defaultValue },
				{ MyStringHash.GetOrCompute("RailingStraight"), defaultValue },
				{ MyStringHash.GetOrCompute("Shower"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallArmorCenter"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallArmorCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallArmorInvCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallArmorSide"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorCorner2Base"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorCorner2Tip"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorCornerInv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorInvCorner2Base"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorInvCorner2Tip"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorRoundCorner"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorRoundCornerInv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorRoundSlope"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorSlope"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorSlope2Base"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorSlope2Tip"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorBlock"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorCorner"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorCorner2Base"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorCorner2Tip"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorCornerInv"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorInvCorner2Base"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorInvCorner2Tip"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorRoundCorner"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorRoundCornerInv"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorRoundSlope"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorSlope"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorSlope2Base"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorSlope2Tip"), enhancedtValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Face"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Flat"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1FlatInv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Inv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Side"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1SideInv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Slope"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2Face"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2Flat"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2FlatInv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2Inv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2SideLeft"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2SideLeftInv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2SideRight"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2SideRightInv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2Slope"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow2x3Flat"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow2x3FlatInv"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow3x3Flat"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWindow3x3FlatInv"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x1Face"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x1Flat"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x1FlatInv"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x1Inv"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x1Side"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x1SideInv"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x1Slope"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x2Face"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x2Flat"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x2FlatInv"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x2Inv"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x2SideLeft"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x2SideLeftInv"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x2SideRight"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x2SideRightInv"), defaultValue },
				{ MyStringHash.GetOrCompute("Window1x2Slope"), defaultValue },
				{ MyStringHash.GetOrCompute("Window2x3Flat"), defaultValue },
				{ MyStringHash.GetOrCompute("Window2x3FlatInv"), defaultValue },
				{ MyStringHash.GetOrCompute("Window3x3Flat"), defaultValue },
				{ MyStringHash.GetOrCompute("Window3x3FlatInv"), defaultValue },
				{ MyStringHash.GetOrCompute("WindowWall"), defaultValue },
				{ MyStringHash.GetOrCompute("WindowWallLeft"), defaultValue },
				{ MyStringHash.GetOrCompute("WindowWallRight"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_CubeBlock)
		};

		private static readonly MasterObjectBuilderRefernece Decoy = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeDecoy"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallDecoy"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Decoy)
		};

		private static readonly MasterObjectBuilderRefernece Door = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockGate"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockOffsetDoor"), defaultValue },
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Door)
		};

		private static readonly MasterObjectBuilderRefernece Drill = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockDrill"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockDrill"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Drill)
		};

		private static readonly MasterObjectBuilderRefernece GravityGenerator = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_GravityGenerator)
		};

		private static readonly MasterObjectBuilderRefernece GravityGeneratorSphere = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_GravityGeneratorSphere)
		};

		private static readonly MasterObjectBuilderRefernece Gyro = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockGyro"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockGyro"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Gyro)
		};

		private static readonly MasterObjectBuilderRefernece HydrogenEngine = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeHydrogenEngine"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallHydrogenEngine"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_HydrogenEngine)
		};

		private static readonly MasterObjectBuilderRefernece InteriorLight = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLight_1corner"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockLight_2corner"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockLight_1corner"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockLight_2corner"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallLight"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallLight"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_InteriorLight)
		};

		private static readonly MasterObjectBuilderRefernece InteriorTurret = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeInteriorTurret"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_InteriorTurret)
		};

		private static readonly MasterObjectBuilderRefernece Jukebox = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("Jukebox"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Jukebox)
		};

		private static readonly MasterObjectBuilderRefernece JumpDrive = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeJumpDrive"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_JumpDrive)
		};

		private static readonly MasterObjectBuilderRefernece Kitchen = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockKitchen"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Kitchen)
		};

		private static readonly MasterObjectBuilderRefernece Ladder2 = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LadderSmall"), defaultValue },
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Ladder2)
		};

		private static readonly MasterObjectBuilderRefernece LandingGear = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLandingGear"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockLandingGear"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_LandingGear)
		};

		private static readonly MasterObjectBuilderRefernece LargeGatlingTurret = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("SmallGatlingTurret"), defaultValue },
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_LargeGatlingTurret)
		};

		private static readonly MasterObjectBuilderRefernece LargeMissileTurret = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("SmallMissileTurret"), defaultValue },
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_LargeMissileTurret)
		};

		private static readonly MasterObjectBuilderRefernece LaserAntenna = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLaserAntenna"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockLaserAntenna"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_LaserAntenna)
		};

		private static readonly MasterObjectBuilderRefernece LCDPanelsBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LabEquipment"), defaultValue },
				{ MyStringHash.GetOrCompute("MedicalStation"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_LCDPanelsBlock)
		};

		private static readonly MasterObjectBuilderRefernece MedicalRoom = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeMedicalRoom"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_MedicalRoom)
		};

		private static readonly MasterObjectBuilderRefernece MergeBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeShipMergeBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallShipMergeBlock"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_MergeBlock)
		};

		private static readonly MasterObjectBuilderRefernece MotorAdvancedRotor = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeAdvancedRotor"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallAdvancedRotor"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_MotorAdvancedRotor)
		};

		private static readonly MasterObjectBuilderRefernece MotorAdvancedStator = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeAdvancedStator"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallAdvancedStator"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_MotorRotor)
		};

		private static readonly MasterObjectBuilderRefernece MotorRotor = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeRotor"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallRotor"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_MotorRotor)
		};

		private static readonly MasterObjectBuilderRefernece MotorStator = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeStator"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallStator"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_MotorStator)
		};

		private static readonly MasterObjectBuilderRefernece MotorSuspension = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("SmallSuspension1x1"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallSuspension1x1mirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallSuspension3x3"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallSuspension3x3mirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallSuspension5x5"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallSuspension5x5mirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("Suspension1x1"), defaultValue },
				{ MyStringHash.GetOrCompute("Suspension1x1mirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("Suspension3x3"), defaultValue },
				{ MyStringHash.GetOrCompute("Suspension3x3mirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("Suspension5x5"), defaultValue },
				{ MyStringHash.GetOrCompute("Suspension5x5mirrored"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_MotorSuspension)
		};

		private static readonly MasterObjectBuilderRefernece MyProgrammableBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeProgrammableBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallProgrammableBlock"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_MyProgrammableBlock)
		};

		private static readonly MasterObjectBuilderRefernece OreDetector = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeOreDetector"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockOreDetector"), defaultValue },

			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_OreDetector)
		};

		private static readonly MasterObjectBuilderRefernece OxygenFarm = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockOxygenFarm"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_OxygenFarm)
		};

		private static readonly MasterObjectBuilderRefernece OxygenGenerator = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("OxygenGeneratorSmall"), defaultValue },
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_OxygenGenerator)
		};

		private static readonly MasterObjectBuilderRefernece OxygenTank = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeHydrogenTank"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeHydrogenTankSmall"), defaultValue },
				{ MyStringHash.GetOrCompute("OxygenTankSmall"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallHydrogenTank"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallHydrogenTankSmall"), defaultValue },
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_OxygenTank)
		};

		private static readonly MasterObjectBuilderRefernece Parachute = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LgParachute"), defaultValue },
				{ MyStringHash.GetOrCompute("SmParachute"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Parachute)
		};

		private static readonly MasterObjectBuilderRefernece Passage = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Passage)
		};

		private static readonly MasterObjectBuilderRefernece PistonBase = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargePistonBase"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallPistonBase"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_PistonBase)
		};

		private static readonly MasterObjectBuilderRefernece PistonTop = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargePistonTop"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallPistonTop"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_PistonTop)
		};

		private static readonly MasterObjectBuilderRefernece Planter = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockPlanters"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Planter)
		};

		private static readonly MasterObjectBuilderRefernece Projector = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockConsole"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeProjector"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallProjector"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Projector)
		};

		private static readonly MasterObjectBuilderRefernece RadioAntenna = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockRadioAntenna"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockRadioAntennaDish"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockRadioAntenna"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_RadioAntenna)
		};

		private static readonly MasterObjectBuilderRefernece Reactor = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLargeGenerator"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallGenerator"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeGenerator"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallGenerator"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Reactor)
		};

		private static readonly MasterObjectBuilderRefernece Refinery = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("Blast Furnace"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeRefinery"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeShipSmallShieldGeneratorBase"), EnergyShieldsValue },
				{ MyStringHash.GetOrCompute("LargeShipLargeShieldGeneratorBase"), EnergyShieldsValue },
				{ MyStringHash.GetOrCompute("SmallShipSmallShieldGeneratorBase"), EnergyShieldsValue },
				{ MyStringHash.GetOrCompute("SmallShipMicroShieldGeneratorBase"), EnergyShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterSA"), EnergyShieldsValue }
			},

			_type = typeof(MyObjectBuilder_Refinery)
		};

		private static readonly MasterObjectBuilderRefernece RemoteControl = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockRemoteControl"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockRemoteControl"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_RemoteControl)
		};

		private static readonly MasterObjectBuilderRefernece SafeZoneBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("SafeZoneBlock"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_SafeZoneBlock)
		};

		private static readonly MasterObjectBuilderRefernece SensorBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockSensor"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSensor"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_SensorBlock)
		};

		private static readonly MasterObjectBuilderRefernece ShipConnector = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("Connector"), defaultValue },
				{ MyStringHash.GetOrCompute("ConnectorMedium"), defaultValue },
				{ MyStringHash.GetOrCompute("ConnectorSmall"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_ShipConnector)
		};

		private static readonly MasterObjectBuilderRefernece ShipGrinder = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeShipGrinder"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallShipGrinder"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_ShipGrinder)
		};

		private static readonly MasterObjectBuilderRefernece ShipWelder = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeShipWelder"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallShipWelder"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_ShipWelder)
		};

		private static readonly MasterObjectBuilderRefernece SmallGatlingGun = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_SmallGatlingGun)
		};

		private static readonly MasterObjectBuilderRefernece SmallMissileLauncher = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeMissileLauncher"), defaultValue },
				{ MyStringHash.GetOrCompute(""), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_SmallMissileLauncher)
		};

		private static readonly MasterObjectBuilderRefernece SmallMissileLauncherReload = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("SmallRocketLauncherReload"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_SmallMissileLauncherReload)
		};

		private static readonly MasterObjectBuilderRefernece SolarPanel = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockSolarPanel"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSolarPanel"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_SolarPanel)
		};

		private static readonly MasterObjectBuilderRefernece SoundBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockSoundBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSoundBlock"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_SoundBlock)
		};

		private static readonly MasterObjectBuilderRefernece SpaceBall = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("SpaceBallLarge"), defaultValue },
				{ MyStringHash.GetOrCompute("SpaceBallSmall"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_SpaceBall)
		};

		private static readonly MasterObjectBuilderRefernece StoreBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("AtmBlock"), defaultValue },
				{ MyStringHash.GetOrCompute("StoreBlock"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_StoreBlock)
		};

		private static readonly MasterObjectBuilderRefernece SurvivalKit = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("SurvivalKit"), defaultValue },
				{ MyStringHash.GetOrCompute("SurvivalKitLarge"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_SurvivalKit)
		};

		private static readonly MasterObjectBuilderRefernece TerminalBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("ControlPanel"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallControlPanel"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_TerminalBlock)
		};

		private static readonly MasterObjectBuilderRefernece TextPanel = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockCorner_LCD_1"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockCorner_LCD_2"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockCorner_LCD_Flat_1"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockCorner_LCD_Flat_2"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeLCDPanel"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeLCDPanelWide"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeTextPanel"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockCorner_LCD_1"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockCorner_LCD_2"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockCorner_LCD_Flat_1"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockCorner_LCD_Flat_2"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallLCDPanel"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallLCDPanelWide"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallTextPanel"), defaultValue },
				{ MyStringHash.GetOrCompute("TransparentLCDLarge"), defaultValue },
				{ MyStringHash.GetOrCompute("TransparentLCDSmall"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_TextPanel)
		};

		private static readonly MasterObjectBuilderRefernece Thrust = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLargeAtmosphericThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockLargeHydrogenThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockLargeThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallAtmosphericThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallHydrogenThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeAtmosphericThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeHydrogenThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallAtmosphericThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallHydrogenThrust"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallThrust"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Thrust)
		};

		private static readonly MasterObjectBuilderRefernece TimerBlock = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("TimerBlockLarge"), defaultValue },
				{ MyStringHash.GetOrCompute("TimerBlockSmall"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_TimerBlock)
		};

		private static readonly MasterObjectBuilderRefernece UpgradeModule = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeEffectivenessModule"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeEnergyModule"), defaultValue },
				{ MyStringHash.GetOrCompute("LargeProductivityModule"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("EmitterST"), DefenseShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterL"), DefenseShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterS"), DefenseShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterLA"), DefenseShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterSA"), DefenseShieldsValue }
			},

			_type = typeof(MyObjectBuilder_UpgradeModule)
		};

		private static readonly MasterObjectBuilderRefernece VendingMachine = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("FoodDispenser"), defaultValue },
				{ MyStringHash.GetOrCompute("VendingMachine"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_VendingMachine)
		};

		private static readonly MasterObjectBuilderRefernece VirtualMass = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("VirtualMassLarge"), defaultValue },
				{ MyStringHash.GetOrCompute("VirtualMassSmall"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_VirtualMass)
		};

		private static readonly MasterObjectBuilderRefernece Warhead = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeWarhead"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWarhead"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Warhead)
		};

		private static readonly MasterObjectBuilderRefernece Wheel = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("RealWheel"), defaultValue },
				{ MyStringHash.GetOrCompute("RealWheel1x1"), defaultValue },
				{ MyStringHash.GetOrCompute("RealWheel1x1mirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("RealWheel5x5"), defaultValue },
				{ MyStringHash.GetOrCompute("RealWheel5x5mirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("RealWheelmirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel1x1"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel1x1mirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel5x5"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel5x5mirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallRealWheelmirrored"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWheel1x1"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWheel3x3"), defaultValue },
				{ MyStringHash.GetOrCompute("SmallWheel5x5"), defaultValue },
				{ MyStringHash.GetOrCompute("Wheel1x1"), defaultValue },
				{ MyStringHash.GetOrCompute("Wheel3x3"), defaultValue },
				{ MyStringHash.GetOrCompute("Wheel5x5"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_Wheel)
		};

		private static readonly MasterObjectBuilderRefernece WindTurbine = new MasterObjectBuilderRefernece()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{
				{ MyStringHash.GetOrCompute("LargeBlockWindTurbine"), defaultValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockValue>
			{

			},

			_type = typeof(MyObjectBuilder_WindTurbine)
		};

	}
}
