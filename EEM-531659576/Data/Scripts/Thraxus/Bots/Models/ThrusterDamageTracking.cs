namespace Eem.Thraxus.Bots.Models
{
	public class ThrusterDamageTracker
	{
		public double DamageTaken;
		public long OffendingEntity;

		public bool ThresholdReached => DamageTaken > 500;

		public ThrusterDamageTracker(long offendingEntity, double damageTaken)
		{
			OffendingEntity = offendingEntity;
			DamageTaken = damageTaken;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Offending Entity: {OffendingEntity} has done {DamageTaken} damage.";
		}
	}
}
