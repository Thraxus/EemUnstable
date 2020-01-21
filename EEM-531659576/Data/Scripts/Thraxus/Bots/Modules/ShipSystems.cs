using System.Collections.Generic;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support;
using Eem.Thraxus.Bots.Modules.Support.Systems;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Bots.Modules
{
	internal class ShipSystems
	{
		private MyCubeGrid _thisGrid;

		private readonly BotSystemsQuestLog _botSystemsQuestLog;

		//private  readonly StructuralIntegrity _structuralIntegrity = new StructuralIntegrity();

		private readonly List<INeedUpdates> _shipSystems = new List<INeedUpdates>();

		public ShipSystems(MyCubeGrid thisGrid, IMyShipController controller)
		{
			_thisGrid = thisGrid;

			_botSystemsQuestLog = new BotSystemsQuestLog(_thisGrid.DisplayName);

			Propulsion propulsion = new Propulsion(_botSystemsQuestLog);

			Navigation navigation = new Navigation(_botSystemsQuestLog);

			Power power = new Power(_botSystemsQuestLog);

			Weapons weapons = new Weapons(_botSystemsQuestLog);

			foreach (MyCubeBlock block in _thisGrid.GetFatBlocks())
			{
				IMyThrust myThrust = block as IMyThrust;
				if (myThrust != null) 
				{
					if (controller.WorldMatrix.Forward * -1 == myThrust.WorldMatrix.Forward)
						propulsion.AddBlock(SystemType.ForwardPropulsion, myThrust);
					if (controller.WorldMatrix.Backward * -1 == myThrust.WorldMatrix.Forward)
						propulsion.AddBlock(SystemType.ReversePropulsion, myThrust);
					if (controller.WorldMatrix.Left * -1 == myThrust.WorldMatrix.Forward)
						propulsion.AddBlock(SystemType.LeftPropulsion, myThrust);
					if (controller.WorldMatrix.Right * -1 == myThrust.WorldMatrix.Forward)
						propulsion.AddBlock(SystemType.RightPropulsion, myThrust);
					if (controller.WorldMatrix.Up * -1 == myThrust.WorldMatrix.Forward)
						propulsion.AddBlock(SystemType.UpPropulsion, myThrust);
					if (controller.WorldMatrix.Down * -1 == myThrust.WorldMatrix.Forward)
						propulsion.AddBlock(SystemType.DownPropulsion, myThrust);
					continue;
				}

				IMyGyro myGyro = block as IMyGyro;
				if (myGyro != null)
				{
					navigation.AddBlock(SystemType.Navigation, myGyro);
					continue;
				}

				IMyPowerProducer myPower = block as IMyPowerProducer;
				if (myPower != null)
				{
					power.AddBlock(SystemType.PowerProducer, (IMyFunctionalBlock) myPower);
					continue;
				}

				IMyLargeTurretBase myLargeTurretBase = block as IMyLargeTurretBase;
				if (myLargeTurretBase != null)
				{
					weapons.AddBlock(SystemType.Weapon, myLargeTurretBase);
					continue;
				}
			}

			_shipSystems.Add(propulsion);
			_shipSystems.Add(navigation);
			_shipSystems.Add(power);
			_shipSystems.Add(weapons);
		}

		public void UpdateIntegrity()
		{
			foreach (INeedUpdates needUpdate in _shipSystems)
			{
				needUpdate.RunMassUpdate();
			}
		}

		public void Close()
		{
			foreach (INeedUpdates needUpdate in _shipSystems)
			{
				needUpdate.Close();
			}
			_botSystemsQuestLog.Close();
			_thisGrid = null;
		}

	}
}
