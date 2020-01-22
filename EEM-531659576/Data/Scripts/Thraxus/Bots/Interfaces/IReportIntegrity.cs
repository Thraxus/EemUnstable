using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;

namespace Eem.Thraxus.Bots.Interfaces
{
	internal interface IReportIntegrity
	{
		SystemType Type { get; }

		int CurrentFunctionalIntegrityRatio();

		bool IsClosed { get; }

		void Close();
	}
}
