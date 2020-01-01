using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Common.Utilities.Statics
{
	public static class Statics
	{
		public static long GlobalTicks => MyAPIGateway.Session.ElapsedPlayTime.Ticks;

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

		//Relative velocity proportional navigation
		//aka: Whip-Nav
		internal static Vector3D CalculateMissileIntercept(Vector3D targetPosition, Vector3D targetVelocity, Vector3D missilePos, Vector3D missileVelocity, double missileAcceleration, double compensationFactor = 1)
		{
			Vector3D missileToTarget = Vector3D.Normalize(targetPosition - missilePos);
			Vector3D relativeVelocity = targetVelocity - missileVelocity;
			Vector3D parallelVelocity = relativeVelocity.Dot(missileToTarget) * missileToTarget;
			Vector3D normalVelocity = (relativeVelocity - parallelVelocity);

			Vector3D normalMissileAcceleration = normalVelocity * compensationFactor;

			if (Vector3D.IsZero(normalMissileAcceleration))
				return missileToTarget;

			double diff = missileAcceleration * missileAcceleration - normalMissileAcceleration.LengthSquared();
			if (diff < 0)
			{
				return normalMissileAcceleration; //fly parallel to the target
			}

			return Math.Sqrt(diff) * missileToTarget + normalMissileAcceleration;
		}

		public static IMyFaction GetFactionById(this long factionId)
		{
			return MyAPIGateway.Session.Factions.TryGetFactionById(factionId);
		}

		public static bool IsPlayer(this long faction)
		{
			return !MyAPIGateway.Session.Factions.TryGetFactionById(faction).IsEveryoneNpc();
		}

		public static bool IsNpc(this long faction)
		{
			return MyAPIGateway.Session.Factions.TryGetFactionById(faction).IsEveryoneNpc();
		}

		public static bool ValidateFactions(IMyFaction leftFaction, IMyFaction rightFaction)
		{
			return (leftFaction == null || rightFaction == null);
		}

		#region Debug methods - should not be used in production code

		public static void PrintTerminalActions(IMyEntity block)
		{
			IMyTerminalBlock myTerminalBlock = block as IMyTerminalBlock;
			if (myTerminalBlock == null) return;
			List<ITerminalAction> results = new List<ITerminalAction>();
			myTerminalBlock.GetActions(results);
			foreach (ITerminalAction terminalAction in results)
			{
				StaticLog.WriteToLog("PrintTerminalActions", $"Actions: {terminalAction.Id} | {terminalAction.Name}", LogType.General);
			}
		}

		#endregion

	}
}
