﻿using System.Collections.Generic;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace Eem.Thraxus.Bots.SessionComps.Models
{
	public class MasterObjectBuilderReference
	{
		public Dictionary<MyStringHash, BlockData> VanillaSubtypeIds = new Dictionary<MyStringHash, BlockData>();
		public Dictionary<MyStringHash, BlockData> ModdedSubtypeIds = new Dictionary<MyStringHash, BlockData>();
		public BlockData DefaultValue = new BlockData { Value = 1, Threat = 1, Type = TargetSystemType.None };
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

		public void Clear()
		{
			VanillaSubtypeIds.Clear();
			ModdedSubtypeIds.Clear();
			Type = null;
		}

		private BlockData GetDefaultValue(MyStringHash subtype)
		{
			StaticLog.WriteToLog("MasterObjectBuilderReference", $"Value not found in any dictionaries: {Type} | {subtype}", LogType.General);
			return DefaultValue;
		}
	}
}