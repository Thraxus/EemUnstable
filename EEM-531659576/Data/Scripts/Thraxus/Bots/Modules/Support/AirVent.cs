using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Common.DataTypes;
using SpaceEngineers.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class AirVent : ISetAlert
	{
		private readonly IMyAirVent _airVent;
		private readonly long _ownerId;
		private readonly AirVentSettings _wartimeSettings;
		private readonly AirVentSettings _peacetimeSettings;

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

		public AirVent(IMyAirVent airVent)
		{
			_airVent = airVent;
			_ownerId = airVent.OwnerId;
			_peacetimeSettings = new AirVentSettings(_airVent.Enabled, _airVent.Depressurize);
			_wartimeSettings = new AirVentSettings(true, true);
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
