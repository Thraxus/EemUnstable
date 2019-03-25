using System;
using Eem.Thraxus.Helpers;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus
{
	public static class BotFabric
	{
		public static BotBase FabricateBot(IMyCubeGrid grid, IMyRemoteControl rc)
		{
			try
			{
				BotTypeBase botType = BotBase.ReadBotType(rc);

				BotBase bot = null;
				switch (botType)
				{
					case BotTypeBase.Fighter:
						bot = new BotTypeFighter(grid);
						break;
					case BotTypeBase.Freighter:
						bot = new BotTypeFreighter(grid);
						break;
					case BotTypeBase.Station:
						bot = new BotTypeStation(grid);
						break;
					case BotTypeBase.None:
						break;
					case BotTypeBase.Invalid:
						break;
					case BotTypeBase.Carrier:
						break;
					default:
						if (Constants.AllowThrowingErrors) throw new Exception("Invalid bot type");
						break;
				}
				return bot;
			}
			catch (Exception scrap)
			{
				grid.LogError("BotFabric.FabricateBot", scrap);
				return null;
			}
		}
	}
}