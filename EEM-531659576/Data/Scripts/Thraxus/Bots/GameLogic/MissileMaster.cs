using System;
using Eem.Thraxus.Bots.SessionComps;
using Eem.Thraxus.Common.DataTypes;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;

namespace Eem.Thraxus.Bots.GameLogic
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_Missile), false)]
	internal class MissileMaster : MyGameLogicComponent
	{
		/// <inheritdoc />
		public override void OnAddedToScene()
		{
			//base.OnAddedToScene();
			//try
			//{
			//	if (!Helpers.Constants.IsServer) return;
			//	DamageHandler.RegisterUnownedMissileImpact(new MissileHistory(((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).LauncherId, ((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).Owner, Entity.GetPosition(), DateTime.Now));
			//}
			//catch (Exception e)
			//{
			//	StaticLogger.WriteToLog("MissileMaster", $"OnAddedToScene Exception! {e}", LogType.Exception);
			//}
		}

		/// <inheritdoc />
		public override void OnRemovedFromScene()
		{
			base.OnRemovedFromScene();
			MyObjectBuilder_Missile missile = (MyObjectBuilder_Missile) Entity.GetObjectBuilder();
			try
			{
				if (!Helpers.Constants.IsServer) return;
				DamageHandler.RegisterUnownedMissileImpact(new 
					MissileHistory(
						missile.OriginEntity,
						missile.LauncherId, 
						missile.Owner, 
						Entity.GetPosition(), 
						DateTime.Now));
			}
			catch (Exception e)
			{
				StaticLog.WriteToLog("MissileMaster", 
					$"OnRemovedFromScene Exception! {e}", LogType.Exception);
			}
		}
	}
}
