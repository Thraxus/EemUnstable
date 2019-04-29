using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Utilities
{
	public static class StaticMethods
	{
		public static IEnumerable<MyEntity> DetectDynamicEntitiesInSphere(Vector3D detectionCenter, double range)
		{
			MyAPIGateway.Session.GPS.AddGps(MyAPIGateway.Session.LocalHumanPlayer.IdentityId, MyAPIGateway.Session.GPS.Create($"DetectEntitiesInSphere {range}", "", detectionCenter, true));
			BoundingSphereD pruneSphere = new BoundingSphereD(detectionCenter, range);
			List<MyEntity> pruneList = new List<MyEntity>();
			MyGamePruningStructure.GetAllEntitiesInSphere(ref pruneSphere, pruneList, MyEntityQueryType.Dynamic);
			return pruneList;
		}

		public static IEnumerable<MyEntity> DetectStaticEntitiesInSphere(Vector3D detectionCenter, double range)
		{
			MyAPIGateway.Session.GPS.AddGps(MyAPIGateway.Session.LocalHumanPlayer.IdentityId, MyAPIGateway.Session.GPS.Create($"DetectEntitiesInSphere {range}", "", detectionCenter, true));
			BoundingSphereD pruneSphere = new BoundingSphereD(detectionCenter, range);
			List<MyEntity> pruneList = new List<MyEntity>();
			MyGamePruningStructure.GetAllEntitiesInSphere(ref pruneSphere, pruneList, MyEntityQueryType.Static);
			return pruneList;
		}

		public static IEnumerable<MyEntity> DetectAllEntitiesInSphere(Vector3D detectionCenter, double range)
		{
			MyAPIGateway.Session.GPS.AddGps(MyAPIGateway.Session.LocalHumanPlayer.IdentityId, MyAPIGateway.Session.GPS.Create($"DetectEntitiesInSphere {range}", "", detectionCenter, true));
			BoundingSphereD pruneSphere = new BoundingSphereD(detectionCenter, range);
			List<MyEntity> pruneList = new List<MyEntity>();
			MyGamePruningStructure.GetAllEntitiesInSphere(ref pruneSphere, pruneList);
			return pruneList;
		}

		public static IEnumerable<MyEntity> DetectPlayersInSphere(Vector3D detectionCenter, double range)
		{
			MyAPIGateway.Session.GPS.AddGps(MyAPIGateway.Session.LocalHumanPlayer.IdentityId, MyAPIGateway.Session.GPS.Create($"DetectEntitiesInSphere {range}", "", detectionCenter, true));
			BoundingSphereD pruneSphere = new BoundingSphereD(detectionCenter, range);
			List<MyEntity> pruneList = new List<MyEntity>();
			MyGamePruningStructure.GetAllEntitiesInSphere(ref pruneSphere, pruneList, MyEntityQueryType.Dynamic);
			List<IMyPlayer> players = new List<IMyPlayer>();
			MyAPIGateway.Multiplayer.Players.GetPlayers(players, x => !x.IsBot);
			pruneList.RemoveAll(x => players.Any(y => y.IdentityId == x.EntityId));
			return pruneList;
		}

		public static IMyPlayer GetPlayerById(this IMyPlayerCollection players, long playerId)
		{
			List<IMyPlayer> myPlayers = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(myPlayers, x => x.IdentityId == playerId);
			return myPlayers.FirstOrDefault();
		}
	}
}
