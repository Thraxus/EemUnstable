using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.Utilities
{
	public static class StaticMethods
	{
		internal static IMyFaction GetFactionById(this long factionId)
		{
			return MyAPIGateway.Session.Factions.TryGetFactionById(factionId);
		}

		private static bool IsPlayer(this long faction)
		{
			return !MyAPIGateway.Session.Factions.TryGetFactionById(faction).IsEveryoneNpc();
		}

		private static bool IsNpc(this long faction)
		{
			return MyAPIGateway.Session.Factions.TryGetFactionById(faction).IsEveryoneNpc();
		}

		private static bool ValidateFactions(IMyFaction leftFaction, IMyFaction rightFaction)
		{
			return (leftFaction == null || rightFaction == null);
		}

		//private static bool IsPirate(this long faction)
		//{
		//	return PirateFactionDictionary.ContainsKey(faction);
		//}

		//private static bool IsLawful(this long faction)
		//{
		//	return LawfulFactionDictionary.ContainsKey(faction);
		//}

		//private static bool IsEnforcement(this long faction)
		//{
		//	return EnforcementFactionDictionary.ContainsKey(faction);
		//}

		//private static bool ValidateNonPirateFactions(IMyFaction leftFaction, IMyFaction rightFaction)
		//{
		//	if (leftFaction == null || rightFaction == null) return false;
		//	return !PirateFactionDictionary.ContainsKey(leftFaction.FactionId) && !PirateFactionDictionary.ContainsKey(rightFaction.FactionId);
		//}

		//private static bool IsPirateFaction(this IMyFaction faction)
		//{
		//	return faction != null && PirateFactionDictionary.ContainsKey(faction.FactionId);
		//}



		//private static bool IsFactionErrant(this long playerFactionId, long npcFactionId)
		//{
		//	return BadRelations.IndexOf(new BadRelation(playerFactionId, npcFactionId)) != -1;
		//}

		//private static bool IsFactionPenitent(this long playerFactionId, long npcFactionId)
		//{
		//	return PenitentFactions.IndexOf(new BadRelation(playerFactionId, npcFactionId)) != -1;
		//}
	}
}
