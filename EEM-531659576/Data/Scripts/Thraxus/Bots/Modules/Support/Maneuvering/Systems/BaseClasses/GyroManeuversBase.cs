using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Maneuvering.Systems.BaseClasses
{
	internal abstract class GyroManeuversBase
	{
		protected IMyGyro Gyro;
		private readonly long _shipOwnerId;
		private readonly long _controllerId;
		public bool IsClosed;

		protected GyroManeuversBase(IMyGyro gyro, long shipOwnerId, long controllerId)
		{
			Gyro = gyro;
			_shipOwnerId = shipOwnerId;
			_controllerId = controllerId;
		}

		public virtual void Yaw(float? x)
		{
			if (IsClosed) return;
		}

		public virtual void Pitch(float? x)
		{
			if (IsClosed) return;
		}

		public virtual void Roll(float? x)
		{
			if (IsClosed) return;
		}

		public void UpdateBlockStatus()
		{
			if (IsClosed) return;
			if (!Gyro.InScene ||
			    Gyro.MarkedForClose ||
			    Gyro.Closed ||
			    Gyro.OwnerId != _shipOwnerId ||
			    Gyro.CubeGrid.EntityId != _controllerId)
				Close();
		}

		public void Close()
		{
			if (IsClosed) return;
			IsClosed = true;
			Gyro = null;
		}
	}
}
