using System;
using System.CodeDom;
using Eem.Thraxus.Utilities;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.Utilities
{
	public static class DamageHandler
	{
		private const string DamageHandlerLogName = "DamageHandler-General";
		private const string DamageHandlerDebugLogName = "DamageHandler-Debug";

		private const int Priority = int.MinValue;

		private static Log _damageHandlerLog;
		private static Log _damageHandlerDebugLog;

		public static void Run()
		{
			_damageHandlerLog = new Log(DamageHandlerLogName);
			_damageHandlerDebugLog = new Log(DamageHandlerDebugLogName);
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(Priority, MyBeforeDamageHandler);

			//MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(Priority, MyAfterDamageHandler);
			//MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(Priority, MyDestroyHandler);
		}

		public static void Unload()
		{
			_damageHandlerLog?.Close();
			_damageHandlerDebugLog?.Close();
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

			long damagedEntity = IdentifyDamagedEntity(block);

			long damageDealer = IdentifyDamageDealer(info);
		}

		private static long IdentifyDamagedEntity(IMySlimBlock block)
		{
			//WriteToLog("IdentifyDamagedEntity", $"The damaged block belongs to entity: {block.CubeGrid.DisplayName}", true);
			return block.CubeGrid.EntityId;
		}

		private static long IdentifyDamageDealer(MyDamageInformation damageInfo)
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
					WriteToLog("IdentifyDamageDealer", $"Possible missile: {damageInfo.Type.String} - {damageInfo.Amount} - {damageInfo.IsDeformation} - {damageInfo.AttackerId}", true);
					return 0;
				}
				
				if (!MyAPIGateway.Entities.TryGetEntityById(damageInfo.AttackerId, out attackingEntity)) return 0;

				WriteToLog("IdentifyDamageDealer", $"All the info: {damageInfo.Type.String} - {damageInfo.Amount} - {damageInfo.IsDeformation} - {damageInfo.AttackerId} - {attackingEntity.EntityId} - {attackingEntity.GetType()}", true);
				FindTheAsshole(attackingEntity);


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

		private static void FindTheAsshole(IMyEntity attacker)
		{
			if (attacker.GetType() == typeof(MyCubeGrid))
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as a CubeGrid!", true);
				return;
			}
			IMyCharacter myCharacter = attacker as IMyCharacter;
			if (myCharacter != null)
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as a Character!", true);
				return;
			}
			IMyAutomaticRifleGun myAutomaticRifle = attacker as IMyAutomaticRifleGun;
			if (myAutomaticRifle != null)
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as an Engineer Rifle!", true);
				return;
			}
			IMyAngleGrinder myAngleGrinder = attacker as IMyAngleGrinder;
			if (myAngleGrinder != null)
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as an Engineer Grinder!", true);
				return;
			}
			IMyHandDrill myHandDrill = attacker as IMyHandDrill;
			if (myHandDrill != null)
			{
				WriteToLog("FindTheAsshole", $"Asshole Identified as an Engineer Drill!", true);
				return;
			}
			WriteToLog("FindTheAsshole", $"Asshole not identified!!!  It was a: {attacker.GetType()}", true);
		}

		private static void MyDestroyHandler(object target, MyDamageInformation info)
		{	// Potentially Unused
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
