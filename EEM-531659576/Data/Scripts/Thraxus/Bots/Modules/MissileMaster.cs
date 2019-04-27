using System;
using System.Runtime.InteropServices;
using Eem.Thraxus.Bots.Utilities;
using Eem.Thraxus.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

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
				Marshall.RegisterNewMissile(new MissileInfo(((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).LauncherId, ((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).Owner), Entity.GetPosition());
			}
			catch (Exception e)
			{
				Marshall.ExceptionLog("MissileMaster", $"OnAddedToScene Exception! {e}");
			}
		}

		/// <inheritdoc />
		public override void OnRemovedFromScene()
		{
			base.OnRemovedFromScene();
			try
			{
				Marshall.RemoveOldMissile(new MissileInfo(((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).LauncherId, ((MyObjectBuilder_Missile)Entity.GetObjectBuilder()).Owner), Entity.GetPosition());
			}
			catch (Exception e)
			{
				Marshall.ExceptionLog("MissileMaster", $"OnRemovedFromScene Exception! {e}");
			}
		}
	}
}
