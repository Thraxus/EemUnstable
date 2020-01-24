namespace Eem.Thraxus.Bots.Interfaces
{
	internal interface INeedUpdates
	{
		bool IsClosed { get; }
		
		void RunMassUpdate(long blockId);

		void Close();
	}
}
