using Eem.Thraxus.Common.Enums;

namespace Eem.Thraxus.Bots.SessionComps.Models
{
	public struct BlockData 
	{
		public int Threat;
		public int Value;
		public bool IsHeavyArmor;
		public bool IsDefenseShields;
		public bool IsEnergyShields;
		public bool IsBars;
		public TargetSystemType Type;

		public override string ToString()
		{
			return $"{Threat} | {Value} | {Type}";
		}
	}
}
