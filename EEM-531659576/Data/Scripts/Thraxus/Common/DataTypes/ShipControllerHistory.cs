using System;

namespace Eem.Thraxus.Common.DataTypes
{
	public struct ShipControllerHistory
	{
		public readonly long Controller;
		public readonly long ControlledEntity;
		public readonly DateTime TimeStamp;

		public ShipControllerHistory(long controller, long controlledEntity, DateTime timeStamp)
		{
			Controller = controller;
			ControlledEntity = controlledEntity;
			TimeStamp = timeStamp;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Controller: {Controller} - ControllerEntity: {ControlledEntity} - TimeStamp: {TimeStamp}";
		}
	}
}