using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Systems.Types;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	//internal class Navigation : INeedUpdates
	//{
	//	private readonly Gyro _gyros;

	//	public Navigation()
	//	{
	//		_gyros = new Gyro(SystemType.Gyro);
	//		_gyros.SystemDamaged += SystemDamaged;
	//	}

	//	private void SystemDamaged(SystemType type, float remainingFunctionalIntegrityRatio)
	//	{
	//		StaticLog.WriteToLog("SystemDamaged", $"{type} | {remainingFunctionalIntegrityRatio}", LogType.General);
	//	}

	//	public void AddBlock(IMyGyro gyro)
	//	{
	//		if (IsClosed) return;
	//		_gyros.AddBlock(gyro);
	//	}


	//	public bool IsClosed { get; private set; }

	//	public void RunMassUpdate()
	//	{
	//		_gyros.RunMassUpdate();
	//	}

	//	public void Close()
	//	{
	//		if (IsClosed) return;
	//		_gyros.Close();
	//		_gyros.SystemDamaged -= SystemDamaged;
	//		IsClosed = true;
	//	}
	//}
}
