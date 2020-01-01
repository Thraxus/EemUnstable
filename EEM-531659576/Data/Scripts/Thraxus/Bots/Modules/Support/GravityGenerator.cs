using Sandbox.Game.Entities;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.ObjectBuilders.ComponentSystem;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class GravityGenerator
	{
		private readonly IMyGravityGenerator _gravityGenerator;
		private readonly long _ownerId;
		private readonly GravityGeneratorSettings _wartimeSettings;
		private readonly GravityGeneratorSettings _peacetimeSettings;

		public GravityGenerator(IMyGravityGenerator gravityGenerator, GravityGeneratorSettings wartimeSettings, GravityGeneratorSettings peacetimeSettings)
		{
			_gravityGenerator = gravityGenerator;
			_ownerId = gravityGenerator.OwnerId;
			_wartimeSettings = wartimeSettings;
			_peacetimeSettings = peacetimeSettings;
		}

		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_gravityGenerator.InScene || _gravityGenerator.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);
			
			return true;
		}

		private void Execute(AlertSetting alertSetting)
		{

			GravityGeneratorSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			_gravityGenerator.Enabled = settings.Enabled;
			_gravityGenerator.Enabled = settings.Enabled;
			_gravityGenerator.FieldSize = settings.FieldSize;
			_gravityGenerator.GravityAcceleration = settings.FieldStrength;
			if (_peacetimeSettings.FieldStrength >= 0) _gravityGenerator.GravityAcceleration *= -1;
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}

	
}
