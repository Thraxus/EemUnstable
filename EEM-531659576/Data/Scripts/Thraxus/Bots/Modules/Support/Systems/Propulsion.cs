using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Systems.Collections;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Common.Utilities.Tools.OnScreenDisplay;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Propulsion : INeedUpdates
	{
		private readonly Dictionary<SystemType, QuestLogDetail> _questLogDetails = new Dictionary<SystemType, QuestLogDetail>();
		private readonly Dictionary<SystemType, EemFunctionalBlockCollection> _shipSystems = new Dictionary<SystemType, EemFunctionalBlockCollection>();
		private readonly QuestScreen _questScreen;

		public bool IsClosed { get; private set; }

		public Propulsion()
		{
			_questScreen = new QuestScreen("Propulsion");
			NewSystem(SystemType.ForwardPropulsion);
			NewSystem(SystemType.ReversePropulsion);
			NewSystem(SystemType.LeftPropulsion);
			NewSystem(SystemType.RightPropulsion);
			NewSystem(SystemType.UpPropulsion);
			NewSystem(SystemType.DownPropulsion);
		}

		public void Close()
		{
			if (IsClosed) return;

			foreach (KeyValuePair<SystemType, EemFunctionalBlockCollection> system in _shipSystems)
			{
				system.Value.Close();
			}
			IsClosed = true;
		}

		private void UpdateQuest(SystemType system, int currentFunctionalIntegrityRatio)
		{
			if (!_questLogDetails.ContainsKey(system)) return;
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {currentFunctionalIntegrityRatio}%");
			_questLogDetails[system].UpdateQuest(newQuest);
			_questScreen.UpdateQuest(_questLogDetails[system]);
		}

		private void NewQuest(SystemType system, float integrityRatio)
		{
			if (_questLogDetails.ContainsKey(system)) return;
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {integrityRatio}%");
			_questLogDetails.Add(system, new QuestLogDetail(newQuest));
			_questScreen.NewQuest(_questLogDetails[system]);
		}

		private void NewSystem(SystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			ThrusterCollection collection = new ThrusterCollection(type);
			//EemFunctionalBlockCollection collection = new EemFunctionalBlockCollection(type);
			NewQuest(type, collection.LastReportedIntegrityRatio);
			_shipSystems.Add(type, collection);
		}
		
		public void AddBlock(SystemType type, IMyFunctionalBlock block)
		{
			if (!_shipSystems.ContainsKey(type)) return;
			_shipSystems[type].AddBlock(block);
			_shipSystems[type].UpdateCurrentFunctionalIntegrityRatio();
			UpdateQuest(type, _shipSystems[type].LastReportedIntegrityRatio);
		}

		public void RunMassUpdate()
		{
			foreach (KeyValuePair<SystemType, EemFunctionalBlockCollection> system in _shipSystems)
			{
				system.Value.UpdateCurrentFunctionalIntegrityRatio();
				UpdateQuest(system.Key, system.Value.LastReportedIntegrityRatio);
			}
		}

		public int GetSystemIntegrity(SystemType type)
		{
			return !_shipSystems.ContainsKey(type) ? 0 : _shipSystems[type].LastReportedIntegrityRatio;
		}
	}
}
