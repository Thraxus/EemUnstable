using Sandbox.ModAPI;

namespace Eem.Thraxus.Factions.Utilities
{
	public static class StaticMethods
	{
		/// <summary>
		/// Utility method. Checks whether an identity is a bot or not
		/// </summary>
		/// <param name="identityId"></param>
		/// <returns>Returns true if the identity is not a bot</returns>
		public static bool ValidPlayer(long identityId)
		{
			return MyAPIGateway.Players.TryGetSteamId(identityId) != 0;
		}

	}
}
