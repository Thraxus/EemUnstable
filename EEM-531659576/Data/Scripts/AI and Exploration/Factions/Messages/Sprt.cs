using System;
using System.Collections.Generic;
using System.Linq;

namespace EemRdx.Factions.Messages
{
	public static class Sprt
	{
		public const string Tag = "SPRT";

		public static readonly Func<string> PeaceNotInterested = () => PeaceNotInterestedMessages.ElementAt(Factions.FactionsRandom.Next(PeaceNotInterestedMessages.Count));

		private static HashSet<string> PeaceNotInterestedMessages { get; } = new HashSet<string>()
		{
			"YOU WHELPS!  Pirates can't be bargained with!  Thanks for letting us know where you're at though...",
			"You're either mad, or a fool.  Who exactly do you think you're speaking to?",
			"Yarrrr....  Haha, no really... Yarrrrr a fool if you think we'd ever talk terms with a group like yours.",
			"If I give you a nice big straw will you go suck the fun out of someone else’s day.  WE'RE P-I-R-A-T-E-S",
			"This number is out of service.",
			"How about this instead... You lay down arms, and we'll come get them."
		};
	}
}
