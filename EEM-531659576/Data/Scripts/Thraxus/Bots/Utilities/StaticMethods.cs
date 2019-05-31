﻿using System.Collections.Generic;
using System.Linq;
using Sandbox.Game;
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
			AddGpsLocation($"DetectDynamicEntitiesInSphere {range}", detectionCenter);
			
			BoundingSphereD pruneSphere = new BoundingSphereD(detectionCenter, range);
			List<MyEntity> pruneList = new List<MyEntity>();
			MyGamePruningStructure.GetAllEntitiesInSphere(ref pruneSphere, pruneList, MyEntityQueryType.Dynamic);
			return pruneList;
		}

		public static IEnumerable<MyEntity> DetectStaticEntitiesInSphere(Vector3D detectionCenter, double range)
		{
			AddGpsLocation($"DetectStaticEntitiesInSphere {range}", detectionCenter);
			
			BoundingSphereD pruneSphere = new BoundingSphereD(detectionCenter, range);
			List<MyEntity> pruneList = new List<MyEntity>();
			MyGamePruningStructure.GetAllEntitiesInSphere(ref pruneSphere, pruneList, MyEntityQueryType.Static);
			return pruneList;
		}

		public static IEnumerable<MyEntity> DetectAllEntitiesInSphere(Vector3D detectionCenter, double range)
		{
			AddGpsLocation($"DetectAllEntitiesInSphere {range}", detectionCenter);
			
			BoundingSphereD pruneSphere = new BoundingSphereD(detectionCenter, range);
			List<MyEntity> pruneList = new List<MyEntity>();
			MyGamePruningStructure.GetAllEntitiesInSphere(ref pruneSphere, pruneList);
			return pruneList;
		}

		public static IEnumerable<MyEntity> DetectPlayersInSphere(Vector3D detectionCenter, double range)
		{
			AddGpsLocation($"DetectPlayersInSphere {range}", detectionCenter);
			
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

		public static IMyIdentity GetIdentityById(long playerId)
		{
			List<IMyIdentity> identityList = new List<IMyIdentity>();
			MyAPIGateway.Players.GetAllIdentites(identityList);
			return identityList.FirstOrDefault(x => x.IdentityId == playerId);
			//List<IMyPlayer> myPlayers = new List<IMyPlayer>();
			//MyAPIGateway.Players.GetPlayers(myPlayers, x => x.IdentityId == playerId);
			//return myPlayers.FirstOrDefault();
		}

		public static void AddGpsLocation(string message, Vector3D location)
		{
			MyAPIGateway.Session.GPS.AddGps(MyAPIGateway.Session.LocalHumanPlayer.IdentityId, MyAPIGateway.Session.GPS.Create(message, "", location, true));
		}

		public static void CreateFakeSmallExplosion(Vector3D position)
		{
			MyExplosionInfo explosionInfo = new MyExplosionInfo()
			{
				PlayerDamage = 0.0f,
				Damage = 0f,
				ExplosionType = MyExplosionTypeEnum.WARHEAD_EXPLOSION_02,
				ExplosionSphere = new BoundingSphereD(position, 0d),
				LifespanMiliseconds = 0,
				ParticleScale = 1f,
				Direction = Vector3.Down,
				VoxelExplosionCenter = position,
				ExplosionFlags = MyExplosionFlags.CREATE_PARTICLE_EFFECT,
				VoxelCutoutScale = 0f,
				PlaySound = true,
				ApplyForceAndDamage = false,
				ObjectsRemoveDelayInMiliseconds = 0
			};
			MyExplosions.AddExplosion(ref explosionInfo);
		}
	}
}
