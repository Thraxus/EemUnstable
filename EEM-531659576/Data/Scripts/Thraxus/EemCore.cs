using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.Enums;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace Eem.Thraxus
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MinValue + 1)]
	public class EemCore : BaseServerSessionComp
	{
		private const string GeneralLogName = "EemCoreGeneral";
		private const string DebugLogName = "EemCoreDebug";
		private const string SessionCompName = "EemCore";

		public static long GlobalTickTimer;

		public EemCore() : base(GeneralLogName, DebugLogName, SessionCompName) { } // Do nothing else

		protected override void SuperEarlySetup()
		{
			base.SuperEarlySetup();
			//GameSettings.Run();
		}

		/// <inheritdoc />
		protected override void EarlySetup()
		{
			base.EarlySetup();
		}

		public override void UpdateBeforeSimulation()
		{
			base.UpdateBeforeSimulation();
			GlobalTickTimer++;
		}

		/// <inheritdoc />
		protected override void LateSetup()
		{
			base.LateSetup();
			WriteToLog("LateSetup", $"Cargo: {MyAPIGateway.Session.SessionSettings.CargoShipsEnabled}", LogType.General);
			WriteToLog("LateSetup", $"Encounters: {MyAPIGateway.Session.SessionSettings.EnableEncounters}", LogType.General);
			WriteToLog("LateSetup", $"Drones: {MyAPIGateway.Session.SessionSettings.EnableDrones}", LogType.General);
			WriteToLog("LateSetup", $"Scripts: {MyAPIGateway.Session.SessionSettings.EnableIngameScripts}", LogType.General);
			WriteToLog("LateSetup", $"Sync: {MyAPIGateway.Session.SessionSettings.SyncDistance}", LogType.General);
			WriteToLog("LateSetup", $"View: {MyAPIGateway.Session.SessionSettings.ViewDistance}", LogType.General);
			WriteToLog("LateSetup", $"PiratePCU: {MyAPIGateway.Session.SessionSettings.PiratePCU}", LogType.General);
			WriteToLog("LateSetup", $"TotalPCU: {MyAPIGateway.Session.SessionSettings.TotalPCU}", LogType.General);

			foreach (MyObjectBuilder_Checkpoint.ModItem mod in MyAPIGateway.Session.Mods)
				WriteToLog("LateSetup", $"Mod: {mod}", LogType.General);
			List<IMyIdentity> identityList = new List<IMyIdentity>();
			MyAPIGateway.Players.GetAllIdentites(identityList);
			foreach (IMyIdentity identity in identityList)
				WriteToLog("LateSetup", $"Identity: {identity.IdentityId} | {identity.DisplayName} | {identity.IsDead}", LogType.General);

			//InformationExporter.Run();	
		}

		/// <inheritdoc />
		protected override void Unload()
		{
			base.Unload();
		}
	}
}