using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Factions.Networking;
using Eem.Thraxus.Helpers;
using Eem.Thraxus.Networking;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using IMyGridTerminalSystem = Sandbox.ModAPI.IMyGridTerminalSystem;
using IMyRadioAntenna = Sandbox.ModAPI.IMyRadioAntenna;
using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;
using IMyShipController = Sandbox.ModAPI.IMyShipController;
using IMyTerminalBlock = Sandbox.ModAPI.IMyTerminalBlock;
using IMyThrust = Sandbox.ModAPI.IMyThrust;

namespace Eem.Thraxus
{
	public abstract class BotBase
	{
		public IMyCubeGrid Grid { get; protected set; }

		protected readonly IMyGridTerminalSystem Term;

		public Vector3D GridPosition => Grid.GetPosition();

		public Vector3D GridVelocity => Grid.Physics.LinearVelocity;

		public float GridSpeed => (float)GridVelocity.Length();

		protected float GridRadius => (float)Grid.WorldVolume.Radius;

		protected bool Initialized
		{
			get
			{
				try
				{
					return Grid != null;
				}
				catch (Exception scrap)
				{
					LogError("initialized", scrap);
					return false;
				}
			}
		}

		public virtual bool IsInitialized => Initialized;

		public MyEntityUpdateEnum Update { get; protected set; }

		public IMyRemoteControl Rc { get; protected set; }

		private IMyFaction _ownerFaction;

		protected string DroneNameProvider => $"Drone_{Rc.EntityId}";

		public string DroneName
		{
			get
			{
				return Rc.Name;
			}
			protected set
			{
				IMyEntity entity = Rc;
				entity.Name = value;
				MyAPIGateway.Entities.SetEntityName(entity);
				//DebugWrite("DroneName_Set", $"Drone EntityName set to {RC.Name}");
			}
		}

		protected bool GridOperable
		{
			get
			{
				try
				{
					return !Grid.MarkedForClose && !Grid.Closed && Grid.InScene;
				}
				catch (Exception scrap)
				{
					LogError("gridoperable", scrap);
					return false;
				}
			}
		}

		protected bool BotOperable;

		protected bool Closed;

		public virtual bool Operable
		{
			get
			{
				try
				{
					return !Closed && IsInitialized && GridOperable && Rc.IsFunctional && BotOperable;
				}
				catch (Exception scrap)
				{
					LogError("Operable", scrap);
					return false;
				}
			}
		}

		public List<IMyRadioAntenna> Antennae { get; protected set; }

		public delegate void OnDamageTaken(IMySlimBlock damagedBlock, MyDamageInformation damage);

		protected event OnDamageTaken OnDamaged;

		public delegate void HOnBlockPlaced(IMySlimBlock block);

		protected event HOnBlockPlaced OnBlockPlaced;

		//protected event Action Alert;

		protected BotBase(IMyCubeGrid grid)
		{
			if (grid == null) return;
			Grid = grid;
			Term = grid.GetTerminalSystem();
			Antennae = new List<IMyRadioAntenna>();
		}

		public static BotTypeBase ReadBotType(IMyRemoteControl rc)
		{
			try
			{
				string customData = rc.CustomData.Trim().Replace("\r\n", "\n");
				List<string> myCustomData = new List<string>(customData.Split('\n'));

				if (customData.IsNullEmptyOrWhiteSpace()) return BotTypeBase.None;
				if (myCustomData.Count < 2)
				{
					if (Constants.AllowThrowingErrors) throw new Exception("CustomData is invalid", new Exception("CustomData consists of less than two lines"));
					return BotTypeBase.Invalid;
				}
				if (myCustomData[0].Trim() != "[EEM_AI]")
				{
					if (Constants.AllowThrowingErrors) throw new Exception("CustomData is invalid", new Exception($"AI tag invalid: '{myCustomData[0]}'"));
					return BotTypeBase.Invalid;
				}

				string[] bottype = myCustomData[1].Split(':');
				if (bottype[0].Trim() != "Type")
				{
					if (Constants.AllowThrowingErrors) throw new Exception("CustomData is invalid", new Exception($"Type tag invalid: '{bottype[0]}'"));
					return BotTypeBase.Invalid;
				}

				BotTypeBase botType = BotTypeBase.Invalid;

				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (bottype[1].Trim())
				{
					case "Fighter":
						botType = BotTypeBase.Fighter;
						break;
					case "Freighter":
						botType = BotTypeBase.Freighter;
						break;
					case "Carrier":
						botType = BotTypeBase.Carrier;
						break;
					case "Station":
						botType = BotTypeBase.Station;
						break;
				}

				return botType;
			}
			catch (Exception scrap)
			{
				rc.CubeGrid.LogError("[STATIC]BotBase.ReadBotType", scrap);
				return BotTypeBase.Invalid;
			}
		}

		protected virtual void DebugWrite(string source, string message, string debugPrefix = "BotBase.")
		{
			Grid.DebugWrite(debugPrefix + source, message);
		}

		public virtual bool Init(IMyRemoteControl rc = null)
		{
			Rc = rc ?? Term.GetBlocksOfType<IMyRemoteControl>(collect: x => x.IsFunctional).FirstOrDefault();
			if (rc == null) return false;
			DroneName = DroneNameProvider;

			Antennae = Term.GetBlocksOfType<IMyRadioAntenna>(collect: x => x.IsFunctional);

			bool hasSetup = ParseSetup();
			if (!hasSetup) return false;

			AiSessionCore.AddDamageHandler(Grid, (block, damage) => { OnDamaged?.Invoke(block, damage); });

			Grid.OnBlockAdded += block => { OnBlockPlaced?.Invoke(block); };

			_ownerFaction = Grid.GetOwnerFaction(true);

			BotOperable = true;

			return true;
		}

		//public virtual void RecompilePBs()
		//{
		//	foreach (IMyProgrammableBlock pb in Term.GetBlocksOfType<IMyProgrammableBlock>())
		//	{
		//		MyAPIGateway.Utilities.InvokeOnGameThread(() => { pb.Recompile(); });
		//	}
		//}

		protected void ReactOnDamage(MyDamageInformation damage, out IMyPlayer damager)
		{
			damager = null;
			try
			{
				AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage.AttackerId:\t{damage.AttackerId}");

				// Inconsequential damage sources, ignore them
				if (damage.IsDeformation || damage.IsMeteor() || damage.IsThruster())
					return;
				
				//if (damage.IsDoneByPlayer(out damager) && damager != null)
				//	if (damager.GetFaction() == null) return;

				if (damage.IsDoneByPlayer(out damager))
				{
					if (damager.GetFaction() == null) return;
					DeclareWar(damager.GetFaction());
					return;
				}
				
				// Issue here is damager == null at this point.  Damager is IMyPlayer
				// Since damager == null, DeclareWar fails.  
				// Need to check for damager == null here, if so, get pilot and declare war on them if they are in a faction

				List<IMyPlayer> possibleAttackingPlayers = new List<IMyPlayer>();
				MyAPIGateway.Players.GetPlayers(possibleAttackingPlayers, 
					player => 
						player.Controller.ControlledEntity.Entity != null &&
						!player.IsBot &&
						player.Character != null &&
						player.Controller.ControlledEntity.Entity is IMyShipController);
				foreach (IMyPlayer possibleAttackingPlayer in possibleAttackingPlayers)
				{
					if (((IMyShipController) possibleAttackingPlayer.Controller.ControlledEntity.Entity).SlimBlock.CubeGrid != 
					    MyAPIGateway.Entities.GetEntityById(damage.AttackerId)) continue;
					damager = possibleAttackingPlayer;
					IMyFaction attackingFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(possibleAttackingPlayer.IdentityId);
					if (attackingFaction == null)
						continue;
					DeclareWar(attackingFaction);
				}

				//HashSet<IMyEntity> possibleAttackingPlayers2 = new HashSet<IMyEntity>();
				//MyAPIGateway.Entities.GetEntities(possibleAttackingPlayers2, entity => entity is IMyShipController);
				//AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"EntListSize:\t{possibleAttackingPlayers2.Count}");

				//foreach (IMyPlayer possibleAttackingPlayer in possibleAttackingPlayers)
				//	AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"player:\t{possibleAttackingPlayer?.IdentityId}\t{possibleAttackingPlayer?.DisplayName}");

				//foreach (IMyEntity possibleAttackingPlayer in possibleAttackingPlayers2)
				//	AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"Entity:\t{possibleAttackingPlayer?.EntityId}\t{possibleAttackingPlayer?.DisplayName}");

				//AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage:\t{damage}");
				//AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage.Amount:\t{damage.Amount}");
				//AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage.AttackerId:\t{damage.AttackerId}");
				//AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damage.Type:\t{damage.Type}");
				//AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damager.IsNull:\t{damager == null}");
				//AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damager.GetFaction()\t{damager?.GetFaction()}");
				//AiSessionCore.DebugLog?.WriteToLog("ReactOnDamage", $"damager.GetFactionIsNull()\t{damager?.GetFaction() == null}");

			}
			catch (Exception scrap)
			{
				Grid.LogError("ReactOnDamage", scrap);
			}
		}

		// Get pilot of a ship
		// var playerEnt = MyAPIGateway.Session.ControlledObject?.Entity as MyEntity;
		// if (playerEnt?.Parent != null) playerEnt = playerEnt.Parent;

		protected void BlockPlacedHandler(IMySlimBlock block)
		{
			if (block == null) return;

			try
			{
				IMyPlayer builder;
				if (!block.IsPlayerBlock(out builder)) return;
				IMyFaction faction = builder.GetFaction();
				if (faction != null)
					DeclareWar(faction);
			}
			catch (Exception scrap)
			{
				Grid.LogError("BlockPlacedHandler", scrap);
			}
		}

		private void DeclareWar(IMyFaction playerFaction)
		{
			//if (MyAPIGateway.Session.IsServer) return;
			try
			{
				if (playerFaction == null) return;
				if (_ownerFaction == null)
					_ownerFaction = Grid.GetOwnerFaction();
				MyAPIGateway.Session.Factions.DeclareWar(_ownerFaction.FactionId, playerFaction.FactionId);
				//Factions.Factions.DeclareFactionWar(_ownerFaction, playerFaction);
			}
			catch (Exception e)
			{
				AiSessionCore.DebugLog?.WriteToLog("DeclareWar", $"Exception!\t{e}");
			}
		}

		//protected virtual void RegisterHostileAction(IMyPlayer player, TimeSpan truceDelay)
		//{
		//	try
		//	{
		//		#region Sanity checks
		//		if (player == null)
		//		{
		//			Grid.DebugWrite("RegisterHostileAction", "Error: Damager is null.");
		//			return;
		//		}

		//		if (_ownerFaction == null)
		//		{
		//			_ownerFaction = Grid.GetOwnerFaction();
		//		}

		//		if (_ownerFaction == null || !_ownerFaction.IsNpc())
		//		{
		//			Grid.DebugWrite("RegisterHostileAction", $"Error: {(_ownerFaction == null ? "can't find own faction" : "own faction isn't recognized as NPC.")}");
		//			return;
		//		}
		//		#endregion

		//		IMyFaction hostileFaction = player.GetFaction();
		//		if (hostileFaction == null)
		//		{
		//			Grid.DebugWrite("RegisterHostileAction", "Error: can't find damager's faction");
		//			return;
		//		}

		//		if (hostileFaction == _ownerFaction)
		//		{
		//			_ownerFaction.Kick(player);
		//			return;
		//		}

		//		//AiSessionCore.WarDeclared = 
		//		AiSessionCore.DeclareWar(_ownerFaction, hostileFaction, truceDelay);
		//		//if (!_ownerFaction.IsLawful()) return;
		//		//AiSessionCore.DeclareWar(Diplomacy.Police, hostileFaction, truceDelay);
		//		//AiSessionCore.DeclareWar(Diplomacy.Army, hostileFaction, truceDelay);
		//	}
		//	catch (Exception scrap)
		//	{
		//		LogError("RegisterHostileAction", scrap);
		//	}
		//}

		//protected virtual void RegisterHostileAction(IMyFaction hostileFaction, TimeSpan truceDelay)
		//{
		//	try
		//	{
		//		if (hostileFaction != null)
		//		{
		//			//AiSessionCore.WarDeclared = 
		//			AiSessionCore.DeclareWar(_ownerFaction, hostileFaction, truceDelay);
		//			//if (!_ownerFaction.IsLawful()) return;
		//			//AiSessionCore.DeclareWar(Diplomacy.Police, hostileFaction, truceDelay);
		//			//AiSessionCore.DeclareWar(Diplomacy.Army, hostileFaction, truceDelay);
		//		}
		//		else
		//		{
		//			Grid.DebugWrite("RegisterHostileAction", "Error: can't find damager's faction");
		//		}
		//	}
		//	catch (Exception scrap)
		//	{
		//		LogError("RegisterHostileAction", scrap);
		//	}
		//}

		//TODO Figure out why there is a NULL REFERENCE EXCEPTION from this call on velocity from MyDetectedEntityInfo
		//	velocity = myCubeGrid.Physics.LinearVelocity; +		$exception	{System.NullReferenceException: Object reference not set to an instance of an object.
		//		at Sandbox.Game.Entities.MyDetectedEntityInfoHelper.Create(MyEntity entity, Int64 sensorOwner, Nullable`1 hitPosition)}
		//		System.NullReferenceException

		protected List<MyDetectedEntityInfo> LookAround(float radius, Func<MyDetectedEntityInfo, bool> filter = null)
		{
			List<MyDetectedEntityInfo> radarData = new List<MyDetectedEntityInfo>();
			BoundingSphereD lookaroundSphere = new BoundingSphereD(GridPosition, radius);

			List<IMyEntity> entitiesAround = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref lookaroundSphere);
			entitiesAround.RemoveAll(x => x == Grid || GridPosition.DistanceTo(x.GetPosition()) < GridRadius * 1.5);

			long ownerId;
			if (_ownerFaction != null)
			{
				ownerId = _ownerFaction.FounderId;
				Grid.DebugWrite("LookAround", "Found owner via faction owner");
			}
			else
			{
				ownerId = Rc.OwnerId;
				Grid.DebugWrite("LookAround", "OWNER FACTION NOT FOUND, found owner via RC owner");
			}

			foreach (IMyEntity detectedEntity in entitiesAround)
			{
				if (detectedEntity is IMyFloatingObject || detectedEntity.Physics == null) continue;
				MyDetectedEntityInfo radarDetectedEntity = MyDetectedEntityInfoHelper.Create(detectedEntity as MyEntity, ownerId);
				if (radarDetectedEntity.Type == MyDetectedEntityType.None || radarDetectedEntity.Type == MyDetectedEntityType.Unknown) continue;
				if (filter == null || filter(radarDetectedEntity)) radarData.Add(radarDetectedEntity);
			}

			//DebugWrite("LookAround", $"Radar entities detected: {String.Join(" | ", RadarData.Select(x => $"{x.Name}"))}");
			return radarData;
		}

		protected List<MyDetectedEntityInfo> LookForEnemies(float radius, bool considerNeutralsAsHostiles = false, Func<MyDetectedEntityInfo, bool> filter = null)
		{
			return !considerNeutralsAsHostiles ? 
				LookAround(radius, x => x.IsHostile() && (filter == null || filter(x))) : 
				LookAround(radius, x => x.IsNonFriendly() && (filter == null || filter(x)));
		}

		/// <summary>
		/// Returns distance from the grid to an object.
		/// </summary>
		protected float Distance(MyDetectedEntityInfo target)
		{
			return (float)Vector3D.Distance(GridPosition, target.Position);
		}

		/// <summary>
		/// Returns distance from the grid to an object.
		/// </summary>
		//protected float Distance(IMyEntity target)
		//{
		//	return (float)Vector3D.Distance(GridPosition, target.GetPosition());
		//}

		//protected Vector3 RelVelocity(MyDetectedEntityInfo target)
		//{
		//	return target.Velocity - GridVelocity;
		//}

		protected float RelSpeed(MyDetectedEntityInfo target)
		{
			return (float)(target.Velocity - GridVelocity).Length();
		}

		//protected Vector3 RelVelocity(IMyEntity target)
		//{
		//	return target.Physics.LinearVelocity - GridVelocity;
		//}

		//protected float RelSpeed(IMyEntity target)
		//{
		//	return (float)(target.Physics.LinearVelocity - GridVelocity).Length();
		//}

		//protected virtual List<IMyTerminalBlock> GetHackedBlocks()
		//{
		//	List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
		//	List<IMyTerminalBlock> hackedBlocks = new List<IMyTerminalBlock>();

		//	Term.GetBlocks(terminalBlocks);

		//	foreach (IMyTerminalBlock block in terminalBlocks)
		//		if (block.IsBeingHacked) hackedBlocks.Add(block);

		//	return hackedBlocks;
		//}

		//protected virtual List<IMySlimBlock> GetDamagedBlocks()
		//{
		//	List<IMySlimBlock> blocks = new List<IMySlimBlock>();
		//	Grid.GetBlocks(blocks, x => x.CurrentDamage > 10);
		//	return blocks;
		//}

		protected bool HasModdedThrusters => SpeedmoddedThrusters.Count > 0;
		protected List<IMyThrust> SpeedmoddedThrusters = new List<IMyThrust>();

		protected void ApplyThrustMultiplier(float thrustMultiplier)
		{
			DemultiplyThrusters();
			foreach (IMyThrust thruster in Term.GetBlocksOfType<IMyThrust>(collect: x => x.IsOwnedByNpc(allowNobody: false, checkBuilder: true)))
			{
				thruster.ThrustMultiplier = thrustMultiplier;
				thruster.OwnershipChanged += Thruster_OnOwnerChanged;
				SpeedmoddedThrusters.Add(thruster);
			}
		}

		protected void DemultiplyThrusters()
		{
			if (!HasModdedThrusters) return;
			foreach (IMyThrust thruster in SpeedmoddedThrusters)
			{
				if (Math.Abs(thruster.ThrustMultiplier - 1) > 0) thruster.ThrustMultiplier = 1;
			}
			SpeedmoddedThrusters.Clear();
		}

		private void Thruster_OnOwnerChanged(IMyTerminalBlock thruster)
		{
			try
			{
				IMyThrust myThruster = thruster as IMyThrust;
				if (myThruster == null) return;
				if (!myThruster.IsOwnedByNpc() && Math.Abs(myThruster.ThrustMultiplier - 1) > 0) myThruster.ThrustMultiplier = 1;
			}
			catch (Exception scrap)
			{
				Grid.DebugWrite("Thruster_OnOwnerChanged", $"{thruster.CustomName} OnOwnerChanged failed: {scrap.Message}");
			}
		}

		protected abstract bool ParseSetup();

		public abstract void Main();

		public virtual void Shutdown()
		{
			Closed = true;
			if (HasModdedThrusters) DemultiplyThrusters();
			AiSessionCore.RemoveDamageHandler(Grid);
		}

		public void LogError(string source, Exception scrap, string debugPrefix = "BotBase.")
		{
			Grid.LogError(debugPrefix + source, scrap);
		}
	}
}