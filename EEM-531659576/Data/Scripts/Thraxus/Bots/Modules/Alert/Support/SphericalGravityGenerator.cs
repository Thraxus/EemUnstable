using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Common.Enums;
using SpaceEngineers.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Alert.Support
{
	internal class SphericalGravityGenerator : ISetAlert
	{
		private readonly IMyGravityGeneratorSphere _gravityGeneratorSphere;
		private readonly long _ownerId;
		private readonly SphericalGravityGeneratorSettings _wartimeSettings;
		private readonly SphericalGravityGeneratorSettings _peacetimeSettings;

		private struct SphericalGravityGeneratorSettings
		{
			public readonly bool Enabled;
			public readonly float Radius;
			public readonly float FieldStrength;

			public SphericalGravityGeneratorSettings(bool enabled, float radius, float fieldStrength)
			{
				Enabled = enabled;
				Radius = radius;
				FieldStrength = fieldStrength;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled} | {Radius} | {FieldStrength}";
			}
		}

		public SphericalGravityGenerator(IMyGravityGeneratorSphere gravityGeneratorSphere)
		{
			_gravityGeneratorSphere = gravityGeneratorSphere;
			_ownerId = gravityGeneratorSphere.OwnerId;
			_peacetimeSettings = new SphericalGravityGeneratorSettings(_gravityGeneratorSphere.Enabled, _gravityGeneratorSphere.Radius, _gravityGeneratorSphere.GravityAcceleration);
			_wartimeSettings = new SphericalGravityGeneratorSettings(true, 450f, 9.81f);
		}

		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_gravityGeneratorSphere.InScene || _gravityGeneratorSphere.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);

			return true;
		}

		public void Close()
		{

		}

		private void Execute(AlertSetting alertSetting)
		{

			SphericalGravityGeneratorSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			_gravityGeneratorSphere.Enabled = settings.Enabled;
			_gravityGeneratorSphere.Radius = settings.Radius;
			_gravityGeneratorSphere.GravityAcceleration = settings.FieldStrength;
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}
}
