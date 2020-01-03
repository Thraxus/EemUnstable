using System.Collections.Generic;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class Timer : ISetAlert
	{
		// TODO: Need to analyze timers to see how they are used in prefabs.
		// Perhaps come up with an algo to determine if it's relevant to war or not... 
		private readonly IMyTimerBlock _timer;
		private readonly long _ownerId;
		private readonly TimerSettings _wartimeSettings;
		private readonly TimerSettings _peacetimeSettings;

		private struct TimerSettings
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

		public Timer(IMyTimerBlock timer)
		{
			_timer = timer;
			_ownerId = timer.OwnerId;
			_wartimeSettings = new TimerSettings(true);
			_peacetimeSettings = new TimerSettings(_timer.Enabled);

			MyObjectBuilder_TimerBlock timerOb = (MyObjectBuilder_TimerBlock) _timer.GetObjectBuilderCubeBlock();
			foreach (MyObjectBuilder_Toolbar.Slot slot in timerOb.Toolbar.Slots)
			{
				StaticLog.WriteToLog("AlertConditions", $"Timer Toolbar - {slot.Item} | {slot.Data.TypeId} | {slot.Data.SubtypeId} | {slot.Data.SubtypeName}", LogType.General);
				if (slot.Data.TypeId == typeof(IMyLargeTurretBase))
					StaticLog.WriteToLog("AlertConditions",$"Timer Toolbar - {slot.Item}",LogType.General);
			}
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
