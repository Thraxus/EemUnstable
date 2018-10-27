using System;
using System.Collections.Generic;
using System.Linq;

namespace EemRdx.Factions.Messages
{
    public static class Sprt
    {
        public const string Tag = "SPRT";

        public static readonly Func<string> FirstPeaceAccepted = () => FirstPeaceAcceptedMessages.ElementAt(Factions.FactionsRandom.Next(FirstPeaceAcceptedMessages.Count));

        private static readonly List<string> FirstPeaceAcceptedMessages = new List<string>()
        {
            $"{Tag} is a pirate group.  So... no?"
        };


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
            "YOU WHELPS!  Pirates can't be bargained with!  Thanks for letting us know where you're at though...",
            "You're either mad, or a fool.  Who exactly do you think you're speaking to?",
            "Yarrrr!  Haha, no really...  Yarrrr a fool if you think we'd ever talk terms with a group like yours.",
            "If I give you a nice big straw... will you go suck the fun out of someone else’s day?  WE ARE P-I-R-A-T-E-S",
            "This number is out of service.",
            "How about this instead... Lay down your arms, and we'll come get them."
        };

        public static readonly Func<string> WarDeclared = () => WarDeclaredMessages.ElementAt(Factions.FactionsRandom.Next(WarDeclaredMessages.Count));

        private static readonly List<string> WarDeclaredMessages = new List<string>()
        {
            "Oh, another war!  Wait a second... Weren't we already...?  DOCTOR!"
        };

        public static readonly Func<string> WarReceived = () => WarReceivedMessages.ElementAt(Factions.FactionsRandom.Next(WarReceivedMessages.Count));

        private static readonly List<string> WarReceivedMessages = new List<string>()
        {
            $"Oh darn, another war.  MORE LOOT!"
        };
    }
}
