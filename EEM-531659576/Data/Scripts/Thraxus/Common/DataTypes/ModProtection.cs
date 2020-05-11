namespace Eem.Thraxus.Common.DataTypes
{
	public struct ModProtection
	{
		public readonly bool Bars;
		public readonly bool EnergyShields;
		public readonly bool DefenseShields;

		public ModProtection(bool bars, bool energyShields, bool defenseShields)
		{
			Bars = bars;
			EnergyShields = energyShields;
			DefenseShields = defenseShields;
		}
	}
}
