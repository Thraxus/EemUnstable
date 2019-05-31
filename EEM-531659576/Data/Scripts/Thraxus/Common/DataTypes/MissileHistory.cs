using System;
using VRageMath;

namespace Eem.Thraxus.Common.DataTypes
{
	public struct MissileHistory
	{
		public readonly long OriginEntity;
		public readonly long OwnerId;
		public readonly long LauncherId;
		public readonly Vector3D Location;
		public readonly DateTime TimeStamp;

		public MissileHistory(long originEntity, long launcherId, long ownerId, Vector3D location, DateTime timeStamp)
		{
			OriginEntity = originEntity;
			LauncherId = launcherId;
			OwnerId = ownerId;
			Location = location;
			TimeStamp = timeStamp;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{OriginEntity} | {OwnerId} | {LauncherId} | {Location} | {TimeStamp}";
		}
	}
}