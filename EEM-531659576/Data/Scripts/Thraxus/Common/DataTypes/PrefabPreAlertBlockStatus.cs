using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;

namespace Eem.Thraxus.Common.DataTypes
{
	public class PrefabPreAlertBlockStatus
	{
		/*
		 *	for doors: check enabled, closed -> on alert, close, disable
		 *	for turrets: check enabled, targeting -> on alert, ensure enabled and targeting players / grids
		 *	for air vents: check enabled, pressurization status -> on alert, depressurize 
		 *	for gravity generators: check enabled, gravity settings -> on alert, disable?  maybe reverse?  maybe max settings? maybe gamble and pick one of the 3?
		 */


		private IMyAdvancedDoor myAdvancedDoor;
		private IMyAirtightHangarDoor myAirtightHangarDoor;
		private IMyLargeGatlingTurret myLargeGatlingTurret;
		private IMyLargeInteriorTurret myLargeInteriorTurret;
		private IMyLargeMissileTurret myLargeMissileTurret;
		private IMyAirVent myAirVent;
		private IMyGravityGeneratorBase myGravityGenerator;


	}
}
