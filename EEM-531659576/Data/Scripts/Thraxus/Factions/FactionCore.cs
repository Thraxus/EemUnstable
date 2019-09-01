using System;
using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Factions.Models;
using Eem.Thraxus.Common.Settings;
using Eem.Thraxus.Common.Utilities.SaveGame;
using Eem.Thraxus.Factions.DataTypes;
using Sandbox;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;


namespace Eem.Thraxus.Factions
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class FactionCore : BaseServerSessionComp
	{

		private const string GeneralLogName = "FactionsGeneral";
		private const string DebugLogName = "FactionsDebug";
		private const string SessionCompName = "Factions";
		private const string SaveVarName = "Eem_Factions";
		private const string SaveFileName = "Eem_Factions.eem";

		/// <inheritdoc />
		public FactionCore() : base(GeneralLogName, DebugLogName, SessionCompName, false) { } // Do nothing else

		// Fields

		private RelationshipManager _relationshipManager;

		public static FactionCore FactionCoreStaticInstance;
		private Dictionary<long, FactionRelation> _factionMaster;

		/// <inheritdoc />
		public override void LoadData()
		{
			base.LoadData();
			_factionMaster = Load.ReadFromFile<Dictionary<long, FactionRelation>>(SaveFileName, typeof(Dictionary<long, FactionRelation>));
			_relationshipManager = _factionMaster == null ? new RelationshipManager() : new RelationshipManager(_factionMaster);
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

		public override void SaveData()
		{
			base.SaveData();
			if (_relationshipManager == null) return;
			Save.WriteToFile(SaveFileName, _relationshipManager.GetSave(), typeof(Dictionary<long, FactionRelation>));
			//MyAPIGateway.Utilities.SetVariable<string>(SaveVarName, MyAPIGateway.Utilities.SerializeToBinary(_relationshipManager.GetSave()).ToString());
		}
		
		/// <inheritdoc />
		protected override void LateSetup()
		{
			base.LateSetup();
			_relationshipManager.OnWriteToLog += WriteToLog;
			_relationshipManager.Run();
			//MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
			WriteToLog("FactionCore", $"Initialized... {UpdateOrder} | SaveState: {_factionMaster?.Count}", LogType.General);
		}

		public void ReInitFactions()
		{

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
			if (_tickTimer % GeneralSettings.FactionNegativeRelationshipAssessment == 0)
				_relationshipManager.CheckNegativeRelationships();
			if (_tickTimer % GeneralSettings.FactionMendingRelationshipAssessment == 0)
				_relationshipManager.CheckMendingRelationships();
			if (_tickTimer % GeneralSettings.TicksPerMinute == 0)
				_relationshipManager.SetRepDebug(GeneralSettings.Random.Next(-30,30));
		}
	}
}
