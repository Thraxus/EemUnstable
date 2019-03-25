using System;
using System.Collections.Generic;
using System.Linq;

namespace Eem.Thraxus.Factions.Messages
{
    public static class Sprt
    {
        public const string Tag = "SPRT";

        public static readonly Func<string> FirstPeaceAccepted = () => FirstPeaceAcceptedMessages.ElementAt(Helpers.Constants.Random.Next(FirstPeaceAcceptedMessages.Count));

        private static readonly List<string> FirstPeaceAcceptedMessages = new List<string>()
        {
            $"{Tag} is a pirate group.  So... no?"
        };


        public static readonly Func<string> PeaceAccepted = () => PeaceAcceptedMessages.ElementAt(Helpers.Constants.Random.Next(PeaceAcceptedMessages.Count));

        private static readonly List<string> PeaceAcceptedMessages = new List<string>()
        {
            "Cheater..."
        };

        public static readonly Func<string> PeaceConsidered = () => PeaceConsideredMessages.ElementAt(Helpers.Constants.Random.Next(PeaceConsideredMessages.Count));

        private static readonly List<string> PeaceConsideredMessages = new List<string>()
        {
            "After careful consideration... no."
        };

        public static readonly Func<string> PeaceProposed = () => PeaceProposedMessages.ElementAt(Helpers.Constants.Random.Next(PeaceProposedMessages.Count));

        private static readonly List<string> PeaceProposedMessages = new List<string>()
        {
            "How the...?"
        };

        public static readonly Func<string> PeaceRejected = () => PeaceRejectedMessages.ElementAt(Helpers.Constants.Random.Next(PeaceRejectedMessages.Count));

        private static readonly List<string> PeaceRejectedMessages = new List<string>()
        {
            "YOU WHELPS!  Pirates can't be bargained with!  Thanks for letting us know where you're at though...",
            "You're either mad, or a fool.  Who exactly do you think you're speaking to?",
            "Yarrrr!  Haha, no really...  Yarrrr a fool if you think we'd ever talk terms with a group like yours.",
            "If I give you a nice big straw... will you go suck the fun out of someone else’s day?  WE ARE P-I-R-A-T-E-S",
            "This number is out of service.",
            "How about this instead... Lay down your arms, and we'll come get them."
        };

        public static readonly Func<string> WarDeclared = () => WarDeclaredMessages.ElementAt(Helpers.Constants.Random.Next(WarDeclaredMessages.Count));

        private static readonly List<string> WarDeclaredMessages = new List<string>()
        {
            "Oh, another war!  Wait a second... Weren't we already...?  DOCTOR!"
        };

        public static readonly Func<string> WarReceived = () => WarReceivedMessages.ElementAt(Helpers.Constants.Random.Next(WarReceivedMessages.Count));

        private static readonly List<string> WarReceivedMessages = new List<string>()
        {
            $"Oh darn, another war.  MORE LOOT!"
        };
    }
}
