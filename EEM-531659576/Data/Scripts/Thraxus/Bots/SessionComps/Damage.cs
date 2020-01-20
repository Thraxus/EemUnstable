using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
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
using VRage.Utils;
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
			public readonly long PlayerId;
			public readonly long Tick;

			public DamageEvent(long shipId, long playerId, long tick)
			{
				ShipId = shipId;
				PlayerId = playerId;
				Tick = tick;
			}

			private bool Equals(DamageEvent other)
			{
				return ShipId == other.ShipId && PlayerId == other.PlayerId && Tick + 2 >= other.Tick;
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
					hashCode = (hashCode * 397) ^ PlayerId.GetHashCode();
					hashCode = (hashCode * 397) ^ Tick.GetHashCode();
					return hashCode;
				}
			}

			public override string ToString()
			{
				return $"{ShipId} | {PlayerId} | {Tick}";
			}
		}

		// Events
		public static event Action<long, long> TriggerAlert;

		// Setup
		protected override void EarlySetup()
		{
			base.EarlySetup();
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(Priority, BeforeDamageHandler);
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(Priority, AfterDamageHandler);
			MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(Priority, AfterDamageHandler);
		}
		
		// Close
		protected override void Unload()
		{
			UnownedMissiles?.Clear();
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

		private void AddToDamageQueue(long shipId, long playerId)
		{
			AddToDamageQueue(new DamageEvent(shipId, playerId, TickCounter));
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
				TriggerAlert?.Invoke(damageEvent.ShipId, damageEvent.PlayerId);
				_damageEventList.Remove(damageEvent);
			}
			_damageEventList.ApplyRemovals();
		}

		private bool DamageAlreadyInQueue(DamageEvent damage)
		{
			return _damageEventList.Contains(damage);
		}

		// Damage Handlers

		private void AfterDamageHandler(object target, MyDamageInformation info)
		{
			//if (info.IsDeformation || info.Amount <= 0f) return;
			//if (info.Type == MyStringHash.GetOrCompute("Deformation")) return;
			//IMySlimBlock block = target as IMySlimBlock;
			//if (block == null) return;
			//AddToDamageQueue(new DamageEvent(block.CubeGrid.EntityId, 0L, TickCounter));
		}

		private void BeforeDamageHandler(object target, ref MyDamageInformation info)
		{
			//WriteToLog("BeforeDamageHandler", $"{info.AttackerId} | {info.Amount} | {info.Type}", LogType.General);
			if (info.IsDeformation) return;
			IMySlimBlock block = target as IMySlimBlock;
			if (block == null) return;
			ProcessPreDamageReporting(new DamageEvent(block.CubeGrid.EntityId, info.AttackerId, TickCounter), info);
		}

		private void ProcessPreDamageReporting(DamageEvent damage, MyDamageInformation info)
		{
			if (_preDamageEvents.Contains(damage)) return;
			_preDamageEvents.Add(damage);
			_preDamageEvents.ApplyAdditions();
			IdentifyDamageDealer(damage.ShipId, info);
			WriteToLog("ProcessPreDamageReporting", $"{damage} | {info.AttackerId} | {info.Amount} | {info.Type}", LogType.General);
		}

		private void CleanPreDamageEvents()
		{
			foreach (DamageEvent damageEvent in _preDamageEvents)
			{
				if (damageEvent.Tick + 1 < TickCounter)
				{
					_preDamageEvents.Remove(damageEvent);
					WriteToLog("CleanPreDamageEvents", $"Removed: {damageEvent} | {TickCounter}", LogType.General);
				}
			}
			_preDamageEvents.ApplyRemovals();
		}

		// Supporting Methods
		private void IdentifyDamageDealer(long damagedEntity, MyDamageInformation damageInfo)
		{
			// Deformation damage must be allowed here since it handles grid collision damage
			// One idea may be scan loaded mods and grab their damage types for their ammo as well?  We'll see... 
			// Missiles from vanilla launchers track their damage id back to the player, so if unowned or npc owned, they will have no owner - need to entity track missiles, woo! (on entity add)

			try
			{
				IMyEntity attackingEntity;
				if (damageInfo.AttackerId == 0)
				{   // possible instance of a missile getting through to here, need to account for it here or dismiss the damage outright if  no owner can be found
					//_instance.WriteToLog("IdentifyDamageDealer", $"AttackedId was 0!: {damageInfo.Type.String} - {damageInfo.Amount} - {damageInfo.IsDeformation} - {damageInfo.AttackerId}", LogType.General);
					CheckForUnownedMissileDamage(damagedEntity);
					return;
				}

				if (!MyAPIGateway.Entities.TryGetEntityById(damageInfo.AttackerId, out attackingEntity)) return;
				//_instance.WriteToLog("IdentifyDamageDealer", $"All the info: {damageInfo.Type.String} - {damageInfo.Amount} - {damageInfo.IsDeformation} - {damageInfo.AttackerId} - {attackingEntity.EntityId} - {attackingEntity.GetType()}", LogType.General);
				FindTheAsshole(damagedEntity, attackingEntity, damageInfo);
			}
			catch (Exception e)
			{
				WriteToStaticLog("IdentifyDamageDealer", e.ToString(), LogType.Exception);
			}
		}

		private void FindTheAsshole(long damagedEntity, IMyEntity attacker, MyDamageInformation damageInfo)
		{
			if (attacker.GetType() == typeof(MyCubeGrid))
			{
				//_instance.WriteToLog("FindTheAsshole", $"Asshole Identified as a CubeGrid!", LogType.General);
				IdentifyOffendingIdentityFromEntity(damagedEntity, attacker);
				return;
			}

			// TODO Grid weapons aren't being detected properly now;  need to get owner from weapon and declare war against them. MUST BE NPC FACTION AWARE for proper declarations between factions, so, for instance, this must report MGE Faction Leader if they are responsible for damage

			if (attacker is IMyLargeTurretBase)
			{
				//_instance.WriteToLog("FindTheAsshole", $"Asshole Identified as a Grid Weapon!", LogType.General);
				//RegisterWarEvent(damagedEntity, attacker.EntityId);
				IdentifyOffendingIdentityFromEntity(damagedEntity, attacker);
				return;
			}

			IMyCharacter myCharacter = attacker as IMyCharacter;
			if (myCharacter != null)
			{
				//_instance.WriteToLog("FindTheAsshole", $"Asshole Identified as a Character! War against: {MyAPIGateway.Entities.GetEntityById(myCharacter.EntityId).DisplayName}", LogType.General);
				AddToDamageQueue(damagedEntity, myCharacter.EntityId);
				return;
			}

			IMyAutomaticRifleGun myAutomaticRifle = attacker as IMyAutomaticRifleGun;
			if (myAutomaticRifle != null)
			{
				//_instance.WriteToLog("FindTheAsshole", $"Asshole Identified as an Engineer Rifle! War against: {MyAPIGateway.Players.GetPlayerById(myAutomaticRifle.OwnerIdentityId).DisplayName}", LogType.General);
				AddToDamageQueue(damagedEntity, myAutomaticRifle.OwnerIdentityId);
				return;
			}

			IMyAngleGrinder myAngleGrinder = attacker as IMyAngleGrinder;
			if (myAngleGrinder != null)
			{
				//_instance.WriteToLog("FindTheAsshole", $"Asshole Identified as an Engineer Grinder! War against: {MyAPIGateway.Players.GetPlayerById(myAngleGrinder.OwnerIdentityId).DisplayName}", LogType.General);
				AddToDamageQueue(damagedEntity, myAngleGrinder.OwnerIdentityId);
				return;
			}

			IMyHandDrill myHandDrill = attacker as IMyHandDrill;
			if (myHandDrill != null)
			{
				//_instance.WriteToLog("FindTheAsshole", $"Asshole Identified as an Engineer Drill! War against: {MyAPIGateway.Players.GetPlayerById(myHandDrill.OwnerIdentityId).DisplayName}", LogType.General);
				AddToDamageQueue(damagedEntity, myHandDrill.OwnerIdentityId);
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
				//_instance.WriteToLog("FindTheAsshole", $"Asshole Identified as a Thruster!", LogType.General);
				IdentifyOffendingIdentityFromEntity(damagedEntity, attacker);
				return;
			}

			WriteToLog("FindTheAsshole", $"Asshole not identified!!!  It was a: {attacker.GetType()}", LogType.General);
		}

		private void CheckForUnownedMissileDamage(long damagedEntity)
		{
			try
			{
				//_instance.WriteToLog("CheckForUnownedMissileDamage", $"Debug 1 - ActiveShipRegistry Count: {BotMarshal.ActiveShipRegistry.Count} | unownedMissiles: {_unownedMissiles.Count}", LogType.General);
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
						IdentifyOffendingIdentityFromEntity(damagedEntity, MyAPIGateway.Entities.GetEntityById(UnownedMissiles[i].LauncherId));
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

		private void IdentifyOffendingIdentityFromEntity(long damagedEntity, IMyEntity offendingEntity)
		{
			try
			{
				//_instance.WriteToLog("IdentifyOffendingIdentityFromEntity", $"Identifying this: {damagedEntity} | {offendingEntity?.EntityId}", LogType.General);
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
							//_instance.WriteToLog("IdentifyOffendingPlayerFromEntity", $"War Target Identified via PlayerShipControllerHistory: {myPlayer.DisplayName}", LogType.General);
							AddToDamageQueue(damagedEntity, myPlayer.IdentityId);
							return;
						}
					}

					List<MyEntity> detectEntitiesInSphere = (List<MyEntity>)Statics.DetectPlayersInSphere(myCubeGrid.GetPosition(), BotSettings.UnownedGridDetectionRange);
					foreach (MyEntity myDetectedEntity in detectEntitiesInSphere)
					{
						myPlayer = MyAPIGateway.Players.GetPlayerById(BotMarshal.PlayerShipControllerHistory[myDetectedEntity.EntityId]);
						if (myPlayer == null || myPlayer.IsBot) continue;
						//_instance.WriteToLog("IdentifyOffendingPlayerFromEntity", $"War Target Identified via Detection: {myPlayer.DisplayName}", LogType.General);
						AddToDamageQueue(damagedEntity, myPlayer.IdentityId);
					}
					return;
				}

				IMyIdentity myIdentity = Statics.GetIdentityById(myCubeGrid.BigOwners.FirstOrDefault());
				if (myIdentity != null)
				{
					//_instance.WriteToLog("IdentifyOffendingPlayerFromEntity", $"War Target Identified via BigOwners: {myIdentity.DisplayName}", LogType.General);
					AddToDamageQueue(damagedEntity, myIdentity.IdentityId);
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
