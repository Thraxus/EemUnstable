using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.BaseClasses;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Common.Utilities.Tools.OnScreenDisplay;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Propulsion : INeedUpdates
	{
		private readonly Dictionary<SystemType, QuestLogDetail> _questLogDetails = new Dictionary<SystemType, QuestLogDetail>();
		private readonly Dictionary<SystemType, MainSystemBase> _shipSystems = new Dictionary<SystemType, MainSystemBase>();
		private readonly QuestScreen _questScreen;
		
		private void UpdateQuest(SystemType system, float integrityRatio)
		{
			if (!_questLogDetails.ContainsKey(system)) return;
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {integrityRatio * 100}%");
			_questLogDetails[system].UpdateQuest(newQuest);
			_questScreen.UpdateQuest(_questLogDetails[system]);
		}

		private void NewQuest(SystemType system, float integrityRatio)
		{
			if (_questLogDetails.ContainsKey(system)) return;
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {integrityRatio * 100}%");
			_questLogDetails.Add(system, new QuestLogDetail(newQuest));
			_questScreen.NewQuest(_questLogDetails[system]);
		}

		private void NewSystem(SystemType type)
		{
			if (_shipSystems.ContainsKey(type)) return;
			MainSystemBase main = new MainSystemBase(type);
			main.SystemDamaged += UpdateQuest;
			NewQuest(type, main.RemainingFunctionalIntegrityRatio);
			_shipSystems.Add(type, main);
		}

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

		public void AddBlock(IMyThrust thruster, SystemType type)
		{
			//StaticLog.WriteToLog("Propulsion: AddBlock", $"{thruster.EntityId} | {thruster.GridThrustDirection} | {type}", LogType.General);
			if (_shipSystems.ContainsKey(type))
				_shipSystems[type].AddBlock(thruster);
		}

		public bool IsClosed { get; private set; }


		public void RunUpdate()
		{
			foreach (KeyValuePair<SystemType, MainSystemBase> system in _shipSystems)
				system.Value.RunUpdate();
		}

		public void Close()
		{
			if (IsClosed) return;

			foreach (KeyValuePair<SystemType, MainSystemBase> system in _shipSystems)
			{
				system.Value.SystemDamaged -= UpdateQuest;
				system.Value.Close();
			}
			IsClosed = true;
		}
	}
}
