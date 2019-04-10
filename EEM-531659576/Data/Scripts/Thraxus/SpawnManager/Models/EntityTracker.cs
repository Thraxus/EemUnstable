using Sandbox.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.SpawnManager.Models
{
	public class EntityTracker
	{
		public EntityTracker()
		{
			MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
		}

		public void Unload()
		{
			MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
		}

		private void OnEntityAdd(IMyEntity myEntity)
		{

		}
	}
}
