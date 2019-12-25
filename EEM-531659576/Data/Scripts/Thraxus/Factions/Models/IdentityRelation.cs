using Eem.Thraxus.Common.Utilities.Tools.Networking;
using Eem.Thraxus.Factions.BaseClasses;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.Models
{
	public class IdentityRelation : RepControl
	{
		public readonly IMyIdentity FromIdentity;

		public IdentityRelation(IMyIdentity fromIdentity)
		{
			FromIdentity = fromIdentity;
			FromRelationId = FromIdentity.IdentityId;
		}

		protected override int GetReputation(long factionId)
		{
			return MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(FromIdentity.IdentityId, factionId);
		}

		protected override void SetReputation(long id, int rep)
		{
			if (!RelationExists(id)) return;
			MyAPIGateway.Session.Factions.SetReputation(FromIdentity.IdentityId, id, rep);
		}
	}
}
