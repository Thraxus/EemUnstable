using SpaceEngineers.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class AirVent
	{
		private readonly IMyAirVent _airVent;
		private readonly long _ownerId;
		private readonly AirVentSettings _wartimeSettings;
		private readonly AirVentSettings _peacetimeSettings;

		public AirVent(IMyAirVent airVent, AirVentSettings wartimeSettings, AirVentSettings peacetimeSettings)
		{
			_airVent = airVent;
			_ownerId = airVent.OwnerId;
			_wartimeSettings = wartimeSettings;
			_peacetimeSettings = peacetimeSettings;
		}

		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_airVent.InScene || _airVent.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);

			return true;
		}

		private void Execute(AlertSetting alertSetting)
		{

			AirVentSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			_airVent.Enabled = settings.Enabled;
			_airVent.Depressurize = settings.Depressurize;
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}


}
