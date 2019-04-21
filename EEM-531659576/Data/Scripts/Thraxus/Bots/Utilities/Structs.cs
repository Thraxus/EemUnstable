namespace Eem.Thraxus.Bots.Utilities
{
	public struct BotOrphan
	{
		public long MyParentId;
		public long MyGrandParentId;

		public BotOrphan(long myParentId, long myGrandParentId)
		{
			MyParentId = myParentId;
			MyGrandParentId = myGrandParentId;
		}
	}
}
