using Eem.Thraxus.Common.Utilities.Statics;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class Sensor
	{
		private readonly IMySensorBlock _sensor;
		private readonly long _ownerId;
		private readonly SensorSettings _wartimeSettings;
		private readonly SensorSettings _peacetimeSettings;

		public Sensor(IMySensorBlock sensor, SensorSettings wartimeSettings, SensorSettings peacetimeSettings)
		{
			_sensor = sensor;
			_ownerId = sensor.OwnerId;
			_wartimeSettings = wartimeSettings;
			_peacetimeSettings = peacetimeSettings;
			//GpsMarker();
		}


		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_sensor.InScene || _sensor.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);

			return true;
		}

		private void Execute(AlertSetting alertSetting)
		{

			SensorSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			_sensor.Enabled = settings.Enabled;
		}

		private void GpsMarker()
		{
			Statics.AddGpsLocation($"{CubeType.Sensor.ToString()} {_sensor.CustomName}: {_sensor.Enabled}", _sensor.GetPosition());
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}


}
