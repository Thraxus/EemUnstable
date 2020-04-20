using System.Collections.Generic;
using Eem.Thraxus.Common.DataTypes;

namespace Eem.Thraxus.Bots.Modules.Targeting.Support
{
	internal class TargetCompare : IComparer<ValidTarget>
	{
		public int Compare(ValidTarget x, ValidTarget y)
		{
			//return x.Threat > y.Threat ? 1 : -1;
			return y.Threat.CompareTo(x.Threat);
		}
	}
}