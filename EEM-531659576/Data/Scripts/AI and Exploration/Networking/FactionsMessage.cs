using EemRdx.Helpers;
using EemRdx.Models;
using ProtoBuf;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable ExplicitCallerInfoArgument

namespace EemRdx.Networking
{
	[ProtoContract]
	public class FactionsChangeMessage : MessageBase
	{
		[ProtoMember(1)] private readonly string _messagePrefix;

		[ProtoMember(2)] private readonly long _leftFaction;

		[ProtoMember(3)] private readonly long _rightFaction;


		public FactionsChangeMessage(string messagePrefix, long leftFaction, long rightFaction)
		{
			_messagePrefix = messagePrefix;
			_leftFaction = leftFaction;
			_rightFaction = rightFaction;
		}

		public FactionsChangeMessage() { }
		
		public override void HandleServer()
		{
			// unused for FactionsChangeMessage
		}

		public override void HandleClient()
		{
			switch (_messagePrefix)
			{
				case (Constants.DeclareWarMessagePrefix):
					MyAPIGateway.Session.Factions.DeclareWar(_leftFaction, _rightFaction);
					break;
				case (Constants.DeclarePeaceMessagePrefix):
					MyAPIGateway.Session.Factions.SendPeaceRequest(_leftFaction, _rightFaction);
					break;
				case (Constants.AcceptPeaceMessagePrefix):
					MyAPIGateway.Session.Factions.AcceptPeace(_leftFaction, _rightFaction);
					break;
				case (Constants.InitFactionsMessagePrefix):
					Messaging.ShowLocalNotification($"InitFactionsMessagePrefix - {AiSessionCore.IsServer}", 20000);
					foreach (IMyFaction leftFaction in Factions.LawfulFactions)
					{
						foreach (IMyFaction rightFaction in Factions.LawfulFactions)
							if (leftFaction != rightFaction)
								if (!leftFaction.IsPeacefulTo(rightFaction))
								{
									Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.DeclarePeaceMessagePrefix, leftFaction.FactionId, rightFaction.FactionId));
									Messaging.SendMessageToClients(new FactionsChangeMessage(Constants.AcceptPeaceMessagePrefix, rightFaction.FactionId, leftFaction.FactionId));
								}
					}
					break;
				default:
					return;
			}
		}
	}
}
