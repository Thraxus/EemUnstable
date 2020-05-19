﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using Eem.Thraxus.Bots.SessionComps.Models;
using Eem.Thraxus.Bots.SessionComps.Support;
using Eem.Thraxus.Common.BaseClasses;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
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
			GlobalEvents.OnRequestTargetInformation += OnRequestTargetInformation;
		}

		private void OnRequestTargetInformation(long requestorId, long request)
		{
			EntityModel response;
			_entityModels.TryGetValue(request, out response);
			GlobalEvents.TargetInformationResponse(requestorId, response);
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
			GlobalEvents.OnRequestTargetInformation -= OnRequestTargetInformation;
			foreach (KeyValuePair<long, EntityModel> entity in _entityModels)
				EntityClose(entity.Value);
		}

		private void EntityClose(EntityModel entity)
		{
			entity.OnWriteToLog -= WriteToLog;
			entity.OnClose -= EntityClose;
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
			entity.OnClose += EntityClose;
			entity.Initialize();
			_entityModels.TryAdd(entity.ThisId, entity);
		}

		private void OnEntityRemoved(IMyEntity myEntity)
		{
			if (myEntity.GetType() != typeof(MyCubeGrid)) return;
		}
	}
}
