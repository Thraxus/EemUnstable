using System.Collections;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Statics;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class Antenna : ISetAlert
	{
		private readonly IMyRadioAntenna _antenna;
		private readonly long _ownerId;
		private readonly bool _alertAntenna;
		private readonly AntennaSettings _peacetimeSettings;
		private readonly AntennaSettings _wartimeSettings;
		
		private struct AntennaSettings
		{
			public readonly bool Enabled;
			public readonly string CustomName;
			public readonly float Range;

			public AntennaSettings(bool enabled, string customName, float range)
			{
				Enabled = enabled;
				CustomName = customName;
				Range = range;
			}

			public override string ToString()
			{
				return $"{Enabled} | {Range} | {CustomName}";
			}
		}

		public Antenna(IMyRadioAntenna antenna)
		{
			_antenna = antenna;
			_ownerId = _antenna.OwnerId;
			_peacetimeSettings = new AntennaSettings(antenna.Enabled, antenna.CustomName, antenna.Radius);
			// TODO: Moving from static grid distress tro faction based distress.  Need to create a method to support this.
			_wartimeSettings = new AntennaSettings(!antenna.Enabled, antenna.CustomName, 1000);
			if (!antenna.Enabled) _alertAntenna = true;
		}


		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_antenna.InScene || _antenna.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);

			return true;
		}

		private void Execute(AlertSetting alertSetting)
		{

			AntennaSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			if (_alertAntenna)
			{
				// TODO: Fill in alert conditions
			}

			_antenna.Enabled = settings.Enabled;
		}

		private void GpsMarker()
		{
			Statics.AddGpsLocation($"Antenna: {_antenna.CustomName}: {_antenna.Enabled}", _antenna.GetPosition());
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}
}
