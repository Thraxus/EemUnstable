using System;
using System.Collections.Generic;
using System.Linq;

namespace EemRdx.Factions.Messages
{
	internal static class UnknownFaction
	{
		public static readonly Func<string> PeaceNotInterested = () => PeaceNotInterestedMessages.ElementAt(Factions.FactionsRandom.Next(PeaceNotInterestedMessages.Count));

		private static HashSet<string> PeaceNotInterestedMessages { get; } = new HashSet<string>()
		{
			"Why would you think we'd be interested in peace with someone like you?  Heh, go find another patsy to play your tricks on.",
			"Unknown Message Two",
			"Unknown Message Three",
			"Unknown Message Four",
			"Unknown Message Five",
			"Unknown Message Six"
		};
	}
}
