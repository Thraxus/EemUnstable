using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.BaseClasses;
using Eem.Thraxus.Common.Enums;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Types
{
	internal class Thruster : EemFunctionalBlock
	{
		public Thruster(BotSystemType type, IMyThrust thruster) : base(type, thruster) { }
	}
}
