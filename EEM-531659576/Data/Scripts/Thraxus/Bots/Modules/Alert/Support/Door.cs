using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Common.Enums;
using Sandbox.ModAPI.Ingame;
using IMyDoor = Sandbox.ModAPI.IMyDoor;

namespace Eem.Thraxus.Bots.Modules.Alert.Support
{
	internal class Door : ISetAlert
	{
		private readonly IMyDoor _door;
		private readonly long _ownerId;
		private readonly DoorSettings _wartimeSettings;
		private readonly DoorSettings _peacetimeSettings;
		private AlertSetting _alertSetting;

		internal struct DoorSettings
		{
			public readonly bool Enabled;
			public readonly DoorStatus DoorStatus;

			public DoorSettings(bool enabled, DoorStatus doorStatus)
			{
				Enabled = enabled;
				DoorStatus = doorStatus;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled} {DoorStatus}";
			}
		}

		public Door(IMyDoor door)
		{
			_door = door;
			_ownerId = door.OwnerId;
			_door.OnDoorStateChanged += OnDoorStateChanged;
			_peacetimeSettings = new DoorSettings(_door.Enabled, _door.Status);
			_wartimeSettings = new DoorSettings(false, DoorStatus.Closed);
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

			if (_alertSetting == AlertSetting.Peacetime)
			{
				if (settings.DoorStatus == DoorStatus.Closed || 
				    settings.DoorStatus == DoorStatus.Closing) return;
				_door.OpenDoor();
				return;
			}

			if (_alertSetting != AlertSetting.Wartime) return;
			if (_door.IsFullyClosed) return;
			_door.Enabled = true;
			_door.CloseDoor();
		}

		public override string ToString()
		{
			return $"{_wartimeSettings} | {_peacetimeSettings}";
		}

	}
}
