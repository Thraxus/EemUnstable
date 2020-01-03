using Eem.Thraxus.Common.DataTypes;
using Sandbox.ModAPI.Ingame;
using IMyDoor = Sandbox.ModAPI.IMyDoor;

namespace Eem.Thraxus.Bots.Modules.Support
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
			public readonly bool Closed;

			public DoorSettings(bool enabled, bool isClosed)
			{
				Enabled = enabled;
				Closed = isClosed;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return $"{Enabled} {Closed}";
			}
		}

		public Door(IMyDoor door)
		{
			_door = door;
			_ownerId = door.OwnerId;
			_door.OnDoorStateChanged += OnDoorStateChanged;
			_peacetimeSettings = new DoorSettings(_door.Enabled, _door.Closed);
			_wartimeSettings = new DoorSettings(false, true);
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

			if (!settings.Closed && 
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
