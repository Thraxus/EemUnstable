using System;
using Eem.Thraxus.Common;
using Eem.Thraxus.Common.SessionComps;
using Sandbox.ModAPI;
using VRage.Game;

namespace Eem.Thraxus.Bots.Utilities
{
	public static class ModDetection
	{
		public static bool DetectMod(ulong modId)
		{
			try
			{
				foreach (MyObjectBuilder_Checkpoint.ModItem mod in MyAPIGateway.Session.Mods)
					if (mod.PublishedFileId == modId) return true;
				return false;
			}
			catch (Exception e)
			{
				StaticLogger.WriteToLog("DetectMod",$"Exception! {e}", LogType.Exception);
				return false;
			}
		}
	}
}
