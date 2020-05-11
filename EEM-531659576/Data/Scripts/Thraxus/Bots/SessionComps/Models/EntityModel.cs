using System;
using System.Collections.Generic;
using Eem.Thraxus.Bots.SessionComps.Interfaces;
using Eem.Thraxus.Bots.SessionComps.Support;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Factions.Utilities;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Eem.Thraxus.Bots.SessionComps.Models
{
	public class EntityModel : LogBaseEvent, IPotentialTarget
	{
		// Interface Implementation 
		public event Action<long> OnClose;
		
		public int Threat { get; private set; }
		
		public int Value { get; private set; }
		
		public List<IMyCubeBlock> Controllers { get; } = new List<IMyCubeBlock>();
		
		public List<IMyCubeBlock> Navigation { get; } = new List<IMyCubeBlock>();
		
		public List<IMyCubeBlock> PowerSystems { get; } = new List<IMyCubeBlock>();
		
		public List<IMyCubeBlock> Propulsion { get; } = new List<IMyCubeBlock>();
		
		public List<IMyCubeBlock> Weapons { get; } = new List<IMyCubeBlock>();

		public long FactionId => MyAPIGateway.Session.Factions.TryGetPlayerFaction(OwnerId)?.FactionId ?? 0;

		public long OwnerId => OwnerType == GridOwnerType.None ? 0 : _thisCubeGrid.BigOwners[0];
		
		public GridOwnerType OwnerType { get; private set; }
		
		public FactionRelationship GetRelationship(long requestingGridOwnerId)
		{
			return FactionId == 0 ? FactionRelationship.Friends :
				MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(requestingGridOwnerId, FactionId) >=
				-500 ? FactionRelationship.Friends : FactionRelationship.Enemies;
		}

		public bool HasBars { get; private set; }
		
		public bool HasHeavyArmor { get; private set; }

		public bool HasShields { get; private set; }
		
		public bool IsClosed { get; private set; }
		
		public bool IsModded { get; private set; }
		
		public Vector3 LinearVelocity => _thisCubeGrid.Physics?.LinearVelocity ?? Vector3.Zero;

		public MyCubeSize Size => _thisCubeGrid.GridSizeEnum;
		
		public GridType GridType { get; private set; }

		public Vector3D Position => _thisMyCubeGrid.GetPosition();
		

		// Class Members
		public readonly IMyEntity ThisEntity;
		
		private readonly MyCubeGrid _thisCubeGrid;

		private readonly IMyCubeGrid _thisMyCubeGrid;

		private readonly List<IMySlimBlock> _blocks = new List<IMySlimBlock>();

		private int BlockCount => _thisCubeGrid.CubeBlocks.Count;
		
		public long ThisId => ThisEntity.EntityId;
		
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
			WriteToLog($"Initialize", $"{BlockCount} | {OwnerType} | {GridType}", LogType.General);
		}

		public void Close(IMyEntity unused)
		{
			// Closing stuff happens here
			if (IsClosed) return;
			IsClosed = true;
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
			Threat = 0;
			Value = 0;
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
			WriteToLog($"GridValuation", $"Total Value: {Value} | Total Threat: {Threat}", LogType.General);
		}

		private void GetBlockValue(IMySlimBlock block, bool negate = false)
		{
			CubeProcessor.ProcessCube(block);
			if (block.FatBlock == null)
			{
				if (HasHeavyArmor == false)
					if (block.BlockDefinition.Id.SubtypeName.Contains($"HeavyBlockArmor")) HasHeavyArmor = true;
				SetValues(new BlockValue {Threat = 1, Value = 1}, negate);
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
				Threat -= value.Threat;
				Value -= value.Value;
				WriteToLog($"GetBlockValue", $"Total Value: {Value} | Total Threat: {Threat}", LogType.General);
				return;
			}
			Threat += value.Threat;
			Value += value.Value;
			//WriteToLog($"GetBlockValue", $"New Value: {value}", LogType.General);
			WriteToLog($"GetBlockValue", $"Total Value: {Value} | Total Threat: {Threat}", LogType.General);
		}

		private void GetOwnerType()
		{
			if (_thisCubeGrid.BigOwners.Count == 0)
			{
				OwnerType = GridOwnerType.None;
				//WriteToLog($"GetOwnerType", $"{OwnerType}", LogType.General);
				return;
			}
			OwnerType = Common.Utilities.Statics.Statics.ValidPlayer(_thisCubeGrid.BigOwners[0]) ? GridOwnerType.Player : GridOwnerType.Npc;
			//WriteToLog($"GetOwnerType", $"{OwnerType}", LogType.General);
		}

		private void GetGridType()
		{
			if (_thisCubeGrid.Physics == null)
			{
				GridType = GridType.Projection;
				//WriteToLog($"GetGridType", $"{GridType}", LogType.General);
				return;
			}

			if (_thisCubeGrid.CubeBlocks.Count < 5)
			{
				GridType = GridType.Debris;
				//WriteToLog($"GetGridType", $"{GridType}", LogType.General);
				return;
			}

			if (_thisCubeGrid.IsStatic || _thisCubeGrid.IsUnsupportedStation)
			{
				GridType = GridType.Station;
				//WriteToLog($"GetGridType", $"{GridType}", LogType.General);
				return;
			}
			GridType = GridType.Ship;
			//WriteToLog($"GetGridType", $"{GridType}", LogType.General);
		}
	}
}