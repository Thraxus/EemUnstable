using Eem.Thraxus.Bots.Models;
using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.BaseClasses;
using Sandbox.ModAPI;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;
using IMyEntity = VRage.ModAPI.IMyEntity;

namespace Eem.Thraxus.Bots.Modules
{
	internal class MultiPartBot : BotBaseAdvanced
	{
		private BotOrphan _myOldParentInfo;

		/// <inheritdoc />
		public MultiPartBot(IMyEntity passedEntity, IMyShipController controller) : base(passedEntity, controller)
		{
			ThisCubeGrid.OnGridSplit += OnGridSplit;
		}

		internal new void SetupBot()
		{
			base.SetupBot();
			OnWriteToLog("SetupBot", $"New Entity -\tId:\t{ThisEntity.EntityId}\tName:\t{ThisEntity.DisplayName}", LogType.General);
			if (BotMarshal.BotOrphans.TryGetValue(ThisEntity.EntityId, out _myOldParentInfo))
				OnWriteToLog("SetupBot", $"I'm a new Entity with Id: {ThisEntity.EntityId}.  My parent was an entity with Id: {_myOldParentInfo.MyParentId}.  My grandparent was an entity with Id: {_myOldParentInfo.MyAncestors}", LogType.General);
		}

		public new void Unload()
		{
			ThisCubeGrid.OnGridSplit -= OnGridSplit;
		}

		private void OnGridSplit(IMyCubeGrid originalGrid, IMyCubeGrid newGrid)
		{
			_myOldParentInfo.MyAncestors.Add(_myOldParentInfo.MyParentId);
			BotMarshal.BotOrphans.TryAdd(newGrid.EntityId, new BotOrphan(originalGrid.EntityId, _myOldParentInfo.MyAncestors, _myOldParentInfo.MyLegacyConfig));
		}
	}
}
