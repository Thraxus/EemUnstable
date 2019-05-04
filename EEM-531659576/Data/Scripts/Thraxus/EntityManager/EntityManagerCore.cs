using Eem.Thraxus.Common.BaseClasses;
using Eem.Thraxus.EntityManager.Models;
using VRage.Game.Components;

namespace Eem.Thraxus.EntityManager
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, priority: int.MaxValue - 1)]
	// ReSharper disable once ClassNeverInstantiated.Global
	internal class EntityManagerCore : BaseServerSessionComp
	{
		private const string GeneralLogName = "EntityManagerGeneral";
		private const string DebugLogName = "EntityManagerDebug";
		private const string SessionCompName = "EntityManagerCore";

		/// <inheritdoc />
		public EntityManagerCore() : base(GeneralLogName, DebugLogName, SessionCompName) { }

		private EntityTracker _entityTracker;

		/// <inheritdoc />
		protected override void EarlySetup()
		{
			base.EarlySetup();
			_entityTracker = new EntityTracker();
			_entityTracker.WriteToLog += WriteToLog;
		}

		/// <inheritdoc />
		protected override void Unload()
		{
			_entityTracker.WriteToLog -= WriteToLog;
			_entityTracker?.Close();
			base.Unload();
		}
	}
}
