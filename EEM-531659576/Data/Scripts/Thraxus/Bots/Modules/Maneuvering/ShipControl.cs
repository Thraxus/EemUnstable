using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Modules.Maneuvering
{
	internal class ShipControl
	{
		// Notes: 
		//	Yaw = [Vector3.Down] The yaw axis has its origin at the center of gravity and is directed towards the bottom of the aircraft
		//	Pitch = [Vector3.Right] The pitch axis has its origin at the center of gravity and is directed to the right
		//	Roll = [Vector3.Forward] The roll axis has its origin at the center of gravity and is directed forward
		//	https://en.wikipedia.org/wiki/Aircraft_principal_axes
		//	GyroStrengthMultiplier is a ModAPI / Def setting
		//	GyroPower is the setting in game for how strong a gyro is
		//	Yaw / Pitch / Roll go from -60.0f to 60.0f
		//	Pitch is reversed (possibly due to inverted flight controls common in games per Lucas)

		private readonly ConcurrentCachingList<IMyGyro> _shipGyros = new ConcurrentCachingList<IMyGyro>();

		internal readonly IMyEntity ThisEntity;
		internal readonly MyCubeGrid ThisCubeGrid;
		internal readonly IMyCubeGrid ThisMyCubeGrid;

		private IMyShipController _shipController;

		private IMyEntity _target;

		private bool ManeuveringActive;

		public ShipControl(IMyShipController shipController, MyCubeGrid thisGrid, IMyCubeGrid thisMyCubeGrid, IMyEntity thisEntity)
		{
			_shipController = shipController;
			ThisCubeGrid = thisGrid;
			ThisMyCubeGrid = thisMyCubeGrid;
			ThisEntity = thisEntity;
			foreach (MyCubeBlock block in ThisCubeGrid.GetFatBlocks())
			{
				IMyGyro myGyro = block as IMyGyro;
				if (myGyro != null)
				{
					_shipGyros.Add(myGyro);
				}
			}
			_shipGyros.ApplyAdditions();

			//ThisCubeGrid.GridSystems.WeaponSystem.
		}

		public void AddGyro(IMyGyro gyro)
		{
			if (_shipGyros.Contains(gyro)) return;
			_shipGyros.Add(gyro);
			_shipGyros.ApplyAdditions();
		}

		public void SetTarget(IMyEntity entity)
		{
			_target = entity;
		}

		public void ActivateManeuvering()
		{
			ManeuveringActive = true;
			foreach (IMyGyro gyro in _shipGyros)
			{
				gyro.GyroOverride = true;
				gyro.Yaw = 0;
				gyro.Pitch = 0;
				gyro.Roll = 0;
				gyro.GyroStrengthMultiplier = 1;	// ModAPI / Def Setting
				gyro.GyroPower = 1;					// Actual Gyro Power
			}
		}

		public void DeactivateManeuvering()
		{
			ManeuveringActive = false;
			foreach (IMyGyro gyro in _shipGyros)
			{
				gyro.GyroOverride = false;
				gyro.Yaw = 0;
				gyro.Pitch = 0;
				gyro.Roll = 0;
				gyro.GyroStrengthMultiplier = 1;
				gyro.GyroPower = 1;
			}
		}

		public void DebugRotate()
		{
			ApplyGyroOverride(1, 3, 5);
		}

		private void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed)
		{
			if (!ManeuveringActive) ActivateManeuvering();
			Vector3D rotationVec = new Vector3D(pitchSpeed, yawSpeed, rollSpeed); 
			MatrixD shipMatrix = _shipController.WorldMatrix;
			Vector3D relativeRotationVec = Vector3D.TransformNormal(rotationVec, shipMatrix);
			foreach (IMyGyro gyro in _shipGyros)
			{
				MatrixD gyroMatrix = gyro.WorldMatrix;
				Vector3D transformedRotationVec = Vector3D.TransformNormal(relativeRotationVec, Matrix.Transpose(gyroMatrix));
				gyro.Pitch = (float)transformedRotationVec.X;
				gyro.Yaw = (float)transformedRotationVec.Y;
				gyro.Roll = (float)transformedRotationVec.Z;
			}
		}

		public void UpdateBlocks()
		{
			foreach (IMyGyro gyro in _shipGyros)
			{
				if (!gyro.InScene || 
				    gyro.MarkedForClose || 
					gyro.Closed ||
				    gyro.OwnerId != _shipController.OwnerId || 
				    gyro.CubeGrid.EntityId != _shipController.CubeGrid.EntityId)
					_shipGyros.Remove(gyro);
			}
			_shipGyros.ApplyRemovals();
		}

		public void Close()
		{
			_shipGyros.ClearList();
			_target = null;
			_shipController = null;
		}
	}
}