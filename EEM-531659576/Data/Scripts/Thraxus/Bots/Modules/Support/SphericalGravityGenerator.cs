using SpaceEngineers.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class SphericalGravityGenerator
	{
		private readonly IMyGravityGeneratorSphere _gravityGeneratorSphere;
		private readonly long _ownerId;
		private readonly SphericalGravityGeneratorSettings _wartimeSettings;
		private readonly SphericalGravityGeneratorSettings _peacetimeSettings;

		public SphericalGravityGenerator(IMyGravityGeneratorSphere gravityGeneratorSphere,
			SphericalGravityGeneratorSettings wartimeSettings, SphericalGravityGeneratorSettings peacetimeSettings)
		{
			_gravityGeneratorSphere = gravityGeneratorSphere;
			_ownerId = gravityGeneratorSphere.OwnerId;
			_wartimeSettings = wartimeSettings;
			_peacetimeSettings = peacetimeSettings;
		}

		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_gravityGeneratorSphere.InScene || _gravityGeneratorSphere.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);

			return true;
		}

		private void Execute(AlertSetting alertSetting)
		{

			SphericalGravityGeneratorSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			_gravityGeneratorSphere.Enabled = settings.Enabled;
			_gravityGeneratorSphere.Radius = settings.FieldSize;
			_gravityGeneratorSphere.GravityAcceleration = settings.FieldStrength;
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}
}
