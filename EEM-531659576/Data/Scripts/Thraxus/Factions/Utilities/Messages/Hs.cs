using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Factions.Utilities.Messages;

namespace Eem.Thraxus.Factions.Messages
{
	public static class Hs
	{
		public const string Tag = "HS";

	    public static readonly Func<string> FirstPeaceAccepted = () => FirstPeaceAcceptedMessages.ElementAt(Helpers.Constants.Random.Next(FirstPeaceAcceptedMessages.Count));

	    private static readonly List<string> FirstPeaceAcceptedMessages = new List<string>()
	    {
	        $"{Tag} specializes in high tech security systems.  Just don't get on our bad side."
	    };

        public static readonly Func<string> PeaceAccepted = () => DefaultDialogs.PeaceAcceptedMessages.ElementAt(Helpers.Constants.Random.Next(DefaultDialogs.PeaceAcceptedMessages.Count));

	    private static readonly List<string> PeaceAcceptedMessages = new List<string>()
	    {
	        $"{Tag} Peace Accepted Placeholder"
	    };

	    public static readonly Func<string> PeaceConsidered = () => DefaultDialogs.PeaceConsideredMessages.ElementAt(Helpers.Constants.Random.Next(DefaultDialogs.PeaceConsideredMessages.Count));

	    private static readonly List<string> PeaceConsideredMessages = new List<string>()
	    {
	        $"{Tag} Peace Considered Placeholder"
	    };

	    public static readonly Func<string> PeaceProposed = () => DefaultDialogs.PeaceProposedMessages.ElementAt(Helpers.Constants.Random.Next(DefaultDialogs.PeaceProposedMessages.Count));

	    private static readonly List<string> PeaceProposedMessages = new List<string>()
	    {
	        $"{Tag} Peace Proposed Placeholder"
	    };

	    public static readonly Func<string> PeaceRejected = () => DefaultDialogs.PeaceRejectedMessages.ElementAt(Helpers.Constants.Random.Next(DefaultDialogs.PeaceRejectedMessages.Count));

	    private static readonly List<string> PeaceRejectedMessages = new List<string>()
	    {
	        $"{Tag} Peace Rejected Placeholder"
	    };

	    public static readonly Func<string> WarDeclared = () => DefaultDialogs.WarDeclaredMessages.ElementAt(Helpers.Constants.Random.Next(DefaultDialogs.WarDeclaredMessages.Count));

	    private static readonly List<string> WarDeclaredMessages = new List<string>()
	    {
	        $"{Tag} War Declared Placeholder"
	    };

	    public static readonly Func<string> WarReceived = () => DefaultDialogs.WarReceiveddMessages.ElementAt(Helpers.Constants.Random.Next(DefaultDialogs.WarReceiveddMessages.Count));

	    private static readonly List<string> WarReceiveddMessages = new List<string>()
	    {
	        $"{Tag} War Received Placeholder"
	    };
    }
}
