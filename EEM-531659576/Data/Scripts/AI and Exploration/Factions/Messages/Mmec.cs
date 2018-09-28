using System;
using System.Collections.Generic;
using System.Linq;

namespace EemRdx.Factions.Messages
{
	internal static class Mmec
	{
		public const string Tag = "MMEC";

	    public static readonly Func<string> PeaceAccepted = () => PeaceAcceptedMessages.ElementAt(Factions.FactionsRandom.Next(PeaceAcceptedMessages.Count));

	    private static readonly List<string> PeaceAcceptedMessages = new List<string>()
	    {
	        "Cheater..."
	    };

	    public static readonly Func<string> PeaceConsidered = () => PeaceConsideredMessages.ElementAt(Factions.FactionsRandom.Next(PeaceConsideredMessages.Count));

	    private static readonly List<string> PeaceConsideredMessages = new List<string>()
	    {
	        "After careful consideration... no."
	    };

	    public static readonly Func<string> PeaceProposed = () => PeaceProposedMessages.ElementAt(Factions.FactionsRandom.Next(PeaceProposedMessages.Count));

	    private static readonly List<string> PeaceProposedMessages = new List<string>()
	    {
	        "How the...?"
	    };

        public static readonly Func<string> PeaceRejected = () => PeaceRejectedMessages.ElementAt(Factions.FactionsRandom.Next(PeaceRejectedMessages.Count));

	    private static readonly List<string> PeaceRejectedMessages = new List<string>()
	    {
	        $"The {Tag} is not interested in peace.  You leave us alone, and we'll leave you alone."
        };

	    public static readonly Func<string> WarDeclared = () => WarDeclaredMessages.ElementAt(Factions.FactionsRandom.Next(WarDeclaredMessages.Count));

	    private static readonly List<string> WarDeclaredMessages = new List<string>()
	    {
	        "Oh, another war!  Wait a second... Weren't we already...?  DOCTOR!"
	    };

	    public static readonly Func<string> WarReceived = () => WarReceivedMessages.ElementAt(Factions.FactionsRandom.Next(WarReceivedMessages.Count));

	    private static readonly List<string> WarReceivedMessages = new List<string>()
	    {
	        $"Oh no, war.  What ever shall we do..."
	    };
    }
}
