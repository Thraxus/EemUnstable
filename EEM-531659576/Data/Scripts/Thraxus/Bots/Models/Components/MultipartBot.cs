using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Common;
using Sandbox.ModAPI;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;
using IMyEntity = VRage.ModAPI.IMyEntity;

namespace Eem.Thraxus.Bots.Models.Components
{
	internal class MultiPartBot : BotBaseAdvanced
	{
		private BotOrphan _myOldParentInfo;

		/// <inheritdoc />
		public MultiPartBot(IMyEntity passedEntity, IMyShipController controller) : base(passedEntity, controller)
		{
			ThisCubeGrid.OnGridSplit += OnGridSplit;
		}

		/// <inheritdoc />
		internal override void SetupBot()
		{
			base.SetupBot();
			Marshall.WriteToLog("SetupBot", $"New Entity -\tId:\t{ThisEntity.EntityId}\tName:\t{ThisEntity.DisplayName}", true);
			if (Marshall.BotOrphans.TryGetValue(ThisEntity.EntityId, out _myOldParentInfo))
				Marshall.WriteToLog("SetupBot", $"I'm a new Entity with Id: {ThisEntity.EntityId}.  My parent was an entity with Id: {_myOldParentInfo.MyParentId}.  My grandparent was an entity with Id: {_myOldParentInfo.MyAncestors}", true);
		}

		public void Unload()
		{
			ThisCubeGrid.OnGridSplit -= OnGridSplit;
		}

		private void OnGridSplit(IMyCubeGrid originalGrid, IMyCubeGrid newGrid)
		{
			_myOldParentInfo.MyAncestors.Add(_myOldParentInfo.MyParentId);
			Marshall.BotOrphans.Add(newGrid.EntityId, new BotOrphan(originalGrid.EntityId, _myOldParentInfo.MyAncestors, _myOldParentInfo.MyLegacyConfig));
		}
	}
}
