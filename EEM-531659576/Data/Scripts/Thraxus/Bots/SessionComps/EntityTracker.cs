using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Factions.Utilities;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Bots.SessionComps
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MinValue + 1)]
	public class EntityTracker : BaseServerSessionComp
	{
		private const string GeneralLogName = "EntityTrackerGeneral";
		private const string DebugLogName = "EntityTrackerDebug";
		private const string SessionCompName = "EntityTracker";
		private const bool NoUpdate = true;

		private readonly ConcurrentDictionary<long, EntityModel> _entityModels = new ConcurrentDictionary<long, EntityModel>();

		public EntityTracker() : base(GeneralLogName, DebugLogName, SessionCompName, NoUpdate) { }  // Do nothing else

		protected override void EarlySetup()
		{
			base.EarlySetup();
			Initialize();
		}

		private void Initialize()
		{
			MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
			MyAPIGateway.Entities.OnEntityRemove += OnEntityRemoved;
		}

		protected override void Unload()
		{
			Close();
			base.Unload();
		}
		
		private void Close()
		{
			MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
			MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemoved;
			foreach (KeyValuePair<long, EntityModel> entity in _entityModels)
				EntityClose(entity.Value);
		}

		private void EntityClose(EntityModel entity)
		{
			entity.OnWriteToLog -= WriteToLog;
			entity.OnTriggerClose -= EntityClose;
			entity.Close(entity.ThisEntity);
			_entityModels.Remove(entity.ThisId);
		}

		private void EntityClose(long entityId)
		{
			EntityModel model;
			if(_entityModels.TryGetValue(entityId, out model))
				EntityClose(model);
		}

		private void OnEntityAdd(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			EntityModel entity = new EntityModel(myEntity);
			entity.OnWriteToLog += WriteToLog;
			_entityModels.TryAdd(entity.ThisId, entity);
			//PrintShipSpawn(myEntity);
		}

		private void OnEntityRemoved(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
			//PrintShipDespawn(myEntity);
		}
	}

	public class EntityModel : LogBaseEvent
	{
		public event TriggerEntityClose OnTriggerClose;
		public delegate void TriggerEntityClose(long entityId);

		public void TriggerClose()
		{
			if(!_isClosed) OnTriggerClose?.Invoke(ThisId);
			_isClosed = true;
		}

		public readonly IMyEntity ThisEntity;
		
		private readonly MyCubeGrid _thisCubeGrid;
		
		private int BlockCount => _thisCubeGrid.CubeBlocks.Count;
		
		public long ThisId => ThisEntity.EntityId;
		
		private GridOwnerType _ownerType;
		
		private GridType _gridType;

		private bool _isClosed;

		public EntityModel(IMyEntity thisEntity)
		{
			ThisEntity = thisEntity;
			_thisCubeGrid = (MyCubeGrid) ThisEntity;
			base.Id = thisEntity.EntityId.ToString();
			Initialize();
		}

		private void Initialize()
		{
			_thisCubeGrid.OnClose += Close;
			_thisCubeGrid.OnBlockOwnershipChanged += ThisCubeGridOnOnBlockOwnershipChanged; 
			_thisCubeGrid.OnBlockAdded += ThisCubeGridOnOnBlockCountChange;
			_thisCubeGrid.OnBlockRemoved += ThisCubeGridOnOnBlockCountChange;
			GetOwnerType();
			GetGridType();
			WriteToLog($"Initialize", $"{BlockCount} | {_ownerType} | {_gridType}", LogType.General);
		}

		public void Close(IMyEntity unused)
		{
			// Closing stuff happens here
			if (_isClosed) return;
			_thisCubeGrid.OnClose -= Close;
			_thisCubeGrid.OnBlockOwnershipChanged -= ThisCubeGridOnOnBlockOwnershipChanged;
			_thisCubeGrid.OnBlockAdded -= ThisCubeGridOnOnBlockCountChange;
			_thisCubeGrid.OnBlockRemoved -= ThisCubeGridOnOnBlockCountChange;
			WriteToLog($"Close", $"I'm out!", LogType.General);
			TriggerClose();
		}

		private void ThisCubeGridOnOnBlockCountChange(IMySlimBlock obj)
		{
			GetGridType();
		}

		private void ThisCubeGridOnOnBlockOwnershipChanged(MyCubeGrid unused)
		{
			GetOwnerType();
		}
		
		private void GetOwnerType()
		{
			if (_thisCubeGrid.BigOwners.Count == 0)
			{
				_ownerType = GridOwnerType.None;
				WriteToLog($"GetOwnerType", $"{_ownerType}", LogType.General);
				return;
			}
			_ownerType = StaticMethods.ValidPlayer(_thisCubeGrid.BigOwners[0]) ? GridOwnerType.Player : GridOwnerType.Npc;
			WriteToLog($"GetOwnerType", $"{_ownerType}", LogType.General);
		}

		private void GetGridType()
		{
			if (_thisCubeGrid.Physics == null)
			{
				_gridType = GridType.Projection;
				WriteToLog($"GetGridType", $"{_gridType}", LogType.General);
				return;
			}

			if (_thisCubeGrid.CubeBlocks.Count < 5)
			{
				_gridType = GridType.Debris;
				WriteToLog($"GetGridType", $"{_gridType}", LogType.General);
				return;
			}

			if (_thisCubeGrid.IsStatic || _thisCubeGrid.IsUnsupportedStation)
			{
				_gridType = GridType.Station;
				WriteToLog($"GetGridType", $"{_gridType}", LogType.General);
				return;
			}
			_gridType = GridType.Ship;
			WriteToLog($"GetGridType", $"{_gridType}", LogType.General);
		}
	}

	public enum GridOwnerType
	{
		Player,
		Npc,
		None
	}

	public enum GridType
	{
		Debris,
		Projection,
		Ship,
		Station
	}
}
