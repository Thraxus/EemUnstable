using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Common.DataTypes;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;

namespace Eem.Thraxus.Bots.Modules.Alert.Support
{
	internal class Turret : ISetAlert
	{
		private readonly IMyLargeTurretBase _turret;
		private readonly long _ownerId;
		private readonly TurretSettings _wartimeSettings;
		private readonly TurretSettings _peacetimeSettings;

		private struct TurretSettings
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

		public Turret(IMyLargeTurretBase turret)
		{
			_turret = turret;
			_ownerId = turret.OwnerId;
			MyObjectBuilder_TurretBase myTurretBase = (MyObjectBuilder_TurretBase) turret.GetObjectBuilderCubeBlock();
			
			_peacetimeSettings = new TurretSettings(
				turret.Enabled,
				turret.EnableIdleRotation,
				myTurretBase.TargetCharacters,
				myTurretBase.TargetLargeGrids,
				myTurretBase.TargetMeteors,
				myTurretBase.TargetMissiles,
				myTurretBase.TargetNeutrals,
				myTurretBase.TargetSmallGrids,
				myTurretBase.TargetStations,
				myTurretBase.Range
				);

			_wartimeSettings = new TurretSettings(
				true, 
				true, 
				true, 
				true, 
				false, 
				true, 
				false, 
				true, 
				true, 
				800);
		}

		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_turret.InScene || _turret.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);
			
			return true;
		}

		public void Close()
		{

		}

		private void Execute(AlertSetting alertSetting)
		{

			TurretSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;
			
			_turret.Enabled = settings.Enabled;
			_turret.EnableIdleRotation = settings.EnableIdleRotation;
			_turret.SetValueBool("TargetCharacters", settings.TargetCharacters);
			_turret.SetValueBool("TargetMeteors", settings.TargetMeteors);
			_turret.SetValueBool("TargetMissiles", settings.TargetMissiles);
			_turret.SetValueBool("TargetNeutrals", settings.TargetNeutrals);
			_turret.SetValueBool("TargetLargeShips", settings.TargetLargeShips);
			_turret.SetValueBool("TargetSmallShips", settings.TargetSmallShips);
			_turret.SetValueBool("TargetStations", settings.TargetStations);
			_turret.SetValueFloat("Range", settings.Range);
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}
}