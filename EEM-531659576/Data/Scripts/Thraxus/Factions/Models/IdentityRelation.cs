using Eem.Thraxus.Factions.BaseClasses;
using Eem.Thraxus.Factions.DataTypes;
using Sandbox.ModAPI;
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
			RelationType = RelationType.Identity;
		}

		public override int GetSeReputation(long factionId)
		{
			return MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(FromIdentity.IdentityId, factionId);
		}

		protected override void SetSeReputation(long id, int rep)
		{
			//StaticLog.WriteToLog("SetReputation", $"Checkpoint Entered...", LogType.General);
			if (!RelationExists(id)) return;
			//StaticLog.WriteToLog("SetReputation", $"Type: {RelationType} - Rep changed between {FromRelationId} and {id} to {rep}", LogType.General);
			MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(FromIdentity.IdentityId, id, rep);
		}
	}
}
