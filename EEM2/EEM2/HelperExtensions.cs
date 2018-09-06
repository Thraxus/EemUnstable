using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Cheetah.AI
{
    public static class OwnershipTools
    {
        public static long PirateId => MyVisualScriptLogicProvider.GetPirateId();

        public static bool IsOwnedByPirates(this IMyTerminalBlock block)
        {
            return block.OwnerId == PirateId;
        }

        public static bool IsOwnedByNpc(this IMyTerminalBlock block, bool allowNobody = true, bool checkBuilder = false)
        {
            if (!checkBuilder)
            {
                if (block.IsOwnedByPirates()) return true;
                if (!allowNobody && block.IsOwnedByNobody()) return false;
                IMyPlayer owner = MyAPIGateway.Players.GetPlayerById(block.OwnerId);
                return owner?.IsBot ?? true;
            }
            else
            {
                if (!block.IsOwnedByNpc(allowNobody)) return false;
                long builderId = block.GetBuiltBy();
                if (!allowNobody && builderId == 0) return false;
                IMyPlayer owner = MyAPIGateway.Players.GetPlayerById(builderId);
                return owner?.IsBot ?? true;
            }
        }

        public static bool IsPirate(this IMyCubeGrid grid, bool strictCheck = false)
        {
            if (grid.BigOwners.Count == 0 || grid.BigOwners[0] == 0) return false;
            if (!strictCheck) return grid.BigOwners.Contains(PirateId);
            return grid.BigOwners.Count == 1 && grid.BigOwners[0] == PirateId;
        }

        public static bool IsNpc(this IMyCubeGrid grid)
        {
            if (grid.IsPirate()) return true;
            if (grid.BigOwners.Count == 0) return false;
            IMyPlayer owner = MyAPIGateway.Players.GetPlayerById(grid.BigOwners[0]);
            return owner?.IsBot ?? true;
        }

        public static bool IsOwnedByNobody(this IMyCubeGrid grid)
        {
            return grid.BigOwners.Count == 0 || grid.BigOwners[0] == 0;
        }

        public static bool IsOwnedByNobody(this IMyCubeBlock block)
        {
            return block.OwnerId == 0;
        }

        public static bool IsBuiltByNobody(this IMyCubeBlock block)
        {
            return block.GetBuiltBy() == 0;
        }

        public static bool IsPlayerBlock(this IMySlimBlock block, out IMyPlayer builder)
        {
            builder = null;
            long builtBy = block.GetBuiltBy();
            if (builtBy == 0) return false;
            builder = MyAPIGateway.Players.GetPlayerById(builtBy);
            return builder != null && !builder.IsBot;
        }

        public static bool IsPlayerBlock(this IMyCubeBlock block, out IMyPlayer owner)
        {
            owner = null;
            if (block.OwnerId != 0)
            {
                return MyAPIGateway.Players.IsValidPlayer(block.OwnerId, out owner);
            }
            else
            {
                long builtBy = block.GetBuiltBy();
                if (builtBy == 0) return false;
                owner = MyAPIGateway.Players.GetPlayerById(builtBy);
                return owner != null && !owner.IsBot;
            }
        }
    }

    public static class TerminalExtensions
    {
        public static IMyGridTerminalSystem GetTerminalSystem(this IMyCubeGrid grid)
        {
            return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
        }

        /// <summary>
        /// Allows GetBlocksOfType to work like a chainable function.
        /// <para />
        /// Enjoy allocating.
        /// </summary>
        public static List<T> GetBlocksOfType<T>(this IMyGridTerminalSystem term, Func<T, bool> collect = null) where T : class, Sandbox.ModAPI.Ingame.IMyTerminalBlock
        {
            List<T> termBlocks = new List<T>();
            term.GetBlocksOfType(termBlocks, collect);
            return termBlocks;
        }

        public static void Trigger(this IMyTimerBlock timer)
        {
            timer.GetActionWithName("TriggerNow").Apply(timer);
        }

        public static List<IMyInventory> GetInventories(this IMyEntity entity)
        {
            if (!entity.HasInventory) return new List<IMyInventory>();

            List<IMyInventory> inventories = new List<IMyInventory>();
            for (int i=0; i<entity.InventoryCount; i++)
            {
                inventories.Add(entity.GetInventory(i));
            }
            return inventories;
        }
    }

    public static class VectorExtensions
    {
        public static double DistanceTo(this Vector3D from, Vector3D to)
        {
            return (to - from).Length();
        }

        public static Vector3D LineTowards(this Vector3D from, Vector3D to, double length)
        {
            return from + (Vector3D.Normalize(to - from) * length);
        }

        public static Vector3D InverseVectorTo(this Vector3D from, Vector3D to, double length)
        {
            return from + (Vector3D.Normalize(from - to) * length);
        }
    }

    public static class GridExtenstions
    {
        /// <summary>
        /// Returns world speed cap, in m/s.
        /// </summary>
        public static float GetSpeedCap(this IMyShipController shipController)
        {
            if (shipController.CubeGrid.GridSizeEnum == MyCubeSize.Small) return MyDefinitionManager.Static.EnvironmentDefinition.SmallShipMaxSpeed;
            if (shipController.CubeGrid.GridSizeEnum == MyCubeSize.Large) return MyDefinitionManager.Static.EnvironmentDefinition.LargeShipMaxSpeed;
            return 100;
        }

        /// <summary>
        /// Returns world speed cap ratio to default cap of 100 m/s.
        /// </summary>
        public static float GetSpeedCapRatioToDefault(this IMyShipController shipController)
        {
            return shipController.GetSpeedCap() / 100;
        }

        public static IMyPlayer FindControllingPlayer(this IMyCubeGrid grid, bool write = true)
        {
            try
            {
                IMyPlayer player;
                IMyGridTerminalSystem term = grid.GetTerminalSystem();
                List<IMyShipController> shipControllers = term.GetBlocksOfType<IMyShipController>(x => x.IsUnderControl);
                if (shipControllers.Count == 0)
                {
                    shipControllers = term.GetBlocksOfType<IMyShipController>(x => x.GetBuiltBy() != 0);
                    if (shipControllers.Count > 0)
                    {
                        IMyShipController mainController = shipControllers.FirstOrDefault(x => x.IsMainCockpit()) ?? shipControllers.First();
                        long id = mainController.GetBuiltBy();
                        player = MyAPIGateway.Players.GetPlayerById(id);
                        if (write && player != null) grid.DebugWrite("Grid.FindControllingPlayer", $"Found cockpit built by player {player.DisplayName}.");
                        return player;
                    }
                    if (write) grid.DebugWrite("Grid.FindControllingPlayer", "No builder player was found.");
                    return null;
                }

                player = MyAPIGateway.Players.GetPlayerById(shipControllers.First().ControllerInfo.ControllingIdentityId);
                if (write && player != null) grid.DebugWrite("Grid.FindControllingPlayer", $"Found player in control: {player.DisplayName}");
                return player;
            }
            catch (Exception scrap)
            {
                grid.LogError("Grid.FindControllingPlayer", scrap);
                return null;
            }
        }

        public static bool HasCockpit(this IMyCubeGrid grid)
        {
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            grid.GetBlocks(blocks, x => false);
            return blocks.Count > 0;
        }

        public static bool HasRemote(this IMyCubeGrid grid)
        {
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            grid.GetBlocks(blocks, x => false);
            return blocks.Count > 0;
        }

        public static bool HasShipController(this IMyCubeGrid grid)
        {
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            grid.GetBlocks(blocks, x => false);
            return blocks.Count > 0;
        }

        public static IMyFaction GetOwnerFaction(this IMyCubeGrid grid, bool recalculateOwners = false)
        {
            try
            {
                if (recalculateOwners)
                    (grid as MyCubeGrid)?.RecalculateOwners();

                IMyFaction factionFromBigowners = null;
                IMyFaction faction;
                if (grid.BigOwners.Count > 0 && grid.BigOwners[0] != 0)
                {
                    long ownerId = grid.BigOwners[0];
                    factionFromBigowners = GeneralExtensions.FindOwnerFactionById(ownerId);
                }
                else
                {
                    grid.LogError("Grid.GetOwnerFaction", new Exception("Cannot get owner faction via BigOwners.", new Exception("BigOwners is empty.")));
                }

                IMyGridTerminalSystem term = grid.GetTerminalSystem();
                List<IMyTerminalBlock> allTermBlocks = new List<IMyTerminalBlock>();
                term.GetBlocks(allTermBlocks);

                if (allTermBlocks.Empty())
                {
                    grid.DebugWrite("Grid.GetOwnerFaction", "Terminal system is empty!");
                    return null;
                }

                IGrouping<string, IMyTerminalBlock> biggestOwnerGroup = allTermBlocks.GroupBy(x => x.GetOwnerFactionTag()).OrderByDescending(gp => gp.Count()).FirstOrDefault();
                if (biggestOwnerGroup != null)
                {
                    string factionTag = biggestOwnerGroup.Key;
                    faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(factionTag);
                    if (faction != null)
                        grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {factionTag} via terminal system");
                    return faction ?? factionFromBigowners;
                }
                else
                {
                    grid.DebugWrite("Grid.GetOwnerFaction", "CANNOT GET FACTION TAGS FROM TERMINALSYSTEM!");
                    List<IMyShipController> controllers = grid.GetBlocks<IMyShipController>();
                    if (controllers.Any())
                    {
                        List<IMyShipController> mainControllers;
                        if (controllers.Any(x => x.IsMainCockpit(), out mainControllers))
                        {
                            faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(mainControllers[0].GetOwnerFactionTag());
                            if (faction != null)
                            {
                                grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {faction.Tag} via main cockpit");
                                return faction;
                            }
                        } // Controls falls down if faction was not found by main cockpit

                        faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(controllers[0].GetOwnerFactionTag());
                        if (faction != null)
                        {
                            grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {faction.Tag} via cockpit");
                            return faction;
                        }
                        else
                        {
                            grid.DebugWrite("Grid.GetOwnerFaction", "Unable to owner faction via cockpit!");
                            faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(allTermBlocks.First().GetOwnerFactionTag());
                            if (faction != null)
                            {
                                grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {faction.Tag} via first terminal block");
                                return faction;
                            }
                            else
                            {
                                grid.DebugWrite("Grid.GetOwnerFaction", "Unable to owner faction via first terminal block!");
                                return factionFromBigowners;
                            }
                        }
                    }
                    else
                    {
                        faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(allTermBlocks.First().GetOwnerFactionTag());
                        if (faction != null)
                        {
                            grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {faction.Tag} via first terminal block");
                            return faction;
                        }

                        grid.DebugWrite("Grid.GetOwnerFaction", "Unable to owner faction via first terminal block!");
                        return factionFromBigowners;
                    }
                }
            }
            catch (Exception scrap)
            {
                grid.LogError("Faction.GetOwnerFaction", scrap);
                return null;
            }
        }

        public static List<T> GetBlocks<T>(this IMyCubeGrid grid, Func<T, bool> selector = null) where T : class, IMyEntity
        {
            List<IMySlimBlock> blocksListSlim = new List<IMySlimBlock>();
            List<T> blocksList = new List<T>();
            grid.GetBlocks(blocksListSlim, x => false);
            foreach (IMySlimBlock blockSlim in blocksListSlim)
            {

                // ReSharper disable SuspiciousTypeConversion.Global
                T block = blockSlim as T;
                // ReSharper restore SuspiciousTypeConversion.Global

                // Not the most efficient method, but GetBlocks only allows IMySlimBlock selector
                if (selector == null || selector(block))
                    blocksList.Add(block);
            }
            return blocksList;
        }

        public static List<IMySlimBlock> GetBlocks(this IMyCubeGrid grid, Func<IMySlimBlock, bool> selector = null, int blockLimit = 0)
        {
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            int i = 0;
            Func<IMySlimBlock, bool> collector = selector;
            if (blockLimit > 0)
            {
                collector = (block) =>
                {
                    if (i >= blockLimit) return false;
                    i++;
                    if (selector != null) return selector(block);
                    return true;
                };
            }

            if (collector == null)
                grid.GetBlocks(blocks);
            else
                grid.GetBlocks(blocks, collector);
            return blocks;
        }

        /// <summary>
        /// Remember, this is only for server-side.
        /// </summary>
        public static void ChangeOwnershipSmart(this IMyCubeGrid grid, long newOwnerId, MyOwnershipShareModeEnum shareMode)
        {
            try
            {
                List<IMyCubeGrid> subgrids = grid.GetAllSubgrids();
                grid.ChangeGridOwnership(newOwnerId, shareMode);
                foreach (IMyCubeGrid subgrid in subgrids)
                {
                    try
                    {
                        subgrid.ChangeGridOwnership(newOwnerId, shareMode);
                    }
                    catch (Exception scrap)
                    {
                        grid.LogError("ChangeOwnershipSmart.ChangeSubgridOwnership", scrap);
                    }
                }
            }
            catch (Exception scrap)
            {
                grid.LogError("ChangeOwnershipSmart", scrap);
            }
        }

        public static void DeleteSmart(this IMyCubeGrid grid)
        {
            if (!MyAPIGateway.Session.IsServer) return;
            List<IMyCubeGrid> subgrids = grid.GetAllSubgrids();
            foreach (IMyCubeGrid subgrid in subgrids)
                subgrid.Close();
            grid.Close();
        }

        public static List<IMyCubeGrid> GetAllSubgrids(this IMyCubeGrid grid)
        {
            try
            {
                return MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Logical);
            }
            catch (Exception scrap)
            {
                grid.LogError("GetAllSubgrids", scrap);
                return new List<IMyCubeGrid>();
            }
        }
    }

    public static class FactionsExtensions
    {
        public static void DeclareWar(this IMyFaction ourFaction, IMyFaction hostileFaction, bool print = false)
        {
            MyAPIGateway.Session.Factions.DeclareWar(ourFaction.FactionId, hostileFaction.FactionId);
            //if (print) AISessionCore.DebugWrite($"{ourFaction.Tag}", $"Declared war on {hostileFaction.Tag}", false);
        }

        public static void ProposePeace(this IMyFaction ourFaction, IMyFaction hostileFaction, bool print = false)
        {
            MyAPIGateway.Session.Factions.SendPeaceRequest(ourFaction.FactionId, hostileFaction.FactionId);
           //if (print) AISessionCore.DebugWrite($"{ourFaction.Tag}", $"Proposed peace to {hostileFaction.Tag}", false);
        }

        public static void AcceptPeace(this IMyFaction ourFaction, IMyFaction hostileFaction, bool print = false)
        {
            MyAPIGateway.Session.Factions.AcceptPeace(hostileFaction.FactionId, ourFaction.FactionId);
            MyAPIGateway.Session.Factions.AcceptPeace(ourFaction.FactionId, hostileFaction.FactionId);
            //if (print) AISessionCore.DebugWrite($"{ourFaction.Tag}", $"Accepted peace from {hostileFaction.Tag}", false);
        }

        public static void DeclinePeace(this IMyFaction ourFaction, IMyFaction hostileFaction)
        {
            MyAPIGateway.Session.Factions.CancelPeaceRequest(ourFaction.FactionId, hostileFaction.FactionId);
        }

        public static bool IsHostileTo(this IMyFaction ourFaction, IMyFaction hostileFaction)
        {
            return MyAPIGateway.Session.Factions.AreFactionsEnemies(ourFaction.FactionId, hostileFaction.FactionId);
        }

        public static bool HasPeaceRequestTo(this IMyFaction ourFaction, IMyFaction hostileFaction)
        {
            return MyAPIGateway.Session.Factions.IsPeaceRequestStateSent(ourFaction.FactionId, hostileFaction.FactionId);
        }

        public static bool HasPeaceRequestFrom(this IMyFaction ourFaction, IMyFaction hostileFaction)
        {
            return MyAPIGateway.Session.Factions.IsPeaceRequestStatePending(ourFaction.FactionId, hostileFaction.FactionId);
        }

        public static bool IsPeacefulTo(this IMyFaction ourFaction, IMyFaction faction, bool considerPeaceRequests = false)
        {
            if (!considerPeaceRequests)
                return MyAPIGateway.Session.Factions.GetRelationBetweenFactions
                    (ourFaction.FactionId, faction.FactionId) != MyRelationsBetweenFactions.Enemies;
            else
                return ourFaction.IsPeacefulTo(faction) || ourFaction.HasPeaceRequestTo(faction);
        }

        public static bool IsLawful(this IMyFaction ownFaction)
        {
            return Diplomacy.LawfulFactionsTags.Contains(ownFaction.Tag);
        }

        public static void Accept(this IMyFaction faction, IMyPlayer player)
        {
            MyAPIGateway.Session.Factions.AcceptJoin(faction.FactionId, player.IdentityId);
        }

        public static void Kick(this IMyFaction faction, IMyPlayer member)
        {
            MyAPIGateway.Session.Factions.KickMember(faction.FactionId, member.IdentityId);
        }
    }

    public static class DamageHelper
    {
        /// <summary>
        /// Determines if damage was done by player.
        /// <para/>
        /// If it's necessary to determine who did the damage, use overload.
        /// </summary>
        /// 

        //public static bool IsDoneByPlayer(this MyDamageInformation damage)
        //{
        //    IMyPlayer didAPlayerDoIt;
        //    return damage.IsDoneByPlayer(out didAPlayerDoIt);
        //}

        private static bool IsDamagedByPlayerWarhead(IMyWarhead warhead, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                if (warhead.OwnerId == 0)
                {
                    damager = MyAPIGateway.Players.GetPlayerById(((MyCubeBlock) warhead).BuiltBy);
                    AISessionCore.DebugWrite("Damage.IsDoneByPlayer", "Attempting to find damager by neutral warhead.", false);
                    return damager != null;
                }
                damager = MyAPIGateway.Players.GetPlayerById(warhead.OwnerId);
                AISessionCore.DebugWrite("Damage.IsDoneByPlayer", "Attempting to find damager by warhead owner.", false);
                return damager != null;
            }
            catch (Exception scrap)
            {
                AISessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check for neutral warheads crashed", scrap));
                return false;
            }
        }

        private static bool IsDamagedByPlayer(IMyGunBaseUser gun, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                damager = MyAPIGateway.Players.GetPlayerById(gun.OwnerId);
                //AISessionCore.DebugWrite($"GunDamage.IsDamagedByPlayer", $"Getting player from gun. ID: {Gun.OwnerId}, player: {(Damager != null ? Damager.DisplayName : "null")}", false);
                return !damager.IsBot; //!damager?.IsBot ?? false;
            }
            catch (Exception scrap)
            {
                AISessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check gun owner crashed", scrap));
                return false;
            }
        }

        private static bool IsDamagedByPlayer(IMyEngineerToolBase tool, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                damager = MyAPIGateway.Players.GetPlayerById(tool.OwnerIdentityId);
                //AISessionCore.DebugWrite($"ToolDamage.IsDamagedByPlayer", $"Getting player from tool. ID: {Tool.OwnerId}, IdentityID: {Tool.OwnerIdentityId}, player: {(Damager != null ? Damager.DisplayName : "null")}", false);
                return damager != null && !damager.IsBot;
            }
            catch (Exception scrap)
            {
                AISessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check gun owner crashed", scrap));
                return false;
            }
        }

        private static bool IsDamagedByPlayerInNeutralGrid(IMyCubeGrid grid, out IMyPlayer damager)
        {
            //AISessionCore.DebugWrite("", "", true);
            AISessionCore.DebugWrite("IsDamagedByPlayerInNeutralGrid: ", "Entering Method.", true);
            damager = null;
            try
            {
                damager = grid.FindControllingPlayer();
                if (damager != null) return !damager.IsBot;

                try
                {
                    List<MyCubeBlock> cubeBlocks = grid.GetBlocks<MyCubeBlock>(x => x.BuiltBy != 0);
                    if (cubeBlocks.Count != 0)
                    {
                        long thatCunningGrieferId = cubeBlocks[0].BuiltBy;
                        damager = MyAPIGateway.Players.GetPlayerById(thatCunningGrieferId);
                        return damager != null;
                    }
                    else
                    {
                        List<IMySlimBlock> slimBlocks = grid.GetBlocks(selector: x => x.GetBuiltBy() != 0, blockLimit: 50);
                        if (slimBlocks.Count == 0) return false; // We give up on this one
                        else
                        {
                            try
                            {
                                damager = MyAPIGateway.Players.GetPlayerById(slimBlocks.First().GetBuiltBy());
                                if (damager != null)
                                {
                                    grid.DebugWrite("Damage.IsDoneByPlayer.FindBuilderBySlimBlocks", $"Found damager player from slim block. Damager is {damager.DisplayName}");
                                }
                                return damager != null;
                            }
                            catch (Exception scrap)
                            {
                                AISessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check grid via SlimBlocks BuiltBy crashed.", scrap));
                                return false;
                            }
                        }
                    }
                }
                catch (Exception scrap)
                {
                    AISessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check grid via BuiltBy crashed.", scrap));
                    return false;
                }
            }
            catch (Exception scrap)
            {
                AISessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check neutral grid crashed", scrap));
                return false;
            }
        }

        private static bool IsDamagedByPlayerGrid(IMyCubeGrid grid, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                long biggestOwner = grid.BigOwners.FirstOrDefault();
                if (biggestOwner != 0)
                {
                    damager = MyAPIGateway.Players.GetPlayerById(biggestOwner);
                    return !damager?.IsBot ?? false;
                }

                return false;
            }
            catch (Exception scrap)
            {
                AISessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check grid via BigOwners crashed", scrap));
                return false;
            }
        }


        /// <summary>
        /// Determines if damage was done by player.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="damager">Provides player who did the damage. Null if damager object is not a player.</param>
        public static bool IsDoneByPlayer(this MyDamageInformation damage, out IMyPlayer damager)
        {
            //AISessionCore.DebugWrite("IsDoneByPlayer: ", $@"Entering Method with damage var: {damage.AttackerId}", true);
            damager = null;
            if (damage.AttackerId == 0) 
            {
                //AISessionCore.DebugWrite("IsDoneByPlayer: ", $@"Damage had no assigned AttackerId: {damage.AttackerId}", true);
                return false;
            }
            try
            {
                IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
                //AISessionCore.DebugWrite("Damage.IsDoneByPlayer", $"Received damage: '{damage.Type}' from '{attackerEntity.GetType()}'", false);
                //if (attackerEntity == null)
                //{
                //    AISessionCore.DebugWrite("Damage.IsDoneByPlayer", "Attacker entity was not found.", false);
                //    return false;
                //}

                if (attackerEntity is IMyMeteor) return false;
                if (attackerEntity is IMyWarhead) return IsDamagedByPlayerWarhead(attackerEntity as IMyWarhead, out damager);
                if (attackerEntity is IMyEngineerToolBase) return IsDamagedByPlayer(attackerEntity as IMyEngineerToolBase, out damager);
                if (attackerEntity is IMyGunBaseUser) return IsDamagedByPlayer(attackerEntity as IMyGunBaseUser, out damager);


                attackerEntity = attackerEntity.GetTopMostParent();

                if (attackerEntity == null)
                {
                    //AISessionCore.DebugWrite("Damage.IsDoneByPlayer", "Cannot acquire the attacker's topmost entity", false);
                    return false;
                }

                if (attackerEntity is IMyCubeGrid)
                {
                    IMyCubeGrid Grid = attackerEntity as IMyCubeGrid;
                    if (Grid.IsPirate()) return false;
                    if (Grid.IsOwnedByNobody()) return IsDamagedByPlayerInNeutralGrid(Grid, out damager);

                    return IsDamagedByPlayerGrid(Grid, out damager);
                }

                return false;
            }
            catch (Exception scrap)
            {
                //AISessionCore.DebugWrite("IsDoneByPlayerException: ", $@"-Exception noted: {scrap} -Base: {scrap.GetBaseException()} -Extended: {scrap.Source}", true);
                AISessionCore.LogError("Damage.IsDoneByPlayer", new Exception("General crash.", scrap));
                return false;
            }
        }

        public static bool IsMeteor(this MyDamageInformation damage)
        {
            IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
            return attackerEntity is IMyMeteor;
        }

        public static bool IsThruster(this MyDamageInformation damage)
        {
            IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
            return attackerEntity is IMyThrust;
        }

        //public static bool IsGrid(this MyDamageInformation damage, out IMyCubeGrid grid)
        //{
        //    grid = MyAPIGateway.Entities.GetEntityById(damage.AttackerId).GetTopMostParent() as IMyCubeGrid;
        //    return grid != null;
        //}

        //public static bool IsGrid(this MyDamageInformation damage)
        //{
        //    return MyAPIGateway.Entities.GetEntityById(damage.AttackerId).GetTopMostParent() is IMyCubeGrid;
        //}
    }

    public static class InventoryHelpers
    {
        public static MyDefinitionId GetBlueprint(this IMyInventoryItem item)
        {
            return new MyDefinitionId(item.Content.TypeId, item.Content.SubtypeId);
        }

        public static bool IsOfType(this MyDefinitionId id, string type)
        {
            return id.TypeId.ToString() == type || id.TypeId.ToString() == "MyObjectBuilder_" + type;
        }

        public static bool IsOfType(this MyObjectBuilder_Base id, string type)
        {
            return id.TypeId.ToString() == type || id.TypeId.ToString() == "MyObjectBuilder_" + type;
        }

        public static bool IsOfType(this IMyInventoryItem item, string type)
        {
            return item.Content.IsOfType(type);
        }
    }

    public class EntityByDistanceSorter : IComparer<IMyEntity>, IComparer<IMySlimBlock>, IComparer<Sandbox.ModAPI.Ingame.MyDetectedEntityInfo>
    {
        public Vector3D Position { get; set; }
        public EntityByDistanceSorter(Vector3D position)
        {
            Position = position;
        }

        public int Compare(IMyEntity x, IMyEntity y)
        {
            double distanceX = Vector3D.DistanceSquared(Position, x.GetPosition());
            double distanceY = Vector3D.DistanceSquared(Position, y.GetPosition());

            if (distanceX < distanceY) return -1;
            return distanceX > distanceY ? 1 : 0;
        }

        public int Compare(Sandbox.ModAPI.Ingame.MyDetectedEntityInfo x, Sandbox.ModAPI.Ingame.MyDetectedEntityInfo y)
        {
            double distanceX = Vector3D.DistanceSquared(Position, x.Position);
            double distanceY = Vector3D.DistanceSquared(Position, y.Position);

            if (distanceX < distanceY) return -1;
            return distanceX > distanceY ? 1 : 0;
        }

        public int Compare(IMySlimBlock x, IMySlimBlock y)
        {
            double distanceX = Vector3D.DistanceSquared(Position, x.CubeGrid.GridIntegerToWorld(x.Position));
            double distanceY = Vector3D.DistanceSquared(Position, y.CubeGrid.GridIntegerToWorld(y.Position));

            if (distanceX < distanceY) return -1;
            return distanceX > distanceY ? 1 : 0;
        }
    }

    /// <summary>
    /// Provides a set of methods to fix some of the LINQ idiocy.
    /// <para/>
    /// Enjoy your allocations.
    /// </summary>
    public static class GenericHelpers
    {
        //public static List<T> Except<T>(this List<T> source, Func<T, bool> sorter)
        //{
        //    return source.Where(x => !sorter(x)).ToList();
        //}

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            HashSet<T> hashset = new HashSet<T>();
            foreach (T item in source)
                hashset.Add(item);
            return hashset;
        }

        /// <summary>
        /// Returns a list with one item excluded.
        /// </summary>
        //public static List<T> Except<T>(this List<T> source, T exclude)
        //{
        //    return source.Where(x => !x.Equals(exclude)).ToList();
        //}

        //public static bool Any<T>(this IEnumerable<T> source, Func<T, bool> sorter, out IEnumerable<T> any)
        //{
        //    any = source.Where(sorter);
        //    return any.Any();
        //}

        /// <summary>
        /// Determines if the sequence has no elements matching a given predicate.
        /// <para />
        /// Basically, it's an inverted Any().
        /// </summary>
        //public static bool None<T>(this IEnumerable<T> source, Func<T, bool> sorter)
        //{
        //    return !source.Any(sorter);
        //}

        //public static IEnumerable<T> Unfitting<T>(this IEnumerable<T> source, Func<T, bool> sorter)
        //{
        //    return source.Where(x => sorter(x) == false);
        //}

        //public static List<T> Unfitting<T>(this List<T> source, Func<T, bool> sorter)
        //{
        //    return source.Where(x => sorter(x) == false).ToList();
        //}

        public static bool Any<T>(this List<T> source, Func<T, bool> sorter, out List<T> any)
        {
            any = source.Where(sorter).ToList();
            return any.Count > 0;
        }

        public static bool Empty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }
    }

    public static class GeneralExtensions
    {
        public static bool IsNullEmptyOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static bool IsValid(this Sandbox.ModAPI.Ingame.MyDetectedEntityInfo entityInfo)
        {
            return !entityInfo.IsEmpty();
        }

        public static bool IsHostile(this Sandbox.ModAPI.Ingame.MyDetectedEntityInfo entityInfo)
        {
            return entityInfo.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies;
        }

        public static bool IsNonFriendly(this Sandbox.ModAPI.Ingame.MyDetectedEntityInfo entityInfo)
        {
            return entityInfo.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies || entityInfo.Relationship == MyRelationsBetweenPlayerAndBlock.Neutral;
        }

        public static IMyEntity GetEntity(this Sandbox.ModAPI.Ingame.MyDetectedEntityInfo entityInfo)
        {
            return MyAPIGateway.Entities.GetEntityById(entityInfo.EntityId);
        }

        /// <summary>
        /// Retrieves entity mass, in tonnes.
        /// </summary>
        public static float GetMassT(this Sandbox.ModAPI.Ingame.MyDetectedEntityInfo entityInfo)
        {
            return entityInfo.GetEntity().Physics.Mass / 1000;
        }

        public static IMyCubeGrid GetGrid(this Sandbox.ModAPI.Ingame.MyDetectedEntityInfo entityInfo)
        {
            if (!entityInfo.IsGrid()) return null;
            return MyAPIGateway.Entities.GetEntityById(entityInfo.EntityId) as IMyCubeGrid;
        }

        public static bool IsGrid(this Sandbox.ModAPI.Ingame.MyDetectedEntityInfo entityInfo)
        {
            return entityInfo.Type == Sandbox.ModAPI.Ingame.MyDetectedEntityType.SmallGrid || entityInfo.Type == Sandbox.ModAPI.Ingame.MyDetectedEntityType.LargeGrid;
        }

        public static void EnsureName(this IMyEntity entity, string desiredName = null)
        {
            if (entity == null) return;
            if (desiredName == null) desiredName = $"Entity_{entity.EntityId}";
            entity.Name = desiredName;
            MyAPIGateway.Entities.SetEntityName(entity, false);
        }

        public static IMyFaction GetFaction(this IMyPlayer player)
        {
            return MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
        }

        public static bool IsMainCockpit(this IMyShipController shipController)
        {
            return ((MyShipController) shipController).IsMainCockpit;
        }

        /// <summary>
        /// Returns block's builder id.
        /// </summary>
        public static long GetBuiltBy(this IMyCubeBlock block)
        {
            return ((MyCubeBlock) block).BuiltBy;
        }

        /// <summary>
        /// Returns block's builder id. WARNING: Heavy!
        /// </summary>
        public static long GetBuiltBy(this IMySlimBlock block)
        {
            MyObjectBuilder_CubeBlock builder = block.GetObjectBuilder();
            return builder.BuiltBy;
        }

        public static bool IsNpc(this IMyFaction faction)
        {
            try
            {
                IMyPlayer owner = MyAPIGateway.Players.GetPlayerById(faction.FounderId);
                if (owner != null) return owner.IsBot;
                else
                {
                    if (!faction.Members.Any()) return true;
                    foreach (KeyValuePair<long, MyFactionMember> membership in faction.Members)
                    {
                        IMyPlayer member = MyAPIGateway.Players.GetPlayerById(membership.Value.PlayerId);
                        if (member == null) continue;
                        if (!member.IsBot) return false;
                    }
                    return true;
                }
            }
            catch (Exception scrap)
            {
                AISessionCore.LogError("Faction.IsNPC", scrap);
                return false;
            }
        }

        public static bool IsPlayerFaction(this IMyFaction faction)
        {
            return !faction.IsNpc();
        }

        /*public static bool IsPeacefulNPC(this IMyFaction Faction)
        {
            try
            {
                if (!Faction.IsNPC()) return false;
                return Diplomacy.LawfulFactionsTags.Contains(Faction.Tag);
            }
            catch (Exception Scrap)
            {
                AISessionCore.LogError("Faction.IsPeacefulNPC", Scrap);
                return false;
            }
        }*/

        public static float GetHealth(this IMySlimBlock block)
        {
            return Math.Min(block.DamageRatio, block.BuildLevelRatio);
        }

        public static IMyFaction FindOwnerFactionById(long identityId)
        {
            Dictionary<long, IMyFaction>.ValueCollection factions = MyAPIGateway.Session.Factions.Factions.Values;
            foreach (IMyFaction faction in factions)
            {
                if (faction.IsMember(identityId)) return faction;
            }
            return null;
        }

        public static string Line(this string str, int lineNumber, string newlineStyle = "\r\n")
        {
            return str.Split(newlineStyle.ToCharArray())[lineNumber];
        }

        public static IMyPlayer GetPlayerById(this IMyPlayerCollection players, long playerId)
        {
            List<IMyPlayer> playersList = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(playersList, x => x.IdentityId == playerId);
            return playersList.FirstOrDefault();
        }

        public static bool IsValidPlayer(this IMyPlayerCollection players, long playerId, out IMyPlayer player, bool checkNonBot = true)
        {
            player = MyAPIGateway.Players.GetPlayerById(playerId);
            if (player == null) return false;
            return !checkNonBot || !player.IsBot;
        }

        public static bool IsValidPlayer(this IMyPlayerCollection players, long playerId, bool checkNonBot = true)
        {
            IMyPlayer player;
            return IsValidPlayer(players, playerId, out player);
        }
    }

    public static class NumberExtensions
    {
        public static int Squared(this int num)
        {
            return (int)Math.Pow(num, 2);
        }

        public static int Cubed(this int num)
        {
            return (int)Math.Pow(num, 3);
        }

        public static float Squared(this float num)
        {
            return (float)Math.Pow(num, 2);
        }

        public static float Cubed(this float num)
        {
            return (float)Math.Pow(num, 3);
        }

        public static float Root(this float num)
        {
            return (float)Math.Sqrt(num);
        }

        public static float Cuberoot(this float num) => (float)Math.Pow(num, (float) 1/3);

        public static double Squared(this double num)
        {
            return Math.Pow(num, 2);
        }

        public static double Cubed(this double num)
        {
            return Math.Pow(num, 3);
        }

        public static double Root(this double num)
        {
            return Math.Sqrt(num);
        }

        public static double Cuberoot(this double num)
        {
            return Math.Pow(num, (double) 1/3);
        }
    }

    public static class DebugHelper
    {
        private static readonly List<int> AlreadyPostedMessages = new List<int>();

        public static void Print(string source, string message, bool antiSpam = true)
        {
            string combined = source + ": " + message;
            int hash = combined.GetHashCode();

            if (!AlreadyPostedMessages.Contains(hash))
            {
                AlreadyPostedMessages.Add(hash);
                MyAPIGateway.Utilities.ShowMessage(source, message);
                VRage.Utils.MyLog.Default.WriteLine(source + $": Debug message: {message}");
                VRage.Utils.MyLog.Default.Flush();
            }
        }

        public static void DebugWrite(this IMyCubeGrid grid, string source, string message, bool antiSpam = true, bool forceWrite = false)
        {
            if (AISessionCore.Debug || forceWrite) Print(grid.DisplayName, $"Debug message from '{source}': {message}");
        }

        public static void LogError(this IMyCubeGrid grid, string source, Exception scrap, bool antiSpam = true, bool forceWrite = false)
        {
            if (!AISessionCore.Debug && !forceWrite) return;
            string displayName = "Unknown Grid";
            try
            {
                displayName = grid.DisplayName;
            }
            finally
            {
                Print(displayName, $"Fatal error in '{source}': {scrap.Message}. {scrap.InnerException?.Message ?? "No additional info was given by the game :("}");
            }
        }
    }
}