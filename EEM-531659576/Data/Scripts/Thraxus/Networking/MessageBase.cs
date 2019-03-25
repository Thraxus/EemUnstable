using Eem.Thraxus.Factions.Networking;
using ProtoBuf;
using Sandbox.ModAPI;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable ExplicitCallerInfoArgument

namespace Eem.Thraxus.Networking
{
	[ProtoInclude(10, typeof(FactionsChangeMessage))]
	[ProtoContract]
	public abstract class MessageBase
	{
		[ProtoMember(1)] protected readonly ulong SenderId;

		protected MessageBase()
		{
			SenderId = MyAPIGateway.Multiplayer.MyId;
		}

		public abstract void HandleServer();

		public abstract void HandleClient();
	}
}
