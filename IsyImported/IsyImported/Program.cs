using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
	partial class Program : MyGridProgram
	{
		// Isy's Inventory Manager
		// ===================
		// Version: 1.7.0
		// Date: 2018-03-22

		//  =======================================================================================
		//                                                                            --- Configuration ---
		//  =======================================================================================

		// --- Sorting ---
		// =======================================================================================

		// Enable sorting?
		bool enableSorting = true;

		// Define the keyword a cargo container has to contain in order to be recognized as a container of the given type.
		const string oreContainer = "Ores";
		const string ingotContainer = "Ingots";
		const string componentContainer = "Components";
		const string toolContainer = "Tools";
		const string ammoContainer = "Ammo";
		const string bottleContainer = "Bottles";

		// Keyword an inventory has to contain to be skipped by the sorting (= no items will be taken out)
		const string lockedContainer = "Locked";

		// Show a fill level in the container's name?
		bool showFillLevel = true;

		// Auto assign new containers if a type is full or not present?
		bool autoAssignContainers = true;

		// Auto assign tool, ammo and bottle containers as one?
		bool toolsAmmoBottlesInOne = true;

		// Fill bottles before storing them in the bottle container?
		bool fillBottles = true;


		// --- Internal item sorting ---
		// =======================================================================================

		// Sort the items inside all containers?
		bool enableInternalSorting = true;

		// Internal sorting pattern. Always combine one of each category, e.g.: 'Ad' for descending item amount (from highest to lowest)
		// 1. Quantifier:
		// A = amount
		// N = name
		// T = type (alphabetical)
		// X = type (number of items)

		// 2. Direction:
		// a = ascending
		// d = descending

		string sortingPattern = "Na";

		// This can also be set per inventory. Just use '(sort:PATTERN)' in the block's name
		// Example: Small Cargo Container 3 (sort:Ad)


		// --- Special Loadout Containers ---
		// =======================================================================================

		// Keyword an inventory has to contain to be filled with a special loadout (see in it's custom data after you renamed it!)
		// Special containers will be filled with your wanted amount of items and never be drained by the auto sorting!
		const string specialContainer = "Special";

		// Take out excess items if a special container contains more items than the wanted ones?
		bool specialContainerTakeOutExcess = true;

		// Produce the needed materials in an assembler if you don't have enough in stock?
		bool produceMissingItems = false;


		// --- Autocrafting ---
		// =======================================================================================

		// Enable autocrafting of components? A LCD named "LCD Autocrafting" is required where you can set the wanted amount!
		// This has multi LCD support. Just name all the LCDs the same and append numbers like: "LCD Autocrafting 1", "LCD Autocrafting 2", ..
		bool enableAutocrafting = true;
		string autocraftingLcd = "LCD Autocrafting";

		// By default, items on the LCD are sorted by amount. If you want alphabetical order, enable it here
		bool sortAlphabetically = false;

		// Disassemble all items above the wanted amount? Be sure to set your wanted amounts BEFORE you activate this!
		// Also, manual crafting WILL be canceled and the crafted items will be disassembled to keep the wanted amounts!
		bool disassembleExcess = false;

		// Sort the assembler queue based on the most needed components?
		bool sortAssemblerQueue = false;


		// --- Ore Balancing ---
		// =======================================================================================

		// Balance the ores in refineries and arc furnaces? (Note: conveyors are turned off to stop them from pulling more)
		bool enableOreBalancing = false;

		// Enable arc priority?
		// Iron, nickel and cobalt will only be processed in arc furnaces when there are other ores, only refineries can process.
		// If the refineries are done, the ores of the arc furnaces are distributed back to them as well.
		bool enableArcPriority = false;

		// Process iron, nickel and cobalt in only in arc furnaces and not in refineries? (this overrides arc priority mode!)
		bool enableArcSpecialization = false;

		// Process stone in refineries?
		bool enableStoneProcessing = true;

		// Sort the refinery and arc queue based on the most needed ingots?
		bool sortRefiningQueue = false;


		// --- Assembler Cleanup ---
		// =======================================================================================

		// This cleans up assemblers, if their inventory is too full and puts the contents back into a cargo container.
		bool enableAssemblerCleanup = true;

		// Set fill level in percent when the assembler should be cleaned up
		double assemblerFillLevelPercentage = 10;


		// --- Oxygen Generator Ice Balancing ---
		// =======================================================================================

		// Enable balancing of ice in oxygen generators?
		bool enableIceBalancing = true;

		// Ice fill level in percent in order to be able to fill bottles? (default: 95)
		double iceFillLevelPercentage = 50;


		// --- Reactor Uranium Balancing ---
		// =======================================================================================

		// Enable balancing of uranium in reactors? (Note: conveyors of reactors are turned off to stop them from pulling more)
		bool enableUraniumBalancing = true;

		// Amount of uranium in each reactor? (default: 100 for large grid reactors, 25 for small grid reactors)
		double uraniumAmountLargeGrid = 100;
		double uraniumAmountSmallGrid = 50;


		// --- LCD panels ---
		// =======================================================================================

		// List of LCD Panels to display various information (single Names or group names).
		// Example: string[] outputLcds = { "LCD 1", "LCD 2" } or string[] outputLcds = { "LCD Group" }
		// If you don't want to use this feature, leave empty brackets: string[] outputLcds = {};
		string[] outputLcds = { "LCD Sorting" };
		bool showContainerStats = true;
		bool showManagedBlocks = true;
		bool showLastAction = true;


		//  =======================================================================================
		//                                                                      --- End of Configuration ---
		//                                                        Don't change anything beyond this point!
		//  =======================================================================================

		// Removed whitespaces at the front to save space

		// Information String
		string informationString = "";
		string lastAction = "";
		string workingIndicator = "/";
		int workingCounter = 0;

		// Generic lists
		List<IMyTerminalBlock> allInventories = new List<IMyTerminalBlock>();
		List<IMyTerminalBlock> allSortableInventories = new List<IMyTerminalBlock>();
		List<IMyTerminalBlock> typeContainers = new List<IMyTerminalBlock>();
		List<IMyRefinery> refineries = new List<IMyRefinery>();
		List<IMyTerminalBlock> refineriesList = new List<IMyTerminalBlock>();
		List<IMyTerminalBlock> arcsList = new List<IMyTerminalBlock>();
		List<IMyAssembler> assemblers = new List<IMyAssembler>();
		List<IMyGasGenerator> oxygenGenerators = new List<IMyGasGenerator>();
		List<IMyReactor> reactors = new List<IMyReactor>();
		List<IMyTextPanel> autocraftingLCDs = new List<IMyTextPanel>();
		List<string> craftables = new List<string>();
		List<string> specialContainerItems = new List<string>();

		// Container type lists
		List<IMyTerminalBlock> oreContainers = new List<IMyTerminalBlock>();
		List<IMyTerminalBlock> ingotContainers = new List<IMyTerminalBlock>();
		List<IMyTerminalBlock> componentContainers = new List<IMyTerminalBlock>();
		List<IMyTerminalBlock> toolContainers = new List<IMyTerminalBlock>();
		List<IMyTerminalBlock> ammoContainers = new List<IMyTerminalBlock>();
		List<IMyTerminalBlock> bottleContainers = new List<IMyTerminalBlock>();
		List<IMyTerminalBlock> specialContainers = new List<IMyTerminalBlock>();

		// Array with all container type names
		string[] containerTypeNames = 
		{
			oreContainer,
			ingotContainer,
			componentContainer,
			toolContainer,
			ammoContainer,
			bottleContainer
		};

		// Lowercase locked and special container shortcuts
		string spCo = specialContainer.ToLower();
		string loCo = lockedContainer.ToLower();

		// Variable for auto assigning new containers
		string assignNewContainer = "";

		// Ore balancing variables
		bool arcPriorityActive = false;
		string noBalance = "nobalance";

		// LCD Scrolling
		int scrollDirection = 1;
		int scrollWait = 3;
		int lineStart = 3;

		// Error handling
		string warning;
		int warningCount = 0;

		// Script timing variables
		double elapsedTime = 0;
		int execCounter = 1;
		DateTime assemblerQueueSorting = DateTime.Now;

		// Default CustomData for the programmable block
		string defaultCustomData =
		"oreContainer=" + oreContainer + "\n" +
		"ingotContainer=" + ingotContainer + "\n" +
		"componentContainer=" + componentContainer + "\n" +
		"toolContainer=" + toolContainer + "\n" +
		"ammoContainer=" + ammoContainer + "\n" +
		"bottleContainer=" + bottleContainer + "\n" +
		"lockedContainer=" + lockedContainer + "\n" +
		"specialContainer=" + specialContainer + "\n\n" +
		"Saved itemIDs:\n============";

		// Autocrafting parts
		string autocraftingHeader = "Isy's Inventory Manager Autocrafting\n====================================\n\n";
		char[] comparators = { '=', '>', '<' };

		// Lists for ore balancing
		List<String> refWhitelist = new List<string> { "Uranium", "Silicon", "Silver", "Gold", "Platinum", "Magnesium", "Iron", "Nickel", "Cobalt", "Scrap" };
		List<String> refSpecificlist = new List<string> { "Uranium", "Silicon", "Silver", "Gold", "Platinum", "Magnesium" };
		List<String> arcWhitelist = new List<string> { "Iron", "Nickel", "Cobalt" };

		// Item type strings
		const string MoB = "MyObjectBuilder_";
		const string Or = "MyObjectBuilder_Ore/";
		const string In = "MyObjectBuilder_Ingot/";
		const string Co = "MyObjectBuilder_Component/";
		const string Am = "MyObjectBuilder_AmmoMagazine/";
		const string Ox = "MyObjectBuilder_OxygenContainerObject/";
		const string Ga = "MyObjectBuilder_GasContainerObject/";
		const string Pg = "MyObjectBuilder_PhysicalGunObject/";
		const string Bp = "MyObjectBuilder_BlueprintDefinition/";

		// Item subtype lists (based on type)
		List<string> oreType = new List<string>();
		List<string> ingotType = new List<string>();
		List<string> componentType = new List<string>();
		List<string> ammoType = new List<string>();
		List<string> oxygenType = new List<string>();
		List<string> hydrogenType = new List<string>();
		List<string> gunType = new List<string>();

		// Method names in case of exceptions
		string[] methodName =
		{
			"",
			"Set the Item IDs",
			"Sorting",
			"Get the assembler queue",
			"Fill special containers",
			"Add fill level to container names",
			"Get global item amount",
			"Autocrafting",
			"Sort the assembler queue",
			"Clean up assemblers",
			"Ore balancing",
			"Ice balancing",
			"Uranium balancing",
			"Internal sorting",
		};

		/// Pre-Run preparations
		public Program()
		{
			// Set UpdateFrequency for starting the programmable block over and over again
			Runtime.UpdateFrequency = UpdateFrequency.Update1;
		}


		void Main()
		{
			try
			{
				// Time measurement to compete against tickrate lag
				if (elapsedTime < 250)
				{
					elapsedTime += Runtime.TimeSinceLastRun.TotalMilliseconds;
					return;
				}
				else
				{
					elapsedTime = 0;
				}

				// Get all connected inventory blocks
				GetInventoryBlocks();

				if (execCounter == 1)
				{
					warningCount = 0;

					// Compare the type names to the saved ones. Rename if necessary
					CheckTypeNames();

					// Auto assign the ressource type name to containers
					if (autoAssignContainers) AssignContainers();

					// Fill fullItemId dictionary
					SetItemIDs();
				}

				if (execCounter == 2)
				{
					// Sort our cargo
					if (enableSorting)
					{
						Sort("Ore", oreContainers, oreContainer);
						Sort("Ingot", ingotContainers, ingotContainer);
						Sort("Component", componentContainers, componentContainer);
						Sort("Gun", toolContainers, toolContainer);
						Sort("Ammo", ammoContainers, ammoContainer);
						Sort("OxygenContainer", bottleContainers, bottleContainer);
						Sort("GasContainer", bottleContainers, bottleContainer);
					}
				}

				if (execCounter == 3)
				{
					// Get the global assembler queue and save it
					if (enableAutocrafting) SetGlobalAssemblerQueue();
				}

				if (execCounter == 4)
				{
					// Fill the special containers
					if (specialContainers.Count != 0) FillSpecialContainers();
				}

				if (execCounter == 5)
				{
					// Add the fill level percentage to containers
					AddFillLevel();
				}

				if (execCounter == 6)
				{
					// Get global item amounts
					if (enableAutocrafting) SetGlobalItemAmount();
				}

				if (execCounter == 7)
				{
					// Autocrafting
					if (enableAutocrafting) Autocrafting();
				}

				if (execCounter == 8)
				{
					// Sort the assembler queue
					if (enableAutocrafting && sortAssemblerQueue) SortAssemblerQueue();
				}

				if (execCounter == 9)
				{
					// Clean up the assemblers
					if (enableAssemblerCleanup) CleanupAssemblers();

					// Find unknown blueprints
					if (enableAutocrafting) saveUnknownBlueprint();
				}

				if (execCounter == 10)
				{
					// Balance the ores in the refineries
					if (enableOreBalancing)
					{
						BalanceOres();
					}
					else if (!enableOreBalancing)
					{
						// If ore balancing is turned off, activate the conveyors again
						foreach (IMyRefinery refinery in refineries)
						{
							refinery.UseConveyorSystem = true;
						}
					}
				}

				if (execCounter == 11)
				{
					// Balance ice
					if (enableIceBalancing)
					{
						BalanceIce();
					}
					else if (!enableIceBalancing)
					{
						// If ice balancing is turned off, activate the conveyors again
						foreach (IMyRefinery oxygenGenerator in oxygenGenerators)
						{
							oxygenGenerator.UseConveyorSystem = true;
						}
					}
				}

				if (execCounter == 12)
				{
					// Balance uranium
					if (enableUraniumBalancing)
					{
						BalanceUranium();
					}
					else if (!enableUraniumBalancing)
					{
						// If uranium balancing is turned off, activate the conveyors again
						foreach (IMyReactor reactor in reactors)
						{
							reactor.UseConveyorSystem = true;
						}
					}
				}

				if (execCounter == 13)
				{
					// Sort item internally
					if (enableInternalSorting)
					{
						SortInternal();
					}
				}

				// Create the information string that is shown to the user
				CreateInformation();

				// Write the information to various channels
				Echo(informationString);
				WriteLCD();

				// Update the script execution counter
				if (execCounter >= 13)
				{
					execCounter = 1;

					// Reset the warning if no warnings were counted
					if (warningCount == 0)
					{
						warning = null;
					}
				}
				else
				{
					execCounter++;
				}
			}
			catch (Exception e)
			{
				informationString = " " + e + "\n\n";
				informationString += "The error occured while executing the following script step:\n" + methodName[execCounter] + " (ID: " + execCounter + ")";
				WriteLCD();
				throw new Exception(informationString);
			}
		}


		void GetInventoryBlocks()
		{
			// Get all type containers
			GridTerminalSystem.SearchBlocksOfName(oreContainer, oreContainers, c => c.HasInventory);
			GridTerminalSystem.SearchBlocksOfName(ingotContainer, ingotContainers, c => c.HasInventory);
			GridTerminalSystem.SearchBlocksOfName(componentContainer, componentContainers, c => c.HasInventory);
			GridTerminalSystem.SearchBlocksOfName(toolContainer, toolContainers, c => c.HasInventory);
			GridTerminalSystem.SearchBlocksOfName(ammoContainer, ammoContainers, c => c.HasInventory);
			GridTerminalSystem.SearchBlocksOfName(bottleContainer, bottleContainers, c => c.HasInventory);

			// Special Containers
			foreach (var container in specialContainers)
			{
				// Clear custom data if the containers doesn't contain the keyword anymore
				if (!container.CustomName.ToLower().Contains(spCo)) container.CustomData = "";
			}
			GridTerminalSystem.SearchBlocksOfName(specialContainer, specialContainers, c => c.HasInventory);

			// Sort all type containers
			SortBlockList(oreContainers);
			SortBlockList(ingotContainers);
			SortBlockList(componentContainers);
			SortBlockList(toolContainers);
			SortBlockList(ammoContainers);
			SortBlockList(bottleContainers);
			SortBlockList(specialContainers);

			// Save all type containers in one list
			typeContainers.Clear();
			typeContainers.AddRange(oreContainers);
			typeContainers.AddRange(ingotContainers);
			typeContainers.AddRange(componentContainers);
			typeContainers.AddRange(toolContainers);
			typeContainers.AddRange(ammoContainers);
			typeContainers.AddRange(bottleContainers);
			typeContainers.AddRange(specialContainers);

			// Get all refineries, assemblers, oxygen generators and reactors
			GridTerminalSystem.GetBlocksOfType<IMyRefinery>(refineries, c => !c.CustomName.ToLower().Contains(loCo) && c.IsWorking);
			GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers, c => !c.CustomName.ToLower().Contains(spCo) && !c.CustomName.ToLower().Contains(loCo) && c.IsWorking);
			GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(oxygenGenerators, c => !c.CustomName.ToLower().Contains(spCo) && !c.CustomName.ToLower().Contains(loCo) && !c.CustomName.ToLower().Contains(noBalance) && c.IsFunctional && c.Enabled);
			GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors, c => !c.CustomName.ToLower().Contains(spCo) && !c.CustomName.ToLower().Contains(loCo) && !c.CustomName.ToLower().Contains(noBalance) && c.IsFunctional && c.Enabled);

			// Get all inventory blocks for autocrafting and sorting
			GridTerminalSystem.SearchBlocksOfName("", allInventories, i => i.HasInventory && i.InventoryCount == 1 && !i.GetType().ToString().Contains("MyCockpit"));
			GridTerminalSystem.SearchBlocksOfName("", allSortableInventories, i => i.HasInventory && i.InventoryCount == 1 &&
												  !i.CustomName.ToLower().Contains(spCo) && !i.CustomName.ToLower().Contains(loCo) &&
												  !i.GetType().ToString().Contains("Reactor") && !i.GetType().ToString().Contains("MyCockpit") &&
												  !i.GetType().ToString().Contains("Gatling") && !i.GetType().ToString().Contains("Missile"));

			// Sort allSortableInventories list ascending by type (for bottle refilling)
			allSortableInventories.Sort((a, b) => a.GetType().ToString().CompareTo(b.GetType().ToString()));
		}


		void SortBlockList(List<IMyTerminalBlock> blockList)
		{
			if (blockList.Count >= 2) blockList.Sort((a, b) => GetPriority(b).CompareTo(GetPriority(a)));
		}


		void CheckTypeNames()
		{
			bool different = false;

			// Compare if the saved type names differ from the actual type names
			if (oreContainer != ReadCustomData("oreContainer"))
			{
				different = true;
			}
			else if (ingotContainer != ReadCustomData("ingotContainer"))
			{
				different = true;
			}
			else if (componentContainer != ReadCustomData("componentContainer"))
			{
				different = true;
			}
			else if (toolContainer != ReadCustomData("toolContainer"))
			{
				different = true;
			}
			else if (ammoContainer != ReadCustomData("ammoContainer"))
			{
				different = true;
			}
			else if (bottleContainer != ReadCustomData("bottleContainer"))
			{
				different = true;
			}
			else if (lockedContainer != ReadCustomData("lockedContainer"))
			{
				different = true;
			}
			else if (specialContainer != ReadCustomData("specialContainer"))
			{
				different = true;
			}

			// If something was different, check all blocks
			if (different)
			{
				// Get all terminal blocks
				List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
				GridTerminalSystem.GetBlocks(allBlocks);

				// Cycle through all the blocks and rename to the new naming scheme
				foreach (var block in allBlocks)
				{
					if (block.CustomName.Contains(ReadCustomData("oreContainer")))
					{
						block.CustomName = block.CustomName.Replace(ReadCustomData("oreContainer"), oreContainer);
					}
					if (block.CustomName.Contains(ReadCustomData("ingotContainer")))
					{
						block.CustomName = block.CustomName.Replace(ReadCustomData("ingotContainer"), ingotContainer);
					}
					if (block.CustomName.Contains(ReadCustomData("componentContainer")))
					{
						block.CustomName = block.CustomName.Replace(ReadCustomData("componentContainer"), componentContainer);
					}
					if (block.CustomName.Contains(ReadCustomData("toolContainer")))
					{
						block.CustomName = block.CustomName.Replace(ReadCustomData("toolContainer"), toolContainer);
					}
					if (block.CustomName.Contains(ReadCustomData("ammoContainer")))
					{
						block.CustomName = block.CustomName.Replace(ReadCustomData("ammoContainer"), ammoContainer);
					}
					if (block.CustomName.Contains(ReadCustomData("bottleContainer")))
					{
						block.CustomName = block.CustomName.Replace(ReadCustomData("bottleContainer"), bottleContainer);
					}
					if (block.CustomName.Contains(ReadCustomData("lockedContainer")))
					{
						block.CustomName = block.CustomName.Replace(ReadCustomData("lockedContainer"), lockedContainer);
					}
					if (block.CustomName.Contains(ReadCustomData("specialContainer")))
					{
						block.CustomName = block.CustomName.Replace(ReadCustomData("specialContainer"), specialContainer);
					}
				}

				// Update saved type names
				WriteCustomData("oreContainer", oreContainer);
				WriteCustomData("ingotContainer", ingotContainer);
				WriteCustomData("componentContainer", componentContainer);
				WriteCustomData("toolContainer", toolContainer);
				WriteCustomData("ammoContainer", ammoContainer);
				WriteCustomData("bottleContainer", bottleContainer);
				WriteCustomData("lockedContainer", lockedContainer);
				WriteCustomData("specialContainer", specialContainer);
			}

		}


		void AssignContainers()
		{
			// Get all containers on the current grid (not on subgrids)
			List<IMyCargoContainer> gridContainers = new List<IMyCargoContainer>();
			GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(gridContainers,
				c => c.CubeGrid == Me.CubeGrid && !c.CustomName.ToLower().Contains(spCo) && !c.CustomName.ToLower().Contains(loCo));

			foreach (var container in gridContainers)
			{
				bool containsType = false;

				foreach (string type in containerTypeNames)
				{
					if (container.CustomName.Contains(type)) containsType = true;
				}

				// If the container doesn't contain a type and isn't locked, add one
				if (!containsType)
				{
					// Add to ore containers if the list is empty
					if (oreContainers.Count == 0 || assignNewContainer == "Ore")
					{
						container.CustomName = container.CustomName + " " + oreContainer;
						oreContainers.Add(container);

						// Add to ingot containers if the list is empty
					}
					else if (ingotContainers.Count == 0 || assignNewContainer == "Ingot")
					{
						container.CustomName = container.CustomName + " " + ingotContainer;
						ingotContainers.Add(container);

						// Add to component containers if the list is empty
					}
					else if (componentContainers.Count == 0 || assignNewContainer == "Component")
					{
						container.CustomName = container.CustomName + " " + componentContainer;
						componentContainers.Add(container);

						// Add to tool containers if the list is empty
					}
					else if (toolContainers.Count == 0 || assignNewContainer == "Gun")
					{
						if (toolsAmmoBottlesInOne)
						{
							container.CustomName = container.CustomName + " " + toolContainer + " " + ammoContainer + " " + bottleContainer;
							toolContainers.Add(container);
							ammoContainers.Add(container);
							bottleContainers.Add(container);
						}
						else
						{
							container.CustomName = container.CustomName + " " + toolContainer;
							toolContainers.Add(container);
						}


						// Add to ammo containers if the list is empty
					}
					else if (ammoContainers.Count == 0 || assignNewContainer == "Ammo")
					{
						if (toolsAmmoBottlesInOne)
						{
							container.CustomName = container.CustomName + " " + toolContainer + " " + ammoContainer + " " + bottleContainer;
							toolContainers.Add(container);
							ammoContainers.Add(container);
							bottleContainers.Add(container);
						}
						else
						{
							container.CustomName = container.CustomName + " " + ammoContainer;
							ammoContainers.Add(container);
						}


						// Add to bottle containers if the list is empty
					}
					else if (bottleContainers.Count == 0 || assignNewContainer == "OxygenBottle" || assignNewContainer == "HydrogenBottle")
					{
						if (toolsAmmoBottlesInOne)
						{
							container.CustomName = container.CustomName + " " + toolContainer + " " + ammoContainer + " " + bottleContainer;
							toolContainers.Add(container);
							ammoContainers.Add(container);
							bottleContainers.Add(container);
						}
						else
						{
							container.CustomName = container.CustomName + " " + bottleContainer;
							bottleContainers.Add(container);
						}
					}

					assignNewContainer = "";
				}
			}

		}


		void SetItemIDs()
		{
			// Load saved itemIDs
			LoadSavedItems();

			// Search all inventories for modded items
			foreach (var inventory in allInventories)
			{
				var items = inventory.GetInventory(0).GetItems();

				foreach (var item in items)
				{
					string fullID = item.ToString();
					string subtype = item.Content.SubtypeName;

					if (fullID.Contains(Or) && !oreType.Contains(subtype))
					{
						oreType.Add(subtype);

						// Also add ores to the refinery lists (but no ice or stone)
						if (!subtype.Contains("Ice") && !subtype.Contains("Stone"))
						{
							if (!refWhitelist.Contains(subtype)) refWhitelist.Add(subtype);
							if (!refSpecificlist.Contains(subtype)) refSpecificlist.Add(subtype);
						}

						SaveItemsToCustomdata(Or + subtype);
					}
					else if (fullID.Contains(In) && !ingotType.Contains(subtype))
					{
						ingotType.Add(subtype);
						SaveItemsToCustomdata(In + subtype);
					}
					else if (fullID.Contains(Co) && !componentType.Contains(subtype))
					{
						componentType.Add(subtype);
						SaveItemsToCustomdata(Co + subtype);
					}
					else if (fullID.Contains(Am) && !ammoType.Contains(subtype))
					{
						ammoType.Add(subtype);
						SaveItemsToCustomdata(Am + subtype);
					}
					else if (fullID.Contains(Ox) && !oxygenType.Contains(subtype))
					{
						oxygenType.Add(subtype);
						SaveItemsToCustomdata(Ox + subtype);
					}
					else if (fullID.Contains(Ga) && !hydrogenType.Contains(subtype))
					{
						hydrogenType.Add(subtype);
						SaveItemsToCustomdata(Ga + subtype);
					}
					else if (fullID.Contains(Pg) && !gunType.Contains(subtype))
					{
						gunType.Add(subtype);
						SaveItemsToCustomdata(Pg + subtype);
					}
				}
			}

			// Sort the lists
			oreType.Sort((a, b) => a.CompareTo(b));
			ingotType.Sort((a, b) => a.CompareTo(b));
			componentType.Sort((a, b) => a.CompareTo(b));
			ammoType.Sort((a, b) => a.CompareTo(b));
			oxygenType.Sort((a, b) => a.CompareTo(b));
			hydrogenType.Sort((a, b) => a.CompareTo(b));
			gunType.Sort((a, b) => a.CompareTo(b));

			// Create the craftables list
			craftables.Clear();
			craftables.AddRange(componentType);
			craftables.AddRange(ammoType);
			craftables.AddRange(oxygenType);
			craftables.AddRange(hydrogenType);
			craftables.AddRange(gunType);

			// Create the special container list
			specialContainerItems.Clear();

			foreach (var item in componentType)
			{
				specialContainerItems.Add(Co.Replace(MoB, "") + item);
			}
			foreach (var item in oreType)
			{
				specialContainerItems.Add(Or.Replace(MoB, "") + item);
			}
			foreach (var item in ingotType)
			{
				specialContainerItems.Add(In.Replace(MoB, "") + item);
			}
			foreach (var item in ammoType)
			{
				specialContainerItems.Add(Am.Replace(MoB, "") + item);
			}
			foreach (var item in oxygenType)
			{
				specialContainerItems.Add(Ox.Replace(MoB, "") + item);
			}
			foreach (var item in hydrogenType)
			{
				specialContainerItems.Add(Ga.Replace(MoB, "") + item);
			}
			foreach (var item in gunType)
			{
				specialContainerItems.Add(Pg.Replace(MoB, "") + item);
			}

			// Create the fullItemId dictionary
			SetFullItemId(Or, oreType);
			SetFullItemId(In, ingotType);
			SetFullItemId(Co, componentType);
			SetFullItemId(Am, ammoType);
			SetFullItemId(Ox, oxygenType);
			SetFullItemId(Ga, hydrogenType);
			SetFullItemId(Pg, gunType);
		}


		void Sort(string currentType, List<IMyTerminalBlock> currentTypeContainers, string typeKeyword)
		{
			// If no containers of the current type exist, stop the execution of the method
			if (currentTypeContainers.Count == 0)
			{
				warning = "There are no containers for type '" + typeKeyword + "'!\nBuild new ones or add the tag to existing ones!";
				warningCount++;
				return;
			}

			IMyTerminalBlock targetContainer = null;
			int targetPriority = int.MinValue;

			// Check if there is space in one of the type containers
			foreach (var container in currentTypeContainers)
			{
				var inventory = container.GetInventory(0);

				// Handling of oxygen and hydrogen tanks
				if (currentType == "OxygenContainer" && container.BlockDefinition.TypeIdString.Contains("OxygenTank") && container.BlockDefinition.SubtypeId.Contains("Hydrogen"))
				{
					continue;
				}
				else if (currentType == "GasContainer" && container.BlockDefinition.TypeIdString.Contains("OxygenTank") && !container.BlockDefinition.SubtypeId.Contains("Hydrogen"))
				{
					continue;
				}

				// If a suitable container was found, check its priority
				if ((float)inventory.CurrentVolume < (float)inventory.MaxVolume * 0.99)
				{
					int containerPriority = GetPriority(container);

					// If the priority was higher than from the one before, set it as the target container
					if (containerPriority > targetPriority)
					{
						targetPriority = containerPriority;
						targetContainer = container;
					}
				}
			}

			// If no empty target container was found, stop the execution of the method
			if (targetContainer == null)
			{
				warning = "All containers for type '" + typeKeyword + "' are full!\nYou should build new cargo containers!";
				warningCount++;
				assignNewContainer = currentType;
				return;
			}

			// Containers
			foreach (var container in allSortableInventories)
			{
				// If a container is the target container or is of the same type with a higher priority, skip it
				if (container == targetContainer || (container.CustomName.Contains(typeKeyword) && GetPriority(container) >= targetPriority) ||
					(currentType == "Ore" && container.GetType().ToString().Contains("MyGasGenerator")))
				{
					continue;
				}

				// Check the owner and give a warning
				if (container.GetOwnerFactionTag() != Me.GetOwnerFactionTag())
				{
					warning = "'" + container.CustomName + "'\nhas a different owner/faction!\nCan't move items from there!";
					warningCount++;
					continue;
				}

				// If current type is oxygen or hydrogen bottles, move them to the first oxygen generator
				if (fillBottles && (currentType == "OxygenContainer" || currentType == "GasContainer") && !container.GetType().ToString().Contains("MyGasGenerator") && oxygenGenerators.Count > 0)
				{
					MoveItems(currentType, container, 0, oxygenGenerators[0], 0);
					continue;
				}

				MoveItems(currentType, container, 0, targetContainer, 0);
			}

			// Refineries
			foreach (var refinery in refineries)
			{
				// If a refinery is the target container or is of the same type with a higher priority, skip it
				if (refinery == targetContainer || (refinery.CustomName.Contains(typeKeyword) && GetPriority(refinery) >= targetPriority))
				{
					continue;
				}

				// Check the owner and give a warning
				if (refinery.GetOwnerFactionTag() != Me.GetOwnerFactionTag())
				{
					warning = "'" + refinery.CustomName + "'\nhas a different owner/faction!\nCan't move items from there!";
					warningCount++;
					continue;
				}

				MoveItems(currentType, refinery, 1, targetContainer, 0);
			}

			// Assemblers
			foreach (IMyAssembler assembler in assemblers)
			{
				// If a assembler is the target container or is of the same type with a higher priority, skip it
				if (assembler.Mode == MyAssemblerMode.Disassembly || assembler == targetContainer ||
					(assembler.CustomName.Contains(typeKeyword) && GetPriority(assembler) >= targetPriority))
				{
					continue;
				}

				// Check the owner and give a warning
				if (assembler.GetOwnerFactionTag() != Me.GetOwnerFactionTag())
				{
					warning = "'" + assembler.CustomName + "'\nhas a different owner/faction!\nCan't move items from there!";
					warningCount++;
					continue;
				}

				// If current type is oxygen or hydrogen bottles, move them to the first oxygen generator
				if (fillBottles && (currentType == "OxygenContainer" || currentType == "GasContainer") && oxygenGenerators.Count > 0)
				{
					MoveItems(currentType, assembler, 1, oxygenGenerators[0], 0);
					continue;
				}

				MoveItems(currentType, assembler, 1, targetContainer, 0);
			}
		}


		void SortInternal()
		{
			// Get the pattern
			char quantifier = '0';
			char direction = '0';

			char[] availableQuantifiers = { 'A', 'N', 'T', 'X' };
			char[] availableDirections = { 'a', 'd' };

			if (sortingPattern.Length == 2)
			{
				quantifier = sortingPattern[0];
				direction = sortingPattern[1];
			}

			// Error management
			if (quantifier.ToString().IndexOfAny(availableQuantifiers) < 0 || direction.ToString().IndexOfAny(availableDirections) < 0)
			{
				warning = "You provided the invalid sorting pattern:\n'" + sortingPattern + "'!\nCan't sort the inventories!";
				warningCount++;
				return;
			}

			foreach (var container in allInventories)
			{
				var inventory = container.GetInventory(0);
				var items = inventory.GetItems();

				// Skip the container if there more than 50 item stacks
				if (items.Count > 50) continue;
				if (Runtime.CurrentInstructionCount > Runtime.MaxInstructionCount * 0.9) return;

				// Save the global pattern into a temporary one
				char tempQuantifier = quantifier;
				char tempDirection = direction;

				// Try to find the second match of the container name beginning with '(sort:'
				string containerPattern = System.Text.RegularExpressions.Regex.Match(container.CustomName, @"(\(sort:)(.{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Groups[2].Value;

				// If a match was found, try to temporarily change the global pattern
				if (containerPattern.Length == 2)
				{
					quantifier = containerPattern[0];
					direction = containerPattern[1];

					// Error management
					if (quantifier.ToString().IndexOfAny(availableQuantifiers) < 0 || direction.ToString().IndexOfAny(availableDirections) < 0)
					{
						warning = "You provided an invalid sorting pattern in:\n'" + container.CustomName + "'!\nUsing global pattern!";
						warningCount++;

						// Restore the global pattern
						quantifier = tempQuantifier;
						direction = tempDirection;
					}
				}

				// Two finger sorting
				for (int i = 0; i <= items.Count - 1; i++)
				{
					for (int j = i + 1; j <= items.Count - 1; j++)
					{
						// Sort based on amount of items
						if (quantifier == 'A')
						{
							if (direction == 'd' && items[i].Amount < items[j].Amount)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}
							else if (direction == 'a' && items[i].Amount > items[j].Amount)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}

							// Sort based on name of items
						}
						else if (quantifier == 'N')
						{
							if (direction == 'd' && items[i].Content.SubtypeId.ToString().CompareTo(items[j].Content.SubtypeId.ToString()) < 0)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}
							else if (direction == 'a' && items[i].Content.SubtypeId.ToString().CompareTo(items[j].Content.SubtypeId.ToString()) > 0)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}

							// Sort based on itemtype
						}
						else if (quantifier == 'T')
						{
							if (direction == 'd' && GetFullItemId(items[i].Content.SubtypeId.ToString()).CompareTo(GetFullItemId(items[j].Content.SubtypeId.ToString())) > 0)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}
							else if (direction == 'a' && GetFullItemId(items[i].Content.SubtypeId.ToString()).CompareTo(GetFullItemId(items[j].Content.SubtypeId.ToString())) < 0)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}

							// Sort based on itemtype and number of items
						}
						else if (quantifier == 'X')
						{
							// 1. Sort on type
							if (direction == 'd' && items[i].Content.TypeId.ToString().CompareTo(items[j].Content.TypeId.ToString()) < 0)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}
							else if (direction == 'a' && items[i].Content.TypeId.ToString().CompareTo(items[j].Content.TypeId.ToString()) > 0)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}
							items = inventory.GetItems();

							// 2. Sort on number of items
							if (direction == 'd' && items[i].Content.TypeId == items[j].Content.TypeId && items[i].Amount < items[j].Amount)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}
							else if (direction == 'a' && items[i].Content.TypeId == items[j].Content.TypeId && items[i].Amount > items[j].Amount)
							{
								inventory.TransferItemTo(inventory, i, j, true);
							}
						}

						items = inventory.GetItems();
					}
				}

				// Restore the global pattern
				quantifier = tempQuantifier;
				direction = tempDirection;
			}
		}


		void FillSpecialContainers()
		{
			foreach (var container in specialContainers)
			{
				// Check the custom data and add new items
				CheckSpecialCustomData(container);

				// Split the container's custom data by linebreaks
				var customData = container.CustomData.Split('\n');

				// Parse every line in the custom data
				foreach (var line in customData)
				{
					// If a line doesn't contain a "=", it's probably the header or empty, so skip it
					if (!line.Contains("=")) continue;

					string itemName = "";
					double wantedAmount = 0;
					var lineContent = line.Split('=');

					// If we get at least 2 items, we can proceed
					if (lineContent.Length >= 2)
					{
						itemName = lineContent[0];
						double.TryParse(lineContent[1], out wantedAmount);
					}
					else
					{
						continue;
					}

					double storedAmount = GetItemAmount(itemName, container);
					double missingAmount = 0;

					// If wanted amount is positive, calculate missing amount
					if (wantedAmount >= 0)
					{
						missingAmount = wantedAmount - storedAmount;

						// If wanted amount is negative, calculate the items to remove above the threshold
					}
					else
					{
						missingAmount = Math.Abs(wantedAmount) - storedAmount;
					}

					// If the item in this quantity isn't already there, search for it
					if (missingAmount >= 1 && wantedAmount >= 0)
					{
						IMyTerminalBlock sourceContainer = FindItem(itemName, true, container);

						// If a container with the item was found, move it to this container
						if (sourceContainer != null)
						{
							MoveItems(itemName, sourceContainer, 0, container, 0, missingAmount);
						}
						else
						{
							lastAction = "Missing: " + Math.Round(missingAmount, 1) + " " + itemName + "\nfor    : '" + container.CustomName + "'!\n\n";

							// If they should be produced, do it (unless it is no ore or ingot)
							if (produceMissingItems && assemblers.Count > 0 && !itemName.Contains("Ore") && !itemName.Contains("Ingot"))
							{
								lastAction += "They were queued in an assembler!";
								QueueInAssembler(MoB + itemName, Math.Ceiling(missingAmount));
							}
							else
							{
								lastAction += "You should produce some more!";
							}
						}

						// If the current quantity exceeds the wanted on, take the items out, if enabled
					}
					else if (missingAmount < 0 && specialContainerTakeOutExcess)
					{
						IMyTerminalBlock targetContainer = FindFreeContainer(container);

						// If a free container was found, move the excess items there
						if (targetContainer != null) MoveItems(itemName, container, 0, targetContainer, 0, Math.Abs(missingAmount));
					}
				}
			}
		}


		void AddFillLevel()
		{
			foreach (var container in typeContainers)
			{
				string oldName = container.CustomName;
				string newName = "";

				// Find the old percentage via Regex
				var findPercent = System.Text.RegularExpressions.Regex.Match(oldName, @"\(\d+\.?\d*\%\)");

				// If a match was found, remove it in the new name
				if (findPercent.Value != "")
				{
					newName = oldName.Replace(findPercent.Value, "").TrimEnd(' ');
				}
				else
				{
					newName = oldName;
				}

				// Get the inventory
				var inventory = container.GetInventory(0);
				string percent = GetPercentString((double)inventory.CurrentVolume, (double)inventory.MaxVolume);

				// Add percentages
				if (showFillLevel)
				{
					newName += " (" + percent + ")";
					newName = newName.Replace("  ", " ");
				}

				// Rename the block if the name has changed
				if (newName != oldName) container.CustomName = newName;
			}
		}


		string GetAutocraftingLcdText()
		{
			// Sort the list of LCDs
			autocraftingLCDs.Sort((a, b) => a.CustomName.CompareTo(b.CustomName));

			// Get the text of all LCDs
			string text = "";

			foreach (var lcd in autocraftingLCDs)
			{
				text += lcd.GetPublicText();

				// Set basic settings on each LCD
				lcd.WritePublicTitle("Craft item manually once to show up here");
				lcd.Font = "Monospace";
				lcd.FontSize = 0.555f;
				lcd.ShowPublicTextOnScreen();
			}

			foreach (var item in craftables)
			{
				if (!text.Contains(item))
				{
					string fullId = GetFullItemId(item);
					double itemAmount = Math.Ceiling(GetGlobalItemAmount(fullId));
					if (itemAmount > 0)
					{
						text += "\n" + item + " " + itemAmount + " = " + itemAmount;
					}
				}
			}

			// Create newText from oldText
			List<string> oldText = text.Split('\n').ToList();
			string newText = "";

			// Remove every line that has no comparator
			oldText.RemoveAll(line => line.IndexOfAny(comparators) <= 0);

			try
			{
				IOrderedEnumerable<string> tempList;
				if (sortAlphabetically)
				{
					tempList = oldText.OrderBy(a => a.Substring(0, a.IndexOf(" ")));
				}
				else
				{
					tempList = oldText.OrderByDescending(a => int.Parse(System.Text.RegularExpressions.Regex.Match(a, @" \d+ ").Value.Replace(" ", "")));
				}

				foreach (var item in tempList)
				{
					newText += item + "\n";
				}
			}
			catch (Exception)
			{
				// ignore
			}

			return newText.TrimEnd('\n');
		}


		void SetAutocraftingLcdText(string text)
		{
			// Add the header back to the text
			text = autocraftingHeader + "Component       Current Amount / Wanted Amount\n\n" + text;

			// If one LCD was found, write all the text to this one, else split it over all LCDs
			if (autocraftingLCDs.Count == 1)
			{
				autocraftingLCDs[0].WritePublicText(text);
			}
			else
			{
				var lines = text.Split('\n');
				int totalLines = lines.Length;
				int writtenLines = 0;
				int linesPerLCD = 32;

				// Cycle through each LCD and write the set amount of lines to it
				foreach (var lcd in autocraftingLCDs)
				{
					int writtenCurrent = 0;
					string newText = "";
					while (writtenLines < totalLines && writtenCurrent < linesPerLCD)
					{
						newText += lines[writtenLines] + "\n";
						writtenLines++;
						writtenCurrent++;
					}

					lcd.WritePublicText(newText);
				}

				// If not all lines could be written after cycling through all LCDs, append the rest to the last LCD
				if (writtenLines < totalLines)
				{
					while (writtenLines < totalLines)
					{
						autocraftingLCDs[autocraftingLCDs.Count - 1].WritePublicText(lines[writtenLines], true);
						writtenLines++;
					}
				}
			}
		}


		void Autocrafting()
		{
			autocraftingLCDs.Clear();

			// Only collect LCDs on the same grid with the autocraftingLcd name
			GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(autocraftingLCDs, lcd => lcd.CubeGrid == Me.CubeGrid && lcd.CustomName.Contains(autocraftingLcd));

			// If no LCD was found, stop the method
			if (autocraftingLCDs.Count == 0) return;

			if (assemblers.Count == 0)
			{
				foreach (var lcd in autocraftingLCDs)
				{
					string text = "Warning!\n\nNo assemblers found!\nBuild assemblers to enable autocrafting!";

					lcd.WritePublicText(text);
					lcd.Font = "Monospace";
					lcd.FontSize = 0.555f;
					lcd.ShowPublicTextOnScreen();
				}
				return;
			}

			var oldText = GetAutocraftingLcdText().Split('\n');
			string newText = "";

			foreach (var line in oldText)
			{

				// Get the old text, read the current amount and parse the wanted amount
				string item = "";
				try
				{
					item = line.Substring(0, line.IndexOf(" "));
				}
				catch
				{
					continue;
				}
				string fullId = GetFullItemId(item);

				// Remove lines if no item ID is available for them
				if (fullId == null) continue;

				double storedAmount = Math.Ceiling(GetGlobalItemAmount(fullId));
				double wantedAmount = 0;
				double.TryParse(line.Substring(line.IndexOfAny(comparators) + 1), out wantedAmount);

				double amountToQueue = Math.Abs(wantedAmount - storedAmount);
				bool blueprintFound;
				MyDefinitionId blueprintId = GetBlueprint(fullId, out blueprintFound);
				double amountInQueue = GetGlobalAssemblerQueue(blueprintId);

				// Prepare the new text
				newText += item + " ";

				string queue = "";
				string comparator = " = ";

				// Try removing the item from the queue
				if (storedAmount >= wantedAmount && amountInQueue > 0) RemoveFromQueue(fullId);

				// If the stored amount is more than the wanted, disassemble the item if active
				if (storedAmount > wantedAmount)
				{
					comparator = " > ";

					// If disassemble is active, disassemble any excess materials
					if (disassembleExcess)
					{
						DisassembleInAssembler(fullId, amountToQueue);
						queue = " [D:";
					}

					// If the stored amount	is less than the wanted, assemble the item
				}
				else if (storedAmount < wantedAmount)
				{
					comparator = " < ";

					// If the there are already item in the queue, just update the queue
					if (amountInQueue > 0)
					{
						queue = " [Q:";

						// Else	queue the item in the assembler
					}
					else
					{
						QueueInAssembler(fullId, amountToQueue);
						queue = " [Q:";
					}

					// If first two do not apply, there has to be a disassembling queue
				}
				else if (amountInQueue > 0)
				{
					DisassembleInAssembler(fullId, 0);
					queue = " [D:";
				}

				if (queue != "")
				{
					if (amountInQueue == 0)
					{
						queue += amountToQueue;
					}
					else
					{
						queue += Math.Round(amountInQueue);
					}
					queue += "]";
				}

				if (!blueprintFound) queue = " [No BP!]";

				// Finalize the new text
				newText += StringRepeat(' ', 29 - (item.Length + storedAmount.ToString().Length + queue.Length));
				newText += storedAmount + queue + comparator + wantedAmount + "\n";
			}

			newText = newText.TrimEnd('\n');
			SetAutocraftingLcdText(newText);
		}


		void BalanceOres()
		{
			refineriesList.Clear();
			arcsList.Clear();
			double refineryFillLevel = 0;
			double arcFillLevel = 0;

			// Cycle through all the refineries to set basics and sort them
			foreach (IMyRefinery refinery in refineries)
			{
				if (refinery.CustomName.Contains(specialContainer))
				{
					refinery.UseConveyorSystem = false;
					continue;
				}
				else if (refinery.CustomName.ToLower().Contains(noBalance))
				{
					refinery.UseConveyorSystem = true;
					continue;
				}
				else
				{
					refinery.UseConveyorSystem = false;
				}

				// Add them to their respective lists
				if (refinery.BlockDefinition.SubtypeId == "Blast Furnace")
				{
					arcsList.Add(refinery);
				}
				else
				{
					refineriesList.Add(refinery);
				}
			}

			// If there are no arc furnaces, deactivate a potential activated 'arcPriorityActive'
			if (arcsList.Count == 0) arcPriorityActive = false;

			// Add stone to the refinery list if set in the config
			if (enableStoneProcessing)
			{
				bool stoneInList = false;

				foreach (var ore in refWhitelist)
				{
					if (ore == "Stone") stoneInList = true;
				}

				if (!stoneInList)
				{
					refWhitelist.Add("Stone");
					refSpecificlist.Add("Stone");
				}
			}

			// Sort the ore lists based on global ingot amount from lowest to highest amount
			refWhitelist.Sort((a, b) => GetGlobalItemAmount(In + a).CompareTo(GetGlobalItemAmount(In + b)));
			refSpecificlist.Sort((a, b) => GetGlobalItemAmount(In + a).CompareTo(GetGlobalItemAmount(In + b)));
			arcWhitelist.Sort((a, b) => GetGlobalItemAmount(In + a).CompareTo(GetGlobalItemAmount(In + b)));

			// Arc priority mode or arc specialization
			if ((enableArcPriority || enableArcSpecialization) && arcsList.Count != 0 && refineriesList.Count != 0)
			{
				IMyTerminalBlock containerWithRefOre = null;
				string oreName = null;

				containerWithRefOre = FindItemList(refSpecificlist, allSortableInventories, out oreName);

				// If no container was found, look whether refinery specific ore is already in the refineries
				if (containerWithRefOre == null)
				{
					containerWithRefOre = FindItemList(refSpecificlist, refineriesList, out oreName);

					// If refinery specific ores were found in a refinery, set the item name to "AlreadyInRefinery"
					if (containerWithRefOre != null) oreName = "AlreadyInRefinery";
				}

				// If refinery specific ore was found, cycle through all refineries
				if (oreName != null)
				{
					arcPriorityActive = true;

					foreach (IMyRefinery refinery in refineriesList)
					{
						// Find a free cargo container to move its contents
						IMyTerminalBlock targetContainer = FindFreeContainer(refinery);

						// If a free cargo container was found, move arc ores to it
						if (targetContainer != null)
						{
							foreach (var ore in arcWhitelist)
							{
								MoveItems(ore, refinery, 0, targetContainer, 0);
							}

							var inventory = refinery.GetInventory(0);

							// Only proceed here, if the ore isn't already in the refinery or the refinery isn't too full
							if (oreName != "AlreadyInRefinery" && (double)inventory.CurrentVolume < (double)inventory.MaxVolume * 0.99)
							{
								// Move as much as possible of the found item in the refinery
								MoveItems(oreName, containerWithRefOre, 0, refinery, 0);
							}
						}
					}

					// If arc specialization is active, just remove any generic ore
				}
				else if (enableArcSpecialization)
				{
					foreach (IMyRefinery refinery in refineriesList)
					{
						// Find a free cargo container to move its contents
						IMyTerminalBlock targetContainer = FindFreeContainer(refinery);

						// If a free cargo container was found, move any generic ore to it
						if (targetContainer != null)
						{
							foreach (var ore in arcWhitelist)
							{
								MoveItems(ore, refinery, 0, targetContainer, 0);
							}
						}
					}
				}
				else
				{
					arcPriorityActive = false;
				}
			}

			// Regular item pull for refineries
			if (!enableArcSpecialization && !arcPriorityActive)
			{
				foreach (var refinery in refineriesList)
				{
					var refInventory = refinery.GetInventory(0);

					// If there is space, find items
					if ((double)refInventory.CurrentVolume < (double)refInventory.MaxVolume * 0.99)
					{
						IMyTerminalBlock sourceContainer = null;
						string itemName = null;

						// Search items based on our whitelist
						sourceContainer = FindItemList(refWhitelist, allSortableInventories, out itemName);

						// If no container with the items was found, there is nothing more to do here
						if (sourceContainer == null)
						{
							break;
						}

						MoveItems(itemName, sourceContainer, 0, refinery, 0);
						break;
					}
				}
			}

			// Regular item pull for arc furnaces
			foreach (var arcFurnace in arcsList)
			{
				var arcInventory = arcFurnace.GetInventory(0);

				// If there is space, find items
				if ((double)arcInventory.CurrentVolume < (double)arcInventory.MaxVolume * 0.99)
				{
					IMyTerminalBlock sourceContainer = null;
					string itemName = null;

					// Search items based on our whitelist
					sourceContainer = FindItemList(arcWhitelist, allSortableInventories, out itemName);

					// If no container with the items was found, there is nothing more to do here
					if (sourceContainer == null)
					{
						break;
					}

					MoveItems(itemName, sourceContainer, 0, arcFurnace, 0);
					break;
				}
			}

			// If a refinery is empty, get half of the inventory of all arc furnaces
			// But only, if arc specialization or arc priority isn't active
			if (!enableArcSpecialization && !arcPriorityActive)
			{
				foreach (var refinery in refineriesList)
				{
					var refInventory = refinery.GetInventory(0);

					if ((double)refInventory.CurrentVolume > (double)refInventory.MaxVolume * 0.005)
					{
						continue;
					}
					else
					{
						foreach (var arcFurnace in arcsList)
						{
							var arcInventory = arcFurnace.GetInventory(0);

							if ((double)arcInventory.CurrentVolume > 0) MoveItems("", arcFurnace, 0, refinery, 0, -0.5);
						}
					}
				}
			}

			// If an arc furnace is empty, get half of the iron, nickel or cobalt of all refineries
			foreach (var arcFurnace in arcsList)
			{
				var arcInventory = arcFurnace.GetInventory(0);

				if ((double)arcInventory.CurrentVolume > (double)arcInventory.MaxVolume * 0.005)
				{
					continue;
				}
				else
				{
					foreach (var refinery in refineriesList)
					{
						var refInventory = refinery.GetInventory(0);

						if ((double)refInventory.CurrentVolume > 0)
						{
							foreach (var ore in arcWhitelist)
							{
								MoveItems(ore, refinery, 0, arcFurnace, 0, -0.5);
							}
						}
					}
				}
			}

			// Get the current fill level to balance properly
			foreach (var refinery in refineries)
			{
				var inventory = refinery.GetInventory(0);

				// Differentiate between arc furnaces and refineries
				if (refinery.BlockDefinition.SubtypeId == "Blast Furnace")
				{
					arcFillLevel += (double)inventory.CurrentVolume / (double)inventory.MaxVolume;
				}
				else
				{
					refineryFillLevel += (double)inventory.CurrentVolume / (double)inventory.MaxVolume;
				}
			}

			// If refineries exist, balance them
			if (refineriesList.Count != 0)
			{
				refineryFillLevel /= refineriesList.Count;
				BalanceRefineryType(refineriesList, refineryFillLevel);
			}

			// If arc furnaces exist, balance them
			if (arcsList.Count != 0)
			{
				arcFillLevel /= arcsList.Count;
				BalanceRefineryType(arcsList, arcFillLevel);
			}

			// Sort the ore with the lowest global ingot amount in all refineries and arcs 
			if (sortRefiningQueue)
			{
				foreach (IMyRefinery refinery in refineries)
				{
					var inventory = refinery.GetInventory(0);
					var items = inventory.GetItems();

					// If there is less than 2 item stacks, continue with the next refinery
					if (items.Count < 2) continue;

					double lowestAmount = Double.MaxValue;
					int itemIndex = 0;

					// Figure out lowest ingot amount
					for (int i = 0; i <= items.Count - 1; i++)
					{
						string subtype = items[i].Content.SubtypeName;
						double ingotAmount = GetGlobalItemAmount(In + subtype);

						if (ingotAmount < lowestAmount)
						{
							lowestAmount = ingotAmount;
							itemIndex = i;
						}
					}

					// Move the ore with the lowest ingot amount to the front
					inventory.TransferItemTo(inventory, itemIndex, 0, true);
				}
			}
		}


		void BalanceRefineryType(List<IMyTerminalBlock> refineryType, double averageFillLevel)
		{
			// Cycle through all refineries and check if a refinery has more than the average fill level
			foreach (var refinery in refineryType)
			{
				var inventory = refinery.GetInventory(0);
				double inventoryVolume = (double)inventory.CurrentVolume;
				double inventoryMaxVolume = (double)inventory.MaxVolume;

				// If a refinery has more than the average fill level, find another one that has less
				if (inventoryVolume > inventoryMaxVolume * (averageFillLevel + 0.001))
				{
					foreach (var targetRefinery in refineryType)
					{
						// Don't exchange with yourself
						if (refinery == targetRefinery) continue;

						var targetInventory = targetRefinery.GetInventory(0);
						double targetInventoryVolume = (double)targetInventory.CurrentVolume;
						double targetInventoryMaxVolume = (double)targetInventory.MaxVolume;

						// Proceed if a refinery with less than the average fill level is found
						if (targetInventoryVolume < targetInventoryMaxVolume * (averageFillLevel - 0.001))
						{
							// Re-read the inventory volumes and items with every new target refinery
							inventoryVolume = (double)inventory.CurrentVolume;
							var items = inventory.GetItems();
							string itemName = items[items.Count - 1].Content.TypeId + "/" + items[items.Count - 1].Content.SubtypeId;
							double itemVolume = GetItemVolume(itemName, refinery);

							// Calculate the amount to move
							double amount = 0;
							amount = (inventoryVolume - inventoryMaxVolume * averageFillLevel) / itemVolume;

							// If the amount would exceed the average fill level in the target container, adjust it
							if (targetInventoryVolume + amount * itemVolume > targetInventoryMaxVolume * averageFillLevel)
							{
								amount = (targetInventoryMaxVolume * averageFillLevel - targetInventoryVolume) / itemVolume;
							}

							// Move the items from one refinery to the other one if needed
							MoveItems("", refinery, 0, targetRefinery, 0, amount);
						}
					}
				}
			}
		}


		void CleanupAssemblers()
		{
			foreach (var assembler in assemblers)
			{
				// If a assembler has a different owner, skip it
				if (assembler.GetOwnerFactionTag() == Me.GetOwnerFactionTag())
				{
					var assemblerInventory = assembler.GetInventory(0);

					// If the inventory is more than 75% full, move the items back to storage
					if ((float)assemblerInventory.CurrentVolume >= (float)assemblerInventory.MaxVolume * (assemblerFillLevelPercentage / 100))
					{
						IMyTerminalBlock targetContainer = FindFreeContainer(assembler);

						// If everything was set correctly, clean up the assembler's inventory
						if (targetContainer != null) MoveItems("", assembler, 0, targetContainer, 0);
					}
				}
			}

		}


		void QueueInAssembler(string fullItemId, double wantedAmount)
		{
			// If no assemblers are present, abort
			if (assemblers.Count == 0) return;

			// Prepare the item for an assembler blueprint item
			bool blueprintFound;
			MyDefinitionId blueprint = GetBlueprint(fullItemId, out blueprintFound);

			// If the blueprint isn't in the dictionary, give feedback in lastAction
			if (!blueprintFound)
			{
				lastAction = "Attempted to craft but canceled:\n'" + fullItemId.Remove(0, fullItemId.IndexOf("/") + 1) + "'\n";
				lastAction += "A valid blueprint for this item couldn't be found!\n";
				lastAction += "Queue it up a few times with no other item in queue\n";
				lastAction += "so that the script can learn its blueprint.";
				return;
			}

			// Set up the assemblers
			foreach (IMyAssembler assembler in assemblers)
			{
				if (assembler.Mode == MyAssemblerMode.Disassembly && assembler.IsProducing) return;
				assembler.Mode = MyAssemblerMode.Assembly;
				assembler.CooperativeMode = false;
				assembler.UseConveyorSystem = true;
				assembler.Repeating = false;
			}

			// Find the current number of queued items of the wanted one
			double amountInQueue = GetGlobalAssemblerQueue(blueprint);

			// Calculate the difference and add needed amounts
			double amountToQueue = wantedAmount - amountInQueue;
			double itemsPerAssembler = Math.Ceiling(amountToQueue / assemblers.Count);

			foreach (IMyAssembler assembler in assemblers)
			{
				// Adjust the items per assembler for the last one in the row
				if (itemsPerAssembler > amountToQueue) itemsPerAssembler = Math.Ceiling(amountToQueue);

				// If the item is not in the queue, add it to front the of the queue
				if (amountToQueue > 0)
				{
					assembler.InsertQueueItem(0, blueprint, itemsPerAssembler);
					amountToQueue -= itemsPerAssembler;
				}
				else
				{
					break;
				}
			}
		}


		void SortAssemblerQueue()
		{
			foreach (IMyAssembler assembler in assemblers)
			{
				// Don't sort assembler in disassembling mode
				if (assembler.Mode == MyAssemblerMode.Disassembly) continue;

				var queue = new List<MyProductionItem>();
				assembler.GetQueue(queue);

				// If there is less than 2 item stacks, continue with the next assembler
				if (queue.Count < 2) continue;

				int queueEnd = queue.Count - 1;

				// If an item can't be produced, move it to the end of the queue
				if (assembler.CurrentProgress == 0)
				{
					assembler.MoveQueueItemRequest(queue[0].ItemId, queueEnd);
					assemblerQueueSorting = DateTime.Now;
					assembler.GetQueue(queue);
					lastAction = "Not enough ressources to produce:\n";
					lastAction += queue[0].BlueprintId.ToString().Replace(Bp, "") + "\n";
					lastAction += "The item was moved to the end of the queue.";
				}

				// Don't sort if an item with missing components was found
				if ((DateTime.Now - assemblerQueueSorting).TotalSeconds < 30) continue;

				double lowestAmount = Double.MaxValue;
				int itemIndex = 0;

				// Figure out lowest component amount
				for (int i = 0; i <= queueEnd; i++)
				{
					foreach (var item in blueprintsDictionary)
					{
						if (item.Value == queue[i].BlueprintId)
						{
							double componentAmount = GetGlobalItemAmount(item.Key);

							if (componentAmount < lowestAmount)
							{
								lowestAmount = componentAmount;
								itemIndex = i;
							}
						}
					}
				}

				// Move the ore with the lowest ingot amount to the front
				assembler.MoveQueueItemRequest(queue[itemIndex].ItemId, 0);
			}
		}


		void RemoveFromQueue(string itemToRemove)
		{
			// Prepare the item for an assembler blueprint item
			bool blueprintFound;
			MyDefinitionId blueprint = GetBlueprint(itemToRemove, out blueprintFound);

			// If the blueprint wasn't found, return
			if (!blueprintFound) return;

			foreach (IMyAssembler assembler in assemblers)
			{
				if (assembler.Mode == MyAssemblerMode.Disassembly) continue;

				var queue = new List<MyProductionItem>();
				assembler.GetQueue(queue);

				for (int i = 0; i < queue.Count; i++)
				{
					if (queue[i].BlueprintId == blueprint) assembler.RemoveQueueItem(i, queue[i].Amount);
				}
			}
		}


		void DisassembleInAssembler(string itemToDestroy, double amountToDestroy)
		{
			IMyTerminalBlock sourceContainer = FindItem(itemToDestroy);

			if (sourceContainer == null) return;

			// Set up the assemblers
			foreach (IMyAssembler assembler in assemblers)
			{
				if (assembler.Mode == MyAssemblerMode.Assembly)
				{
					assembler.ClearQueue();
					assembler.Mode = MyAssemblerMode.Disassembly;
					assembler.UseConveyorSystem = false;
				}

				// Reactivate repeating mode, if the assemblers isn't producing anymore
				if (!assembler.IsProducing)
				{
					assembler.Repeating = false;
					assembler.Repeating = true;
				}
			}

			// Calculate the item amounts per assembler
			double itemsPerAssembler = Math.Ceiling(amountToDestroy / assemblers.Count);

			foreach (IMyAssembler assembler in assemblers)
			{
				// Adjust the items per assembler for the last one in the row
				if (itemsPerAssembler > amountToDestroy) itemsPerAssembler = Math.Ceiling(amountToDestroy);

				// If there are items to destroy, move them to the assembler
				if (amountToDestroy > 0)
				{
					MoveItems(itemToDestroy, sourceContainer, 0, assembler, 1, itemsPerAssembler);
					amountToDestroy -= itemsPerAssembler;
				}
				else
				{
					break;
				}
			}
		}


		void BalanceIce()
		{
			// 1. Try to pull new ice from the containers or put it back
			double percentage = iceFillLevelPercentage / 100;
			string iceName = "MyObjectBuilder_Ore/Ice";
			double iceVolume = 0;

			// Check if the overall fill level is too high or too low
			foreach (IMyGasGenerator oxygenGenerator in oxygenGenerators)
			{
				// If no volume for ice has been found so far, get it
				if (iceVolume == 0) iceVolume = GetItemVolume(iceName, oxygenGenerator);

				oxygenGenerator.UseConveyorSystem = false;
				var inventory = oxygenGenerator.GetInventory(0);
				double iceAmount = GetItemAmount(iceName, oxygenGenerator);
				double currentFillLevel = iceAmount * iceVolume;
				double maxFillLevel = (double)inventory.MaxVolume;

				// If the fill level is too high, move ice out
				if (currentFillLevel > maxFillLevel * (percentage + 0.001))
				{
					// Find free cargo container
					IMyTerminalBlock targetContainer = FindFreeContainer(oxygenGenerator);

					// If a container was found, proceed
					if (targetContainer != null)
					{
						double amountToMove = (currentFillLevel - maxFillLevel * percentage) / iceVolume;

						MoveItems(iceName, oxygenGenerator, 0, targetContainer, 0, amountToMove);
					}

					// If the fill level is too low, move ice in
				}
				else if (currentFillLevel < maxFillLevel * (percentage - 0.001))
				{
					// Find a container with ice
					IMyTerminalBlock sourceContainer = FindItem(iceName);

					// If a container was found, proceed
					if (sourceContainer != null)
					{
						double amountToMove = (maxFillLevel * percentage - currentFillLevel) / iceVolume;

						MoveItems(iceName, sourceContainer, 0, oxygenGenerator, 0, amountToMove);
					}
				}

			}

			// 2. Balance the oxygen generators
			double totalIceAmount = 0;
			double totalCargoVolume = 0;

			foreach (var oxygenGenerator in oxygenGenerators)
			{

				// Get their current ice amount and add it to the total amount
				totalIceAmount += GetItemAmount(iceName, oxygenGenerator);

				// Get their max cargo volume and add it to the total cargo volume
				var inventory = oxygenGenerator.GetInventory(0);
				totalCargoVolume += (double)inventory.MaxVolume;
			}

			double averagePercentage = (totalIceAmount * iceVolume) / totalCargoVolume;

			// Cycle through all source blocks and check if a one has more than the average item amount
			foreach (var sourceGenerator in oxygenGenerators)
			{
				// Get the item amount and the inventory volumes
				var sourceInventory = sourceGenerator.GetInventory(0);
				double sourceItemAmount = GetItemAmount(iceName, sourceGenerator);
				double sourceFillLevel = sourceItemAmount * iceVolume;
				double sourceMaxFillLevel = (double)sourceInventory.MaxVolume;

				// If a source block has more than the average item amount, find another one that has less
				if (sourceFillLevel > sourceMaxFillLevel * (averagePercentage + 0.001))
				{
					foreach (var targetGenerator in oxygenGenerators)
					{
						// Don't exchange with yourself
						if (sourceGenerator == targetGenerator) continue;

						// Get the item amount and the inventory volumes
						var targetInventory = targetGenerator.GetInventory(0);
						double targetItemAmount = GetItemAmount(iceName, targetGenerator);
						double targetFillLevel = targetItemAmount * iceVolume;
						double targetMaxFillLevel = (double)targetInventory.MaxVolume;

						// Proceed if a block with less than the average amount is found
						if (targetFillLevel < targetMaxFillLevel * (averagePercentage - 0.001))
						{
							// Calculate the needed items to get the average amount
							double neededItems = ((targetMaxFillLevel * averagePercentage) - targetFillLevel) / iceVolume;

							// If substracting the needed items from sourceItemAmount is still above average, move it and continue with the next block
							if ((sourceItemAmount - neededItems) * iceVolume >= sourceMaxFillLevel * averagePercentage && neededItems > 5)
							{
								sourceItemAmount -= neededItems;

								MoveItems(iceName, sourceGenerator, 0, targetGenerator, 0, neededItems);
								continue;
							}

							// If substracting the needed items from sourceItemAmount puts it below average, move enough to hit average in the source and break the loop
							if ((sourceItemAmount - neededItems) * iceVolume < sourceMaxFillLevel * averagePercentage && neededItems > 5)
							{
								double itemsToMove = (sourceItemAmount * iceVolume - sourceMaxFillLevel * averagePercentage) / iceVolume;

								MoveItems(iceName, sourceGenerator, 0, targetGenerator, 0, itemsToMove);
								break;
							}
						}
					}
				}
			}
		}


		void BalanceUranium()
		{
			// 1. Try to pull new uranium from the containers or put it back
			string uraniumName = "MyObjectBuilder_Ingot/Uranium";
			double totalUraniumAmount = 0;
			double totalDesiredUraniumAmount = 0;

			// Check if the overall fill level is too high or too low
			foreach (IMyReactor reactor in reactors)
			{
				reactor.UseConveyorSystem = false;
				double uraniumAmount = GetItemAmount(uraniumName, reactor);
				double desiredUraniumAmount = uraniumAmountLargeGrid;

				// Determine grid size
				if (reactor.CubeGrid.GridSize == 0.5f) desiredUraniumAmount = uraniumAmountSmallGrid;

				// Sum up the total desired uranium amount for balancing
				totalDesiredUraniumAmount += desiredUraniumAmount;

				// If the amount is too high, move uranium out
				if (uraniumAmount > desiredUraniumAmount + 0.05)
				{
					// Find free cargo container
					IMyTerminalBlock targetContainer = FindFreeContainer(reactor);

					// If a container was found, proceed
					if (targetContainer != null)
					{
						double amountToMove = uraniumAmount - desiredUraniumAmount;

						MoveItems(uraniumName, reactor, 0, targetContainer, 0, amountToMove);
					}

					// If the amount is too low, move uranium in
				}
				else if (uraniumAmount < desiredUraniumAmount - 0.05)
				{
					// Find a container with uranium
					IMyTerminalBlock sourceContainer = FindItem(uraniumName);

					// If a container was found, proceed
					if (sourceContainer != null)
					{
						double amountToMove = desiredUraniumAmount - uraniumAmount;

						MoveItems(uraniumName, sourceContainer, 0, reactor, 0, amountToMove);
					}
				}

				// Sum the total uranium amount in the reactors for balancing
				totalUraniumAmount += GetItemAmount(uraniumName, reactor);
			}

			// 2. Balance the reactors
			double averageUraniumLevel = totalUraniumAmount / totalDesiredUraniumAmount;

			foreach (var sourceReactor in reactors)
			{
				double sourceAmount = GetItemAmount(uraniumName, sourceReactor);
				double averageSourceAmount = averageUraniumLevel * uraniumAmountLargeGrid;

				// Recalculate level if it is a small grid reactor
				if (sourceReactor.CubeGrid.GridSize == 0.5f) averageSourceAmount = averageUraniumLevel * uraniumAmountSmallGrid;

				// If a reactor has more than the average amount, share it
				if (sourceAmount > averageSourceAmount + 0.05)
				{
					// Search the other reactors
					foreach (var targetReactor in reactors)
					{
						// Don't exchange with yourself
						if (sourceReactor == targetReactor) continue;

						// Get the target uranium amount
						double targetAmount = GetItemAmount(uraniumName, targetReactor);
						double averageTargetAmount = averageUraniumLevel * uraniumAmountLargeGrid;

						// Recalculate level if it is a small grid reactor
						if (targetReactor.CubeGrid.GridSize == 0.5f) averageTargetAmount = averageUraniumLevel * uraniumAmountSmallGrid;

						// If the target reactor has less than average, proceed
						if (targetAmount < averageTargetAmount - 0.05)
						{
							// Update source uranium amount
							sourceAmount = GetItemAmount(uraniumName, sourceReactor);

							// Calculate needed uranium
							double neededAmount = averageTargetAmount - targetAmount;

							// If substracting the needed uranium from sourceUraniumAmount is still above average, move it and continue with the next reactor
							if (sourceAmount - neededAmount >= averageSourceAmount)
							{

								MoveItems(uraniumName, sourceReactor, 0, targetReactor, 0, neededAmount);
								continue;
							}

							// If substracting the needed uranium from sourceUraniumAmount puts it below average, move enough to hit average in the source and break the loop
							if (sourceAmount - neededAmount < averageSourceAmount)
							{
								neededAmount = sourceAmount - averageSourceAmount;

								MoveItems(uraniumName, sourceReactor, 0, targetReactor, 0, neededAmount);
								break;
							}
						}
					}
				}
			}
		}


		void CreateInformation()
		{
			workingCounter++;
			switch (workingCounter % 4)
			{
				case 0: workingIndicator = "/"; break;
				case 1: workingIndicator = "-"; break;
				case 2: workingIndicator = "\\"; break;
				case 3: workingIndicator = "|"; break;
			}

			// Terminal / LCD information string
			informationString = "Isy's Inventory Manager " + workingIndicator + "\n=========================\n\n";

			// Add warning message for minor errors
			if (warning != null)
			{
				informationString += "Warning!\n";
				informationString += warning + "\n\n";
			}

			// Container stats
			if (showContainerStats)
			{
				informationString += "Statistics for " + typeContainers.Count + " sorted cargo containers:\n\n";

				// Ore containers
				informationString += CreateContainerString(oreContainers, "Ores");

				// Ingot containers
				informationString += CreateContainerString(ingotContainers, "Ingots");

				// Component containers
				informationString += CreateContainerString(componentContainers, "Components");

				// Tool containers
				informationString += CreateContainerString(toolContainers, "Tools");

				// Ammo containers
				informationString += CreateContainerString(ammoContainers, "Ammo");

				// Bottle containers
				informationString += CreateContainerString(bottleContainers, "Bottles");
			}

			// Managed blocks
			if (showManagedBlocks && (refineries.Count > 0 || assemblers.Count > 0 || oxygenGenerators.Count > 0 || reactors.Count > 0))
			{
				informationString += "Managed blocks by type:\n";

				// Refineries
				if (enableOreBalancing && refineriesList.Count > 0)
				{
					informationString += refineriesList.Count + " Refineries: ";
					if (enableArcPriority)
					{
						informationString += "Arc Priority ON";
					}
					else
					{
						informationString += "Arc Priority OFF";
					}

					if (enableStoneProcessing)
					{
						informationString += " | Refine stone ON\n";
					}
					else
					{
						informationString += " | Refine stone OFF\n";
					}
				}

				// Arc furnaces
				if (enableOreBalancing && arcsList.Count > 0)
				{
					informationString += arcsList.Count + " Arc furnaces: ";
					if (enableArcSpecialization)
					{
						informationString += "Arc Specialization ON\n";
					}
					else
					{
						informationString += "Arc Specialization OFF\n";
					}
				}

				// Assemblers
				if (enableAutocrafting && assemblers.Count > 0)
				{
					informationString += assemblers.Count + " Assemblers: ";
					if (disassembleExcess)
					{
						informationString += "Disassembling ON\n";
					}
					else
					{
						informationString += "Disassembling OFF\n";
					}
				}

				// Oxygen Generators
				if (enableIceBalancing && oxygenGenerators.Count > 0)
				{
					informationString += oxygenGenerators.Count + " Oxygen Generators: set Fill Level -> " + iceFillLevelPercentage + "%\n";
				}

				// Reactors
				if (enableUraniumBalancing && reactors.Count > 0)
				{
					informationString += reactors.Count + " Reactors: set amount -> " + uraniumAmountLargeGrid + " LG / " + uraniumAmountSmallGrid + " SG Uranium\n";
				}
			}

			// Last action
			if (showLastAction && lastAction != "")
			{
				informationString += "\n";
				informationString += "Last Action:\n" + lastAction;
			}
		}


		string CreateContainerString(List<IMyTerminalBlock> containerList, string typeName)
		{
			double volume = 0, volumeMax = 0;

			foreach (var container in containerList)
			{
				var inventory = container.GetInventory(0);
				volume += (double)inventory.CurrentVolume;
				volumeMax += (double)inventory.MaxVolume;
			}

			string heading = containerList.Count + "x " + typeName + ":";
			string current = GetVolumeString(volume);
			string max = GetVolumeString(volumeMax);
			string percent = GetPercentString(volume, volumeMax);

			// First line
			StringBuilder firstLine = new StringBuilder(heading);
			firstLine.Append(StringRepeat(' ', 26 - (firstLine.Length + current.Length)));
			firstLine.Append(current + " / " + max);
			firstLine.Append(StringRepeat(' ', 52 - (firstLine.Length + percent.Length)));
			firstLine.Append(percent + "\n");

			// Second line
			StringBuilder secondLine = new StringBuilder("[" + StringRepeat('.', 50) + "]\n\n");
			int fillLevel = (int)Math.Ceiling(50 * (volume / volumeMax));
			try
			{
				secondLine.Replace(".", "I", 1, fillLevel);
			}
			catch (Exception)
			{
				// ignore
			}

			return firstLine.Append(secondLine).ToString();
		}


		void WriteLCD()
		{
			// Only use this function if there are LCDs
			if (outputLcds.Length > 0)
			{
				var lcds = new List<IMyTextPanel>();

				// Cycle through all the items in regularLcds to find groups or LCDs
				foreach (var item in outputLcds)
				{
					// If the item is a group, get the LCDs and join the list with lcds list
					var lcdGroup = GridTerminalSystem.GetBlockGroupWithName(item);
					if (lcdGroup != null)
					{
						var tempLcds = new List<IMyTextPanel>();
						lcdGroup.GetBlocksOfType<IMyTextPanel>(tempLcds);
						lcds.AddRange(tempLcds);
						// Else try adding a single LCD
					}
					else
					{
						IMyTextPanel regularLcd = GridTerminalSystem.GetBlockWithName(item) as IMyTextPanel;
						if (regularLcd != null)
						{
							lcds.Add(regularLcd);
						}
					}
				}

				// Figure out the amount of lines for scrolling content
				var lines = informationString.TrimEnd('\n').Split('\n');
				string lcdText = "";

				if (lines.Length > 35)
				{
					if (execCounter % 6 == 0)
					{
						if (scrollWait > 0) scrollWait--;
						if (scrollWait <= 0) lineStart += scrollDirection;

						if (lineStart + 32 >= lines.Length && scrollWait <= 0)
						{
							scrollDirection = -1;
							scrollWait = 3;
						}
						if (lineStart <= 3 && scrollWait <= 0)
						{
							scrollDirection = 1;
							scrollWait = 3;
						}
					}
				}
				else
				{
					lineStart = 3;
					scrollDirection = 1;
					scrollWait = 3;
				}

				// Always create header
				for (var line = 0; line < 3; line++)
				{
					lcdText += lines[line] + "\n";
				}

				// Create scrolling content based on the starting line
				for (var line = lineStart; line < lines.Length; line++)
				{
					lcdText += lines[line] + "\n";
				}

				foreach (var lcd in lcds)
				{
					// Print contents to its public text
					lcd.WritePublicTitle("Isy's Inventory Manager");
					lcd.Font = "Monospace";
					lcd.FontSize = 0.5f;
					lcd.WritePublicText(lcdText, false);
					lcd.ShowPublicTextOnScreen();
				}
			}
		}


		void MoveItems(string itemToMove, IMyTerminalBlock sourceContainer, int sourceSlot, IMyTerminalBlock targetContainer, int targetSlot, double amount = -1)
		{
			// If the amount is below 1, return
			if (amount < 1 && amount != -0.5 && amount != -1) return;

			// Get the source container's inventory
			var sourceInventory = sourceContainer.GetInventory(sourceSlot);
			var items = sourceInventory.GetItems();

			// Get the target container's inventory
			var targetInventory = targetContainer.GetInventory(targetSlot);

			// Cycle through every item and to find the wanted one
			for (var i = items.Count - 1; i >= 0; i--)
			{
				// If the item is the wanted one, move the specified amount to the target container
				if (items[i].ToString().Contains(itemToMove))
				{
					// Get the volume before we send items and the item volume
					double volumeBefore = (double)sourceInventory.CurrentVolume;
					string typeId = items[i].Content.TypeId.ToString();
					string subtypeId = items[i].Content.SubtypeName;
					string itemName = typeId + "/" + subtypeId;
					double itemVolume = GetItemVolume(itemName, sourceContainer, sourceSlot);

					// Adjust the amount if it is non stackable
					if (typeId.Contains(Ox) || typeId.Contains(Ga) || typeId.Contains(Pg)) amount = Math.Ceiling(amount);

					// Send the actual wished amount
					if (amount > 0) sourceInventory.TransferItemTo(targetInventory, i, null, true, (VRage.MyFixedPoint)amount);

					// Send half of the item stack
					if (amount == -0.5)
					{
						double halfItemStack = Math.Ceiling((double)items[i].Amount / 2);
						sourceInventory.TransferItemTo(targetInventory, i, null, true, (VRage.MyFixedPoint)halfItemStack);
					}

					// Send the whole item stack
					if (amount == -1) sourceInventory.TransferItemTo(targetInventory, i, null, true);

					// Get the volume after we sent items
					double volumeAfter = (double)sourceInventory.CurrentVolume;

					// Create information
					string typeIdShort = typeId.Replace("MyObjectBuilder_", "");
					double movedItemAmount = (volumeBefore - volumeAfter) / itemVolume;
					string itemString = Math.Round(movedItemAmount, 2) + " " + subtypeId + " " + typeIdShort;

					// If the volume changed, something was sent, so proceed
					if (volumeBefore != volumeAfter)
					{
						lastAction = "Moved: " + itemString + "\nfrom : '" + sourceContainer.CustomName + "'\nto   : '" + targetContainer.CustomName + "'";

						// If an amount was given, substract the moved amount and see, if we need to move more
						if (amount > 0)
						{
							amount -= movedItemAmount;

							// If enough items were moved, return
							if (amount <= 0.01) return;
						}
					}
					else
					{
						string amountStr = Math.Round(amount, 2).ToString();
						if (amount == -1) amountStr = "all";
						itemString = amountStr + " " + subtypeId + " " + typeIdShort;
						lastAction = "Couldn't move: " + itemString + "\nfrom : '" + sourceContainer.CustomName + "'\nto   : '" + targetContainer.CustomName + "'\n";
						lastAction += "Check: Conveyor connection and owner/faction!";
					}
				}
			}
		}


		double GetItemAmount(string itemToCount, IMyTerminalBlock blockToInspect, int inventorySlot = 0)
		{
			// Get the block's inventory
			var items = blockToInspect.GetInventory(inventorySlot).GetItems();
			double itemAmount = 0;

			// Cycle through every item to find the wanted one
			for (var i = items.Count - 1; i >= 0; i--)
			{
				// If the item is the wanted one, return the container name
				if (items[i].ToString().Contains(itemToCount)) itemAmount += (double)items[i].Amount;
			}

			return itemAmount;
		}


		IMyTerminalBlock FindItem(string itemToFind, bool findInSpecial = false, IMyTerminalBlock sourceContainer = null)
		{
			foreach (var container in allSortableInventories)
			{
				// Don't find ice in oxygen generators because this will break ice balancing
				if (itemToFind == "MyObjectBuilder_Ore/Ice" && container.GetType().ToString().Contains("MyGasGenerator")) continue;

				// Get the container's inventory
				var items = container.GetInventory(0).GetItems();

				// Cycle through every item to find the wanted one
				for (var i = items.Count - 1; i >= 0; i--)
				{
					// If the item is the wanted one, return the container name
					if (items[i].ToString().Contains(itemToFind)) return container;
				}
			}

			// Special Container steal
			if (findInSpecial)
			{
				foreach (var container in specialContainers)
				{
					// Don't find an item in a lower or same priority special container
					if (GetPriority(container) >= GetPriority(sourceContainer)) continue;

					// Get the container's inventory
					var items = container.GetInventory(0).GetItems();

					// Cycle through every item to find the wanted one
					for (var i = items.Count - 1; i >= 0; i--)
					{
						// If the item is the wanted one, return the container name
						if (items[i].ToString().Contains(itemToFind)) return container;
					}
				}
			}

			return null;
		}


		IMyTerminalBlock FindItemList(List<String> itemsToFind, List<IMyTerminalBlock> containerList, out string foundItem, string typeID = null)
		{
			if (typeID == null) typeID = Or;

			// Cycle through each item in the list and see, if it is available
			foreach (var itemToFind in itemsToFind)
			{
				string fullID = typeID + itemToFind;

				if (GetGlobalItemAmount(fullID) > 0)
				{
					// Cycle through all containers to find the item
					foreach (var container in containerList)
					{
						var items = container.GetInventory(0).GetItems();

						foreach (var item in items)
						{
							// If the item is found, return the container name
							if (item.ToString().Contains(fullID))
							{
								foundItem = item.Content.TypeId + "/" + item.Content.SubtypeId;
								return container;
							}
						}
					}
				}
				else
				{
					continue;
				}
			}

			foundItem = null;
			return null;
		}


		IMyTerminalBlock FindFreeContainer(IMyTerminalBlock blockOnTheGrid)
		{
			// Get all containers on the block's grid
			List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
			GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers,
				c => c.CubeGrid == blockOnTheGrid.CubeGrid && !c.CustomName.ToLower().Contains(spCo) && !c.CustomName.ToLower().Contains(loCo));

			// If no containers exist on the current grid, search on all grids
			if (cargoContainers.Count == 0)
			{
				GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers,
					c => !c.CustomName.ToLower().Contains(spCo) && !c.CustomName.ToLower().Contains(loCo));
			}

			// If there are still none, give a warning and return null
			if (cargoContainers.Count == 0)
			{
				warning = "'" + blockOnTheGrid.CustomName + "'\nhas no containers on its grid!\nCan't move its items!";
				warningCount++;
				return null;
			}
			else
			{
				IMyTerminalBlock freeContainer = null;

				// Cycle through all the containers to find one that is not full
				foreach (var container in cargoContainers)
				{
					// If a container is the same as the blockOnTheGrid, skip it
					if (container == blockOnTheGrid) continue;

					var inventory = container.GetInventory(0);

					// If a suitable container was found, stop the loop
					if ((float)inventory.CurrentVolume < (float)inventory.MaxVolume * 0.99)
					{
						freeContainer = container;
						break;
					}
				}

				// If no empty target container was found, return null, else return container
				if (freeContainer == null)
				{
					warning = "'" + blockOnTheGrid.CustomName + "'\nhas no empty containers on its grid!\nCan't move its items!";
					warningCount++;
					return null;
				}
				else
				{
					return freeContainer;
				}
			}
		}


		double GetItemVolume(string item, IMyTerminalBlock containerWithItem, int containerSlot = 0)
		{
			// Look up the dictionary if the item already exists
			VRage.MyFixedPoint volume = GetItemVolume(item);

			if (volume > 0) return (double)volume;

			// Get the inventory and items of the container
			var inventory = containerWithItem.GetInventory(containerSlot);
			var items = inventory.GetItems();
			int itemIndex = -1;
			volume = (VRage.MyFixedPoint)0.00037;

			// Cycle through all items to find the index of our item
			for (var i = items.Count - 1; i >= 0; i--)
			{
				// If the item was found, break the loop and save the index
				if (items[i].ToString().Contains(item) && items[i].Amount >= 1)
				{
					itemIndex = i;
					break;
				}
			}

			// If the index is still -1, no valid item(amount) could be found, so find the item in the cargo
			if (itemIndex == -1)
			{
				IMyTerminalBlock otherContainerWithItem = FindItem(item);

				// If it was found elsewhere, check quantity
				if (otherContainerWithItem != null)
				{
					containerWithItem = otherContainerWithItem;
					inventory = containerWithItem.GetInventory(0);
					items = inventory.GetItems();

					// Cycle through all items to find the index of our item
					for (var i = items.Count - 1; i >= 0; i--)
					{
						// If the item was found, break the loop and save the index
						if (items[i].ToString().Contains(item) && items[i].Amount >= 1)
						{
							itemIndex = i;
							break;
						}
					}
				}
			}

			// Find a free container to transfer a test item to
			IMyTerminalBlock targetContainer = FindFreeContainer(containerWithItem);

			// If the item or a free container was not found, return 0
			if (itemIndex == -1 || targetContainer == null) return (double)volume;

			// Get the target inventory and the source volume
			var targetInventory = targetContainer.GetInventory(0);
			VRage.MyFixedPoint volumeBefore = inventory.CurrentVolume;

			// Transfer the test item
			inventory.TransferItemTo(targetInventory, itemIndex, null, false, 1);

			// Get the new volume
			VRage.MyFixedPoint volumeAfter = inventory.CurrentVolume;

			// Transfer the test item back
			inventory.TransferItemFrom(targetInventory, targetInventory.GetItems().Count - 1, null, true, 1);

			// Calculate the volume
			volume = volumeBefore - volumeAfter;

			// Save it in the dictionary and return
			SetItemVolume(item, volume);
			return (double)volume;
		}


		int GetPriority(IMyTerminalBlock block)
		{
			string priorityString = System.Text.RegularExpressions.Regex.Match(block.CustomName, @"[Pp]\d+").Value.ToUpper().Replace("P", "");
			int priority = 0;
			bool success = Int32.TryParse(priorityString, out priority);

			// If no priority was found in the block name, set the priority to negative block ID
			if (!success)
			{
				Int32.TryParse(block.EntityId.ToString().Substring(0, 4), out priority);
				priority = -priority;
			}

			return priority;
		}


		string GetPercentString(double numerator, double denominator)
		{
			string percentage = Math.Round(numerator / denominator * 100, 1) + "%";
			if (denominator == 0)
			{
				return "0%";
			}
			else
			{
				return percentage;
			}
		}


		string StringRepeat(char charToRepeat, int numberOfRepetitions)
		{
			if (numberOfRepetitions <= 0) return "";
			return new string(charToRepeat, numberOfRepetitions);
		}


		string GetVolumeString(double value)
		{
			string unit = "kL";

			if (value < 1)
			{
				value *= 1000;
				unit = "L";
			}
			else if (value >= 1000 && value < 1000000)
			{
				value /= 1000;
				unit = "ML";
			}
			else if (value >= 1000000 && value < 1000000000)
			{
				value /= 1000000;
				unit = "BL";
			}
			else if (value >= 1000000000)
			{
				value /= 1000000000;
				unit = "TL";
			}

			return Math.Round(value, 2) + " " + unit;
		}


		string ReadCustomData(string field)
		{
			CheckCustomData();
			var customData = Me.CustomData.Split('\n');

			int index = 0;
			customDataDictionary.TryGetValue(field, out index);

			return customData[index].Replace(field + "=", "");
		}


		void WriteCustomData(string field, string value = "")
		{
			CheckCustomData();
			var customData = Me.CustomData.Split('\n');

			int index = 0;
			customDataDictionary.TryGetValue(field, out index);

			customData[index] = field + "=" + value;

			string newCustomData = "";
			foreach (var line in customData)
			{
				newCustomData += line + "\n";
			}
			newCustomData = newCustomData.TrimEnd('\n');
			Me.CustomData = newCustomData;
		}


		void CheckCustomData()
		{
			var customData = Me.CustomData.Split('\n');
			if (customData.Length < customDataDictionary.Count)
			{
				Me.CustomData = defaultCustomData;
			}
		}


		void CheckSpecialCustomData(IMyTerminalBlock container)
		{
			List<string> oldData = container.CustomData.TrimEnd('\n').Split('\n').ToList();
			List<string> newData = new List<string> {
		"Define the amounts of items you want to store in this container.",
		"Negative amounts will act as a limiter and will remove any items",
		"above the set value without putting items into the container.",
		"",
		"Example: -100 will remove items when their quantity is above 100",
		""
	};

			foreach (var item in specialContainerItems)
			{
				if (oldData.Exists(i => i.Contains(item)))
				{
					newData.Add(oldData.Find(i => i.Contains(item)).Replace(" ", ""));
				}
				else
				{
					newData.Add(item + "=0");
				}
			}

			container.CustomData = string.Join("\n", newData);
		}


		void SaveItemsToCustomdata(string fullItemId)
		{
			// Attempt to write the modded item to the programmable block
			var customData = Me.CustomData.Split('\n');
			string newCustomData = "";
			bool isItemInData = false;

			foreach (var line in customData)
			{
				newCustomData += line + "\n";

				// If the item is already there, break the loop
				if (line == fullItemId)
				{
					isItemInData = true;
					break;
				}
			}

			// If the item is not in the data yet, write a new data
			if (!isItemInData)
			{
				newCustomData += fullItemId;
				Me.CustomData = newCustomData;
			}
		}


		void LoadSavedItems()
		{
			var customData = Me.CustomData.Split('\n');

			foreach (var line in customData)
			{
				var itemIdParts = line.Split('/');

				// If there are not two resulsts, skip this line
				if (itemIdParts.Length != 2) continue;

				string subtype = itemIdParts[1];

				// Check the line and add its entry to the correct list
				if (line.Contains(Or) && !oreType.Contains(subtype))
				{
					oreType.Add(subtype);

					// Also add ores to the refinery lists (but no ice or stone)
					if (!subtype.Contains("Ice") && !subtype.Contains("Stone"))
					{
						if (!refWhitelist.Contains(subtype)) refWhitelist.Add(subtype);
						if (!refSpecificlist.Contains(subtype)) refSpecificlist.Add(subtype);
					}
				}
				else if (line.Contains(In) && !ingotType.Contains(subtype))
				{
					ingotType.Add(subtype);
				}
				else if (line.Contains(Co) && !componentType.Contains(subtype))
				{
					componentType.Add(subtype);
				}
				else if (line.Contains(Am) && !ammoType.Contains(subtype))
				{
					ammoType.Add(subtype);
				}
				else if (line.Contains(Ox) && !oxygenType.Contains(subtype))
				{
					oxygenType.Add(subtype);
				}
				else if (line.Contains(Ga) && !hydrogenType.Contains(subtype))
				{
					hydrogenType.Add(subtype);
				}
				else if (line.Contains(Pg) && !gunType.Contains(subtype))
				{
					gunType.Add(subtype);
				}
			}
		}


		void saveUnknownBlueprint()
		{
			foreach (var assembler in assemblers)
			{
				var items = assembler.GetInventory(1).GetItems();
				var queue = new List<MyProductionItem>();
				assembler.GetQueue(queue);

				if (items.Count == 1 && queue.Count == 1)
				{
					string itemId = items[0].Content.TypeId + "/" + items[0].Content.SubtypeName;
					bool keyFound = false;
					GetBlueprint(itemId, out keyFound);

					if (keyFound) continue;

					SetBlueprint(itemId, queue[0].BlueprintId);
					Storage += itemId + "=" + queue[0].BlueprintId + ";";
				}
			}
		}


		void saveFoundBlueprint(string fullItemId, MyDefinitionId blueprintId)
		{
			var blueprints = Storage.Split(';');
			bool isInStorage = false;
			string newStorage = "";

			foreach (var line in blueprints)
			{
				var item = line.Split('=');

				if (item[0].Contains(fullItemId))
				{
					newStorage += fullItemId + "=" + blueprintId + ";";
					isInStorage = true;
				}
				else if (line != "")
				{
					newStorage += line + ";";
				}
			}

			if (!isInStorage)
			{
				newStorage += fullItemId + "=" + blueprintId + ";";
			}

			Storage = newStorage;
		}


		string loadSavedBlueprint(string fullItemId)
		{
			var blueprints = Storage.Split(';');

			foreach (var line in blueprints)
			{
				var item = line.Split('=');

				if (item.Length != 2) continue;
				if (item[0].Contains(fullItemId)) return item[1];
			}

			return "";
		}


		//  Dictionaries and methods for dictionary access

		// Custom data dicitonary
		Dictionary<string, int> customDataDictionary = new Dictionary<string, int>()
{
	{ "oreContainer", 0 },
	{ "ingotContainer", 1 },
	{ "componentContainer", 2 },
	{ "toolContainer", 3 },
	{ "ammoContainer", 4 },
	{ "bottleContainer", 5 },
	{ "lockedContainer", 6 },
	{ "specialContainer", 7 }
};

		// Global item amount dictionary
		Dictionary<string, double> globalItemAmountDictionary = new Dictionary<string, double>();

		void SetGlobalItemAmount()
		{
			globalItemAmountDictionary.Clear();

			foreach (var container in allSortableInventories)
			{
				var items = container.GetInventory(0).GetItems();

				// Cycle through every item to find the wanted one
				foreach (var item in items)
				{
					string fullItemId = item.Content.TypeId + "/" + item.Content.SubtypeId;
					if (globalItemAmountDictionary.ContainsKey(fullItemId))
					{
						globalItemAmountDictionary[fullItemId] += (double)item.Amount;
					}
					else
					{
						globalItemAmountDictionary[fullItemId] = (double)item.Amount;
					}
				}
			}
		}

		double GetGlobalItemAmount(string itemToGet)
		{
			double result = 0;
			globalItemAmountDictionary.TryGetValue(itemToGet, out result);
			return result;
		}

		// Global assembler queue dicitonary: string blueprintId => double amount
		Dictionary<MyDefinitionId, double> globalAssemblerQueueDictionary = new Dictionary<MyDefinitionId, double>();

		// Set the global assembler queue: blueprintId => amount
		void SetGlobalAssemblerQueue()
		{
			globalAssemblerQueueDictionary.Clear();

			foreach (IMyAssembler assembler in assemblers)
			{
				var queue = new List<MyProductionItem>();
				assembler.GetQueue(queue);

				foreach (var item in queue)
				{
					MyDefinitionId itemBPId = item.BlueprintId;
					if (globalAssemblerQueueDictionary.ContainsKey(itemBPId))
					{
						globalAssemblerQueueDictionary[itemBPId] += (double)item.Amount;
					}
					else
					{
						globalAssemblerQueueDictionary[itemBPId] = (double)item.Amount;
					}
				}
			}
		}

		// Get the assembler queue of a given item bluprint
		double GetGlobalAssemblerQueue(MyDefinitionId itemBlueprintId)
		{
			double result = 0;
			globalAssemblerQueueDictionary.TryGetValue(itemBlueprintId, out result);
			return result;
		}

		// Blueprint dictionary for assembler queues: string fullItemId => MyDefinitionId blueprintId
		readonly Dictionary<string, MyDefinitionId> blueprintsDictionary = new Dictionary<string, MyDefinitionId>();

		MyDefinitionId GetBlueprint(string fullItemId, out bool keyFound)
		{
			// Try getting the key out of the dictionary
			MyDefinitionId blueprint;
			keyFound = blueprintsDictionary.TryGetValue(fullItemId, out blueprint);

			// If it wasn't found, try setting it
			if (!keyFound)
			{
				string[] variants = { loadSavedBlueprint(fullItemId), "", "Component", "Magazine" };
				bool blueprintCorrect = false;

				for (int i = 0; i < variants.Length; i++)
				{
					string itemBlueprint = variants[i];

					if (i > 0)
					{
						itemBlueprint = Bp + fullItemId.Remove(0, fullItemId.IndexOf("/") + 1).Replace("Item", "") + variants[i];
					}

					MyDefinitionId.TryParse(itemBlueprint, out blueprint);

					try
					{
						blueprintCorrect = assemblers[0].CanUseBlueprint(blueprint);
					}
					catch (Exception)
					{
						// If it didn't work, continue with the next variant
						continue;
					}

					// If the blueprint was correct, add it to the dictionary and save it to storage
					if (blueprintCorrect)
					{
						SetBlueprint(fullItemId, blueprint);
						saveFoundBlueprint(fullItemId, blueprint);
						keyFound = true;
						break;
					}
				}
			}

			return blueprint;
		}

		void SetBlueprint(string fullItemId, MyDefinitionId blueprint)
		{
			blueprintsDictionary[fullItemId] = blueprint;
		}

		// Item volume dictionary (will be dynamically created when moving items)
		Dictionary<string, VRage.MyFixedPoint> itemVolumeDictionary = new Dictionary<string, VRage.MyFixedPoint>();

		VRage.MyFixedPoint GetItemVolume(string item)
		{
			VRage.MyFixedPoint result = 0;
			itemVolumeDictionary.TryGetValue(item, out result);
			return result;
		}

		void SetItemVolume(string item, VRage.MyFixedPoint volume)
		{
			itemVolumeDictionary[item] = volume;
		}

		// Item FullId dictionary
		Dictionary<string, string> itemFullIdDictionary = new Dictionary<string, string>();

		void SetFullItemId(string typeId, List<string> subtypeIdList)
		{
			foreach (var item in subtypeIdList)
			{
				itemFullIdDictionary[item] = typeId + item;
			}
		}

		string GetFullItemId(string subtypeId)
		{
			string fullItemId = Co + subtypeId;
			itemFullIdDictionary.TryGetValue(subtypeId, out fullItemId);
			return fullItemId;
		}

	}
}