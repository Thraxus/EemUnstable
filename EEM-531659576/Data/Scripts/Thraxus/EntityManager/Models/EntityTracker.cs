using System;
using System.Collections.Generic;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.EntityManager.Models
{
	public class EntityTracker : LogBase
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

		private void OnEntityAdd(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			PrintShipSpawn(myEntity);
		}

		private void OnEntityRemoved(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			PrintShipDespawn(myEntity);
		}

		private void PrintShipSpawn(IMyEntity myEntity)
		{
			try
			{
				if (MyAPIGateway.Multiplayer.Players.Count == 0)
				{
					WriteToLog("PrintShipSpawn", $"Id:\t{myEntity.EntityId}\tDisplayName:\t{myEntity.DisplayName}\tPlayer:\t{0}\tDistanceTo:\t{0}", LogType.General);
					return;
				}
				List<IMyPlayer> playerList = new List<IMyPlayer>();
				MyAPIGateway.Multiplayer.Players.GetPlayers(playerList);
				MyCubeGrid mygrid = (MyCubeGrid)myEntity;
				foreach (IMyPlayer myPlayer in playerList)
				{
					WriteToLog("PrintShipSpawn", $"Id:\t{mygrid.EntityId}\tDisplayName:\t{mygrid.DisplayName}\tPCU:\t{mygrid.BlocksPCU}\tPlayer:\t{myPlayer.DisplayName}\tDistanceTo:\t{Vector3D.Distance(myPlayer.GetPosition(), myEntity.GetPosition())}", LogType.General);
				}
			}
			catch (Exception e)
			{
				WriteToLog("PrintShipSpawn", $"Exception! {e}", LogType.Exception);
			}
			
		}

		private void PrintShipDespawn(IMyEntity myEntity)
		{
			try
			{
				MyCubeGrid mygrid = (MyCubeGrid)myEntity;
				WriteToLog("PrintShipDespawn", $"Id:\t{mygrid.EntityId}\tDisplayName:\t{mygrid.DisplayName}\tPCU:\t{mygrid.BlocksPCU}", LogType.General);
			}
			catch (Exception e)
			{
				WriteToLog("PrintShipDespawn", $"Exception! {e}", LogType.Exception);
			}
		}
	}
}
