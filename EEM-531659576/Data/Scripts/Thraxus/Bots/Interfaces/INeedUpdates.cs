namespace Eem.Thraxus.Bots.Interfaces
{
	internal interface INeedUpdates
	{
		bool IsClosed { get; }
		
		void RunUpdate();

		void Close();
	}
}
