using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.SessionComps.Support;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Factions.Utilities;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.SessionComps.Models
{
	public class EntityModel : LogBaseEvent
	{
		public event Action<long> OnClose;
		
		public readonly IMyEntity ThisEntity;
		
		private readonly MyCubeGrid _thisCubeGrid;

		private readonly IMyCubeGrid _thisMyCubeGrid;

		private readonly List<IMySlimBlock> _blocks = new List<IMySlimBlock>();

		private int BlockCount => _thisCubeGrid.CubeBlocks.Count;

		private int _gridValue = 0;

		private int _gridThreat = 0;
		
		public long ThisId => ThisEntity.EntityId;
		
		private GridOwnerType _ownerType;
		
		private GridType _gridType;

		private bool _isClosed;

		public EntityModel(IMyEntity thisEntity)
		{
			ThisEntity = thisEntity;
			_thisCubeGrid = (MyCubeGrid) ThisEntity;
			_thisMyCubeGrid = (IMyCubeGrid) ThisEntity;
			base.Id = thisEntity.EntityId.ToString();
			_thisCubeGrid.OnClose += Close;
			_thisCubeGrid.OnBlockOwnershipChanged += OwnershipChanged;
			_thisCubeGrid.OnBlockAdded += BlockAdded;
			_thisCubeGrid.OnBlockRemoved += BlockRemoved;
			_thisCubeGrid.OnGridSplit += GridSplit;
		}

		public void Initialize()
		{
			GetOwnerType();
			GetGridType();
			GridValuation();
			WriteToLog($"Initialize", $"{BlockCount} | {_ownerType} | {_gridType}", LogType.General);
		}

		public void Close(IMyEntity unused)
		{
			// Closing stuff happens here
			if (_isClosed) return;
			_isClosed = true;
			_thisCubeGrid.OnClose -= Close;
			_thisCubeGrid.OnBlockOwnershipChanged -= OwnershipChanged;
			_thisCubeGrid.OnBlockAdded -= BlockAdded;
			_thisCubeGrid.OnBlockRemoved -= BlockRemoved;
			_thisCubeGrid.OnGridSplit -= GridSplit;
			WriteToLog($"Close", $"I'm out!", LogType.General);
			OnClose?.Invoke(ThisId);
		}
		
		private void BlockAdded(IMySlimBlock block)
		{
			GetGridType();
			GetBlockValue(block);
			WriteToLog($"BlockAdded", $"{block.BlockDefinition.GetType()}", LogType.General);
		}

		private void BlockRemoved(IMySlimBlock block)
		{
			GetGridType();
			GetBlockValue(block, true);
			WriteToLog($"BlockRemoved", $"{block.BlockDefinition.GetType()}", LogType.General);
		}

		private void GridSplit(MyCubeGrid unused, MyCubeGrid alsoUnused)
		{
			WriteToLog($"GridSplit", $"Resetting... {unused.EntityId} | {alsoUnused.EntityId}", LogType.General);
			_gridThreat = 0;
			_gridValue = 0;
			GridValuation();
		}

		private void OwnershipChanged(MyCubeGrid unused)
		{
			GetOwnerType();
		}

		private void GridValuation()
		{
			_blocks.Clear();
			_thisMyCubeGrid.GetBlocks(_blocks);

			foreach (IMySlimBlock block in _blocks)
				GetBlockValue(block);
			_blocks.Clear();
			WriteToLog($"GridValuation", $"Total Value: {_gridValue} | Total Threat: {_gridThreat}", LogType.General);
		}

		private void GetBlockValue(IMySlimBlock block, bool negate = false)
		{
			if (block.FatBlock == null)
			{
				SetValues(new BlockValue(){Threat = 1, Value = 1}, negate);
				return;
			}
			BlockValue value = Statics.GetBlockValue(block.FatBlock.BlockDefinition.TypeId);
			SetValues(value, negate);
			//WriteToLog($"GetBlockValue", $"{block.FatBlock.GetType()} | {block.FatBlock.BlockDefinition.TypeId} | {block.FatBlock.BlockDefinition.GetType()} -- {value}", LogType.General);
			//WriteToLog($"GetBlockValue", $"{block.FatBlock.GetType()} | {block.BlockDefinition} | {block.BlockDefinition.GetType()} -- {value}", LogType.General);
		}

		private void SetValues(BlockValue value, bool negate)
		{
			WriteToLog($"GetBlockValue", $"New Value: {value} | Negate: {negate}", LogType.General);
			if (negate)
			{
				_gridThreat -= value.Threat;
				_gridValue -= value.Value;
				WriteToLog($"GetBlockValue", $"Total Value: {_gridValue} | Total Threat: {_gridThreat}", LogType.General);
				return;
			}
			_gridThreat += value.Threat;
			_gridValue += value.Value;
			//WriteToLog($"GetBlockValue", $"New Value: {value}", LogType.General);
			WriteToLog($"GetBlockValue", $"Total Value: {_gridValue} | Total Threat: {_gridThreat}", LogType.General);
		}

		private void GetOwnerType()
		{
			if (_thisCubeGrid.BigOwners.Count == 0)
			{
				_ownerType = GridOwnerType.None;
				//WriteToLog($"GetOwnerType", $"{_ownerType}", LogType.General);
				return;
			}
			_ownerType = StaticMethods.ValidPlayer(_thisCubeGrid.BigOwners[0]) ? GridOwnerType.Player : GridOwnerType.Npc;
			//WriteToLog($"GetOwnerType", $"{_ownerType}", LogType.General);
		}

		private void GetGridType()
		{
			if (_thisCubeGrid.Physics == null)
			{
				_gridType = GridType.Projection;
				//WriteToLog($"GetGridType", $"{_gridType}", LogType.General);
				return;
			}

			if (_thisCubeGrid.CubeBlocks.Count < 5)
			{
				_gridType = GridType.Debris;
				//WriteToLog($"GetGridType", $"{_gridType}", LogType.General);
				return;
			}

			if (_thisCubeGrid.IsStatic || _thisCubeGrid.IsUnsupportedStation)
			{
				_gridType = GridType.Station;
				//WriteToLog($"GetGridType", $"{_gridType}", LogType.General);
				return;
			}
			_gridType = GridType.Ship;
			//WriteToLog($"GetGridType", $"{_gridType}", LogType.General);
		}
	}
}