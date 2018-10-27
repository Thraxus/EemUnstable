﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EemRdx.Factions.Messages
{
	public static class Ucmf
	{
		public const string Tag = "UCMF";

	    public static readonly Func<string> FirstPeaceAccepted = () => FirstPeaceAcceptedMessages.ElementAt(Factions.FactionsRandom.Next(FirstPeaceAcceptedMessages.Count));

	    private static readonly List<string> FirstPeaceAcceptedMessages = new List<string>()
	    {
	        $"{Tag} protects civil interests, especially trade.  Do not cross us."
	    };


        public static readonly Func<string> PeaceAccepted = () => DefaultDialogs.PeaceAcceptedMessages.ElementAt(Factions.FactionsRandom.Next(DefaultDialogs.PeaceAcceptedMessages.Count));

	    private static readonly List<string> PeaceAcceptedMessages = new List<string>()
	    {
	        $"{Tag} Peace Accepted Placeholder"
	    };

	    public static readonly Func<string> PeaceConsidered = () => DefaultDialogs.PeaceConsideredMessages.ElementAt(Factions.FactionsRandom.Next(DefaultDialogs.PeaceConsideredMessages.Count));

	    private static readonly List<string> PeaceConsideredMessages = new List<string>()
	    {
	        $"{Tag} Peace Considered Placeholder"
	    };

	    public static readonly Func<string> PeaceProposed = () => DefaultDialogs.PeaceProposedMessages.ElementAt(Factions.FactionsRandom.Next(DefaultDialogs.PeaceProposedMessages.Count));

	    private static readonly List<string> PeaceProposedMessages = new List<string>()
	    {
	        $"{Tag} Peace Proposed Placeholder"
	    };

	    public static readonly Func<string> PeaceRejected = () => DefaultDialogs.PeaceRejectedMessages.ElementAt(Factions.FactionsRandom.Next(DefaultDialogs.PeaceRejectedMessages.Count));

	    private static readonly List<string> PeaceRejectedMessages = new List<string>()
	    {
	        $"{Tag} Peace Rejected Placeholder"
	    };

	    public static readonly Func<string> WarDeclared = () => DefaultDialogs.WarDeclaredMessages.ElementAt(Factions.FactionsRandom.Next(DefaultDialogs.WarDeclaredMessages.Count));

	    private static readonly List<string> WarDeclaredMessages = new List<string>()
	    {
	        $"{Tag} War Declared Placeholder"
	    };

	    public static readonly Func<string> WarReceived = () => DefaultDialogs.WarReceiveddMessages.ElementAt(Factions.FactionsRandom.Next(DefaultDialogs.WarReceiveddMessages.Count));

	    private static readonly List<string> WarReceiveddMessages = new List<string>()
	    {
	        $"{Tag} War Received Placeholder"
	    };
    }
}
