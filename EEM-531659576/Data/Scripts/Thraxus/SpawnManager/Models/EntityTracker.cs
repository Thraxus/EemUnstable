using System;
using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.SpawnManager.Models
{
	public class EntityTracker
	{
		public EntityTracker()
		{
			MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
			MyAPIGateway.Entities.OnEntityRemove += OnEntityRemoved;
		}

		public void Close()
		{
			MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
			MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemoved;
		}

		private static void OnEntityAdd(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			PrintRangeToPlayerZero(myEntity);
		}

		private static void OnEntityRemoved(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			PrintShipDespawn(myEntity);
		}

		private static void PrintRangeToPlayerZero(IMyEntity myEntity)
		{
			try
			{
				if (MyAPIGateway.Multiplayer.Players.Count == 0)
				{
					SpawnManagerCore.WriteToLog("PrintRangeToPlayerZero", $"Id:\t{myEntity.EntityId}\tDisplayName:\t{myEntity.DisplayName}\tPlayer:\t{0}\tDistanceTo:\t{0}", true);
					return;
				}
				List<IMyPlayer> playerList = new List<IMyPlayer>();
				MyAPIGateway.Multiplayer.Players.GetPlayers(playerList);
				MyCubeGrid mygrid = (MyCubeGrid)myEntity;
				foreach (IMyPlayer myPlayer in playerList)
				{
					SpawnManagerCore.WriteToLog("PrintRangeToPlayerZero", $"Id:\t{mygrid.EntityId}\tDisplayName:\t{mygrid.DisplayName}\tPCU:\t{mygrid.BlocksPCU}\tPlayer:\t{myPlayer.DisplayName}\tDistanceTo:\t{Vector3D.Distance(myPlayer.GetPosition(), myEntity.GetPosition())}", true);
				}
			}
			catch (Exception e)
			{
				SpawnManagerCore.ExceptionLog("PrintRangeToPlayerZero", $"Exception! {e}");
			}
			
		}

		private static void PrintShipDespawn(IMyEntity myEntity)
		{
			try
			{
				MyCubeGrid mygrid = (MyCubeGrid)myEntity;
				SpawnManagerCore.WriteToLog("PrintShipDespawn", $"Id:\t{mygrid.EntityId}\tDisplayName:\t{mygrid.DisplayName}\tPCU:\t{mygrid.BlocksPCU}", true);
			}
			catch (Exception e)
			{
				SpawnManagerCore.ExceptionLog("PrintShipDespawn", $"Exception! {e}");
			}
		}
	}
}
