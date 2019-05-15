using System.Collections.Generic;

namespace Eem.Thraxus.Common.DataTypes
{
	public struct BotOrphan
	{
		public readonly long MyParentId;
		public readonly List<long> MyAncestors;
		public readonly EemPrefabConfig MyLegacyConfig;

		public BotOrphan(long myParentId, List<long> myAncestors, EemPrefabConfig myLegacyConfig)
		{
			MyParentId = myParentId;
			MyAncestors = myAncestors;
			MyLegacyConfig = myLegacyConfig;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Parent: {MyParentId} - MyAncestors: {MyAncestors?.ToArray()} - MyConfig: {MyLegacyConfig}";
		}
	}
}