using System.Text;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Types
{
	internal class Thruster : EemFunctionalBlock
	{
		private IMyThrust _thruster;

		public float GetMaxThrust()
		{
			if (IsClosed) return 0;
			if (IsDestroyed()) return 0;
			if (!IsFunctional) return 0;
			return _thruster.MaxThrust;
		}

		public float GetMaxEffectiveThrust()
		{
			if (IsClosed) return 0;
			if (IsDestroyed()) return 0;
			if (!IsFunctional) return 0;
			return _thruster.MaxEffectiveThrust;
		}

		public Thruster(SystemType type, IMyThrust thruster) : base(type, thruster)
		{
			_thruster = thruster;
		}

		public override void Close()
		{
			base.Close();
			_thruster = null;
		}

		public override string ToString()
		{
			return new StringBuilder($"{base.ToString()} | {_thruster.MaxThrust}").ToString();
		}
	}
}
