using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Extensions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Helpers
{
	public static class DamageHelper
	{
		/// <summary>
		/// Determines if damage was done by player.
		/// <para/>
		/// If it's necessary to determine who did the damage, use overload.
		/// </summary>
		//public static bool IsDoneByPlayer(this MyDamageInformation damage)
		//{
		//	IMyPlayer trash;
		//	return damage.IsDoneByPlayer(out trash);
		//}

		private static bool IsDamagedByPlayerWarhead(IMyWarhead warhead, out IMyPlayer damager)
		{
			damager = null;
			try
			{
				if (warhead.OwnerId == 0)
				{
					damager = MyAPIGateway.Players.GetPlayerById(((MyCubeBlock) warhead).BuiltBy);
					AiSessionCore.DebugWrite("Damage.IsDoneByPlayer", "Attempting to find damager by neutral warhead.");
					return damager != null;
				}
				else
				{
					damager = MyAPIGateway.Players.GetPlayerById(warhead.OwnerId);
					AiSessionCore.DebugWrite("Damage.IsDoneByPlayer", "Attempting to find damager by warhead owner.");
					return damager != null;
				}
			}
			catch (Exception scrap)
			{
				AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check for neutral warheads crashed", scrap));
				return false;
			}
		}

		private static bool IsDamagedByPlayer(IMyGunBaseUser gun, out IMyPlayer damager)
		{
			damager = null;
			try
			{
				damager = MyAPIGateway.Players.GetPlayerById(gun.OwnerId);
				//AISessionCore.DebugWrite($"GunDamage.IsDamagedByPlayer", $"Getting player from gun. ID: {Gun.OwnerId}, player: {(Damager != null ? Damager.DisplayName : "null")}", false);
				return !damager?.IsBot ?? false;
			}
			catch (Exception scrap)
			{
				AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check gun owner crashed", scrap));
				return false;
			}
		}

		private static bool IsDamagedByPlayer(IMyEngineerToolBase tool, out IMyPlayer damager)
		{
			damager = null;
			try
			{
				damager = MyAPIGateway.Players.GetPlayerById(tool.OwnerIdentityId);
				//AISessionCore.DebugWrite($"ToolDamage.IsDamagedByPlayer", $"Getting player from tool. ID: {Tool.OwnerId}, IdentityID: {Tool.OwnerIdentityId}, player: {(Damager != null ? Damager.DisplayName : "null")}", false);
				return damager != null && !damager.IsBot;
			}
			catch (Exception scrap)
			{
				AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check gun owner crashed", scrap));
				return false;
			}
		}

		private static bool IsDamagedByPlayerInNeutralGrid(IMyCubeGrid grid, out IMyPlayer damager)
		{
			damager = null;
			try
			{
				damager = grid.FindControllingPlayer();
				if (damager != null) return !damager.IsBot;

				try
				{
					List<MyCubeBlock> cubeBlocks = grid.GetBlocks<MyCubeBlock>(x => x.BuiltBy != 0);
					if (cubeBlocks.Count != 0)
					{
						long thatCunningGrieferId = cubeBlocks[0].BuiltBy;
						damager = MyAPIGateway.Players.GetPlayerById(thatCunningGrieferId);
						return damager != null;
					}
					else
					{
						List<IMySlimBlock> slimBlocks = grid.GetBlocks(Selector: x => x.GetBuiltBy() != 0, BlockLimit: 50);
						if (slimBlocks.Count == 0) return false; // We give up on this one
						else
						{
							try
							{
								damager = MyAPIGateway.Players.GetPlayerById(slimBlocks.First().GetBuiltBy());
								if (damager != null)
								{
									grid.DebugWrite("Damage.IsDoneByPlayer.FindBuilderBySlimBlocks", $"Found damager player from slim block. Damager is {damager.DisplayName}");
								}
								return damager != null;
							}
							catch (Exception scrap)
							{
								AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check grid via SlimBlocks BuiltBy crashed.", scrap));
								return false;
							}
						}
					}
				}
				catch (Exception scrap)
				{
					AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check grid via BuiltBy crashed.", scrap));
					return false;
				}
			}
			catch (Exception scrap)
			{
				AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check neutral grid crashed", scrap));
				return false;
			}
		}

		private static bool IsDamagedByPlayerGrid(IMyCubeGrid grid, out IMyPlayer damager)
		{
			damager = null;
			try
			{
				long biggestOwner = grid.BigOwners.FirstOrDefault();
				if (biggestOwner == 0) return false;
				damager = MyAPIGateway.Players.GetPlayerById(biggestOwner);
				return !damager?.IsBot ?? false;

			}
			catch (Exception scrap)
			{
				AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check grid via BigOwners crashed", scrap));
				return false;
			}
		}


		/// <summary>
		/// Determines if damage was done by player.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="damager">Provides player who did the damage. Null if damager object is not a player.</param>
		public static bool IsDoneByPlayer(this MyDamageInformation damage, out IMyPlayer damager)
		{
			damager = null;
			
			try
			{
				IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
				if (damage.IsDeformation || damage.AttackerId == 0 || attackerEntity == null) return false;
				
				if (attackerEntity is IMyMeteor) return false;
				if (attackerEntity is IMyWarhead) return IsDamagedByPlayerWarhead(attackerEntity as IMyWarhead, out damager);
				if (attackerEntity is IMyEngineerToolBase) return IsDamagedByPlayer(attackerEntity as IMyEngineerToolBase, out damager);
				if (attackerEntity is IMyGunBaseUser) return IsDamagedByPlayer(attackerEntity as IMyGunBaseUser, out damager);

				attackerEntity = attackerEntity.GetTopMostParent();

				if (attackerEntity == null)
				{
					AiSessionCore.DebugLog?.WriteToLog("IsDoneByPlayer", $"attackerEntity was NULL");
					AiSessionCore.DebugWrite("Damage.IsDoneByPlayer", "Cannot acquire the attacker's topmost entity", antiSpam: false);
					return false;
				}

				if (!(attackerEntity is IMyCubeGrid)) return false;
				IMyCubeGrid grid = attackerEntity as IMyCubeGrid;
				if (grid.IsPirate()) return false;
				//grid.GetOwnerFaction()
				return grid.IsOwnedByNobody() ? IsDamagedByPlayerInNeutralGrid(grid, out damager) : IsDamagedByPlayerGrid(grid, out damager);

			}
			catch (Exception scrap)
			{
				AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("General crash.", scrap));
				return false;
			}
		}

		public static bool IsMeteor(this MyDamageInformation damage)
		{
			IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
			return attackerEntity is IMyMeteor;
		}

		public static bool IsThruster(this MyDamageInformation damage)
		{
			IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
			return attackerEntity is IMyThrust;
		}

		//public static bool IsGrid(this MyDamageInformation damage, out IMyCubeGrid grid)
		//{
		//	grid = MyAPIGateway.Entities.GetEntityById(damage.AttackerId).GetTopMostParent() as IMyCubeGrid;
		//	return grid != null;
		//}

		//public static bool IsGrid(this MyDamageInformation damage)
		//{
		//	IMyCubeGrid grid = MyAPIGateway.Entities.GetEntityById(damage.AttackerId).GetTopMostParent() as IMyCubeGrid;
		//	return grid != null;
		//}
	}
}