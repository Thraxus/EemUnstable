using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.DataTypes;
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
		List<IMyCubeBlock> Controllers { get; }

		List<IMyCubeBlock> Navigation { get; }

		List<IMyCubeBlock> PowerSystems { get; }

		List<IMyCubeBlock> Propulsion { get; }

		List<IMyCubeBlock> Weapons { get; }
		

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