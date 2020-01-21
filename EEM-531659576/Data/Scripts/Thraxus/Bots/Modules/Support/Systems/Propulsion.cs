using System.Collections.Generic;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Systems.Collections;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Propulsion : INeedUpdates
	{
		private readonly Dictionary<SystemType, EemFunctionalBlockCollection> _shipSystems = new Dictionary<SystemType, EemFunctionalBlockCollection>();
		private readonly BotSystemsQuestLog _questScreen;

		public bool IsClosed { get; private set; }

		public Propulsion(BotSystemsQuestLog questScreen)
		{
			_questScreen = questScreen;
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
			_questScreen.UpdateQuest(system, currentFunctionalIntegrityRatio);
		}

		private void NewQuest(SystemType system, int integrityRatio)
		{
			_questScreen.NewQuest(system, integrityRatio);
		}

		private void NewSystem(SystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			ThrusterCollection collection = new ThrusterCollection(type);
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
