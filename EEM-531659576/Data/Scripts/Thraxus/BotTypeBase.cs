namespace Eem.Thraxus
{
	public enum BotTypeBase
	{
		None,
		Invalid,
		Station,
		Fighter,
		Freighter,
		Carrier
	}

	/*public sealed class InvalidBot : BotBase
	{
		static public readonly BotTypes BotType = BotTypes.None;

		public override bool Operable
		{
			get
			{
				return false;
			}
		}

		public InvalidBot(IMyCubeGrid Grid = null) : base(Grid)
		{
		}

		public override bool Init(IMyRemoteControl RC = null)
		{
			return false;
		}

		public override void Main()
		{
			// Empty
		}

		protected override bool ParseSetup()
		{
			return false;
		}
	}*/
}