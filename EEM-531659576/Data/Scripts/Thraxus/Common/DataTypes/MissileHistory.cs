using System;
using VRageMath;

namespace Eem.Thraxus.Common.DataTypes
{
	public struct MissileHistory
	{
		public readonly long OwnerId;
		public readonly long LauncherId;
		public readonly Vector3D Location;
		public readonly DateTime TimeStamp;

		public MissileHistory(long launcherId, long ownerId, Vector3D location, DateTime timeStamp)
		{
			LauncherId = launcherId;
			OwnerId = ownerId;
			Location = location;
			TimeStamp = timeStamp;
		}
	}
}