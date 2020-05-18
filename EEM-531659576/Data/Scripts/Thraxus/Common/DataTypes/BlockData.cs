using Sandbox.Game.Entities;
using VRage.Game.VisualScripting.ScriptBuilder.Nodes;

namespace Eem.Thraxus.Common.DataTypes
{
	public struct BlockData 
	{
		public int Threat;
		public int Value;
		public bool IsHeavyArmor;
		public bool IsDefenseShields;
		public bool IsEnergyShields;
		public bool IsBars;

		public override string ToString()
		{
			return $"{Threat} | {Value}";
		}
	}
}
