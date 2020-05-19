using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.Statics;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.SessionComps
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 1000)]
	public class Damage : BaseServerSessionComp
	{
		private const string GeneralLogName = "DamageGeneral";
		private const string DebugLogName = "DamageDebug";
		private const string SessionCompName = "Damage";

		public Damage() : base(GeneralLogName, DebugLogName, SessionCompName, false) { } // Do nothing else

		// Static Fields
		private static readonly List<MissileHistory> UnownedMissiles = new List<MissileHistory>();

		// Fields

		// Collections
		private readonly ConcurrentDictionary<long, ThrusterDamageTracker> _thrusterDamageTrackers = new ConcurrentDictionary<long, ThrusterDamageTracker>();
		private readonly ConcurrentCachingList<DamageEvent> _damageEventList = new ConcurrentCachingList<DamageEvent>();
		private readonly ConcurrentCachingList<DamageEvent> _preDamageEvents = new ConcurrentCachingList<DamageEvent>();

		// Structs
		private struct DamageEvent
		{
			public readonly long ShipId;
			public readonly long BlockId;
			public readonly long PlayerId;
			public readonly long Tick;

			public DamageEvent(long shipId, long blockId, long playerId, long tick)
			{
				ShipId = shipId;
				BlockId = blockId;
				PlayerId = playerId;
				Tick = tick;
			}

			private bool Equals(DamageEvent other)
			{
				return ShipId == other.ShipId && BlockId == other.BlockId && PlayerId == other.PlayerId && Tick + 2 >= other.Tick;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				return obj is DamageEvent && Equals((DamageEvent) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hashCode = ShipId.GetHashCode();
					hashCode = (hashCode * 397) ^ BlockId.GetHashCode();
					hashCode = (hashCode * 397) ^ PlayerId.GetHashCode();
					hashCode = (hashCode * 397) ^ Tick.GetHashCode();
					return hashCode;
				}
			}

			public override string ToString()
			{
				return $"{ShipId} | {BlockId} | {PlayerId} | {Tick}";
			}
		}

		// Events
		public static event Action<long, long, long> TriggerAlert;

		// Setup
		protected override void EarlySetup()
		{
			base.EarlySetup();
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(Priority, BeforeDamageHandler);
		}
		
		// Close
		protected override void Unload()
		{
			UnownedMissiles?.Clear();
			_damageEventList?.ClearList();
			_preDamageEvents?.ClearList();
			_thrusterDamageTrackers.Clear();
			base.Unload();
		}

		// Session Methods

		protected override void RunBeforeSimUpdate()
		{
			base.RunBeforeSimUpdate();
			ProcessDamageQueue();
			CleanPreDamageEvents();
		}

		// Damage Queue

		private void AddToDamageQueue(long shipId, long blockId, long playerId)
		{
			AddToDamageQueue(new DamageEvent(shipId, blockId, playerId, TickCounter));
		}

		private void AddToDamageQueue(DamageEvent damage)
		{
			if (DamageAlreadyInQueue(damage)) return;
			_damageEventList.Add(damage);
			_damageEventList.ApplyAdditions();
			WriteToLog("AddToDamageQueue", $"{damage}", LogType.General);
		}

		private void ProcessDamageQueue()
		{
			foreach (DamageEvent damageEvent in _damageEventList)
			{
				TriggerAlert?.Invoke(damageEvent.ShipId, damageEvent.BlockId, damageEvent.PlayerId);
				_damageEventList.Remove(damageEvent);
			}
			_damageEventList.ApplyRemovals();
		}

		private bool DamageAlreadyInQueue(DamageEvent damage)
		{
			return _damageEventList.Contains(damage);
		}

		// Damage Handlers

		private void BeforeDamageHandler(object target, ref MyDamageInformation info)
		{
			//WriteToLog("BeforeDamageHandler", $"{info.AttackerId} | {info.Amount} | {info.Type}", LogType.General);
			if (info.IsDeformation) return;
			IMySlimBlock block = target as IMySlimBlock;
			if (block == null) return;
			long blockId = 0;
			IMyCubeBlock fatBlock = block.FatBlock;
			if (fatBlock != null) blockId = fatBlock.EntityId;
			ProcessPreDamageReporting(new DamageEvent(block.CubeGrid.EntityId, blockId, info.AttackerId, TickCounter), info);
		}

		private void ProcessPreDamageReporting(DamageEvent damage, MyDamageInformation info)
		{
			if (_preDamageEvents.Contains(damage)) return;
			_preDamageEvents.Add(damage);
			_preDamageEvents.ApplyAdditions();
			IdentifyDamageDealer(damage.ShipId, damage.BlockId, info);
		}

		private void CleanPreDamageEvents()
		{
			foreach (DamageEvent damageEvent in _preDamageEvents)
			{
				if (damageEvent.Tick + 1 < TickCounter)
				{
					_preDamageEvents.Remove(damageEvent);
				}
			}
			_preDamageEvents.ApplyRemovals();
		}

		// Supporting Methods
		private void IdentifyDamageDealer(long damagedEntity, long damagedBlock, MyDamageInformation damageInfo)
		{
			// Deformation damage must be allowed here since it handles grid collision damage
			// One idea may be scan loaded mods and grab their damage types for their ammo as well?  We'll see... 
			// Missiles from vanilla launchers track their damage id back to the player, so if unowned or npc owned, they will have no owner - need to entity track missiles, woo! (on entity add)

			try
			{
				IMyEntity attackingEntity;
				if (damageInfo.AttackerId == 0)
				{   // possible instance of a missile getting through to here, need to account for it here or dismiss the damage outright if  no owner can be found
					CheckForUnownedMissileDamage(damagedEntity, damagedBlock);
					return;
				}

				if (!MyAPIGateway.Entities.TryGetEntityById(damageInfo.AttackerId, out attackingEntity)) return;
				FindTheAsshole(damagedEntity, damagedBlock, attackingEntity, damageInfo);
			}
			catch (Exception e)
			{
				WriteToStaticLog("IdentifyDamageDealer", e.ToString(), LogType.Exception);
			}
		}

		private void FindTheAsshole(long damagedEntity, long damagedBlock, IMyEntity attacker, MyDamageInformation damageInfo)
		{
			if (attacker.GetType() == typeof(MyCubeGrid))
			{
				IdentifyOffendingIdentityFromEntity(damagedEntity, damagedBlock, attacker);
				return;
			}

			if (attacker is IMyLargeTurretBase)
			{
				IdentifyOffendingIdentityFromEntity(damagedEntity, damagedBlock, attacker);
				return;
			}

			IMyCharacter myCharacter = attacker as IMyCharacter;
			if (myCharacter != null)
			{
				AddToDamageQueue(damagedEntity, damagedBlock, myCharacter.EntityId);
				return;
			}

			IMyAutomaticRifleGun myAutomaticRifle = attacker as IMyAutomaticRifleGun;
			if (myAutomaticRifle != null)
			{
				AddToDamageQueue(damagedEntity, damagedBlock, myAutomaticRifle.OwnerIdentityId);
				return;
			}

			IMyAngleGrinder myAngleGrinder = attacker as IMyAngleGrinder;
			if (myAngleGrinder != null)
			{
				AddToDamageQueue(damagedEntity, damagedBlock, myAngleGrinder.OwnerIdentityId);
				return;
			}

			IMyHandDrill myHandDrill = attacker as IMyHandDrill;
			if (myHandDrill != null)
			{
				AddToDamageQueue(damagedEntity, damagedBlock, myHandDrill.OwnerIdentityId);
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
				IdentifyOffendingIdentityFromEntity(damagedEntity, damagedBlock, attacker);
				return;
			}

			WriteToLog("FindTheAsshole", $"Asshole not identified!!!  It was a: {attacker.GetType()}", LogType.General);
		}

		private void CheckForUnownedMissileDamage(long damagedEntity, long damagedBlock)
		{
			try
			{
				for (int i = UnownedMissiles.Count - 1; i >= 0; i--)
				{
					TimeSpan timeSinceLastAttack = DateTime.Now - UnownedMissiles[i].TimeStamp;
					if (timeSinceLastAttack.TotalSeconds > 5f)
					{
						UnownedMissiles.RemoveAtFast(i);
						continue;
					}
					bool identified = false;
					for (int index = BotMarshal.ActiveShipRegistry.Count - 1; index >= 0; index--)
					{
						if (!CheckForImpactedEntity(BotMarshal.ActiveShipRegistry[index], UnownedMissiles[i].Location)) continue;
						IdentifyOffendingIdentityFromEntity(damagedEntity, damagedBlock, MyAPIGateway.Entities.GetEntityById(UnownedMissiles[i].LauncherId));
						identified = true;
					}

					if (identified)
						UnownedMissiles.RemoveAtFast(i);
				}
			}
			catch (Exception e)
			{
				WriteToStaticLog("CheckForUnownedMissileDamage", e.ToString(), LogType.Exception);
			}
		}

		private void IdentifyOffendingIdentityFromEntity(long damagedEntity, long damagedBlock, IMyEntity offendingEntity)
		{
			try
			{
				IMyCubeGrid myCubeGrid = offendingEntity?.GetTopMostParent() as IMyCubeGrid;
				if (myCubeGrid == null) return;
				if (myCubeGrid.BigOwners.Count == 0)
				{   // This should only trigger when a player is being a cheeky fucker
					IMyPlayer myPlayer;
					long tmpId;
					if (BotMarshal.PlayerShipControllerHistory.TryGetValue(myCubeGrid.EntityId, out tmpId))
					{
						myPlayer = MyAPIGateway.Players.GetPlayerById(tmpId);
						if (myPlayer != null && !myPlayer.IsBot)
						{
							AddToDamageQueue(damagedEntity, damagedBlock, myPlayer.IdentityId);
							return;
						}
					}

					List<MyEntity> detectEntitiesInSphere = (List<MyEntity>)Statics.DetectTopMostEntitiesInSphere(myCubeGrid.GetPosition(), BotSettings.UnownedGridDetectionRange);
					foreach (MyEntity myDetectedEntity in detectEntitiesInSphere)
					{
						myPlayer = MyAPIGateway.Players.GetPlayerById(BotMarshal.PlayerShipControllerHistory[myDetectedEntity.EntityId]);
						if (myPlayer == null || myPlayer.IsBot) continue;
						AddToDamageQueue(damagedEntity, damagedBlock, myPlayer.IdentityId);
					}
					return;
				}

				IMyIdentity myIdentity = Statics.GetIdentityById(myCubeGrid.BigOwners.FirstOrDefault());
				if (myIdentity != null)
				{
					AddToDamageQueue(damagedEntity, damagedBlock, myIdentity.IdentityId);
					return;
				}

				WriteToLog("IdentifyOffendingPlayerFromEntity", $"War Target is an elusive shithead! {myCubeGrid.BigOwners.FirstOrDefault()}", LogType.General);
			}
			catch (Exception e)
			{
				WriteToStaticLog("IdentifyOffendingPlayer", e.ToString(), LogType.Exception);
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
				WriteToStaticLog("CheckForImpactedEntity", e.ToString(), LogType.Exception);
				return false;
			}
		}

		//public static void BarsSuspected(IMyEntity shipEntity)
		//{   // Takes the ship id for the grid suspected of being under attack by BARS and casts spells to see if its true or not
		//	WriteToLog("BarsSuspected", $"Triggered by: {shipEntity.DisplayName}", LogType.General);
		//	List<IMyEntity> detectedBars = BuildAndRepairSystem.DetectAllBars(shipEntity.GetPosition(), BotSettings.UnownedGridDetectionRange);
		//	if (detectedBars.Count == 0) return;
		//	foreach (IMyEntity bars in detectedBars)
		//	{
		//		//BotMarshal.RegisterNewPriorityTarget(shipEntity.EntityId, new TargetEntity(bars, BaseTargetPriorities.Bars));
		//		Statics.AddGpsLocation("BaRS", bars.GetPosition());
		//		//((IMyCubeBlock)bars).SlimBlock.DecreaseMountLevel(((IMyCubeBlock)bars).SlimBlock.Integrity - 10, null);
		//		((IMyCubeBlock)bars).SlimBlock.DoDamage(((IMyCubeBlock)bars).SlimBlock.Integrity * 0.9f, EnergyShields.BypassKey, true, new MyHitInfo(), 0L);
		//		Statics.CreateFakeSmallExplosion(bars.GetPosition());
		//		AddToDamageQueue(shipEntity.EntityId, ((IMyCubeBlock)bars).OwnerId);
		//	}
		//}

		public static void RegisterUnownedMissileImpact(MissileHistory missileInfo)
		{
			UnownedMissiles.Add(missileInfo);
		}

	}
}
