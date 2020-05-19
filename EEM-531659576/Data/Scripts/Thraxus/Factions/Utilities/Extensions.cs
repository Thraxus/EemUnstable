using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.Utilities
{
	public static class Extensions
	{
		public static bool IsPlayerPirate(this IMyFaction faction)
		{
			try
			{
				return GeneralSettings.PlayerFactionExclusionList.Any(x => faction.Description.StartsWith(x));
			}
			catch (Exception e)
			{
				StaticLog.WriteToLog("CheckPiratePlayerOptIn", $"Exception! {e}", LogType.Exception);
				return false;
			}
		}
		
		public static bool IsFactionMemberOnline(this MyFactionMember member)
		{
			List<IMyPlayer> players = new List<IMyPlayer>();
			MyAPIGateway.Multiplayer.Players.GetPlayers(players);
			return players.Any(x => x.IdentityId == member.PlayerId);
		}
	}
}
