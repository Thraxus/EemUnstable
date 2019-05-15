namespace Eem.Thraxus.Common.DataTypes
{
	public struct ModProtection
	{
		public readonly bool Bars;
		public readonly bool EnergyShields;

		public ModProtection(bool bars, bool energyShields)
		{
			Bars = bars;
			EnergyShields = energyShields;
		}
	}
}
