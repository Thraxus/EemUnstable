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

		//private Dictionary<SystemType, QuestLogDetail> questLogDetails = new Dictionary<SystemType, QuestLogDetail>();

		private readonly QuestScreen _questScreen;
		private QuestLogDetail _forwardDetail;
		private QuestLogDetail _reverseDetail;
		private QuestLogDetail _leftDetail;
		private QuestLogDetail _rightDetail;
		private QuestLogDetail _upDetail;
		private QuestLogDetail _downDetail;
		
		private void UpdateQuest(SystemType system, float integrityRatio)
		{
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {integrityRatio * 100}%");
			switch (system)
			{
				case SystemType.ForwardPropulsion:
					_forwardDetail.UpdateQuest(newQuest);
					_questScreen.UpdateQuest(_forwardDetail);
					break;
				case SystemType.ReversePropulsion:
					_reverseDetail.UpdateQuest(newQuest);
					_questScreen.UpdateQuest(_reverseDetail);
					break;
				case SystemType.LeftPropulsion:
					_leftDetail.UpdateQuest(newQuest);
					_questScreen.UpdateQuest(_leftDetail);
					break;
				case SystemType.RightPropulsion:
					_rightDetail.UpdateQuest(newQuest);
					_questScreen.UpdateQuest(_rightDetail);
					break;
				case SystemType.UpPropulsion:
					_upDetail.UpdateQuest(newQuest);
					_questScreen.UpdateQuest(_upDetail);
					break;
				case SystemType.DownPropulsion:
					_downDetail.UpdateQuest(newQuest);
					_questScreen.UpdateQuest(_downDetail);
					break;
				default:
					return;
			}
		}

		private void NewQuest(SystemType system, float integrityRatio)
		{
			StringBuilder newQuest = new StringBuilder($"{system} Integrity: {integrityRatio * 100}%");
			switch (system)
			{
				case SystemType.ForwardPropulsion:
					_forwardDetail = new QuestLogDetail(newQuest);
					_questScreen.NewQuest(_forwardDetail);
					break;
				case SystemType.ReversePropulsion:
					_reverseDetail = new QuestLogDetail(newQuest);
					_questScreen.NewQuest(_reverseDetail);
					break;
				case SystemType.LeftPropulsion:
					_leftDetail = new QuestLogDetail(newQuest);
					_questScreen.NewQuest(_leftDetail);
					break;
				case SystemType.RightPropulsion:
					_rightDetail = new QuestLogDetail(newQuest);
					_questScreen.NewQuest(_rightDetail);
					break;
				case SystemType.UpPropulsion:
					_upDetail = new QuestLogDetail(newQuest);
					_questScreen.NewQuest(_upDetail);
					break;
				case SystemType.DownPropulsion:
					_downDetail = new QuestLogDetail(newQuest);
					_questScreen.NewQuest(_downDetail);
					break;
				default:
					return;
			}
		}

		public Propulsion()
		{
			_questScreen = new QuestScreen("Propulsion");
			
			_forwardThrusters = new Thruster(SystemType.ForwardPropulsion);
			_forwardThrusters.SystemDamaged += UpdateQuest;
			NewQuest(SystemType.ForwardPropulsion, _forwardThrusters.RemainingFunctionalIntegrityRatio);

			_reverseThrusters = new Thruster(SystemType.ReversePropulsion);
			_reverseThrusters.SystemDamaged += UpdateQuest;
			NewQuest(SystemType.ReversePropulsion, _reverseThrusters.RemainingFunctionalIntegrityRatio);

			_leftThrusters = new Thruster(SystemType.LeftPropulsion);
			_leftThrusters.SystemDamaged += UpdateQuest;
			NewQuest(SystemType.LeftPropulsion, _leftThrusters.RemainingFunctionalIntegrityRatio);

			_rightThrusters = new Thruster(SystemType.RightPropulsion);
			_rightThrusters.SystemDamaged += UpdateQuest;
			NewQuest(SystemType.RightPropulsion, _rightThrusters.RemainingFunctionalIntegrityRatio);

			_upThrusters = new Thruster(SystemType.UpPropulsion);
			_upThrusters.SystemDamaged += UpdateQuest;
			NewQuest(SystemType.UpPropulsion, _upThrusters.RemainingFunctionalIntegrityRatio);

			_downThrusters = new Thruster(SystemType.DownPropulsion);
			_downThrusters.SystemDamaged += UpdateQuest;
			NewQuest(SystemType.DownPropulsion, _downThrusters.RemainingFunctionalIntegrityRatio);
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
