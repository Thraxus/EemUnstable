using System;
using System.Xml;
using System.Xml.Serialization;
using ProtoBuf;

namespace Eem.Thraxus.Factions.DataTypes
{
	//[Serializable]
	[ProtoContract]
	public struct FactionRelation
	{
		//[XmlElement("FactionId")]
		[ProtoMember(1)]
		public readonly long FactionId;
		//[XmlElement("FactionTag")]
		[ProtoMember(2)]
		public readonly string FactionTag;
		//[XmlElement("Reputation")]
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
