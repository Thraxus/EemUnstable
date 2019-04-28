using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Eem.Thraxus.Common;
using Eem.Thraxus.Utilities;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Screens.Helpers;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Utilities
{
	[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, priority: int.MaxValue)]
	// ReSharper disable once ClassNeverInstantiated.Global
	internal class BotMarshal : MySessionComponentBase
	{
		private static BotMarshal _instance;

		public static MyConcurrentList<long> ActiveShipRegistry;
		public static MyConcurrentDictionary<long, MyConcurrentList<ShipControllerHistory>> PlayerShipControllerHistory;

		private const string GeneralLogName = "BotGeneral";
		private const string DebugLogName = "BotDebug";

		private static Log _botGeneralLog;
		private static Log _botDebugLog;

		private bool _setupComplete;

		public static Dictionary<long, BotOrphan> BotOrphans;

		/// <inheritdoc />
		public override void LoadData()
		{
			_instance = this;
			base.LoadData();
		}

		/// <inheritdoc />
		public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
		{
			base.Init(sessionComponent);
			if (!Helpers.Constants.IsServer || _setupComplete) return;
			if (!_setupComplete) Setup();
		}

		private void Setup()
		{
			_setupComplete = true;
			_botGeneralLog = new Log(GeneralLogName);
			if (Helpers.Constants.DebugMode) _botDebugLog = new Log(DebugLogName);
			BotOrphans = new Dictionary<long, BotOrphan>();
			ActiveShipRegistry = new MyConcurrentList<long>();
			PlayerShipControllerHistory = new MyConcurrentDictionary<long, MyConcurrentList<ShipControllerHistory>>();
			DamageHandler.Run();
		}

		/// <inheritdoc />
		public override void BeforeStart()
		{
			base.BeforeStart();
		}

		private void ControllerInfoOnControlAcquired(IMyEntityController controller)
		{
			try
			{
				ShipControllerHistory tmp = new ShipControllerHistory(
					controller.ControlledEntity.ControllerInfo.ControllingIdentityId,
					controller.ControlledEntity,
					DateTime.Now);

				if (PlayerShipControllerHistory.ContainsKey(controller.ControlledEntity.ControllerInfo.ControllingIdentityId))
					PlayerShipControllerHistory[controller.ControlledEntity.ControllerInfo.ControllingIdentityId].Add(tmp);
				else
					PlayerShipControllerHistory.Add(controller.ControlledEntity.ControllerInfo.ControllingIdentityId, new MyConcurrentList<ShipControllerHistory> { tmp });
				WriteToLog("ControlAcquiredDelegate", $"Added: {tmp}");
			}
			catch (Exception e)
			{
				ExceptionLog("ControlAcquiredDelegate", $"Exception! {e}");
			}
		}

		/// <inheritdoc />
		protected override void UnloadData()
		{
			base.UnloadData();
			if (!Helpers.Constants.IsServer) return;
			Unload();
			_instance = null;
		}

		private void Unload()
		{
			DamageHandler.Unload();
			ActiveShipRegistry?.Clear();
			PlayerShipControllerHistory?.Clear();
			BotOrphans?.Clear();
			_botDebugLog?.Close();
			_botGeneralLog?.Close();
		}

		public static void RegisterNewEntity(long entityId)
		{
			try
			{
				ActiveShipRegistry.Add(entityId);
			}
			catch (Exception e)
			{
				ExceptionLog("RegisterNewEntity", e.ToString());
			}
		}

		public static void RemoveDeadEntity(long entityId)
		{
			try
			{
				ActiveShipRegistry.Remove(entityId);
			}
			catch (Exception e)
			{
				ExceptionLog("RemoveDeadEntity", e.ToString());
			}
		}

		private static readonly object WriteLocker = new object();

		public static void WriteToLog(string caller, string message, bool general = false)
		{
			lock (WriteLocker)
			{
				if (general) _botGeneralLog?.WriteToLog(caller, message);
				_botDebugLog?.WriteToLog(caller, message);
			}
		}

		public static void ExceptionLog(string caller, string message)
		{
			WriteToLog(caller, $"Exception! {message}", true);
		}
	}
}
