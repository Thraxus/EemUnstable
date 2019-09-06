using System;
using System.Linq;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.StaticMethods;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.EntityManager.Models
{
	public class EntityTracker : LogBaseEvent
	{
		public EntityTracker()
		{
			MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
			MyAPIGateway.Entities.OnEntityRemove += OnEntityRemoved;
		}

		public void Close()
		{
			MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
			MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemoved;
		}

		private void OnEntityAdd(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			PrintShipSpawn(myEntity);
		}

		private void OnEntityRemoved(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			PrintShipDespawn(myEntity);
		}

		private void PrintShipSpawn(IMyEntity myEntity)
		{
			try
			{
				MyCubeGrid myCubeGrid = (MyCubeGrid)myEntity;
				if (myCubeGrid.BigOwners.Count > 0)
				{
					IMyIdentity identityById = StaticMethods.GetIdentityById(myCubeGrid.BigOwners.FirstOrDefault());
					string factiontag = MyAPIGateway.Session.Factions.TryGetPlayerFaction(identityById.IdentityId).Tag;
					if (string.IsNullOrEmpty(factiontag))
						factiontag = "NONE";
					WriteToLog("PrintShipSpawn", $"ShipId:\t{myEntity.EntityId}\tShipDisplayName:\t{myEntity.DisplayName}\tOwningFaction:\t{factiontag}", LogType.General);
					return;
				}
				WriteToLog("PrintShipSpawn", $"ShipId:\t{myEntity.EntityId}\tShipDisplayName:\t{myEntity.DisplayName}", LogType.General);
			}
			catch (Exception e)
			{
				WriteToLog("PrintShipSpawn", $"Exception! {e}", LogType.Exception);
			}
		}

		private void PrintShipDespawn(IMyEntity myEntity)
		{
			try
			{
				MyCubeGrid mygrid = (MyCubeGrid)myEntity;
				WriteToLog("PrintShipDespawn", $"Id:\t{mygrid.EntityId}\tDisplayName:\t{mygrid.DisplayName}\tPCU:\t{mygrid.BlocksPCU}", LogType.General);
			}
			catch (Exception e)
			{
				WriteToLog("PrintShipDespawn", $"Exception! {e}", LogType.Exception);
			}
		}
	}
}
