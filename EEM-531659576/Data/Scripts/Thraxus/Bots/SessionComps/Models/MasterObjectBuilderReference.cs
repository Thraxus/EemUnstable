using System.Collections.Generic;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace Eem.Thraxus.Bots.SessionComps.Models
{
	public class MasterObjectBuilderReference
	{
		public Dictionary<MyStringHash, BlockData> VanillaSubtypeIds;
		public Dictionary<MyStringHash, BlockData> ModdedSubtypeIds;
		public BlockData DefaultValue = new BlockData { Value = 1, Threat = 1 };
		public MyObjectBuilderType Type;

		public BlockData GetValue(MyObjectBuilderType type, MyStringHash subtype)
		{
			Type = type;
			BlockData returnValue;
			if (VanillaSubtypeIds.TryGetValue(subtype, out returnValue))
				return returnValue;
			if (ModdedSubtypeIds.TryGetValue(subtype, out returnValue))
				return returnValue;
			return GetDefaultValue(subtype);
		}

		private BlockData GetDefaultValue(MyStringHash subtype)
		{
			StaticLog.WriteToLog("MasterObjectBuilderReference", $"Value not found in any dictionaries: {Type} | {subtype}", LogType.General);
			return DefaultValue;
		}
	}
}