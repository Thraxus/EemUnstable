using Eem.Thraxus.Bots.Modules.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Reporting.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Reporting.Systems.Types
{
	internal class Thruster : EemFunctionalBlock
	{
		public Thruster(BotSystemType type, IMyThrust thruster) : base(type, thruster) { }
	}
}
