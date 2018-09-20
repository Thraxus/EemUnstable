using System;
using System.Collections.Generic;
using System.Linq;

namespace EemRdx.Factions.Messages
{
	internal static class Mmec
	{
		public const string Tag = "MMEC";

		public static readonly Func<string> PeaceNotInterested = () => PeaceNotInterestedMessages.ElementAt(Factions.FactionsRandom.Next(PeaceNotInterestedMessages.Count));

		private static HashSet<string> PeaceNotInterestedMessages { get; } = new HashSet<string>()
		{
			"The MMEC is not interested in peace.  You leave us alone, and we'll leave you alone.",
			"MMEC Message Two",
			"MMEC Message Three",
			"MMEC Message Four",
			"MMEC Message Five",
			"MMEC Message Six"
		};
	}
}
