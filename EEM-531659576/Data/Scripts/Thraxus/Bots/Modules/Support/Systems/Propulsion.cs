using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.Types;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Eem.Thraxus.Common.Utilities.Tools.OnScreenDisplay;
using Sandbox.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Propulsion : INeedUpdates
	{


		private readonly Thruster _forwardThrusters;
		private readonly Thruster _reverseThrusters;
		private readonly Thruster _leftThrusters;
		private readonly Thruster _rightThrusters;
		private readonly Thruster _upThrusters;
		private readonly Thruster _downThrusters;

		private readonly QuestScreen _questScreen;
		private StringBuilder _lastForwardQuest;
		private StringBuilder _lastBackwardsQuest;
		private StringBuilder _lastLeftQuest;
		private StringBuilder _lastRightQuest;
		private StringBuilder _lastUpQuest;
		private StringBuilder _lastDownQuest;

		private void UpdateQuest(SystemType system, float remainingFunctionalIntegrityRatio)
		{
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {remainingFunctionalIntegrityRatio * 100}%");
			switch (system)
			{
				case SystemType.ForwardPropulsion:
					_questScreen.UpdateQuest(_lastForwardQuest.ToString(), newQuest.ToString());
					_lastForwardQuest = newQuest;
					break;
				case SystemType.ReversePropulsion:
					_questScreen.UpdateQuest(_lastBackwardsQuest.ToString(), newQuest.ToString());
					_lastBackwardsQuest = newQuest;
					break;
				case SystemType.LeftPropulsion:
					_questScreen.UpdateQuest(_lastLeftQuest.ToString(), newQuest.ToString());
					_lastLeftQuest = newQuest;
					break;
				case SystemType.RightPropulsion:
					_questScreen.UpdateQuest(_lastRightQuest.ToString(), newQuest.ToString());
					_lastRightQuest = newQuest;
					break;
				case SystemType.UpPropulsion:
					_questScreen.UpdateQuest(_lastUpQuest.ToString(), newQuest.ToString());
					_lastUpQuest = newQuest;
					break;
				case SystemType.DownPropulsion:
					_questScreen.UpdateQuest(_lastDownQuest.ToString(), newQuest.ToString());
					_lastDownQuest = newQuest;
					break;
				default:
					return;
			}
		}

		private void AddQuest(SystemType system, float remainingFunctionalIntegrityRatio)
		{
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {remainingFunctionalIntegrityRatio * 100}%");
			_questScreen.NewQuest(newQuest.ToString());
			switch (system)
			{
				case SystemType.ForwardPropulsion:
					_lastForwardQuest = newQuest;
					return;
				case SystemType.ReversePropulsion:
					_lastBackwardsQuest = newQuest;
					return;
				case SystemType.LeftPropulsion:
					_lastLeftQuest = newQuest;
					return;
				case SystemType.RightPropulsion:
					_lastRightQuest = newQuest;
					return;
				case SystemType.UpPropulsion:
					_lastUpQuest = newQuest;
					return;
				case SystemType.DownPropulsion:
					_lastDownQuest = newQuest;
					return;
				default:
					return;
			}
		}
		
		public Propulsion()
		{
			_questScreen = new QuestScreen("Propulsion");
			
			_forwardThrusters = new Thruster(SystemType.ForwardPropulsion);
			_forwardThrusters.SystemDamaged += UpdateQuest;
			AddQuest(SystemType.ForwardPropulsion, _forwardThrusters.RemainingFunctionalIntegrityRatio);

			_reverseThrusters = new Thruster(SystemType.ReversePropulsion);
			_reverseThrusters.SystemDamaged += UpdateQuest;
			AddQuest(SystemType.ReversePropulsion, _reverseThrusters.RemainingFunctionalIntegrityRatio);

			_leftThrusters = new Thruster(SystemType.LeftPropulsion);
			_leftThrusters.SystemDamaged += UpdateQuest;
			AddQuest(SystemType.LeftPropulsion, _leftThrusters.RemainingFunctionalIntegrityRatio);

			_rightThrusters = new Thruster(SystemType.RightPropulsion);
			_rightThrusters.SystemDamaged += UpdateQuest;
			AddQuest(SystemType.RightPropulsion, _rightThrusters.RemainingFunctionalIntegrityRatio);

			_upThrusters = new Thruster(SystemType.UpPropulsion);
			_upThrusters.SystemDamaged += UpdateQuest;
			AddQuest(SystemType.UpPropulsion, _upThrusters.RemainingFunctionalIntegrityRatio);

			_downThrusters = new Thruster(SystemType.DownPropulsion);
			_downThrusters.SystemDamaged += UpdateQuest;
			AddQuest(SystemType.DownPropulsion, _downThrusters.RemainingFunctionalIntegrityRatio);
		}

		public void AddBlock(IMyThrust thruster, Vector3I vector)
		{
			StaticLog.WriteToLog("Propulsion: AddBlock", $"{thruster.EntityId} | {thruster.GridThrustDirection} | {vector}", LogType.General);
			if (vector == Vector3I.Forward)
				_forwardThrusters.AddBlock(thruster);
			if (vector == Vector3I.Backward)
				_reverseThrusters.AddBlock(thruster);
			if (vector == Vector3I.Left)
				_leftThrusters.AddBlock(thruster);
			if (vector == Vector3I.Right)
				_rightThrusters.AddBlock(thruster);
			if (vector == Vector3I.Up)
				_upThrusters.AddBlock(thruster);
			if (vector == Vector3I.Down)
				_downThrusters.AddBlock(thruster);
		}

		public bool IsClosed { get; private set; }


		public void RunUpdate()
		{
			_forwardThrusters.RunUpdate();
			_reverseThrusters.RunUpdate();
			_leftThrusters.RunUpdate();
			_rightThrusters.RunUpdate();
			_upThrusters.RunUpdate();
			_downThrusters.RunUpdate();
		}

		public void Close()
		{
			if (IsClosed) return;
			_forwardThrusters.Close();
			_reverseThrusters.Close();
			_leftThrusters.Close();
			_rightThrusters.Close();
			_upThrusters.Close();
			_downThrusters.Close();

			_forwardThrusters.SystemDamaged -= UpdateQuest;
			_reverseThrusters.SystemDamaged -= UpdateQuest;
			_leftThrusters.SystemDamaged -= UpdateQuest;
			_rightThrusters.SystemDamaged -= UpdateQuest;
			_upThrusters.SystemDamaged -= UpdateQuest;
			_downThrusters.SystemDamaged -= UpdateQuest;
			IsClosed = true;
		}
	}
}
