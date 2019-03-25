using Eem.Thraxus.Extensions;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Helpers
{
	public static class OwnershipTools
	{
		public static long PirateId => MyVisualScriptLogicProvider.GetPirateId();

		public static bool IsOwnedByPirates(this IMyTerminalBlock block)
		{
			return block.OwnerId == PirateId;
		}

		public static bool IsOwnedByNpc(this IMyTerminalBlock block, bool allowNobody = true, bool checkBuilder = false)
		{
			if (!checkBuilder)
			{
				if (block.IsOwnedByPirates()) return true;
				if (!allowNobody && block.IsOwnedByNobody()) return false;
				IMyPlayer owner = MyAPIGateway.Players.GetPlayerById(block.OwnerId);
				return owner?.IsBot ?? true;
			}
			else
			{
				if (!block.IsOwnedByNpc(allowNobody)) return false;
				long builderId = block.GetBuiltBy();
				if (!allowNobody && builderId == 0) return false;
				IMyPlayer owner = MyAPIGateway.Players.GetPlayerById(builderId);
				return owner == null || owner.IsBot;
			}
		}

		public static bool IsPirate(this IMyCubeGrid grid, bool strictCheck = false)
		{
			if (grid.BigOwners.Count == 0 || grid.BigOwners[0] == 0) return false;
			if (!strictCheck) return grid.BigOwners.Contains(PirateId);
			return grid.BigOwners.Count == 1 && grid.BigOwners[0] == PirateId;
		}

		public static bool IsNpc(this IMyCubeGrid grid)
		{
			if (grid.IsPirate()) return true;
			if (grid.BigOwners.Count == 0) return false;
			IMyPlayer owner = MyAPIGateway.Players.GetPlayerById(grid.BigOwners[0]);
			return owner == null || owner.IsBot;
		}

		public static bool IsOwnedByNobody(this IMyCubeGrid grid)
		{
			return grid.BigOwners.Count == 0 || grid.BigOwners[0] == 0;
		}

		public static bool IsOwnedByNobody(this IMyCubeBlock block)
		{
			return block.OwnerId == 0;
		}

		public static bool IsBuiltByNobody(this IMyCubeBlock block)
		{
			return block.GetBuiltBy() == 0;
		}

		public static bool IsPlayerBlock(this IMySlimBlock block, out IMyPlayer builder)
		{
			builder = null;
			long builtBy = block.GetBuiltBy();
			if (builtBy == 0) return false;
			builder = MyAPIGateway.Players.GetPlayerById(builtBy);
			return builder != null && !builder.IsBot;
		}

		public static bool IsPlayerBlock(this IMyCubeBlock block, out IMyPlayer owner)
		{
			owner = null;
			if (block.OwnerId != 0)
			{
				return MyAPIGateway.Players.IsValidPlayer(block.OwnerId, out owner);
			}
			else
			{
				long builtBy = block.GetBuiltBy();
				if (builtBy == 0) return false;
				owner = MyAPIGateway.Players.GetPlayerById(builtBy);
				return owner != null && !owner.IsBot;
			}
		}
	}
}