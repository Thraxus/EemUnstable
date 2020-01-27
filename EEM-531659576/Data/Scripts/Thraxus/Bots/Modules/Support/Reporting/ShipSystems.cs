using System.Collections.Generic;
using Eem.Thraxus.Bots.Interfaces;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems;
using Eem.Thraxus.Bots.Modules.Support.Reporting.Systems.Support;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using IMyUserControllableGun = Sandbox.ModAPI.Ingame.IMyUserControllableGun;

namespace Eem.Thraxus.Bots.Modules.Support.Reporting
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

			Turrets turrets = new Turrets(_botSystemsQuestLog);

			FixedWeapons fixedWeapons = new FixedWeapons(_botSystemsQuestLog);

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
					turrets.AddBlock(SystemType.Turret, myLargeTurretBase);
					continue;
				}

				if (block is IMyUserControllableGun)
				{
					fixedWeapons.AddBlock(SystemType.FixedWeapon, (IMyFunctionalBlock) block);
				}

			}

			_shipSystems.Add(propulsion);
			_shipSystems.Add(navigation);
			_shipSystems.Add(power);
			_shipSystems.Add(turrets);
			_shipSystems.Add(fixedWeapons);
		}

		public void UpdateIntegrity(long blockId)
		{
			foreach (INeedUpdates needUpdate in _shipSystems)
			{
				needUpdate.RunMassUpdate(blockId);
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
