using Eem.Thraxus.Bots.Modules.Support.Systems;
using Eem.Thraxus.Bots.Modules.Support.Systems.Support;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.Modules
{
	internal class ShipSystems
	{
		private readonly Navigation _navigation = new Navigation();

		private readonly Power _power = new Power();

		private readonly Propulsion _propulsion = new Propulsion();

		private  readonly StructuralIntegrity _structuralIntegrity = new StructuralIntegrity();

		private readonly Weapons _weapons = new Weapons();

		private readonly MyCubeGrid _thisGrid;

		public ShipSystems(MyCubeGrid thisGrid, IMyShipController controller)
		{
			_thisGrid = thisGrid;

			foreach (MyCubeBlock block in _thisGrid.GetFatBlocks())
			{
				IMyThrust myThrust = block as IMyThrust;
				if (myThrust != null) 
				{
					if (controller.WorldMatrix.Forward * -1 == myThrust.WorldMatrix.Forward)
						_propulsion.AddBlock(myThrust, SystemType.ForwardPropulsion);
					if (controller.WorldMatrix.Backward * -1 == myThrust.WorldMatrix.Forward)
						_propulsion.AddBlock(myThrust, SystemType.ReversePropulsion);
					if (controller.WorldMatrix.Left * -1 == myThrust.WorldMatrix.Forward)
						_propulsion.AddBlock(myThrust, SystemType.LeftPropulsion);
					if (controller.WorldMatrix.Right * -1 == myThrust.WorldMatrix.Forward)
						_propulsion.AddBlock(myThrust, SystemType.RightPropulsion);
					if (controller.WorldMatrix.Up * -1 == myThrust.WorldMatrix.Forward)
						_propulsion.AddBlock(myThrust, SystemType.UpPropulsion);
					if (controller.WorldMatrix.Down * -1 == myThrust.WorldMatrix.Forward)
						_propulsion.AddBlock(myThrust, SystemType.DownPropulsion);
					continue;
				}

				IMyGyro myGyro = block as IMyGyro;
				if (myGyro != null)
				{
					
					continue;
				}
				
				IMyPowerProducer myPower = block as IMyPowerProducer;
				if (myPower != null)
				{

					continue;
				}
				
				IMyLargeTurretBase myLargeTurretBase = block as IMyLargeTurretBase;
				if (myLargeTurretBase != null)
				{

					continue;
				}
			}
		}

		public void UpdateIntegrity()
		{
			_propulsion.RunUpdate();
		}

		public void Close()
		{
			_propulsion.Close();
		}

	}
}
