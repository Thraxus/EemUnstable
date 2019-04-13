using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.SpawnManager.Models
{
	public class EntityTracker
	{
		public EntityTracker()
		{
			MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
		}

		public void Close()
		{
			MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
		}

		private void OnEntityAdd(IMyEntity myEntity)
		{
			SpawnManagerCore.WriteToLog("OnEntityAdd", $"{myEntity.EntityId}\t{myEntity.Name}\t{myEntity.DisplayName}\t{myEntity.GetType()}\t{myEntity.GetObjectBuilder().GetType()}", true);
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			SpawnManagerCore.WriteToLog("OnEntityAdd", $"Spawn passed the test...", true);
		}
	}
}
