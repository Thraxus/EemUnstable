using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Extensions;
using Eem.Thraxus.Helpers;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

//using Sandbox.Game;

namespace Eem.Thraxus
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_RemoteControl), false)]
	public class BotAi : MyGameLogicComponent
	{
		private IMyRemoteControl Rc { get; set; }

		public IMyCubeGrid Grid { get; private set; }

		private BotBase Ai { get; set; }

		private bool IsOperable { get; set; }

		private bool _inited;

		// Entry point into the game

		// The grid component's updating is governed by AI.Update, and the functions are called as flagged in update.

		private bool WelcomeMessageDisplayed { get; set; } = true;  // Remove = true for debug

		public override void UpdateOnceBeforeFrame()
		{
			if (!_inited) InitAi();
		}

		public override void UpdateBeforeSimulation10()
		{
			if (!CanOperate) return;

			if (!WelcomeMessageDisplayed)
			{
				//Messaging.ShowLocalNotification($"I have arrived: {Ai.DroneName}");
				WelcomeMessageDisplayed = true;
			}
			Run();
		}

		// These two are disabled for now, don't think they are necessary
		//public override void UpdateBeforeSimulation100() { if (CanOperate) Run(); }
		//public override void UpdateBeforeSimulation() { if (CanOperate) Run(); }

		public bool CanOperate
		{
			get
			{
				try
				{
					if (Ai != null) return IsOperable && Grid.InScene && Ai.Operable;
					return false;
				}
				catch (Exception scrap)
				{
					LogError("CanOperate", scrap);
					return false;
				}
			}
		}


		/// <summary>
		/// Provides a simple way to recompile all PBs on the grid, with given delay.
		/// </summary>
		private readonly System.Timers.Timer _recompileDelay = new System.Timers.Timer(500);
		
		public override void Init(MyObjectBuilder_EntityBase objectBuilder)
		{
			base.Init(objectBuilder);
			Rc = Entity as IMyRemoteControl;
			Grid = Rc?.CubeGrid.GetTopMostParent() as IMyCubeGrid;
			if (Rc != null) 
				NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
		}
		
		public void InitAi()
		{
			//if (!MyAPIGateway.Multiplayer.IsServer) return;
			if (!Constants.IsServer) return;
			if (Constants.DisableAi)
			{
				IsOperable = true;
				_inited = true;
				DebugWrite("GridComponent.InitAI", "Type:None, shutting down.");
				Shutdown(notify: false);
				return;
			}
			
			//SetupRecompileTimer();
			try
			{
				if (Grid.Physics == null || ((MyEntity) Grid).IsPreview) return;
			}
			catch (Exception scrap)
			{
				LogError("InitAI[grid check]", scrap);
			}

			try
			{
				if (string.IsNullOrWhiteSpace(Rc.CustomData) || !Rc.CustomData.Contains("[EEM_AI]"))
				{
					Shutdown(notify: false);
					return;
				}

				if (!Rc.IsOwnedByNpc())
				{
					DebugWrite("GridComponent.InitAI", "RC is not owned by NPC!");
					Shutdown(notify: false);
					return;
				}

				if (Rc.CustomData.Contains("Faction:"))
				{
					try
					{
						TryAssignToFaction();
					}
					catch(Exception scrap)
					{
						LogError("TryAssignToFaction", scrap);
					}
				}

				if (Rc.CustomData.Contains("Type:None"))
				{
					IsOperable = true;
					_inited = true;
					DebugWrite("GridComponent.InitAI", "Type:None, shutting down.");
					Shutdown(notify: false);
					return;
				}

				//BotTypeBase botType = BotBase.ReadBotType(Rc);
				//switch (botType)
				//{
				//	case BotTypeBase.None:
				//		DebugWrite("GridComponent.InitAI", "Skipping grid — no setup found");
				//		break;
				//	case BotTypeBase.Invalid:
				//		LogError("GridComponent.InitAI", new Exception("Bot type is not valid!", new Exception()));
				//		break;
				//	case BotTypeBase.Station:
				//		break;
				//	case BotTypeBase.Fighter:
				//		break;
				//	case BotTypeBase.Freighter:
				//		break;
				//	case BotTypeBase.Carrier:
				//		break;
				//}

				//DebugWrite("GridComponent.InitAI", $"Bot found. Bot type: {BotType.ToString()}");
			}
			catch (Exception scrap)
			{
				LogError("GridComponent.InitAI", scrap);
				return;
			}

			try
			{
				Ai = BotFabric.FabricateBot(Grid, Rc);

				if (Ai == null)
				{
					DebugWrite("GridComponent.InitAI", "Bot Fabricator yielded null");
					Shutdown();
					return;
				}

				bool init = Ai.Init(Rc);

				if (init)
				{
					//DebugWrite("GridComponent.InitAI", "AI.Init() successfully initialized AI component");
				}
				else
				{
					DebugWrite("GridComponent.InitAI", "AI.Init() returned false — bot initialization failed somewhy");
					Shutdown();
					return;
				}
				if (Ai.Update != default(MyEntityUpdateEnum)) NeedsUpdate |= Ai.Update;
				Rc.OnMarkForClose += (trash) => { Shutdown(); };
				IsOperable = true;
				_inited = true;
			}
			catch (Exception scrap)
			{
				LogError("GridComponent.InitAI", scrap);
				Shutdown();
			}
		}
		
		private void TryAssignToFaction()
		{
			try
			{
				//if (!MyAPIGateway.Multiplayer.IsServer) return;
				if (!Constants.IsServer) return;
				if (string.IsNullOrWhiteSpace(Rc.CustomData)) return;

				string customData = Rc.CustomData.Replace("\r\n", "\n");
				if (!Rc.CustomData.Contains("Faction:")) return;

				List<string> split = customData.Split('\n').Where(x => x.Contains("Faction:")).ToList();
				if (!split.Any()) return;
				string factionLine = split[0].Trim();
				string[] lineSplit = factionLine.Split(':');
				if (lineSplit.Length != 2)
				{
					Grid.LogError("TryAssignToFaction", new Exception("Cannot assign to faction", new Exception($"Line '{factionLine}' cannot be parsed.")));
					return;
				}
				string factionTag = lineSplit[1].Trim();

				if (factionTag == "Nobody")
				{
					Grid.ChangeOwnershipSmart(0, MyOwnershipShareModeEnum.All);
				}
				else
				{
					IMyFaction faction = MyAPIGateway.Session.Factions.Factions.Values.FirstOrDefault(x => x.Tag == factionTag);

					if (faction == null)
					{
						Grid.LogError("TryAssignToFaction", new Exception($"Faction with tag '{factionTag}' was not found!"));
						return;
					}

					try
					{
						Grid.ChangeOwnershipSmart(faction.FounderId, MyOwnershipShareModeEnum.None);
					}
					catch (Exception scrap)
					{
						LogError("TryAssignToFaction.ChangeGridOwnership", scrap);
					}
				}
			}
			catch (Exception scrap)
			{
				LogError("TryAssignToFaction.ParseCustomData", scrap);
			}
		}

		public void DebugWrite(string source, string message, string debugPrefix = "RemoteComponent.")
		{
			Grid.DebugWrite(debugPrefix + source, message);
		}

		public void LogError(string source, Exception scrap, string debugPrefix = "RemoteComponent.")
		{
			Grid.LogError(debugPrefix + source, scrap);
		}

		private void Run()
		{
			try
			{
				if (CanOperate)
					Ai.Main();
				else
					Shutdown();
			}
			catch (Exception scrap)
			{
				LogError("Run|AI.Main()", scrap);
			}
		}

		private void Shutdown(bool notify = true)
		{
			IsOperable = false;
			try
			{
				if (Ai != null && Ai.IsInitialized) Ai.Shutdown();
				//(Grid as MyCubeGrid).Editable = true;
			}
			catch (Exception scrap)
			{
				LogError("Shutdown", scrap);
			}
			if (notify) DebugWrite("Shutdown", "RC component shut down.");
		}

		//private MyObjectBuilder_EntityBase _builder { get; set; }

		//public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
		//{
		//	return copy ? _builder.Clone() as MyObjectBuilder_EntityBase : _builder;
		//}
	}
}