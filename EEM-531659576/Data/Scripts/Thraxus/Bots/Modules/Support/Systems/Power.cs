using System.Runtime.InteropServices;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Eem.Thraxus.Bots.Modules.Support.Systems.Types;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Microsoft.Xml.Serialization.GeneratedAssembly;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;

namespace Eem.Thraxus.Bots.Modules.Support.Systems
{
	internal class Power : INeedUpdates
	{
		private readonly PowerProducer _powerProducers;

		public Power()
		{
			_powerProducers = new PowerProducer(SystemType.PowerProducer);
			_powerProducers.SystemDamaged += SystemDamaged;
		}

		private void SystemDamaged(SystemType type, float remainingFunctionalIntegrityRatio)
		{
			StaticLog.WriteToLog("SystemDamaged", $"{type} | {remainingFunctionalIntegrityRatio}", LogType.General);
		}

		public void AddBlock(IMyPowerProducer producer)
		{
			if (IsClosed) return;
			_powerProducers.AddBlock((IMyFunctionalBlock) producer);
		}

		public bool IsClosed { get; private set; }

		public void RunUpdate()
		{
			_powerProducers.RunUpdate();
		}

		public void Close()
		{
			if (IsClosed) return;
			_powerProducers.Close();
			_powerProducers.SystemDamaged -= SystemDamaged;
			IsClosed = true;
		}
	}
}
