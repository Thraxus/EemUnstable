using Sandbox.ModAPI.Ingame;
using IMyDoor = Sandbox.ModAPI.IMyDoor;

namespace Eem.Thraxus.Bots.Modules.Support
{
	internal class Door
	{
		private readonly IMyDoor _door;
		private readonly long _ownerId;
		private readonly DoorSettings _wartimeSettings;
		private readonly DoorSettings _peacetimeSettings;
		private AlertSetting _alertSetting;

		public Door(IMyDoor door, DoorSettings wartimeSettings, DoorSettings peacetimeSettings)
		{
			_door = door;
			_ownerId = door.OwnerId;
			_door.OnDoorStateChanged += OnDoorStateChanged;
			_wartimeSettings = wartimeSettings;
			_peacetimeSettings = peacetimeSettings;
			_alertSetting = AlertSetting.Peacetime;
		}

		private void OnDoorStateChanged(IMyDoor myDoor, bool x)
		{
			if (myDoor.OwnerId != _ownerId) return;
			if (!x && _alertSetting == AlertSetting.Wartime)
				_door.Enabled = false;
		}

		public bool SetAlert(AlertSetting alertSetting)
		{
			if (!_door.InScene || _door.OwnerId != _ownerId)
				return false;

			_alertSetting = alertSetting;
			Execute();

			return true;
		}

		public void Close()
		{
			_door.OnDoorStateChanged -= OnDoorStateChanged;
		}

		private void Execute()
		{

			DoorSettings settings =
				_alertSetting == AlertSetting.Peacetime ? _peacetimeSettings : _wartimeSettings;

			_door.Enabled = settings.Enabled;

			if (!settings.IsClosed && 
			    (_door.Status == DoorStatus.Closed || _door.Status == DoorStatus.Closing))
			{
				_door.Enabled = true;
				_door.OpenDoor();
				return;
			}
			
			if (!_door.IsFullyClosed)
			{
				_door.Enabled = true;
				_door.CloseDoor();
				return;
			}
			_door.Enabled = settings.Enabled;
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}

	}
}
