using VRage.Game.VisualScripting.ScriptBuilder.Nodes;

namespace Eem.Thraxus.Common.DataTypes
{
	public struct FactionRelationship
	{
		public readonly long FactionId;
		public readonly int Reputation;
		public readonly string Tag;

		public FactionRelationship(long id, int rep, string tag)
		{
			FactionId = id;
			Reputation = rep;
			Tag = tag;
		}
	}
}
