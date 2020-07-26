using System.Collections.Generic;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.Support;
using Eem.Thraxus.Common.Enums;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.FunctionalReporting.Systems.BaseClasses
{
	internal abstract class ShipSystemBase : INeedUpdates
	{
		protected readonly Dictionary<BotSystemType, EemFunctionalBlockCollection> _shipSystems = new Dictionary<BotSystemType, EemFunctionalBlockCollection>();
		private readonly BotSystemsQuestLog _questScreen;

		protected ShipSystemBase(BotSystemsQuestLog questScreen)
		{
			_questScreen = questScreen;
		}

		public bool IsClosed { get; private set; }

		public void Close()
		{
			if (IsClosed) return;

			foreach (KeyValuePair<BotSystemType, EemFunctionalBlockCollection> system in _shipSystems)
			{
				system.Value.Close();
			}
			IsClosed = true;
		}

		private void UpdateQuest(BotSystemType system, int currentFunctionalIntegrityRatio)
		{
			_questScreen.UpdateQuest(system, currentFunctionalIntegrityRatio);
		}

		protected void NewQuest(BotSystemType system, int integrityRatio)
		{
			_questScreen.NewQuest(system, integrityRatio);
		}

		protected abstract void NewSystem(BotSystemType type);

		public void AddBlock(BotSystemType type, IMyFunctionalBlock block)
		{
			if (!_shipSystems.ContainsKey(type)) return;
			_shipSystems[type].AddBlock(block);
			_shipSystems[type].UpdateCurrentFunctionalIntegrityRatio(block.EntityId);
			UpdateQuest(type, _shipSystems[type].LastReportedIntegrityRatio);
		}

		public void RunMassUpdate(long blockId)
		{
			foreach (KeyValuePair<BotSystemType, EemFunctionalBlockCollection> system in _shipSystems)
			{
				bool updated = system.Value.UpdateCurrentFunctionalIntegrityRatio(blockId);
				if(updated) UpdateQuest(system.Key, system.Value.LastReportedIntegrityRatio);
			}
		}

		public int GetSystemIntegrity(BotSystemType type)
		{
			return !_shipSystems.ContainsKey(type) ? 0 : _shipSystems[type].LastReportedIntegrityRatio;
		}
	}
}
