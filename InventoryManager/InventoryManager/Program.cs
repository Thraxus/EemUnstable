using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using System.Runtime.CompilerServices;
using Sandbox.Game.GameSystems;
using SharpDX.Toolkit.Graphics;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage.Game.Entity;
using VRageMath;

namespace IngameScript
{
	internal partial class Program : MyGridProgram
	{
		/// Rules: 
		///	Bottles will be stored in Oxy Generators by default with ice if available
		/// Ice will be balanced in all Oxy Generators by default with preference given to the bottle storage
		/// Uranium in reactors will be managed by this script
		/// Pulling inventory out of non-standard cargo containers (such as welders, mods that use welders as a base, or cockpits) may be added later
		/// Preference for storage will be given to large cargo's over small
		/// Assemblers that are not working will have their inventory emptied
		///		This includes both components and raw materials
		/// No crafting in this version; inventory management only
		/// No refinery management in this version
		///			

		private const string OreContainer = "Ores";
		private const string IngotContainer = "Ingots";
		private const string ComponentContainer = "Components";
		private const string ToolContainer = "Tools";
		private const string AmmoContainer = "Ammo";
		private const string BottleContainer = "Bottles";
		private const string IgnoreContainer = "Ignore";
		private const string CustomConatiner = "Custom";
		private const double LargeReactorUranium = 100;
		private const double SmallReactorUranium = 50;
		private const double SmallGridUraniumMultiplier = 0.1;


		private readonly List<IMyCargoContainer> _largeCargos = new List<IMyCargoContainer>();
		private readonly List<IMyCargoContainer> _smallCargos = new List<IMyCargoContainer>();
		private readonly List<IMyCargoContainer> _usedCargos = new List<IMyCargoContainer>();
		private readonly List<IMyTerminalBlock> _allInventoryTypes = new List<IMyTerminalBlock>();
		private List<IMyGasGenerator> _gasgenerators = new List<IMyGasGenerator>();
		private List<IMyAssembler> _assemblers = new List<IMyAssembler>();
		private List<IMyReactor> _reactors = new List<IMyReactor>();

		private const string LargeCargoName = "InvManager Lg Cargo";
		private const string MediumCargoName = "InvManager Md Cargo";
		private const string SmallCargoName = "InvManager Sm Cargo";

		private int priority = 0;
		private int minContainers = 5;

		// This file contains your actual script.
		//
		// You can either keep all your code here, or you can create separate
		// code files to make your program easier to navigate while coding.
		//
		// In order to add a new utility class, right-click on your project, 
		// select 'New' then 'Add Item...'. Now find the 'Space Engineers'
		// category under 'Visual C# Items' on the left hand side, and select
		// 'Utility Class' in the main area. Name it in the box below, and
		// press OK. This utility class will be merged in with your code when
		// deploying your final script.
		//
		// You can also simply create a new utility class manually, you don't
		// have to use the template if you don't want to. Just do so the first
		// time to see what a utility class looks like.
		public void Main(string argument, UpdateType updateSource)
		{
			Echo("Pass: " + priority++);



			GetAllInventories();
			HandleLargeCargos();
			

			//foreach (IMyCargoContainer cargo in _largeCargos)
			//{
			//	cargo.CustomName = cargo.CustomName + "Lg " + priority++;
			//}


		}

		private enum CargoType
		{
			Ore = 1,		// OreContainer,
			Ingot = 2,		// IngotContainer,
			Component = 3,	// ComponentContainer
			Tool = 4,		// ToolContainer
			Ammo = 5		// AmmoContainer
		}

		public string GetCargoType(Enum type) => 
			Equals(type, CargoType.Ore) ? OreContainer : 
				(Equals(type, CargoType.Ingot) ? IngotContainer :
					(Equals(type, CargoType.Component) ? ComponentContainer :
						(Equals(type, CargoType.Tool) ? ToolContainer :
							(Equals(type, CargoType.Ammo) ? AmmoContainer : "Unknown"))));

		public void HandleLargeCargos()
		{
			Echo("Looking for Cargo Type: " + GetCargoType(CargoType.Component));
			GridTerminalSystem.GetBlocksOfType(_largeCargos, container =>
				container.DefinitionDisplayNameText.Contains("Large") && container.CustomName.Contains(GetCargoType(CargoType.Component)));
			if (_largeCargos.Count == 0) Echo("No Cargo's for that type were found");
			else
			{
				Echo(_largeCargos.Count + " large cargo containers found");
			}
		}

		public void MoveComponents(List<IMyCargoContainer> _cargos)
		{

		}

		public void GetAllInventories()
		{ //_allInventoryTypes
			const string co = "MyObjectBuilder_Component/";

			GridTerminalSystem.GetBlocksOfType(_allInventoryTypes, block => block.HasInventory
			                                                                && block.IsWorking
			                                                                && !block.CustomName.Contains(
				                                                                GetCargoType(CargoType.Component)));
																			//);
																			// && block.GetInventory().GetItems()[0].Content
																			// .SubtypeName.Contains("SteelPlate"));
			// && block.CheckConnectionAllowed



			/// Figure this nonsense out - GetItems will throw an index out of range if it's empty!?@
			foreach (IMyTerminalBlock block in _allInventoryTypes)
			{
				if(block.GetInventory().GetItems().Exists())
			}

			if (_allInventoryTypes.Count > 0)
			{
				foreach (IMyTerminalBlock block in _allInventoryTypes)
					Echo("This inventory contains components: " + block.DefinitionDisplayNameText);
			}
			else Echo("No inventories on this grid contain components to sort.");

		}

		public Program()
		{
			// Set UpdateFrequency for starting the programmable block over and over again
			Runtime.UpdateFrequency = UpdateFrequency.Update100;
		}

		public void Save()
		{
			// Called when the program needs to save its state. Use
			// this method to save your state to the Storage field
			// or some other means. 
			// 
			// This method is optional and can be removed if not
			// needed.
		}


	}
}