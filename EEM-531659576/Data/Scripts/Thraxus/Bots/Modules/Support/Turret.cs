using System;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class Turret
	{
		private readonly IMyLargeTurretBase _turret;
		private readonly long _ownerId;
		private readonly TurretSettings _wartimeSettings;
		private readonly TurretSettings _peacetimeSettings;

		public Turret(IMyLargeTurretBase turret, TurretSettings warTimeSettings, TurretSettings peaceTimeSettings)
		{
			_turret = turret;
			_ownerId = turret.OwnerId;
			_wartimeSettings = warTimeSettings;
			_peacetimeSettings = peaceTimeSettings;
		}

		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_turret.InScene || _turret.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);
			
			return true;
		}

		private void Execute(AlertSetting alertSetting)
		{

			TurretSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;
			
			_turret.Enabled = settings.Enabled;
			_turret.EnableIdleRotation = settings.EnableIdleRotation;
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