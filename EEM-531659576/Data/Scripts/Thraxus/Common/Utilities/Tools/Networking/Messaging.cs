﻿using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Common.Utilities.Tools.Networking
{
	public static class Messaging
	{
		private static List<IMyPlayer> _playerCache = new List<IMyPlayer>();

		public static void Register()
		{
			MyAPIGateway.Multiplayer.RegisterMessageHandler(Settings.GeneralSettings.NetworkId, MessageHandler);
			MyAPIGateway.Utilities.MessageEntered += ChatMessageHandler;
		}

		public static void UnRegister()
		{
			MyAPIGateway.Multiplayer.UnregisterMessageHandler(Settings.GeneralSettings.NetworkId, MessageHandler);
			lock (_playerCache)
			{
				_playerCache = null;
			}
			MyAPIGateway.Utilities.MessageEntered -= ChatMessageHandler;
		}

		private static void MessageHandler(byte[] bytes)
		{
			MessageBase m = MyAPIGateway.Utilities.SerializeFromBinary<MessageBase>(bytes);
			if (Settings.GeneralSettings.IsServer)
				m.HandleServer();
			else
				m.HandleClient();
		}

		// ReSharper disable once RedundantAssignment
		private static void ChatMessageHandler(string message, ref bool sendToOthers)
		{
			if (!message.ToLower().StartsWith(ChatHandler.ChatCommandPrefix.ToLower()))
			{
				sendToOthers = true;
				return;
			}
			sendToOthers = false;
			ChatHandler.HandleChatMessage(message);
		}

		public static void SendMessageTo(ulong steamId, MessageBase message, bool reliable = true)
		{
			byte[] d = MyAPIGateway.Utilities.SerializeToBinary(message);
			if (!reliable && d.Length >= 1000)
				throw new Exception($"Attempting to send unreliable message beyond message size limits! Message type: {message.GetType()} Content: {string.Join(" ", d)}");
			MyAPIGateway.Multiplayer.SendMessageTo(Settings.GeneralSettings.NetworkId, d, steamId, reliable);
		}

		public static void SendMessageToServer(MessageBase message, bool reliable = true)
		{
			byte[] d = MyAPIGateway.Utilities.SerializeToBinary(message);
			if (!reliable && d.Length >= 1000)
				throw new Exception($"Attempting to send unreliable message beyond message size limits! Message type: {message.GetType()} Content: {string.Join(" ", d)}");
			MyAPIGateway.Multiplayer.SendMessageToServer(Settings.GeneralSettings.NetworkId, d, reliable);
		}

		public static void SendMessageToClients(MessageBase message, bool reliable = true, params ulong[] ignore)
		{
			byte[] d = MyAPIGateway.Utilities.SerializeToBinary(message);
			if (!reliable && d.Length >= 1000)
				throw new Exception($"Attempting to send unreliable message beyond message size limits! Message type: {message.GetType()} Content: {string.Join(" ", d)}");

			lock (_playerCache)
			{
				MyAPIGateway.Players.GetPlayers(_playerCache);
				foreach (IMyPlayer player in _playerCache)
				{
					ulong steamId = player.SteamUserId;
					if (ignore?.Contains(steamId) == true)
						continue;
					MyAPIGateway.Multiplayer.SendMessageTo(Settings.GeneralSettings.NetworkId, d, steamId, reliable);
				}
				_playerCache.Clear();
			}
		}

		/// <summary>
		/// Sends a message to a specific player
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <param name="duration">Optional. How long to display the message for</param>
		/// <param name="color">Optional.  Color of the sender's name in chat - remember to check it against MyFontEnum else, errors</param>
		public static void ShowLocalNotification(string message, int duration = Settings.GeneralSettings.DefaultLocalMessageDisplayTime, string color = MyFontEnum.Green)
		{
			MyVisualScriptLogicProvider.ShowNotification(message, duration, color);
		}

		/// <summary>
		/// Sends a message to the entire server
		/// </summary>
		/// <param name="message">Message to send</param>
		/// <param name="duration">Optional. How long to display the message for</param>
		/// <param name="color">Optional. Color to send the message in</param>
		public static void SendMessageToServer(string message, int duration = Settings.GeneralSettings.DefaultServerMessageDisplayTime, string color = MyFontEnum.Red)
		{
			MyVisualScriptLogicProvider.ShowNotificationToAll(message, duration, color);
		}

		/// <summary>
		/// Sends a message to a specific player
		/// </summary>
		/// <param name="message">The message to send</param>
		/// <param name="sender">Who is sending the message</param>
		/// <param name="recipient">Player to receive the message</param>
		/// <param name="color">Optional.  Color of the sender's name in chat</param>
		public static void SendMessageToPlayer(string message, string sender, long recipient, string color = MyFontEnum.Blue)
		{
			//if(MyAPIGateway.Multiplayer.Players.GetPlayerById(recipient).Character.IsPlayer)
			MyVisualScriptLogicProvider.SendChatMessage(message, sender, recipient, color);
		}
	}
}
