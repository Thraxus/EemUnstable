namespace Eem.Thraxus.Common.DataTypes
{
	public struct ProcessedCube
	{
		public bool IsHeavyArmor;
		public bool IsBars;
		public bool IsDefenseShield;
		public bool IsEnergyShield;
		public bool IsModded;

		public BlockData BlockValue;
		
		public override string ToString()
		{
			return $"{BlockValue}";
		}
	}
}