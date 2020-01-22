using System;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;

namespace Eem.Thraxus.Bots.Interfaces
{
	internal interface IReportDamage : INeedUpdates
	{
		event Action<SystemType, float> SystemDamaged;
		
		bool IsFunctional { get; }
		
		bool IsDestroyed { get; }
		
		float MaxIntegrity { get; }

		float FunctionalIntegrity { get; }
		
		float CurrentIntegrity { get; }
		
		float RemainingFunctionalIntegrity { get; }

		float RemainingIntegrityRatio { get; }

		float LastUpdatedIntegrity { get; }
	}
}
