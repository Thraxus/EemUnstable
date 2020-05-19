using Eem.Thraxus.Bots.Modules.Reporting.Systems.Support;

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
