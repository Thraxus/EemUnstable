using Eem.Thraxus.Common.Enums;

namespace Eem.Thraxus.Bots.Interfaces
{
	internal interface IReportIntegrity
	{
		BotSystemType Type { get; }

		int CurrentFunctionalIntegrityRatio();

		bool IsClosed { get; }

		void Close();
	}
}
