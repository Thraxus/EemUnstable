using ProtoBuf;

namespace Eem.Thraxus.Factions.DataTypes
{
	[ProtoContract]
	public struct FactionRelation
	{
		[ProtoMember(1)]
		public readonly long FactionId;
		
		[ProtoMember(2)]
		public readonly string FactionTag;
		
		[ProtoMember(3)]
		public readonly int Reputation;

		public FactionRelation(long factionId, string factionTag, int reputation)
		{
			FactionId = factionId;
			FactionTag = factionTag;
			Reputation = reputation;
		}

		public override string ToString()
		{
			return $"Tag: {FactionTag} ID: {FactionId} Rep: {Reputation}";
		}
	}
}
