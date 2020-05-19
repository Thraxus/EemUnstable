using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Common.Enums;
using SpaceEngineers.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Alert.Support
{
	internal class Timer : ISetAlert
	{
		// TODO: Need to analyze timers to see how they are used in prefabs.
		// TODO: Timers need work, just disabling them for now - if I don't, they can contain items that disrupt other 
		//	alert settings.  So... broad strokes disabling it is.
		// Perhaps come up with an algo to determine if it's relevant to war or not... 
		private readonly IMyTimerBlock _timer;
		private readonly long _ownerId;
		private readonly TimerSettings _wartimeSettings;
		private readonly TimerSettings _peacetimeSettings;


		private readonly bool _alertOnTimer;
		private readonly bool _alertOffTimer;
		private const string AlertOnText = "[Alert_On]";
		private const string AlertOffText = "[Alert_Off]";

		private struct TimerSettings
		{
			public readonly bool Enabled;

			public TimerSettings(bool enabled)
			{
				Enabled = enabled;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled}";
			}
		}

		public Timer(IMyTimerBlock timer)
		{
			_timer = timer;
			_ownerId = timer.OwnerId;
			_wartimeSettings = new TimerSettings(true);
			_peacetimeSettings = new TimerSettings(_timer.Enabled);

			if (timer.CustomName.Contains(AlertOnText))
				_alertOnTimer = true;

			if (timer.CustomName.Contains(AlertOffText))
				_alertOffTimer = true;


			_timer.Enabled = false;
			//timer.

			//MyObjectBuilder_TimerBlock timerOb = (MyObjectBuilder_TimerBlock) _timer.GetObjectBuilderCubeBlock();
			//for (int i = timerOb.Toolbar.Slots.Count - 1; i >= 0; i--)
			//{
			//	MyObjectBuilder_Toolbar.Slot slot = timerOb.Toolbar.Slots[i];

			//	if (slot.Data.TypeId == typeof(MyObjectBuilder_ToolbarItemTerminalGroup))
			//	{
			//		StaticLog.WriteToLog("AlertConditions",
			//			$"Found a group! {((MyObjectBuilder_ToolbarItemTerminalGroup)slot.Data).SubtypeId} | {((MyObjectBuilder_ToolbarItemTerminalGroup)slot.Data).SubtypeName}", LogType.General);


			//		MyObjectBuilder_ToolbarItemTerminalGroup terminalGroup = (MyObjectBuilder_ToolbarItemTerminalGroup) slot.Data;

			//		IMyEntity entity = MyAPIGateway.Entities.GetEntityById(terminalGroup?.BlockEntityId);

			//		StaticLog.WriteToLog("AlertConditions",
			//			$"Found a group! {entity.GetType()}", LogType.General);


			//		//turret = MyAPIGateway.Entities.GetEntityById(((MyObjectBuilder_ToolbarItemTerminalGroup)slot.Data)
			//		//	.BlockEntityId) as IMyLargeTurretBase;

			//		//if (turret != null)
			//		//{
			//		//	StaticLog.WriteToLog("AlertConditions",
			//		//		$"Found a turret via group! {turret.CustomName}", LogType.General);
			//		//	continue;
			//		//}

			//		//IMyEntity me =
			//		//	MyAPIGateway.Entities.GetEntityById(((MyObjectBuilder_ToolbarItemTerminalGroup)slot.Data)
			//		//		.BlockEntityId);
			//		//StaticLog.WriteToLog("AlertConditions",
			//		//	$"TerminalGroup Identified: {me.GetType()} | {me.DisplayName} | {me.}", LogType.General);
			//	}

			//	if (slot.Data.TypeId == typeof(MyObjectBuilder_ToolbarItemTerminalBlock))
			//	{
			//		MyObjectBuilder_ToolbarItemTerminalBlock terminalBlock = (MyObjectBuilder_ToolbarItemTerminalBlock) slot.Data;

			//		IMyEntity entity = MyAPIGateway.Entities.GetEntityById(terminalBlock?.BlockEntityId);


			//		StaticLog.WriteToLog("AlertConditions",
			//			$"Found a block! {entity.GetType()}", LogType.General);

			//		IMyGravityGenerator gravity = entity as IMyGravityGenerator;

			//		if (gravity == null) continue;
			//		StaticLog.WriteToLog("AlertConditions",
			//			$"I'm a gravity generator!", LogType.General);


			//		timerOb.Toolbar.Slots[i] = new MyObjectBuilder_Toolbar.Slot();
			//		continue;

			//		//turret = MyAPIGateway.Entities.GetEntityById(((MyObjectBuilder_ToolbarItemTerminalBlock)slot.Data)
			//		//	.BlockEntityId) as IMyLargeTurretBase;

			//		//if (turret != null)
			//		//{
			//		//	StaticLog.WriteToLog("AlertConditions",
			//		//		$"Found a turret via block! {turret.CustomName}", LogType.General);
			//		//	continue;
			//		//}
			//		//IMyEntity me =
			//		//	MyAPIGateway.Entities.GetEntityById(((MyObjectBuilder_ToolbarItemTerminalBlock)slot.Data)
			//		//		.BlockEntityId);
			//		//StaticLog.WriteToLog("AlertConditions", $"TerminalBlock Identified: {me.GetType()}",
			//		//	LogType.General);
			//	}
		}


		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_timer.InScene || _timer.OwnerId != _ownerId)
				return false;

			Execute(alertSetting);

			return true;
		}

		public void Close()
		{

		}

		private void Execute(AlertSetting alertSetting)
		{

			_timer.Enabled = false;

			TimerSettings settings =
				alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			//_timer.Enabled = settings.Enabled;

			//if (alertSetting == AlertSetting.Peacetime && _alertOffTimer)
			//	_timer.Trigger();

			//if (alertSetting == AlertSetting.Wartime && _alertOnTimer)
			//	_timer.Trigger();
			//if (_timer.Enabled && alertSetting == AlertSetting.Wartime) _timer.Trigger();
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}
	}
}
