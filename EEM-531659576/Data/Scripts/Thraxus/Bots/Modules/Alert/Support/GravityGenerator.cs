using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Common.Enums;
using SpaceEngineers.Game.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Modules.Alert.Support
{
	internal class GravityGenerator : ISetAlert
	{
		private readonly IMyGravityGenerator _gravityGenerator;
		private readonly long _ownerId;
		private readonly GravityGeneratorSettings _wartimeSettings;
		private readonly GravityGeneratorSettings _peacetimeSettings;

		private struct GravityGeneratorSettings
		{
			public readonly bool Enabled;
			public readonly Vector3 FieldSize;
			public readonly float FieldStrength;

			public GravityGeneratorSettings(bool enabled, Vector3 fieldSize, float fieldStrength)
			{
				Enabled = enabled;
				FieldSize = fieldSize;
				FieldStrength = fieldStrength;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled} | {FieldSize} | {FieldStrength}";
			}
		}

		public GravityGenerator(IMyGravityGenerator gravityGenerator)
		{
			_gravityGenerator = gravityGenerator;
			_ownerId = gravityGenerator.OwnerId;
			_peacetimeSettings = new GravityGeneratorSettings(_gravityGenerator.Enabled, _gravityGenerator.FieldSize, _gravityGenerator.GravityAcceleration);
			_wartimeSettings = new GravityGeneratorSettings(true, new Vector3(150, 150, 150), 9.81f * (_gravityGenerator.GravityAcceleration >= 0 ? -1 : 1));
		}

		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_gravityGenerator.InScene || _gravityGenerator.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);
			
			return true;
		}

		public void Close()
		{

		}

		private void Execute(AlertSetting alertSetting)
		{

			GravityGeneratorSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;
			_gravityGenerator.Enabled = settings.Enabled;
			_gravityGenerator.FieldSize = settings.FieldSize;
			_gravityGenerator.GravityAcceleration = settings.FieldStrength;
			//if (_peacetimeSettings.FieldStrength >= 0) _gravityGenerator.GravityAcceleration *= -1;
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}

	
}
