using System.Collections.Generic;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.BaseClasses
{
	internal abstract class ShipSystemBase : INeedUpdates
	{
		protected readonly Dictionary<SystemType, EemFunctionalBlockCollection> _shipSystems = new Dictionary<SystemType, EemFunctionalBlockCollection>();
		private readonly BotSystemsQuestLog _questScreen;

		protected ShipSystemBase(BotSystemsQuestLog questScreen)
		{
			_questScreen = questScreen;
		}

		public bool IsClosed { get; private set; }

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

		protected void NewQuest(SystemType system, int integrityRatio)
		{
			_questScreen.NewQuest(system, integrityRatio);
		}

		protected abstract void NewSystem(SystemType type);

		public void AddBlock(SystemType type, IMyFunctionalBlock block)
		{
			if (!_shipSystems.ContainsKey(type)) return;
			_shipSystems[type].AddBlock(block);
			_shipSystems[type].UpdateCurrentFunctionalIntegrityRatio(block.EntityId);
			UpdateQuest(type, _shipSystems[type].LastReportedIntegrityRatio);
		}

		public void RunMassUpdate(long blockId)
		{
			foreach (KeyValuePair<SystemType, EemFunctionalBlockCollection> system in _shipSystems)
			{
				bool updated = system.Value.UpdateCurrentFunctionalIntegrityRatio(blockId);
				if(updated) UpdateQuest(system.Key, system.Value.LastReportedIntegrityRatio);
			}
		}

		public int GetSystemIntegrity(SystemType type)
		{
			return !_shipSystems.ContainsKey(type) ? 0 : _shipSystems[type].LastReportedIntegrityRatio;
		}
	}
}
