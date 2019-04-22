using System;
using System.Collections.Generic;
using System.Linq;

namespace Eem.Thraxus.Factions.Utilities.Messages

// Thanks @Kreeg from ARMCO for some submissions to the below!
{
	internal static class DefaultDialogs
	{
		public const string DefaultTag = "TheUnknown";

		public static readonly Func<string> CatchAll = () => CatchAllMessages.ElementAt(Helpers.Constants.Random.Next(CatchAllMessages.Count));
        
		private static readonly List<string> CatchAllMessages = new List<string>()
		{
			"Qo' chay' naDev poHlIj Sov, 'ach scum naDevvo'!",
			"quSDaq ba’lu’’a’",
			"nuqDaq ‘oH puchpa’’e’",
			"Hab SoSlI’ Quch",
			"Heghlu’meH QaQ jajvam",
			"qaStaH nuq jay’",
			"Atrast tunsha. Totarnia amgetol tavash aeduc.",
			"Kalnath-par kallak, Kalnath-gat parthas"
		};

	    public const string CollectiveTag = "The Lawful Collective";

        public static readonly Func<string> CollectiveDisappointment = () => CollectiveDisappointmentMessages.ElementAt(Helpers.Constants.Random.Next(CollectiveDisappointmentMessages.Count));

        private static readonly List<string> CollectiveDisappointmentMessages = new List<string>()
		{
			"Yo ho ho, it's a pirate life fer ye!"
		};

	    public static readonly Func<string> CollectiveReprieve = () => CollectiveReprieveMessages.ElementAt(Helpers.Constants.Random.Next(CollectiveReprieveMessages.Count));

	    private static readonly List<string> CollectiveReprieveMessages = new List<string>()
	    {
	        "We've discussed your request, and will take it into individual consideration.",
            "Hrm... Maybe.",
            "Cindy!  Who writes these messages?  Wait, is this thing on?  Uh... Hi.  We're discussing your proposal now."
	    };

	    public static readonly Func<string> CollectiveWelcome = () => CollectiveWelcomeMessages.ElementAt(Helpers.Constants.Random.Next(CollectiveWelcomeMessages.Count));

	    private static readonly List<string> CollectiveWelcomeMessages = new List<string>()
	    {
	        "The Galactic Federation of United Factions is happy to welcome you into the fold, young traveler.",
            "Welcome to the party, pal!",
            "You're in luck!  We're having a sale on new factions this month.  First one is free!  Welcome!",
            "Four score and several eon's ago... Oh, hello."
	    };

        public static readonly List<string> PeaceAcceptedMessages = new List<string>()
	    {
	        "We decided to give you another shot.    ",
	        "I hope this time isn’t like the last.",
	        "You’re running out of chances.",
	        "We hope your time on the other side has taught you a lesson!",
	        "We support the SEPD.",
	        "I don’t know if you truly appreciate the opportunity we’re giving you here.",
	        "You’re on thin ice.",
	        "We’ve got our eyes on you.",
	        "For the record, I despise the entire notion, but it was agreed, and so it shall be.",
            "Don’t make us regret this decision.",
	        "I have no use for fools to breath, so do not waste this, your only, chance to live.",
            "Blessed is the day, You are redeemed of sin.",
            "Welcome to the Family."
        };


	    public static readonly List<string> PeaceConsideredMessages = new List<string>()
	    {
	        "We will consider your request.",
	        "What guarantees do we have that you’re not just in this for the credits?",
	        "Our council will meet on this matter shortly.  ",
	        "You’ve proven to be trouble in the past.  We’ll have to think about this.",
	        "If we give you another chance, what’s in it for us?",
	        "We’ll have to discuss your worth as a faction.",
	        "Given the circumstances, you’ll understand if this decision takes some time."
        };


	    public static readonly List<string> PeaceProposedMessages = new List<string>()
	    {
	        $"Peace Proposed Placeholder"
	    };


	    public static readonly List<string> PeaceRejectedMessages = new List<string>()
	    {
	        "Pirate scum!  Search for easy targets elsewhere! ",
	        "Ha, this is a joke, right?",
	        "What’s that saying?  Yo ho ho, it’s a pirate life for thee?  ",
	        "Shoo Pirate, don’t bother me!",
	        "SEPD, what’s your emergency?",
	        "How’d you get this number?",
	        "Um. No.",
	        "By the Queen, you are an ugly mess, No, no, no, just no.",
            "Nice try!",
	        "We know where you live.  ",
	        "I don't believe we can ever come to terms.",
            "You can’t be serious…",
	        "I don’t think the Intergalactic Authority would appreciate us saying yes..."
        };


	    public static readonly List<string> WarDeclaredMessages = new List<string>()
	    {
	        "Fool me once, shame on me.  Fool me twice...",
	        "You asked for this.",
	        "Don’t start crying now!",
	        "It’s too late for you now.",
	        "You’ve gone too far this time!",
	        "Why would you push us to this point?",
	        "Later, chump!",
	        "Bye Bye Pretty one, I'll use your skull on the wall.",
            "BOOM!",
	        "What’s that ship worth again?",
	        "Can't roll the dice perfect every time, some times you lose. Like now.",
            "Now you’re in trouble.",
	        "Well, that's a ticket. And that's a ticket. And that's another ticket. Okay, You die.",
            "You've been granted that wish of visiting heaven. You're welcome."
        };


	    public static readonly List<string> WarReceiveddMessages = new List<string>()
	    {
	        "What did we ever do to deserve this?",
            "We’re a peaceful people.  We also have big guns.",
            "If you think you can win, you’re sadly mistaken!",
            "Fine, if this is how it’s going to be, then this is how it’s going to be.",
            "Oh brother…",
            "I mean… if you insist.",
            "I hope you’re up to date on your insurance payments.",
            "Did you just say what I thought you said?",
            "Not again…",
            ":("
        };
    }
}










