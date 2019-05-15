using Eem.Thraxus.Common.Settings;

namespace Eem.Thraxus.Common.DataTypes
{
	public class ThrusterDamageTracker
	{
		public double DamageTaken;
		private readonly long _offendingEntity;

		public bool ThresholdReached => DamageTaken > BotSettings.MaxAllowedThrusterDamage;

		public ThrusterDamageTracker(long offendingEntity, double damageTaken)
		{
			_offendingEntity = offendingEntity;
			DamageTaken = damageTaken;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Offending Entity: {_offendingEntity} has done {DamageTaken} damage.";
		}
	}
}
