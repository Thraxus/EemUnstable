using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Common.Enums;
using SpaceEngineers.Game.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Modules.Alert.Support
{
	internal class Light : ISetAlert
	{
		private readonly IMyInteriorLight _light;
		private readonly long _ownerId;
		private readonly LightSettings _wartimeSettings;
		private readonly LightSettings _peacetimeSettings;

		public Light(IMyInteriorLight light)
		{
			_light = light;
			_ownerId = light.OwnerId;

			_peacetimeSettings = new LightSettings(
				light.Enabled,
				light.BlinkIntervalSeconds,
				light.BlinkLength,
				light.BlinkOffset,
				light.Color,
				light.Falloff,
				light.Intensity, 
				light.Radius);

			_wartimeSettings = new LightSettings(
				true, 
				2.5f,
				20f,
				0f,
				Color.Red,
				5f,
				4f, 
				10f
				);

		}

		private struct LightSettings
		{
			public readonly bool Enabled;
			public readonly float BlinkInterval;
			public readonly float BlinkLength;
			public readonly float BlinkOffset;
			public readonly Color Color;
			public readonly float Falloff;
			public readonly float Intensity;
			public readonly float Radius;

			public LightSettings(bool enabled, float blinkInterval, float blinkLength, float blinkOffset, Color color, float falloff, float intensity, float radius)
			{
				Enabled = enabled;
				BlinkInterval = blinkInterval;
				BlinkLength = blinkLength;
				BlinkOffset = blinkOffset;
				Color = color;
				Falloff = falloff;
				Intensity = intensity;
				Radius = radius;
			}

			public override string ToString()
			{
				return $"{Enabled} | {BlinkInterval} | {BlinkLength} | {BlinkOffset} | {Color} | {Falloff} | {Intensity} | {Radius}";
			}
		}



		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_light.InScene || _light.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);

			return true;
		}

		private void Execute(AlertSetting alertSetting)
		{

			_light.Enabled = false;

			LightSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			_light.Enabled = settings.Enabled;
			_light.BlinkIntervalSeconds = settings.BlinkInterval;
			_light.BlinkLength = settings.BlinkLength;
			_light.BlinkOffset = settings.BlinkOffset;
			_light.Color = settings.Color;
			_light.Falloff = settings.Falloff;
			_light.Intensity = settings.Intensity;
			_light.Radius = settings.Radius;

			//_timer.Enabled = settings.Enabled;

			//if (alertSetting == AlertSetting.Peacetime && _alertOffTimer)
			//	_timer.Trigger();

			//if (alertSetting == AlertSetting.Wartime && _alertOnTimer)
			//	_timer.Trigger();
			//if (_timer.Enabled && alertSetting == AlertSetting.Wartime) _timer.Trigger();
		}

		public void Close()
		{
			
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}
}
