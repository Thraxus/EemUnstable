using System;
using Eem.Thraxus.Utilities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Bots.Utilities
{
	public static class DamageHandler
	{
		private const string DamageHandlerLogName = "DamageHandler-General";
		private const string DamageHandlerDebugLogName = "DamageHandler-Debug";

		private static Log _damageHandlerLog;
		private static Log _damageHandlerDebugLog;

		public static void Run()
		{
			_damageHandlerLog = new Log(DamageHandlerLogName);
			_damageHandlerDebugLog = new Log(DamageHandlerDebugLogName);
			MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, MyAfterDamageHandler);
			MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, MyBeforeDamageHandler);
			MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, MyDestroyHandler);
		}

		public static void Unload()
		{
			_damageHandlerLog?.Close();
			_damageHandlerDebugLog?.Close();
		}

		private static void MyDestroyHandler(object target, MyDamageInformation info)
		{
			WriteToLog("MyDestroyHandler", $"Invoked with {target} and {info.Amount} from {info.AttackerId}", true);
		}

		private static void MyBeforeDamageHandler(object target, ref MyDamageInformation info)
		{
			WriteToLog("MyBeforeDamageHandler", $"Invoked with {target} and {info.Amount} from {info.AttackerId}", true);
		}

		private static void MyAfterDamageHandler(object target, MyDamageInformation info)
		{
			WriteToLog("MyAfterDamageHandler", $"Invoked with {target} and {info.Amount} from {info.AttackerId}", true);
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
