using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Bots.Models;
using Eem.Thraxus.Bots.Settings;
using Eem.Thraxus.Common;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Utilities;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Utilities
{
	public static class DamageHandler
	{
		private const string DamageHandlerLogName = "DamageHandler-General";
		private const string DamageHandlerDebugLogName = "DamageHandler-Debug";

		private const int Priority = int.MinValue;

		private static List<MissileHistory> _unownedMissiles;
		private static ConcurrentDictionary<long, ThrusterDamageTracker> _thrusterDamageTrackers;

		private static Log _damageHandlerLog;
		private static Log _damageHandlerDebugLog;

		public static event TriggerAlertRequest TriggerAlert;
		public delegate void TriggerAlertRequest(long shipId);

		public static void Run()
		{
			_damageHandlerLog = new Log(DamageHandlerLogName);
			_damageHandlerDebugLog = new Log(DamageHandlerDebugLogName);
			_unownedMissiles = new List<MissileHistory>();
			_thrusterDamageTrackers = new ConcurrentDictionary<long, ThrusterDamageTracker>();
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(Priority, MyBeforeDamageHandler);

			//MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(Priority, MyAfterDamageHandler);
			//MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(Priority, MyDestroyHandler);
		}

		private static void ControllerInfoOnControlAcquired(IMyEntityController obj)
		{
			WriteToLog("ControllerInfoOnControlAcquired", $"Object Controlled! {obj.ControlledEntity.Entity.EntityId} - {obj.ControlledEntity.Entity.DisplayName}", true);
		}

		public static void Unload()
		{
			_damageHandlerLog?.Close();
			_damageHandlerDebugLog?.Close();
			_unownedMissiles?.Clear();
			_thrusterDamageTrackers.Clear();
		}

		private static void MyBeforeDamageHandler(object target, ref MyDamageInformation info)
		{   // Important one, should show all damage events, even instant destruction
			// Testing shows meteors don't do damage, just remove blocks and do deformation, so not bothering to track that for now... 
			// BaRS doesn't show up here at all, will need to make a special case processed on the grid to account for BaRS damage using integrity lowering with no matching damage case
			//	idea there is basically to declare war on any BaRS system owner within 250m of the grid to account for fly by block grinding

			if (info.IsDeformation) return;
			IMySlimBlock block = target as IMySlimBlock;
			if (block == null) return;

			//WriteToLog("MyBeforeDamageHandler", $"Invoked with {target} taking {info.Amount} damage from {info.AttackerId}", true);

			long damagedEntity = IdentifyCubeGrid(block);
			long damageDealer = IdentifyDamageDealer(damagedEntity, info);
		}

		private static long IdentifyCubeGrid(IMySlimBlock block)
		{
			//WriteToLog("IdentifyDamagedEntity", $"The damaged block belongs to entity: {block.CubeGrid.DisplayName}", true);
			return block.CubeGrid.EntityId;
		}

		private static long IdentifyDamageDealer(long damagedEntity, MyDamageInformation damageInfo)
		{
			// Deformation damage must be allowed here since it handles grid collision damage
			// One idea may be scan loaded mods and grab their damage types for their ammo as well?  We'll see... 
			// Missiles from vanilla launchers track their damage id back to the player, so if unowned or npc owned, they will have no owner - need to entity track missiles, woo! (on entity add)


			try
			{
				//WriteToLog("IdentifyDamageDealer", $"All the info: {damageInfo.Type.String} - {damageInfo.Amount} - {damageInfo.IsDeformation} - {damageInfo.AttackerId}", true);
				long damageDealer = 0;
				IMyEntity attackingEntity;

				if (damageInfo.AttackerId == 0)
				{
					// possible instance of a missile getting through to here, need to account for it here or dismiss the damage outright if  no owner can be found
					//WriteToLog("IdentifyDamageDealer", $"Possible missile: {damageInfo.Type.String} - {damageInfo.Amount} - {damageInfo.IsDeformation} - {damageInfo.AttackerId}", true);
					CheckForUnownedMissileDamage(damageInfo);
					return 0;
				}

				if (!MyAPIGateway.Entities.TryGetEntityById(damageInfo.AttackerId, out attackingEntity)) return 0;

				WriteToLog("IdentifyDamageDealer", $"All the info: {damageInfo.Type.String} - {damageInfo.Amount} - {damageInfo.IsDeformation} - {damageInfo.AttackerId} - {attackingEntity.EntityId} - {attackingEntity.GetType()}", true);
				FindTheAsshole(damagedEntity, attackingEntity, damageInfo);


				//if (damageInfo.Type.GetType() == typeof(IMyEngineerToolBase))
				//	WriteToLog("IdentifyDamageDealer", $"The damaged block comes from a player tool", true);

				//if (damageInfo.Type.GetType() == typeof(MyGunBase))
				//	WriteToLog("IdentifyDamageDealer", $"The damaged block comes from a player weapon", true);

				//WriteToLog("IdentifyDamageDealer", $"Actual type: {damageInfo.Type.GetType()} -- {damageInfo.Type.String}", true);

				return damageDealer;
			}
			catch (Exception e)
			{
				ExceptionLog("IdentifyDamageDealer", e.ToString());
				return 0;
			}


		}

		private static void FindTheAsshole(long damagedEntity, IMyEntity attacker, MyDamageInformation damageInfo)
		{
			if (attacker.GetType() == typeof(MyCubeGrid))
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as a CubeGrid!", true);
				IdentifyOffendingPlayerFromEntity(attacker.EntityId);
				return;
			}

			IMyCharacter myCharacter = attacker as IMyCharacter;
			if (myCharacter != null)
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as a Character! War against: {MyAPIGateway.Entities.GetEntityById(myCharacter.EntityId).DisplayName}", true);
				return;
			}

			IMyAutomaticRifleGun myAutomaticRifle = attacker as IMyAutomaticRifleGun;
			if (myAutomaticRifle != null)
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as an Engineer Rifle! War against: {MyAPIGateway.Players.GetPlayerById(myAutomaticRifle.OwnerIdentityId).DisplayName}", true);
				return;
			}

			IMyAngleGrinder myAngleGrinder = attacker as IMyAngleGrinder;
			if (myAngleGrinder != null)
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as an Engineer Grinder! War against: {MyAPIGateway.Players.GetPlayerById(myAngleGrinder.OwnerIdentityId).DisplayName}", true);
				return;
			}

			IMyHandDrill myHandDrill = attacker as IMyHandDrill;
			if (myHandDrill != null)
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as an Engineer Drill! War against: {MyAPIGateway.Players.GetPlayerById(myHandDrill.OwnerIdentityId).DisplayName}", true);
				return;
			}

			IMyThrust myThruster = attacker as IMyThrust;
			if (myThruster != null)
			{

				long damagedTopMost = MyAPIGateway.Entities.GetEntityById(damagedEntity).GetTopMostParent().EntityId;
				if (!BotMarshal.ActiveShipRegistry.Contains(damagedTopMost)) return;
				if (!_thrusterDamageTrackers.TryAdd(damagedTopMost, new ThrusterDamageTracker(attacker.EntityId, damageInfo.Amount)))
					_thrusterDamageTrackers[damagedTopMost].DamageTaken += damageInfo.Amount;
				if (!_thrusterDamageTrackers[damagedTopMost].ThresholdReached) return;
				WriteToLog("FindTheAsshole", $"Asshole Identified as a Thruster!", true);
				IdentifyOffendingPlayerFromEntity(attacker.EntityId);
				return;
			}

			WriteToLog("FindTheAsshole", $"Asshole not identified!!!  It was a: {attacker.GetType()}", true);
		}

		private static void CheckForUnownedMissileDamage(MyDamageInformation damageInfo)
		{
			try
			{
				for (int i = _unownedMissiles.Count - 1; i >= 0; i--)
				{
					bool identified = false;
					for (int index = BotMarshal.ActiveShipRegistry.Count - 1; index >= 0; index--)
					{
						if (!CheckForImpactedEntity(BotMarshal.ActiveShipRegistry[index], _unownedMissiles[i].Location)) continue;
						//WriteToLog("CheckForUnownedMissileDamage", $"Unknown Missile {_unownedMissiles[i].LauncherId} hit {BotMarshal.ActiveShipRegistry[index]}", true);
						IdentifyOffendingPlayerFromEntity(_unownedMissiles[i].LauncherId);
						identified = true;
					}

					if (_unownedMissiles[i].TimeStamp.AddSeconds(10) > DateTime.Now || identified)
						_unownedMissiles.RemoveAtFast(i);
				}
			}
			catch (Exception e)
			{
				ExceptionLog("CheckForUnownedMissileDamage", e.ToString());
			}
		}

		private static void IdentifyOffendingPlayerFromEntity(long offendingEntity)
		{
			try
			{
				//WriteToLog("IdentifyOffendingPlayerFromEntity", $"offendingGridId: {offendingEntity}", true);
				IMyEntity myEntity;
				if (!MyAPIGateway.Entities.TryGetEntityById(offendingEntity, out myEntity)) return;
				IMyCubeGrid myCubeGrid = myEntity.GetTopMostParent() as IMyCubeGrid;
				if (myCubeGrid == null) return;
				IMyPlayer myPlayer;
				if (myCubeGrid.BigOwners.Count == 0)
				{
					long tmpId;
					if (BotMarshal.PlayerShipControllerHistory.TryGetValue(myCubeGrid.EntityId, out tmpId))
					{ //{MyAPIGateway.Players.GetPlayerById(BotMarshal.PlayerShipControllerHistory[myCubeGrid.EntityId]).DisplayName}
					  //myPlayer = MyAPIGateway.Players.GetPlayerById(BotMarshal.PlayerShipControllerHistory[myCubeGrid.EntityId]);
					  myPlayer = MyAPIGateway.Players.GetPlayerById(tmpId);
						if (myPlayer != null && !myPlayer.IsBot)
							WriteToLog("IdentifyOffendingPlayerFromEntity", $"War Target Identified via PlayerShipControllerHistory: {myPlayer.DisplayName}", true);
						return;
					}

					List<MyEntity> detectEntitiesInSphere = (List<MyEntity>)StaticMethods.DetectPlayersInSphere(myCubeGrid.GetPosition(), Constants.UnownedGridDetectionRange);
					foreach (MyEntity myDetectedEntity in detectEntitiesInSphere)
					{
						myPlayer = MyAPIGateway.Players.GetPlayerById(BotMarshal.PlayerShipControllerHistory[myDetectedEntity.EntityId]);
						if (myPlayer != null && !myPlayer.IsBot)
							WriteToLog("IdentifyOffendingPlayerFromEntity", $"War Target Identified via Detection: {myPlayer.DisplayName}", true);
					}
					return;
				}

				myPlayer = MyAPIGateway.Players.GetPlayerById(myCubeGrid.BigOwners.FirstOrDefault());
				if (myPlayer != null && !myPlayer.IsBot)
				{
					WriteToLog("IdentifyOffendingPlayerFromEntity", $"War Target Identified via BigOwners: {myPlayer.DisplayName}", true);
					return;
				}

				WriteToLog("IdentifyOffendingPlayerFromEntity", $"War Target is an elusive shithead!", true);
			}
			catch (Exception e)
			{
				ExceptionLog("IdentifyOffendingPlayer", e.ToString());
			}
		}

		private static bool CheckForImpactedEntity(long entityId, Vector3D impactLocation)
		{
			try
			{
				IMyEntity myEntity;
				if (!MyAPIGateway.Entities.TryGetEntityById(entityId, out myEntity)) return false;
				return myEntity.PositionComp.WorldAABB.Contains(impactLocation) == ContainmentType.Contains;
			}
			catch (Exception e)
			{
				ExceptionLog("CheckForImpactedEntity", e.ToString());
				return false;
			}
		}

		public static void RegisterNewMissile(MissileHistory missileInfo)
		{
			MyAPIGateway.Session.GPS.AddGps(MyAPIGateway.Session.LocalHumanPlayer.IdentityId, MyAPIGateway.Session.GPS.Create($"Add: {missileInfo.LauncherId} -- {missileInfo.OwnerId}", "", missileInfo.Location, true));

		}

		public static void RegisterUnownedMissileImpact(MissileHistory missileInfo)
		{
			MyAPIGateway.Session.GPS.AddGps(MyAPIGateway.Session.LocalHumanPlayer.IdentityId, MyAPIGateway.Session.GPS.Create($"Remove: {missileInfo.LauncherId} -- {missileInfo.OwnerId}", "", missileInfo.Location, true));
			if (missileInfo.OwnerId == 0) _unownedMissiles.Add(missileInfo);
			WriteToLog("RegisterUnownedMissileImpact", $"Missile {missileInfo.LauncherId} added to registry.", true);
		}

		private static void MyDestroyHandler(object target, MyDamageInformation info)
		{   // Potentially Unused
			//WriteToLog("MyDestroyHandler", $"Invoked with {target} taking {info.Amount} damage from {info.AttackerId}", true);
		}

		private static void MyAfterDamageHandler(object target, MyDamageInformation info)
		{   // Potentially Unused
			//WriteToLog("MyAfterDamageHandler", $"Invoked with {target} taking {info.Amount} damage from {info.AttackerId}", true);
		}

		private static readonly object WriteLocker = new object();

		private static void WriteToLog(string caller, string message, bool general = false)
		{
			lock (WriteLocker)
			{
				if (general) _damageHandlerLog?.WriteToLog(caller, message);
				_damageHandlerDebugLog?.WriteToLog(caller, message);
			}
		}

		private static void ExceptionLog(string caller, string message)
		{
			WriteToLog(caller, $"Exception! {message}", true);
		}



	}
}
