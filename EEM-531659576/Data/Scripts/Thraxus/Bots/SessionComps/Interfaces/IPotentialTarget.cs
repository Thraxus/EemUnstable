using System.Collections.Concurrent;
using Eem.Thraxus.Common.Enums;
using VRage.Collections;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.SessionComps.Interfaces
{
	public interface IPotentialTarget : IClose
	{
		// Targeting Characteristics
		int Threat { get; }
		
		int Value { get; }


		// Critical Systems
		ConcurrentDictionary<TargetSystemType, ConcurrentCachingList<IMyCubeBlock>> TargetSystems { get; }

		// Ownership Information
		long FactionId { get; }

		long OwnerId { get; }

		GridOwnerType OwnerType { get; }

		FactionRelationship GetRelationship(long requestingGridOwnerId);


		// Grid Information
		bool HasBars { get; }

		bool HasHeavyArmor { get; }

		bool HasDefenseShields { get; }

		bool HasEnergyShields { get; }

		bool IsClosed { get; }

		Vector3 LinearVelocity { get; }

		MyCubeSize Size { get; }

		GridType GridType { get; }

		Vector3D Position { get; }
	}
}