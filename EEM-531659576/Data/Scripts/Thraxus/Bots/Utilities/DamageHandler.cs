using System;
using Eem.Thraxus.Utilities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Game.ModAPI;

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
			
			if (info.IsDeformation || info.AttackerId == 0) return;
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
			long damageDealer = 0;

			try
			{
				WriteToLog("IdentifyDamageDealer", $"All the info: {damageInfo.Type.String} - {damageInfo.Amount} - {damageInfo.IsDeformation} - {damageInfo.AttackerId}", true);

				//if (damageInfo.Type.GetType() == typeof(IMyEngineerToolBase))
				//	WriteToLog("IdentifyDamageDealer", $"The damaged block comes from a player tool", true);

				//if (damageInfo.Type.GetType() == typeof(MyGunBase))
				//	WriteToLog("IdentifyDamageDealer", $"The damaged block comes from a player weapon", true);

				//WriteToLog("IdentifyDamageDealer", $"Actual type: {damageInfo.Type.GetType()} -- {damageInfo.Type.String}", true);

			}
			catch (Exception e)
			{
				ExceptionLog("IdentifyDamageDealer", e.ToString());
				return damageDealer;
			}

			return damageDealer;
		}

		private static void MyDestroyHandler(object target, MyDamageInformation info)
		{	// Potentially Unused
			WriteToLog("MyDestroyHandler", $"Invoked with {target} taking {info.Amount} damage from {info.AttackerId}", true);
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
