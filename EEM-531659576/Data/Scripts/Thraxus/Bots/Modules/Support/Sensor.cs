using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Statics;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class Sensor : ISetAlert
	{
		private readonly IMySensorBlock _sensor;
		private readonly long _ownerId;
		private readonly SensorSettings _wartimeSettings;
		private readonly SensorSettings _peacetimeSettings;

		private struct SensorSettings
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

		public Sensor(IMySensorBlock sensor)
		{
			_sensor = sensor;
			_ownerId = sensor.OwnerId;
			_peacetimeSettings = new SensorSettings(sensor.Enabled);
			_wartimeSettings = new SensorSettings(false);
			//GpsMarker();
		}


		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_sensor.InScene || _sensor.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);

			return true;
		}

		public void Close()
		{

		}

		private void Execute(AlertSetting alertSetting)
		{

			SensorSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			_sensor.Enabled = settings.Enabled;
		}

		private void GpsMarker()
		{
			Statics.AddGpsLocation($"Sensor: {_sensor.CustomName}: {_sensor.Enabled}", _sensor.GetPosition());
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}
}
