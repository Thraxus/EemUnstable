using System;
using System.Collections.Generic;
using System.Linq;

namespace EemRdx.Factions.Messages
{
	internal static class DefaultDialogs
	{
		public const string DefaultTag = "TheUnknown";

		public static readonly Func<string> CatchAll = () => CatchAllMessages.ElementAt(Factions.FactionsRandom.Next(CatchAllMessages.Count));
        
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

        public static readonly Func<string> CollectiveDisappointment = () => CollectiveDisappointmentMessages.ElementAt(Factions.FactionsRandom.Next(CollectiveDisappointmentMessages.Count));

        private static readonly List<string> CollectiveDisappointmentMessages = new List<string>()
		{
			$"Yo ho ho, it's a pirate life fer ye!"
		};

	    public static readonly Func<string> CollectiveReprieve = () => CollectiveReprieveMessages.ElementAt(Factions.FactionsRandom.Next(CollectiveReprieveMessages.Count));

	    private static readonly List<string> CollectiveReprieveMessages = new List<string>()
	    {
	        $"We've discussed your request, and will take it into individual consideration."
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
	        "Don’t make us regret this decision."
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
	        "Nice try!",
	        "We know where you live.  ",
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
	        "BOOM!",
	        "What’s that ship worth again?",
	        "Now you’re in trouble."
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
