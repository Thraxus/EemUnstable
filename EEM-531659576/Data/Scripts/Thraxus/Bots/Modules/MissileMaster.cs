using System;
using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Common;
using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;

namespace Eem.Thraxus.Bots.Modules
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_Missile), false)]
	internal class MissileMaster : MyGameLogicComponent
	{
		/// <inheritdoc />
		public override void OnAddedToScene()
		{
			base.OnAddedToScene();
			try
			{
				if (!Helpers.Constants.IsServer) return;
				DamageHandler.RegisterNewMissile(new MissileHistory(((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).LauncherId, ((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).Owner, Entity.GetPosition(),DateTime.Now));
			}
			catch (Exception e)
			{
				BotMarshal.ExceptionLog("MissileMaster", $"OnAddedToScene Exception! {e}");
			}
		}

		/// <inheritdoc />
		public override void OnRemovedFromScene()
		{
			base.OnRemovedFromScene();
			try
			{
				if (!Helpers.Constants.IsServer) return;
				DamageHandler.RegisterUnownedMissileImpact(new MissileHistory(((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).LauncherId, ((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).Owner, Entity.GetPosition(), DateTime.Now));
			}
			catch (Exception e)
			{
				BotMarshal.ExceptionLog("MissileMaster", $"OnRemovedFromScene Exception! {e}");
			}
		}
	}
}
