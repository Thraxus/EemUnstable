using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.SessionComps.Models;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.Utils;
using Sandbox.Common.ObjectBuilders;
using ObjectBuilders.SafeZone;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots.SessionComps.Support
{
	public static class Reference
	{
		private static MasterObjectBuilderReference refernece = new MasterObjectBuilderReference();

		public static BlockData GetBlockValue(IMySlimBlock block)
		{
			try
			{
				refernece.Clear();
				if (ReferenceValues.TryGetValue(block.BlockDefinition.Id.TypeId, out refernece))
				{
					return refernece.GetValue(block.BlockDefinition.Id.TypeId, block.BlockDefinition.Id.SubtypeId);
				}
				return GetDefaultValue(block.BlockDefinition.Id.TypeId, block.BlockDefinition.Id.SubtypeId);
			}
			catch (Exception e)
			{
				StaticLog.WriteToLog("Reference: GetBlockValue", $"{e}", LogType.Exception);
				return GetDefaultValue(block.BlockDefinition.Id.TypeId, block.BlockDefinition.Id.SubtypeId);
			}
		}

		private static BlockData GetDefaultValue(MyObjectBuilderType type, MyStringHash subtype)
		{
			StaticLog.WriteToLog("Reference", $"Value not found in any dictionaries: {type} | {subtype}", LogType.General);
			return DefaultBlockValue;
		}

		private static readonly Dictionary<MyObjectBuilderType, MasterObjectBuilderReference> ReferenceValues = new Dictionary<MyObjectBuilderType, MasterObjectBuilderReference>
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

		// Generics
		private static readonly BlockData DefaultBlockValue = new BlockData { Value = 1, Threat = 1 };
		private static readonly BlockData GenericWeaponBlockValue = new BlockData { Value = 50, Threat = 300, Type = TargetSystemType.Weapon };

		// Armor
		private static readonly BlockData HeavyArmorBlockValue = new BlockData { Value = 5, Threat = 5, IsHeavyArmor = true };

		// Cargo
		private static readonly BlockData CargoContainerValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Secondary };
		private static readonly BlockData ShipConnectorValue = new BlockData { Value = 20, Threat = 10 };

		// Communications
		private static readonly BlockData BeaconValue = new BlockData { Value = 20, Threat = 10 };
		private static readonly BlockData LaserAntennaValue = new BlockData { Value = 20, Threat = 10 };
		private static readonly BlockData RadioAntennaValue = new BlockData { Value = 20, Threat = 10 };

		// Controllers
		private static readonly BlockData CockpitValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Controller };
		private static readonly BlockData RemoteControlValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Controller };

		// Manufacturing
		private static readonly BlockData AssemblerValue = new BlockData { Value = 20, Threat = 10 };
		private static readonly BlockData RefineryValue = new BlockData { Value = 20, Threat = 10 };
		private static readonly BlockData SurvivalKitValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Secondary };

		// Medical
		private static readonly BlockData CryoChamberValue = new BlockData { Value = 20, Threat = 10 };
		private static readonly BlockData MedicalRoomValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Secondary };

		// Navigation
		private static readonly BlockData GyroValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Navigation };

		// Power Producers or Providers
		private static readonly BlockData BatteryBlockValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Power };
		private static readonly BlockData HydrogenEngineValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Power };
		private static readonly BlockData ReactorValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Power };
		private static readonly BlockData SolarPanelValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Power };
		private static readonly BlockData WindTurbineValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Power };

		// Utility
		private static readonly BlockData GasTankValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Secondary };
		private static readonly BlockData JumpDriveValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Secondary };
		private static readonly BlockData MyProgrammableBlockValue = new BlockData { Value = 20, Threat = 10 };
		private static readonly BlockData OxygenGeneratorValue = new BlockData { Value = 20, Threat = 10 };
		private static readonly BlockData OxygenTankValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Secondary };
		private static readonly BlockData ThrustValue = new BlockData { Value = 20, Threat = 10, Type = TargetSystemType.Propulsion };

		// Weapons
		private static readonly BlockData InteriorTurretValue = new BlockData { Value = 50, Threat = 300, Type = TargetSystemType.Weapon };
		private static readonly BlockData LargeGatlingTurretValue = new BlockData { Value = 50, Threat = 300, Type = TargetSystemType.Weapon };
		private static readonly BlockData LargeMissileTurretValue = new BlockData { Value = 50, Threat = 300, Type = TargetSystemType.Weapon };
		private static readonly BlockData SmallGatlingGunValue = new BlockData { Value = 50, Threat = 300, Type = TargetSystemType.Weapon };
		private static readonly BlockData SmallMissileLauncherValue = new BlockData { Value = 50, Threat = 300, Type = TargetSystemType.Weapon };
		private static readonly BlockData SmallMissileLauncherReloadValue = new BlockData { Value = 50, Threat = 300, Type = TargetSystemType.Weapon };
		private static readonly BlockData WarheadValue = new BlockData { Value = 50, Threat = 300, Type = TargetSystemType.Weapon }; 

		// Misc
		private static readonly BlockData DecoyValue = new BlockData { Value = 30, Threat = 100, Type = TargetSystemType.Decoy };

		// Modded Blocks
		private static readonly BlockData BuildAndRepairSystemValue = new BlockData { Value = 500, Threat = 500, IsBars = true };
		private static readonly BlockData DefenseShieldsValue = new BlockData { Value = 200, Threat = 300, IsDefenseShields = true };
		private static readonly BlockData EnergyShieldsValue = new BlockData { Value = 200, Threat = 300, IsEnergyShields = true };
		private static readonly BlockData WeaponCoreBlockValue = new BlockData { Value = 50, Threat = 300 };

		private static readonly MasterObjectBuilderReference AirtightHangarDoor = new MasterObjectBuilderReference
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute(""), DefaultBlockValue },
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_AirtightHangarDoor)
		};

		private static readonly MasterObjectBuilderReference AirtightSlideDoor = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockSlideDoor"), DefaultBlockValue },
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_AirtightSlideDoor)
		};

		private static readonly MasterObjectBuilderReference AirVent = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("SmallAirVent"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute(""), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_AirVent)
		};

		private static readonly MasterObjectBuilderReference Assembler = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("BasicAssembler"), AssemblerValue },
				{ MyStringHash.GetOrCompute("LargeAssembler"), AssemblerValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Assembler)
		};

		private static readonly MasterObjectBuilderReference BatteryBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockBatteryBlock"), BatteryBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockBatteryBlock"), BatteryBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallBatteryBlock"), BatteryBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_BatteryBlock)
		};

		private static readonly MasterObjectBuilderReference Beacon = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockBeacon"), BeaconValue },
				{ MyStringHash.GetOrCompute("SmallBlockBeacon"), BeaconValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Beacon)
		};

		private static readonly MasterObjectBuilderReference ButtonPanel = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("ButtonPanelLarge"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ButtonPanelSmall"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_ButtonPanel)
		};

		private static readonly MasterObjectBuilderReference CameraBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeCameraBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallCameraBlock"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_CameraBlock)
		};

		private static readonly MasterObjectBuilderReference CargoContainer = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLargeContainer"), CargoContainerValue },
				{ MyStringHash.GetOrCompute("LargeBlockLockerRoom"), CargoContainerValue },
				{ MyStringHash.GetOrCompute("LargeBlockLockerRoomCorner"), CargoContainerValue },
				{ MyStringHash.GetOrCompute("LargeBlockLockers"), CargoContainerValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallContainer"), CargoContainerValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeContainer"), CargoContainerValue },
				{ MyStringHash.GetOrCompute("SmallBlockMediumContainer"), CargoContainerValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallContainer"), CargoContainerValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_CargoContainer),
			DefaultValue = CargoContainerValue
		};

		private static readonly MasterObjectBuilderReference Cockpit = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("CockpitOpen"), CockpitValue },
				{ MyStringHash.GetOrCompute("DBSmallBlockFighterCockpit"), CockpitValue },
				{ MyStringHash.GetOrCompute("LargeBlockBathroom"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockBathroomOpen"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockCockpit"), CockpitValue },
				{ MyStringHash.GetOrCompute("LargeBlockCockpitIndustrial"), CockpitValue },
				{ MyStringHash.GetOrCompute("LargeBlockCockpitSeat"), CockpitValue },
				{ MyStringHash.GetOrCompute("LargeBlockCouch"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockCouchCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockDesk"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockDeskCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockToilet"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("OpenCockpitLarge"), CockpitValue },
				{ MyStringHash.GetOrCompute("OpenCockpitSmall"), CockpitValue },
				{ MyStringHash.GetOrCompute("PassengerSeatLarge"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("PassengerSeatSmall"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockCockpit"), CockpitValue },
				{ MyStringHash.GetOrCompute("SmallBlockCockpitIndustrial"), CockpitValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Cockpit),
			DefaultValue = CockpitValue
		};

		private static readonly MasterObjectBuilderReference Collector = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("Collector"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("CollectorSmall"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Collector)
		};

		private static readonly MasterObjectBuilderReference ContractBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("ContractBlock"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_ContractBlock)
		};

		private static readonly MasterObjectBuilderReference Conveyor = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockConveyor"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockConveyor"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallShipConveyorHub"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Conveyor)
		};

		private static readonly MasterObjectBuilderReference ConveyorConnector = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("ConveyorFrameMedium"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ConveyorTube"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeCurved"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeCurvedMedium"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeMedium"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeSmall"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ConveyorTubeSmallCurved"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_ConveyorConnector)
		};

		private static readonly MasterObjectBuilderReference ConveyorSorter = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockConveyorSorter"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("MediumBlockConveyorSorter"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockConveyorSorter"), DefaultBlockValue },
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_ConveyorSorter)
		};

		private static readonly MasterObjectBuilderReference CryoChamber = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockBed"), CryoChamberValue },
				{ MyStringHash.GetOrCompute("LargeBlockCryoChamber"), CryoChamberValue },
				{ MyStringHash.GetOrCompute("SmallBlockCryoChamber"), CryoChamberValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_CryoChamber), 
			DefaultValue = CryoChamberValue
		};

		private static readonly MasterObjectBuilderReference CubeBlock = new MasterObjectBuilderReference
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("DeadAstronaut"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeDeadAstronaut"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeRailStraight"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeRoundArmor_Corner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeRoundArmor_CornerInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeRoundArmor_Slope"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Monolith"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Stereolith"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ArmorCenter"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ArmorCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ArmorInvCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("ArmorSide"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Catwalk"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("CatwalkCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("CatwalkRailingEnd"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("CatwalkRailingHalfLeft"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("CatwalkRailingHalfRight"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("CatwalkStraight"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("CatwalkWall"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("DeadBody01"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("DeadBody02"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("DeadBody03"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("DeadBody04"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("DeadBody05"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("DeadBody06"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Freight1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Freight2"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Freight3"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("GratedHalfStairs"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("GratedHalfStairsMirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("GratedStairs"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("HalfArmorBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("HalfSlopeArmorBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("HeavyHalfArmorBlock"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("HeavyHalfSlopeArmorBlock"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorCorner2Base"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorCorner2Tip"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorCornerInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorInvCorner2Base"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorInvCorner2Tip"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorRoundCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorRoundCornerInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorRoundSlope"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorSlope"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorSlope2Base"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockArmorSlope2Tip"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockDeskChairless"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockDeskChairlessCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockInteriorWall"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeCoverWall"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeCoverWallHalf"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeHalfArmorBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeHalfSlopeArmorBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorBlock"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorCorner"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorCorner2Base"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorCorner2Tip"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorCornerInv"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorInvCorner2Base"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorInvCorner2Tip"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorRoundCorner"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorRoundCornerInv"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorRoundSlope"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorSlope"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorSlope2Base"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyBlockArmorSlope2Tip"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyHalfArmorBlock"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeHeavyHalfSlopeArmorBlock"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("LargeInteriorPillar"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeRamp"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeStairs"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeSteelCatwalk"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeSteelCatwalk2Sides"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeSteelCatwalkCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeSteelCatwalkPlate"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeWindowEdge"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeWindowSquare"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RailingCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RailingDiagonal"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RailingDouble"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RailingHalfLeft"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RailingHalfRight"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RailingStraight"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Shower"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallArmorCenter"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallArmorCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallArmorInvCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallArmorSide"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorCorner2Base"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorCorner2Tip"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorCornerInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorInvCorner2Base"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorInvCorner2Tip"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorRoundCorner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorRoundCornerInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorRoundSlope"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorSlope"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorSlope2Base"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockArmorSlope2Tip"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorBlock"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorCorner"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorCorner2Base"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorCorner2Tip"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorCornerInv"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorInvCorner2Base"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorInvCorner2Tip"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorRoundCorner"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorRoundCornerInv"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorRoundSlope"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorSlope"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorSlope2Base"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallHeavyBlockArmorSlope2Tip"), HeavyArmorBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Face"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Flat"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1FlatInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Inv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Side"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1SideInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x1Slope"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2Face"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2Flat"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2FlatInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2Inv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2SideLeft"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2SideLeftInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2SideRight"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2SideRightInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow1x2Slope"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow2x3Flat"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow2x3FlatInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow3x3Flat"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWindow3x3FlatInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x1Face"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x1Flat"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x1FlatInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x1Inv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x1Side"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x1SideInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x1Slope"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x2Face"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x2Flat"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x2FlatInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x2Inv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x2SideLeft"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x2SideLeftInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x2SideRight"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x2SideRightInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window1x2Slope"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window2x3Flat"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window2x3FlatInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window3x3Flat"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Window3x3FlatInv"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("WindowWall"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("WindowWallLeft"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("WindowWallRight"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_CubeBlock)
		};

		private static readonly MasterObjectBuilderReference Decoy = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeDecoy"), DecoyValue },
				{ MyStringHash.GetOrCompute("SmallDecoy"), DecoyValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Decoy),
			DefaultValue = DecoyValue
		};

		private static readonly MasterObjectBuilderReference Door = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockGate"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockOffsetDoor"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute(""), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Door)
		};

		private static readonly MasterObjectBuilderReference Drill = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockDrill"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockDrill"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Drill)
		};

		private static readonly MasterObjectBuilderReference GravityGenerator = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute(""), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_GravityGenerator)
		};

		private static readonly MasterObjectBuilderReference GravityGeneratorSphere = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute(""), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_GravityGeneratorSphere)
		};

		private static readonly MasterObjectBuilderReference Gyro = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockGyro"), GyroValue },
				{ MyStringHash.GetOrCompute("SmallBlockGyro"), GyroValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Gyro),
			DefaultValue = GyroValue
		};

		private static readonly MasterObjectBuilderReference HydrogenEngine = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeHydrogenEngine"), HydrogenEngineValue },
				{ MyStringHash.GetOrCompute("SmallHydrogenEngine"), HydrogenEngineValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_HydrogenEngine),
			DefaultValue = HydrogenEngineValue
		};

		private static readonly MasterObjectBuilderReference InteriorLight = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLight_1corner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockLight_2corner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockLight_1corner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockLight_2corner"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallLight"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallLight"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_InteriorLight)
		};

		private static readonly MasterObjectBuilderReference InteriorTurret = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeInteriorTurret"), InteriorTurretValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_InteriorTurret),

			DefaultValue = GenericWeaponBlockValue
		};

		private static readonly MasterObjectBuilderReference Jukebox = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("Jukebox"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Jukebox)
		};

		private static readonly MasterObjectBuilderReference JumpDrive = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeJumpDrive"), JumpDriveValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_JumpDrive)
		};

		private static readonly MasterObjectBuilderReference Kitchen = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockKitchen"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Kitchen)
		};

		private static readonly MasterObjectBuilderReference Ladder2 = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LadderSmall"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute(""), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Ladder2)
		};

		private static readonly MasterObjectBuilderReference LandingGear = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLandingGear"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockLandingGear"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_LandingGear)
		};

		private static readonly MasterObjectBuilderReference LargeGatlingTurret = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("SmallGatlingTurret"), LargeGatlingTurretValue },
				{ MyStringHash.GetOrCompute(""), LargeGatlingTurretValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_LargeGatlingTurret),

			DefaultValue = GenericWeaponBlockValue
		};

		private static readonly MasterObjectBuilderReference LargeMissileTurret = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("SmallMissileTurret"), LargeMissileTurretValue },
				{ MyStringHash.GetOrCompute(""), LargeMissileTurretValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_LargeMissileTurret),

			DefaultValue = GenericWeaponBlockValue
		};

		private static readonly MasterObjectBuilderReference LaserAntenna = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLaserAntenna"), LaserAntennaValue },
				{ MyStringHash.GetOrCompute("SmallBlockLaserAntenna"), LaserAntennaValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_LaserAntenna),
			DefaultValue = LaserAntennaValue
		};

		private static readonly MasterObjectBuilderReference LCDPanelsBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LabEquipment"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("MedicalStation"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_LCDPanelsBlock)
		};

		private static readonly MasterObjectBuilderReference MedicalRoom = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeMedicalRoom"), MedicalRoomValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_MedicalRoom),
			DefaultValue = MedicalRoomValue
		};

		private static readonly MasterObjectBuilderReference MergeBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeShipMergeBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallShipMergeBlock"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_MergeBlock)
		};

		private static readonly MasterObjectBuilderReference MotorAdvancedRotor = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeAdvancedRotor"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallAdvancedRotor"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_MotorAdvancedRotor)
		};

		private static readonly MasterObjectBuilderReference MotorAdvancedStator = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeAdvancedStator"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallAdvancedStator"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_MotorRotor)
		};

		private static readonly MasterObjectBuilderReference MotorRotor = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeRotor"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallRotor"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_MotorRotor)
		};

		private static readonly MasterObjectBuilderReference MotorStator = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeStator"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallStator"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_MotorStator)
		};

		private static readonly MasterObjectBuilderReference MotorSuspension = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("SmallSuspension1x1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallSuspension1x1mirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallSuspension3x3"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallSuspension3x3mirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallSuspension5x5"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallSuspension5x5mirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Suspension1x1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Suspension1x1mirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Suspension3x3"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Suspension3x3mirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Suspension5x5"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Suspension5x5mirrored"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_MotorSuspension)
		};

		private static readonly MasterObjectBuilderReference MyProgrammableBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeProgrammableBlock"), MyProgrammableBlockValue },
				{ MyStringHash.GetOrCompute("SmallProgrammableBlock"), MyProgrammableBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_MyProgrammableBlock),
			DefaultValue = MyProgrammableBlockValue
		};

		private static readonly MasterObjectBuilderReference OreDetector = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeOreDetector"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockOreDetector"), DefaultBlockValue },

			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_OreDetector)
		};

		private static readonly MasterObjectBuilderReference OxygenFarm = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockOxygenFarm"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_OxygenFarm)
		};

		private static readonly MasterObjectBuilderReference OxygenGenerator = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("OxygenGeneratorSmall"), OxygenGeneratorValue },
				{ MyStringHash.GetOrCompute(""), OxygenGeneratorValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_OxygenGenerator), 
			DefaultValue = OxygenGeneratorValue
		};

		private static readonly MasterObjectBuilderReference OxygenTank = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeHydrogenTank"), GasTankValue },
				{ MyStringHash.GetOrCompute("LargeHydrogenTankSmall"), GasTankValue },
				{ MyStringHash.GetOrCompute("OxygenTankSmall"), OxygenTankValue },
				{ MyStringHash.GetOrCompute("SmallHydrogenTank"), GasTankValue },
				{ MyStringHash.GetOrCompute("SmallHydrogenTankSmall"), GasTankValue },
				{ MyStringHash.GetOrCompute(""), OxygenTankValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_OxygenTank), 
			DefaultValue = GasTankValue
		};

		private static readonly MasterObjectBuilderReference Parachute = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LgParachute"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmParachute"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Parachute)
		};

		private static readonly MasterObjectBuilderReference Passage = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute(""), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Passage)
		};

		private static readonly MasterObjectBuilderReference PistonBase = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargePistonBase"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallPistonBase"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_PistonBase)
		};

		private static readonly MasterObjectBuilderReference PistonTop = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargePistonTop"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallPistonTop"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_PistonTop)
		};

		private static readonly MasterObjectBuilderReference Planter = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockPlanters"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Planter)
		};

		private static readonly MasterObjectBuilderReference Projector = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockConsole"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeProjector"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallProjector"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Projector)
		};

		private static readonly MasterObjectBuilderReference RadioAntenna = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockRadioAntenna"), RadioAntennaValue },
				{ MyStringHash.GetOrCompute("LargeBlockRadioAntennaDish"), RadioAntennaValue },
				{ MyStringHash.GetOrCompute("SmallBlockRadioAntenna"), RadioAntennaValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_RadioAntenna), 
			DefaultValue = RadioAntennaValue
		};

		private static readonly MasterObjectBuilderReference Reactor = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLargeGenerator"), ReactorValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallGenerator"), ReactorValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeGenerator"), ReactorValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallGenerator"), ReactorValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Reactor),
			DefaultValue = ReactorValue
		};

		private static readonly MasterObjectBuilderReference Refinery = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("Blast Furnace"), RefineryValue },
				{ MyStringHash.GetOrCompute("LargeRefinery"), RefineryValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeShipSmallShieldGeneratorBase"), EnergyShieldsValue },
				{ MyStringHash.GetOrCompute("LargeShipLargeShieldGeneratorBase"), EnergyShieldsValue },
				{ MyStringHash.GetOrCompute("SmallShipSmallShieldGeneratorBase"), EnergyShieldsValue },
				{ MyStringHash.GetOrCompute("SmallShipMicroShieldGeneratorBase"), EnergyShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterSA"), EnergyShieldsValue }
			},

			Type = typeof(MyObjectBuilder_Refinery),
			DefaultValue = RefineryValue
		};

		private static readonly MasterObjectBuilderReference RemoteControl = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockRemoteControl"), RemoteControlValue },
				{ MyStringHash.GetOrCompute("SmallBlockRemoteControl"), RemoteControlValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_RemoteControl),
			DefaultValue = RemoteControlValue
		};

		private static readonly MasterObjectBuilderReference SafeZoneBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("SafeZoneBlock"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_SafeZoneBlock)
		};

		private static readonly MasterObjectBuilderReference SensorBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockSensor"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockSensor"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_SensorBlock)
		};

		private static readonly MasterObjectBuilderReference ShipConnector = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("Connector"), ShipConnectorValue },
				{ MyStringHash.GetOrCompute("ConnectorMedium"), ShipConnectorValue },
				{ MyStringHash.GetOrCompute("ConnectorSmall"), ShipConnectorValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_ShipConnector), 
			DefaultValue = ShipConnectorValue
		};

		private static readonly MasterObjectBuilderReference ShipGrinder = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeShipGrinder"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallShipGrinder"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_ShipGrinder)
		};

		private static readonly MasterObjectBuilderReference ShipWelder = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeShipWelder"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallShipWelder"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("SELtdLargeNanobotBuildAndRepairSystem"), BuildAndRepairSystemValue },
				{ MyStringHash.GetOrCompute("SELtdSmallNanobotBuildAndRepairSystem"), BuildAndRepairSystemValue }
			},

			Type = typeof(MyObjectBuilder_ShipWelder)
		};

		private static readonly MasterObjectBuilderReference SmallGatlingGun = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute(""), SmallGatlingGunValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_SmallGatlingGun), 

			DefaultValue = GenericWeaponBlockValue
		};

		private static readonly MasterObjectBuilderReference SmallMissileLauncher = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeMissileLauncher"), SmallMissileLauncherValue },
				{ MyStringHash.GetOrCompute(""), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_SmallMissileLauncher),
			DefaultValue = GenericWeaponBlockValue
		};

		private static readonly MasterObjectBuilderReference SmallMissileLauncherReload = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("SmallRocketLauncherReload"), SmallMissileLauncherReloadValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_SmallMissileLauncherReload),
			DefaultValue = GenericWeaponBlockValue
		};

		private static readonly MasterObjectBuilderReference SolarPanel = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockSolarPanel"), SolarPanelValue },
				{ MyStringHash.GetOrCompute("SmallBlockSolarPanel"), SolarPanelValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_SolarPanel), 
			DefaultValue = SolarPanelValue
		};

		private static readonly MasterObjectBuilderReference SoundBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockSoundBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockSoundBlock"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_SoundBlock)
		};

		private static readonly MasterObjectBuilderReference SpaceBall = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("SpaceBallLarge"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SpaceBallSmall"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_SpaceBall)
		};

		private static readonly MasterObjectBuilderReference StoreBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("AtmBlock"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("StoreBlock"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_StoreBlock)
		};

		private static readonly MasterObjectBuilderReference SurvivalKit = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("SurvivalKit"), SurvivalKitValue },
				{ MyStringHash.GetOrCompute("SurvivalKitLarge"), SurvivalKitValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_SurvivalKit), 
			DefaultValue = SurvivalKitValue
		};

		private static readonly MasterObjectBuilderReference TerminalBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("ControlPanel"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallControlPanel"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_TerminalBlock)
		};

		private static readonly MasterObjectBuilderReference TextPanel = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockCorner_LCD_1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockCorner_LCD_2"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockCorner_LCD_Flat_1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeBlockCorner_LCD_Flat_2"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeLCDPanel"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeLCDPanelWide"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeTextPanel"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockCorner_LCD_1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockCorner_LCD_2"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockCorner_LCD_Flat_1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallBlockCorner_LCD_Flat_2"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallLCDPanel"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallLCDPanelWide"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallTextPanel"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("TransparentLCDLarge"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("TransparentLCDSmall"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_TextPanel)
		};

		private static readonly MasterObjectBuilderReference Thrust = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockLargeAtmosphericThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("LargeBlockLargeHydrogenThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("LargeBlockLargeThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallAtmosphericThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallHydrogenThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("LargeBlockSmallThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeAtmosphericThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeHydrogenThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("SmallBlockLargeThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallAtmosphericThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallHydrogenThrust"), ThrustValue },
				{ MyStringHash.GetOrCompute("SmallBlockSmallThrust"), ThrustValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Thrust),
			DefaultValue = ThrustValue
		};

		private static readonly MasterObjectBuilderReference TimerBlock = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("TimerBlockLarge"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("TimerBlockSmall"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_TimerBlock)
		};

		private static readonly MasterObjectBuilderReference UpgradeModule = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeEffectivenessModule"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeEnergyModule"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("LargeProductivityModule"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("EmitterST"), DefenseShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterL"), DefenseShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterS"), DefenseShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterLA"), DefenseShieldsValue },
				{ MyStringHash.GetOrCompute("EmitterSA"), DefenseShieldsValue }
			},

			Type = typeof(MyObjectBuilder_UpgradeModule)
		};

		private static readonly MasterObjectBuilderReference VendingMachine = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("FoodDispenser"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("VendingMachine"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_VendingMachine)
		};

		private static readonly MasterObjectBuilderReference VirtualMass = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("VirtualMassLarge"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("VirtualMassSmall"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_VirtualMass)
		};

		private static readonly MasterObjectBuilderReference Warhead = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeWarhead"), WarheadValue },
				{ MyStringHash.GetOrCompute("SmallWarhead"), WarheadValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Warhead),

			DefaultValue = GenericWeaponBlockValue
		};

		private static readonly MasterObjectBuilderReference Wheel = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("RealWheel"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RealWheel1x1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RealWheel1x1mirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RealWheel5x5"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RealWheel5x5mirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("RealWheelmirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel1x1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel1x1mirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel5x5"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallRealWheel5x5mirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallRealWheelmirrored"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWheel1x1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWheel3x3"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("SmallWheel5x5"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Wheel1x1"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Wheel3x3"), DefaultBlockValue },
				{ MyStringHash.GetOrCompute("Wheel5x5"), DefaultBlockValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_Wheel)
		};

		private static readonly MasterObjectBuilderReference WindTurbine = new MasterObjectBuilderReference()
		{
			VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{
				{ MyStringHash.GetOrCompute("LargeBlockWindTurbine"), WindTurbineValue }
			},

			ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>
			{

			},

			Type = typeof(MyObjectBuilder_WindTurbine),
			DefaultValue = WindTurbineValue
		};

	}
}
