using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Eem.Thraxus.Factions.DataTypes
{
	//[ProtoContract]
	//public struct FactionRelationSaveState
	//{
	//	[ProtoMember(1)]
	//	public readonly long FactionId;

	//	[ProtoMember(2)]
	//	public readonly string FactionTag;

	//	[ProtoMember(3)]
	//	public readonly int Reputation;

	//	public FactionRelationSaveState(long factionId, string factionTag, int reputation)
	//	{
	//		FactionId = factionId;
	//		FactionTag = factionTag;
	//		Reputation = reputation;
	//	}

	//	public override string ToString()
	//	{
	//		return $"Tag: {FactionTag} ID: {FactionId} Rep: {Reputation}";
	//	}
	//}

	public struct PendingWar
	{
		public readonly long IdentityId;
		public readonly long Against;

		public PendingWar(long identityId, long against)
		{
			IdentityId = identityId;
			Against = against;
		}

		public override string ToString()
		{
			return $"{IdentityId} | {Against}";
		}
	}

	[ProtoContract]
	public struct SaveData
	{
		[ProtoMember(1)] public readonly HashSet<RelationSave> FactionSave;
		[ProtoMember(2)] public readonly HashSet<RelationSave> IdentitySave;

		public SaveData(HashSet<RelationSave> relationSave, HashSet<RelationSave> identitySave)
		{
			FactionSave = relationSave;
			IdentitySave = identitySave;
		}

		public bool IsEmpty => (FactionSave == null && IdentitySave == null);

		public override string ToString()
		{
			return $"FactionSave.Count: {FactionSave?.Count} | IdentitySave.Count: {IdentitySave?.Count}";
		}
	}

	[ProtoContract]
	public struct RelationSave
	{
		[ProtoMember(1)] public readonly long FromId;
		[ProtoMember(2)] public readonly HashSet<Relation> ToFactionRelations;


		public RelationSave(long fromId, HashSet<Relation> toFactionRelations)
		{
			FromId = fromId;
			ToFactionRelations = toFactionRelations;
		}

		public override string ToString()
		{
			return $"FromId: {FromId} | ToFactionIds Count: {ToFactionRelations.Count}";
		}

		public string ToStringExtended()
		{
			StringBuilder returnString = new StringBuilder();
			returnString.Append("\n");
			foreach (Relation relation in ToFactionRelations)
			{
				returnString.Append($"FromId: {FromId} | {relation}\n");
			}
			return returnString.ToString();
		}
	}

	[ProtoContract]
	public struct Relation
	{
		[ProtoMember(2)] public readonly long FactionId;
		[ProtoMember(3)] public readonly int Rep;

		public Relation(long factionId, int rep)
		{
			FactionId = factionId;
			Rep = rep;
		}

		public override string ToString()
		{
			return $"FactionId: {FactionId} | Rep: {Rep}";
		}
	}

	//[ProtoContract]
	//public struct FullFactionRelationSave
	//{
	//	[ProtoMember(1)] public readonly List<FactionRelationSave> FactionRelationSaves;

	//	public FullFactionRelationSave(List<FactionRelationSave> factionRelationSaves)
	//	{
	//		FactionRelationSaves = factionRelationSaves;
	//	}

	//	public override string ToString()
	//	{
	//		return $"FullFactionRelationSave Size: {FactionRelationSaves.Count}";
	//	}
	//}

	//[ProtoContract]
	//public struct FactionRelationSave
	//{
	//	[ProtoMember(1)] public readonly long FromFactionId;
	//	[ProtoMember(2)] public readonly long ToFactionId;
	//	[ProtoMember(3)] public readonly int Rep;

	//	public FactionRelationSave(long fromFactionId, long toFactionId, int rep)
	//	{
	//		FromFactionId = fromFactionId;
	//		ToFactionId = toFactionId;
	//		Rep = rep;
	//		ToFactionId = toFactionId;
	//	}

	//	public override string ToString()
	//	{
	//		return $"FromFactionId: {FromFactionId} | ToFactionId: {ToFactionId} | Rep: {Rep}";
	//	}
	//}

	//[ProtoContract]
	//public class IdentityRelationSave
	//{
	//	[ProtoMember(1)] public readonly long FromIdentityId;
	//	[ProtoMember(2)] public readonly HashSet<long> ToFactionIds;

	//	public IdentityRelationSave(long fromIdentity, HashSet<long> toFactions)
	//	{
	//		FromIdentityId = fromIdentity;
	//		ToFactionIds = toFactions;
	//	}

	//	public override string ToString()
	//	{
	//		return $"FromIdentityId: {FromIdentityId} | ToFactionIds Count: {ToFactionIds.Count}";
	//	}
	//}

}
