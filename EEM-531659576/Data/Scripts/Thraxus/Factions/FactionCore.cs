using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Factions.Models;
using Eem.Thraxus.Common.Settings;
using Sandbox.ModAPI;
using VRage.Game.Components;


namespace Eem.Thraxus.Factions
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class FactionCore : BaseServerSessionComp
	{

		private const string GeneralLogName = "FactionsGeneral";
		private const string DebugLogName = "FactionsDebug";
		private const string SessionCompName = "Factions";

		/// <inheritdoc />
		public FactionCore() : base(GeneralLogName, DebugLogName, SessionCompName) { } // Do nothing else

		// Fields

		private RelationshipManager _relationshipManager;

		public static FactionCore FactionCoreStaticInstance;

		/// <inheritdoc />
		public override void LoadData()
		{
			base.LoadData();
			FactionCoreStaticInstance = this;
		}
		
		// Init Methods
		
		/// <summary>
		/// Runs every tick before the simulation is updated
		/// </summary>
		public override void UpdateBeforeSimulation()
		{
			base.UpdateBeforeSimulation();
			TickTimer();
		}

		/// <inheritdoc />
		protected override void LateSetup()
		{
			base.LateSetup();
			_relationshipManager = new RelationshipManager();
			_relationshipManager.OnWriteToLog += WriteToLog;
			_relationshipManager.Run();
			MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			WriteToLog("FactionCore", $"Initialized... {UpdateOrder}", LogType.General);
		}
		
		/// <inheritdoc />
		protected override void Unload()
		{
			if (_relationshipManager != null)
			{
				_relationshipManager.OnWriteToLog -= WriteToLog;
				_relationshipManager.Close();
			}
			FactionCoreStaticInstance = null;
			base.Unload();
		}

		// Core Logic Methods

		/// <summary>
		/// Increments every server tick
		/// </summary>
		private ulong _tickTimer;

		/// <summary>
		/// Processes certain things at set intervals
		/// </summary>
		private void TickTimer()
		{
			_tickTimer++;
			if (_tickTimer % Settings.FactionNegativeRelationshipAssessment == 0)
				_relationshipManager.CheckNegativeRelationships();
			if (_tickTimer % Settings.FactionMendingRelationshipAssessment == 0)
				_relationshipManager.CheckMendingRelationships();
		}
	}
}
