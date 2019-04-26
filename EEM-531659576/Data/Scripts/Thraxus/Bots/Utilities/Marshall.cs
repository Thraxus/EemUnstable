using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Eem.Thraxus.Common;
using Eem.Thraxus.Utilities;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;

namespace Eem.Thraxus.Bots.Utilities
{
	[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, priority: int.MaxValue)]
	// ReSharper disable once ClassNeverInstantiated.Global
	internal class Marshall : MySessionComponentBase
	{
		private static Marshall _instance;

		public static ConcurrentDictionary<long, ConcurrentQueue<long>> ActiveShipDictionary;

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
			ActiveShipDictionary = new ConcurrentDictionary<long, ConcurrentQueue<long>>();
			DamageHandler.Run();
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
			BotOrphans?.Clear();
			_botDebugLog?.Close();
			_botGeneralLog?.Close();
		}

		public static void RegisterNewEntity(long entityId)
		{
			try
			{
				ActiveShipDictionary.TryAdd(entityId, new ConcurrentQueue<long>());
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
				ActiveShipDictionary.Remove(entityId);
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
