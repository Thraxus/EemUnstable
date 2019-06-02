using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.Settings;

namespace Eem.Thraxus.Factions.Utilities.Messages
{
	public static class Civl
	{
		public const string Tag = "CIVL";

	    public static readonly Func<string> FirstPeaceAccepted = () => FirstPeaceAcceptedMessages.ElementAt(Settings.Random.Next(FirstPeaceAcceptedMessages.Count));

	    private static readonly List<string> FirstPeaceAcceptedMessages = new List<string>()
	    {
	        $"{Tag} is a collective of peaceful travelers and civil servants. Please, let us know if there is anything we can do for you."
	    };

        public static readonly Func<string> PeaceAccepted = () => DefaultDialogs.PeaceAcceptedMessages.ElementAt(Settings.Random.Next(DefaultDialogs.PeaceAcceptedMessages.Count));

		private static readonly List<string> PeaceAcceptedMessages = new List<string>()
		{
			$"{Tag} Peace Accepted Placeholder"
		};

		public static readonly Func<string> PeaceConsidered = () => DefaultDialogs.PeaceConsideredMessages.ElementAt(Settings.Random.Next(DefaultDialogs.PeaceConsideredMessages.Count));

		private static readonly List<string> PeaceConsideredMessages = new List<string>()
		{
			$"{Tag} Peace Considered Placeholder"
		};

		public static readonly Func<string> PeaceProposed = () => DefaultDialogs.PeaceProposedMessages.ElementAt(Settings.Random.Next(DefaultDialogs.PeaceProposedMessages.Count));

		private static readonly List<string> PeaceProposedMessages = new List<string>()
		{
			$"{Tag} Peace Proposed Placeholder"
		};

		public static readonly Func<string> PeaceRejected = () => DefaultDialogs.PeaceRejectedMessages.ElementAt(Settings.Random.Next(DefaultDialogs.PeaceRejectedMessages.Count));

		private static readonly List<string> PeaceRejectedMessages = new List<string>()
		{
			$"{Tag} Peace Rejected Placeholder"
		};

		public static readonly Func<string> WarDeclared = () => DefaultDialogs.WarDeclaredMessages.ElementAt(Settings.Random.Next(DefaultDialogs.WarDeclaredMessages.Count));

		private static readonly List<string> WarDeclaredMessages = new List<string>()
		{
			$"{Tag} War Declared Placeholder"
		};

		public static readonly Func<string> WarReceived = () => DefaultDialogs.WarReceiveddMessages.ElementAt(Settings.Random.Next(DefaultDialogs.WarReceiveddMessages.Count));

		private static readonly List<string> WarReceiveddMessages = new List<string>()
		{
			$"{Tag} War Received Placeholder"
		};
	}
}
