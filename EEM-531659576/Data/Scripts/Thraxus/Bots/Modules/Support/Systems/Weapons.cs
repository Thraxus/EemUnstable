using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Systems.Types;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Weapons : INeedUpdates
	{
		private readonly LargeTurretBase _turrets;

		public Weapons()
		{
			_turrets = new LargeTurretBase(SystemType.Gyro);
			_turrets.SystemDamaged += SystemDamaged;
		}

		private void SystemDamaged(SystemType type, float remainingFunctionalIntegrityRatio)
		{
			StaticLog.WriteToLog("SystemDamaged", $"{type} | {remainingFunctionalIntegrityRatio}", LogType.General);
		}

		public void AddBlock(IMyLargeTurretBase gyro)
		{
			if (IsClosed) return;
			_turrets.AddBlock(gyro);
		}


		public bool IsClosed { get; private set; }

		public void RunUpdate()
		{
			_turrets.RunUpdate();
		}

		public void Close()
		{
			if (IsClosed) return;
			_turrets.Close();
			_turrets.SystemDamaged -= SystemDamaged;
			IsClosed = true;
		}
	}
}