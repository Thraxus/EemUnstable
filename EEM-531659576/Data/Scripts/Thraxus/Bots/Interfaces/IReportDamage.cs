using System;
using Eem.Thraxus.Common.Enums;

namespace Eem.Thraxus.Bots.Interfaces
{
	internal interface IReportDamage : INeedUpdates
	{
		event Action<BotSystemType, float> SystemDamaged;
		
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
