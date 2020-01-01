using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRageMath;
using IMySensorBlock = Sandbox.ModAPI.Ingame.IMySensorBlock;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal struct AirVentSettings
	{
		public readonly bool Depressurize;
		public readonly bool Enabled;

		public AirVentSettings(bool enabled, bool depressurize)
		{
			Enabled = enabled;
			Depressurize = depressurize;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Enabled} | {Depressurize}";
		}
	}

	internal struct DoorSettings
	{
		public readonly bool Enabled;
		public readonly bool IsClosed;

		public DoorSettings(bool enabled, bool isClosed)
		{
			Enabled = enabled;
			IsClosed = isClosed;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Enabled} {IsClosed}";
		}
	}

	internal struct GravityGeneratorSettings
	{
		public readonly bool Enabled;
		public readonly Vector3 FieldSize;
		public readonly float FieldStrength;

		public GravityGeneratorSettings(bool enabled, Vector3 fieldSize, float fieldStrength)
		{
			Enabled = enabled;
			FieldSize = fieldSize;
			FieldStrength = fieldStrength;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Enabled} | {FieldSize} | {FieldStrength}";
		}
	}

	internal struct SphericalGravityGeneratorSettings
	{
		public readonly bool Enabled;
		public readonly float FieldSize;
		public readonly float FieldStrength;

		public SphericalGravityGeneratorSettings(bool enabled, float fieldSize, float fieldStrength)
		{
			Enabled = enabled;
			FieldSize = fieldSize;
			FieldStrength = fieldStrength;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Enabled} | {FieldSize} | {FieldStrength}";
		}
	}

	internal struct SensorSettings
	{
		public readonly bool Enabled;

		public SensorSettings(bool enabled)
		{
			Enabled = enabled;
		}

		public override string ToString()
		{
			return $"{Enabled}";
		}
	}

	internal struct TimerSettings
	{
		public readonly bool Enabled;

		public TimerSettings(bool enabled)
		{
			Enabled = enabled;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Enabled}";
		}
	}

	internal struct TurretSettings
	{
		public readonly bool Enabled;
		public readonly bool EnableIdleRotation;
		public readonly bool TargetCharacters;
		public readonly bool TargetLargeShips;
		public readonly bool TargetMeteors;
		public readonly bool TargetMissiles;
		public readonly bool TargetNeutrals;
		public readonly bool TargetSmallShips;
		public readonly bool TargetStations;

		public readonly float Range;

		public TurretSettings(bool enabled, bool enableIdleRotation, bool targetCharacters, bool targetLargeShips, bool targetMeteors, bool targetMissiles, bool targetNeutrals, bool targetSmallShips, bool targetStations, float range)
		{
			Enabled = enabled;
			EnableIdleRotation = enableIdleRotation;
			TargetCharacters = targetCharacters;
			TargetLargeShips = targetLargeShips;
			TargetMeteors = targetMeteors;
			TargetMissiles = targetMissiles;
			TargetNeutrals = targetNeutrals;
			TargetSmallShips = targetSmallShips;
			TargetStations = targetStations;
			Range = range;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Enabled} | {EnableIdleRotation} | {TargetCharacters} | {TargetLargeShips} | {TargetMeteors} | {TargetMissiles} | {TargetSmallShips} | {TargetStations} | {Range}";
		}
	}







	internal struct GridAirVents
	{
		public readonly IMyAirVent AirVent;
		public readonly AirVentSettings WarTimeSettings;
		public readonly AirVentSettings PeaceTimeSettings;

		public GridAirVents(IMyAirVent airVent, AirVentSettings warTimeSettings, AirVentSettings peaceTimeSettings)
		{
			AirVent = airVent;
			WarTimeSettings = warTimeSettings;
			PeaceTimeSettings = peaceTimeSettings;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{AirVent.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
		}
	}

	internal struct GridDoors
	{
		public readonly IMyDoor Door;
		public readonly DoorSettings WarTimeSettings;
		public readonly DoorSettings PeaceTimeSettings;

		public GridDoors(IMyDoor door, DoorSettings warTimeSettings, DoorSettings peaceTimeSettings)
		{
			Door = door;
			WarTimeSettings = warTimeSettings;
			PeaceTimeSettings = peaceTimeSettings;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Door.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
		}
	}

	internal struct GridGravityGenerators
	{
		public readonly IMyGravityGenerator GravityGenerator;
		public readonly GravityGeneratorSettings WarTimeSettings;
		public readonly GravityGeneratorSettings PeaceTimeSettings;

		public GridGravityGenerators(IMyGravityGenerator gravityGenerator, GravityGeneratorSettings warTimeSettings, GravityGeneratorSettings peaceTimeSettings)
		{
			GravityGenerator = gravityGenerator;
			WarTimeSettings = warTimeSettings;
			PeaceTimeSettings = peaceTimeSettings;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{GravityGenerator.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
		}
	}

	internal struct GridSphericalGravityGenerators
	{
		public readonly IMyGravityGeneratorSphere SphericalGravityGenerator;
		public readonly SphericalGravityGeneratorSettings WarTimeSettings;
		public readonly SphericalGravityGeneratorSettings PeaceTimeSettings;

		public GridSphericalGravityGenerators(IMyGravityGeneratorSphere gravityGenerator, SphericalGravityGeneratorSettings warTimeSettings, SphericalGravityGeneratorSettings peaceTimeSettings)
		{
			SphericalGravityGenerator = gravityGenerator;
			WarTimeSettings = warTimeSettings;
			PeaceTimeSettings = peaceTimeSettings;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{SphericalGravityGenerator.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
		}
	}

	internal struct GridSensors
	{
		public readonly Sandbox.ModAPI.Ingame.IMySensorBlock SensorBlock;
		public readonly SensorSettings WarTimeSettings;
		public readonly SensorSettings PeaceTimeSettings;

		public GridSensors(IMySensorBlock sensorBlock, SensorSettings warTimeSettings, SensorSettings peaceTimeSettings)
		{
			SensorBlock = sensorBlock;
			WarTimeSettings = warTimeSettings;
			PeaceTimeSettings = peaceTimeSettings;
		}


		public override string ToString()
		{
			return $"{SensorBlock.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
		}
	}

	internal struct GridTimers
	{
		public readonly IMyTimerBlock TimerBlock;
		public readonly TimerSettings WarTimeSettings;
		public readonly TimerSettings PeaceTimeSettings;

		public GridTimers(IMyTimerBlock timer, TimerSettings warTimeSettings, TimerSettings peaceTimeSettings)
		{
			TimerBlock = timer;
			WarTimeSettings = warTimeSettings;
			PeaceTimeSettings = peaceTimeSettings;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{TimerBlock.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
		}
	}

	internal struct GridTurrets
	{
		public readonly IMyLargeTurretBase Turret;
		public readonly TurretSettings WarTimeSettings;
		public readonly TurretSettings PeaceTimeSettings;

		public GridTurrets(IMyLargeTurretBase largeTurretBase, TurretSettings warTimeSettings, TurretSettings peaceTimeSettings)
		{
			Turret = largeTurretBase;
			WarTimeSettings = warTimeSettings;
			PeaceTimeSettings = peaceTimeSettings;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Turret.CustomName} | {PeaceTimeSettings} | {WarTimeSettings}";
		}
	}



}
