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
    }
}
