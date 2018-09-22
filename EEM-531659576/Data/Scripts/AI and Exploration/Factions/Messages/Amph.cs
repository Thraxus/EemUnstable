using System;
using System.Collections.Generic;
using System.Linq;

namespace EemRdx.Factions.Messages
{
	public static class Amph
	{
		public const string Tag = "AMPH";

		public static readonly Func<string> PeaceAccepted = () => PeaceAcceptedMessages.ElementAt(Factions.FactionsRandom.Next(PeaceAcceptedMessages.Count));

		private static readonly List<string> PeaceAcceptedMessages = new List<string>()
		{
			$"{Tag} Peace Accepted Placeholder"
		};

		public static readonly Func<string> PeaceConsidered = () => PeaceConsideredMessages.ElementAt(Factions.FactionsRandom.Next(PeaceConsideredMessages.Count));

		private static readonly List<string> PeaceConsideredMessages = new List<string>()
		{
			$"{Tag} Peace Considered Placeholder"
		};

		public static readonly Func<string> PeaceProposed = () => PeaceProposedMessages.ElementAt(Factions.FactionsRandom.Next(PeaceProposedMessages.Count));

		private static readonly List<string> PeaceProposedMessages = new List<string>()
		{
			$"{Tag} Peace Proposed Placeholder"
		};

		public static readonly Func<string> PeaceRejected = () => PeaceRejectedMessages.ElementAt(Factions.FactionsRandom.Next(PeaceRejectedMessages.Count));

		private static readonly List<string> PeaceRejectedMessages = new List<string>()
		{
			$"{Tag} Peace Rejected Placeholder"
		};

		public static readonly Func<string> WarDeclared = () => WarDeclaredMessages.ElementAt(Factions.FactionsRandom.Next(WarDeclaredMessages.Count));

		private static readonly List<string> WarDeclaredMessages = new List<string>()
		{
			$"{Tag} War Declared Placeholder"
		};

		public static readonly Func<string> WarReceived = () => WarReceiveddMessages.ElementAt(Factions.FactionsRandom.Next(WarReceiveddMessages.Count));

		private static readonly List<string> WarReceiveddMessages = new List<string>()
		{
			$"{Tag} War Received Placeholder"
		};
	}
}
