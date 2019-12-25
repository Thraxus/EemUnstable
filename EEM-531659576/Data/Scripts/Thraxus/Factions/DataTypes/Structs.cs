using System.Collections.Generic;
using ProtoBuf;
using VRage.Game.ModAPI;

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
		[ProtoMember(1)] public readonly List<FactionRelationSave> RelationSave;
		[ProtoMember(2)] public readonly List<IdentityRelationSave> IdentitySave;

		public SaveData(List<FactionRelationSave> relationSave, List<IdentityRelationSave> identitySave)
		{
			RelationSave = relationSave;
			IdentitySave = identitySave;
		}

		public bool IsEmpty => (RelationSave == null || IdentitySave == null);
	}

	[ProtoContract]
	public struct FullFactionRelationSave
	{
		[ProtoMember(1)] public readonly List<FactionRelationSave> FactionRelationSaves;

		public FullFactionRelationSave(List<FactionRelationSave> factionRelationSaves)
		{
			FactionRelationSaves = factionRelationSaves;
		}

		public override string ToString()
		{
			return $"FullFactionRelationSave Size: {FactionRelationSaves.Count}";
		}
	}

	[ProtoContract]
	public struct FactionRelationSave
	{
		[ProtoMember(1)] public readonly long FromFactionId;
		[ProtoMember(2)] public readonly long ToFactionId;
		[ProtoMember(3)] public readonly int Rep;

		public FactionRelationSave(long fromFactionId, long toFactionId, int rep)
		{
			FromFactionId = fromFactionId;
			ToFactionId = toFactionId;
			Rep = rep;
			ToFactionId = toFactionId;
		}

		public override string ToString()
		{
			return $"FromFactionId: {FromFactionId} | ToFactionId: {ToFactionId} | Rep: {Rep}";
		}
	}

	[ProtoContract]
	public class IdentityRelationSave
	{
		[ProtoMember(1)] public readonly long FromIdentityId;
		[ProtoMember(2)] public readonly HashSet<long> ToFactionIds;

		public IdentityRelationSave(long fromIdentity, HashSet<long> toFactions)
		{
			FromIdentityId = fromIdentity;
			ToFactionIds = toFactions;
		}

		public override string ToString()
		{
			return $"FromIdentityId: {FromIdentityId} | ToFactionIds Count: {ToFactionIds.Count}";
		}
	}

}
