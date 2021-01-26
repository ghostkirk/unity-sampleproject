/**
 * Copyright 2021 The Nakama Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Nakama.TinyJson;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Nakama.Examples.Example03Chat
{
	/// <summary>
	/// This example showcases realtime chat with Nakama server.
	///
	/// See <a href="https://heroiclabs.com/docs/social-realtime-chat/">Nakama Docs</a> for more info.
	///
	/// </summary>
	public class Example03Chat : MonoBehaviour
	{
		//  Properties ------------------------------------
		private ExampleButton _SendChatMessageButton { get { return _examplesUI.ExampleButton01; } }
		private ExampleButton _DisconnectButton { get { return _examplesUI.ExampleButton02; } }
		private ExampleButton _ConnectButton { get { return _examplesUI.ExampleButton03; } }


		//  Fields ----------------------------------------
		[SerializeField] private ExamplesUI _examplesUI = null;

		private ExampleSessionClient _exampleSessionClient = null;
		private ISocket _socket = null;
		private ISession _session = null;
		private IClient _client = null;
		private IChannel _channel = null;


		//  Unity Methods   -------------------------------
		protected async void Start()
		{
			_SendChatMessageButton.Button.onClick.AddListener(SendChatMessageButton_OnClicked);
			_DisconnectButton.Button.onClick.AddListener(DisconnectButton_OnClicked);
			_ConnectButton.Button.onClick.AddListener(ConnectButton_OnClicked);

			// Create Client
			_exampleSessionClient = new ExampleSessionClient();
			await _exampleSessionClient.Authenticate();

			// Store Common References
			_client = _exampleSessionClient.Client;
			_session = _exampleSessionClient.Session;

			// Create Socket
			_socket = _client.NewSocket(useMainThread : true);
			_socket.Closed += Socket_OnClosed;
			_socket.Connected += Socket_OnConnected;
			_socket.ReceivedError += Socket_OnReceivedError;
			_socket.ReceivedChannelMessage += Socket_ReceivedChannelMessage;

			RefreshUI();
			ConnectButton_OnClicked();
		}


		//  Other Methods ---------------------------------
		private void RefreshUI()
		{
			// Refresh button interactivity
			bool isConnected = _socket.IsConnected;
			_SendChatMessageButton.Button.interactable = isConnected;
			_DisconnectButton.Button.interactable = isConnected;
			_ConnectButton.Button.interactable = !isConnected;

			// Display text info
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time = {DateTime.Now.ToLocalTime()}");
			if (isConnected)
			{
				stringBuilder.AppendLine($"Session.UserId = {_session.UserId}");
				stringBuilder.AppendLine($"Session.Username = {_session.Username}");
			}

			stringBuilder.AppendLine($"Socket.IsConnected = {isConnected}");
			SetBodyText(stringBuilder.ToString());
		}


		private void SetBodyText(string message)
		{
			if (_examplesUI != null)
			{
				_examplesUI.BodyText.text = message;
			}

			Debug.Log(message);
		}


		//  Event Handlers --------------------------------
		private async void ConnectButton_OnClicked()
		{
			// Empty UI
			RefreshUI();

			// Connect Socket
			await _socket.ConnectAsync(_session);

			// Join Chat
			string chatRoomName = "My Chat Topic";
			bool isPersistent = true;
			bool isHidden = false;
			_channel = await _socket.JoinChatAsync(chatRoomName, ChannelType.Room, isPersistent, isHidden);

			// Populate UI
			RefreshUI();
		}


		private async void SendChatMessageButton_OnClicked()
		{
			SetBodyText($"SendChatMessageButton_OnClicked()");

			var chatMessageJson = new Dictionary<string, string> { { "hello", "world" } }.ToJson();
			await _socket.WriteChatMessageAsync(_channel.Id, chatMessageJson);
		}


		private async void DisconnectButton_OnClicked()
		{
			SetBodyText($"DisconnectButton_OnClicked()");

			// Empty UI
			RefreshUI();

			await _socket.LeaveChatAsync(_channel);

			await _socket.CloseAsync();

			// Populate UI
			RefreshUI();
		}

		private void Socket_ReceivedChannelMessage(IApiChannelMessage obj)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time: {DateTime.Now.ToLocalTime()}");
			stringBuilder.AppendLine($"Socket_ReceivedChannelMessage()");
			stringBuilder.AppendLine($"\tJson = {obj.Content}");
			SetBodyText(stringBuilder.ToString());
		}

		private void Socket_OnReceivedError(Exception exception)
		{
			SetBodyText($"Socket_OnReceivedError() exception={exception}.");
			RefreshUI();
		}


		private void Socket_OnConnected()
		{
			SetBodyText($"Socket_OnConnected()");
			RefreshUI();
		}


		private void Socket_OnClosed()
		{
			SetBodyText($"Socket_OnClosed()");
			RefreshUI();
		}
	}
}
