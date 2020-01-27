using System.Text;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Types
{
	internal class Thruster : EemFunctionalBlock
	{
		public Thruster(SystemType type, IMyThrust thruster) : base(type, thruster) { }
	}
}
