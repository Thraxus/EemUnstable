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
			//SpawnManagerCore.WriteToLog("OnEntityAdd", $"{myEntity.EntityId}\t{myEntity.Name}\t{myEntity.DisplayName}\t{myEntity.GetType()}\t{myEntity.GetObjectBuilder().GetType()}", true);
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			PrintRangeToPlayerZero(myEntity);
		}

		private static void OnEntityRemoved(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			SpawnManagerCore.WriteToLog("OnEntityRemoved", $"Id:\t{myEntity.EntityId}\tDisplayName:\t{myEntity.DisplayName}\tPlayer:\t{0}\tDistanceTo:\t{0}", true);
		}

		private static void PrintRangeToPlayerZero(IMyEntity entity)
		{
			if (MyAPIGateway.Multiplayer.Players.Count == 0)
			{
				SpawnManagerCore.WriteToLog("PrintRangeToPlayerZero", $"Id:\t{entity.EntityId}\tDisplayName:\t{entity.DisplayName}\tPlayer:\t{0}\tDistanceTo:\t{0}", true);
				return;
			}
			List<IMyPlayer> playerList = new List<IMyPlayer>();
			MyAPIGateway.Multiplayer.Players.GetPlayers(playerList);
			SpawnManagerCore.WriteToLog("PrintRangeToPlayerZero", $"Id:\t{entity.EntityId}\tDisplayName:\t{entity.DisplayName}\tPlayer:\t{playerList[playerList.Count - 1].DisplayName}\tDistanceTo:\t{Vector3D.Distance(playerList[playerList.Count - 1].GetPosition(), entity.GetPosition())}", true);
		}
	}
}
