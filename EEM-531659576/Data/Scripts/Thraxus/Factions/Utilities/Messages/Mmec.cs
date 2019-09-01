using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.Settings;

namespace Eem.Thraxus.Factions.Utilities.Messages
{
	internal static class Mmec
	{
		public const string Tag = "MMEC";

	    public static readonly Func<string> FirstPeaceAccepted = () => FirstPeaceAcceptedMessages.ElementAt(GeneralSettings.Random.Next(FirstPeaceAcceptedMessages.Count));

	    private static readonly List<string> FirstPeaceAcceptedMessages = new List<string>()
	    {
	        $"{Tag} doesn't believe in peace!  Shoo!"
	    };

        public static readonly Func<string> PeaceAccepted = () => PeaceAcceptedMessages.ElementAt(GeneralSettings.Random.Next(PeaceAcceptedMessages.Count));

	    private static readonly List<string> PeaceAcceptedMessages = new List<string>()
	    {
	        "Cheater..."
	    };

	    public static readonly Func<string> PeaceConsidered = () => PeaceConsideredMessages.ElementAt(GeneralSettings.Random.Next(PeaceConsideredMessages.Count));

	    private static readonly List<string> PeaceConsideredMessages = new List<string>()
	    {
	        "After careful consideration... no."
	    };

	    public static readonly Func<string> PeaceProposed = () => PeaceProposedMessages.ElementAt(GeneralSettings.Random.Next(PeaceProposedMessages.Count));

	    private static readonly List<string> PeaceProposedMessages = new List<string>()
	    {
	        "How the...?"
	    };

        public static readonly Func<string> PeaceRejected = () => PeaceRejectedMessages.ElementAt(GeneralSettings.Random.Next(PeaceRejectedMessages.Count));

	    private static readonly List<string> PeaceRejectedMessages = new List<string>()
	    {
	        $"The {Tag} is not interested in peace.  You leave us alone, and we'll leave you alone."
        };

	    public static readonly Func<string> WarDeclared = () => WarDeclaredMessages.ElementAt(GeneralSettings.Random.Next(WarDeclaredMessages.Count));

	    private static readonly List<string> WarDeclaredMessages = new List<string>()
	    {
	        "Oh, another war!  Wait a second... Weren't we already...?  DOCTOR!"
	    };

	    public static readonly Func<string> WarReceived = () => WarReceivedMessages.ElementAt(GeneralSettings.Random.Next(WarReceivedMessages.Count));

	    private static readonly List<string> WarReceivedMessages = new List<string>()
	    {
	        $"Oh no, war.  What ever shall we do..."
	    };
    }
}
