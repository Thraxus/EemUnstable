using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using VRage.Utils;

namespace Eem.Thraxus.Bots.Modules.ModManagers
{
	public static class EnergyShields
	{
		private static readonly Type EnergyShieldsType = typeof(MyObjectBuilder_Refinery);

		public static readonly MyStringHash BypassKey = MyStringHash.GetOrCompute("IgnoreShields"); 

		private static IEnumerable<string> EnergyShieldsDefinitions { get; } = new List<string>()
		{
			"LargeShipSmallShieldGeneratorBase",
			"LargeShipLargeShieldGeneratorBase",
			"SmallShipMicroShieldGeneratorBase",
			"SmallShipSmallShieldGeneratorBase"
		};

	}
}
