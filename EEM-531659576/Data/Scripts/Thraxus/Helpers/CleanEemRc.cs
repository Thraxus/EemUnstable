using System;
using System.Linq;
using Eem.Thraxus.Common.Settings;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Eem.Thraxus.Helpers
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_RemoteControl), false)]
    public class CleanEemRc : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
	        if (!Settings.DisableCleanup) return;
				try
            {
                //if(!MyAPIGateway.Multiplayer.IsServer) // only server-side/SP
                if (!Settings.IsServer)
                    return;

                IMyRemoteControl rc = (IMyRemoteControl)Entity;
                IMyCubeGrid grid = rc.CubeGrid;

                if (grid.Physics == null || !rc.IsWorking || !Settings.AllNpcFactions.Contains(rc.GetOwnerFactionTag()))
                {
                  //  if (Settings.CleanupDebug)
                        //EemCore.GeneralLog.WriteToLog("CleanEemRc", grid.DisplayName + " (" + grid.EntityId + " @ " + grid.WorldMatrix.Translation + ") is not valid; " + (grid.Physics == null ? "Phys=null" : "Phys OK") + "; " + (rc.IsWorking ? "RC OK" : "RC Not working!") + "; " + (!Factions.Settings.Settings.AllNpcFactions.Contains(rc.GetOwnerFactionTag()) ? "Owner faction tag is not in NPC list (" + rc.GetOwnerFactionTag() + ")" : "Owner Faction OK"));

                    return;
                }

                if (!rc.CustomData.Contains(Settings.CleanupRcTag))
                {
                    //if (Settings.CleanupDebug)
                       // EemCore.GeneralLog.WriteToLog("CleanEemRc", grid.DisplayName + " (" + grid.EntityId + " @ " + grid.WorldMatrix.Translation + ") RC does not contain the " + Settings.CleanupRcTag + "tag!");

                    return;
                }

                if (Settings.CleanupRcExtraTags.Length > 0)
                {
                    bool hasExtraTag = Settings.CleanupRcExtraTags.Any(tag => rc.CustomData.Contains(tag));

                    if (!hasExtraTag)
                    {
                       // if (Settings.CleanupDebug)
                          //  EemCore.GeneralLog.WriteToLog("CleanEemRc", grid.DisplayName + " (" + grid.EntityId + " @ " + grid.WorldMatrix.Translation + ") RC does not contain one of the extra tags!");

                        return;
                    }
                }

               // if (Settings.CleanupDebug)
                  //  EemCore.GeneralLog.WriteToLog("CleanEemRc", "Checking RC '" + rc.CustomName + "' from grid '" + grid.DisplayName + "' (" + grid.EntityId + ") for any nearby players...");

                int rangeSq = CleanEem.RangeSq;
                Vector3D gridCenter = grid.WorldAABB.Center;

                if (rangeSq <= 0)
                {
                   // if (Settings.CleanupDebug)
                       // EemCore.GeneralLog.WriteToLog("CleanEemRc", "- WARNING: Range not assigned yet, ignoring grid for now.");

                    return;
                }

                //check if any player is within range of the ship
                foreach (IMyPlayer player in CleanEem.Players)
                {
                    if (Vector3D.DistanceSquared(player.GetPosition(), gridCenter) <= rangeSq)
                    {
                     //   if (Settings.CleanupDebug)
                      //      EemCore.GeneralLog.WriteToLog("CleanEemRc", " - player '" + player.DisplayName + "' is within " + Math.Round(Math.Sqrt(rangeSq), 1) + "m of it, not removing.");

                        return;
                    }
                }

             //   if (Settings.CleanupDebug)
              //      EemCore.GeneralLog.WriteToLog("CleanEemRc", " - no player is within " + Math.Round(Math.Sqrt(rangeSq), 1) + "m of it, removing...");

              //  EemCore.GeneralLog.WriteToLog("CleanEemRc", "NPC ship '" + grid.DisplayName + "' (" + grid.EntityId + ") removed.");

                CleanEem.GetAttachedGrids(grid); // this gets all connected grids and places them in Exploration.grids (it clears it first)

                foreach (IMyCubeGrid g in CleanEem.Grids)
                {
	                g.Close(); // this only works server-side
                   // EemCore.GeneralLog.WriteToLog("CleanEemRc", "  - subgrid '" + g.DisplayName + "' (" + g.EntityId + ") removed.");
                }

                grid.Close(); // this only works server-side
            }
            catch (Exception e)
            {//  EemCore.GeneralLog.WriteToLog("CleanEemRc", $"Exception: {e}");
            }
        }
    }
}