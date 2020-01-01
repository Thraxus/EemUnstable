using SpaceEngineers.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class Timer
	{
		private readonly IMyTimerBlock _timer;
		private readonly long _ownerId;
		private readonly TimerSettings _wartimeSettings;
		private readonly TimerSettings _peacetimeSettings;

		public Timer(IMyTimerBlock timer, TimerSettings wartimeSettings, TimerSettings peacetimeSettings)
		{
			_timer = timer;
			_ownerId = timer.OwnerId;
			_wartimeSettings = wartimeSettings;
			_peacetimeSettings = peacetimeSettings;
		}

		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_timer.InScene || _timer.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);

			return true;
		}

		private void Execute(AlertSetting alertSetting)
		{

			TimerSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			_timer.Enabled = settings.Enabled;
			if (_timer.Enabled && alertSetting == AlertSetting.Wartime) _timer.Trigger();
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}
}
