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
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nakama.Examples.Example05AuthoritativeMultiplayer
{
	/// <summary>
	/// Used to easily read/write op code
	/// </summary>
	public enum MyMatchStateType
	{
		MyPlayerMoveMatchStateType
	}

	public class MyPlayerMoveMatchState
	{
		public int X, Y, Z;

		public MyPlayerMoveMatchState(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public override string ToString()
		{
			return $"[ MyPlayerMoveMatchState (X={X}, Y={Y}, Z={Z}) ]";
		}

	}

	/// <summary>
	/// This example showcases realtime multiplayer with Nakama server.
	///
	/// See <a href="https://heroiclabs.com/docs/gameplay-multiplayer-server-multiplayer/">Nakama Docs</a> for more info.
	///
	/// </summary>
	public class Example05AuthoritativeMultiplayer : MonoBehaviour
	{
		//  Properties ------------------------------------
		private ExampleButton SendMatchStateButton { get { return _examplesUI.ExampleButton01; } }
		private ExampleButton DisconnectButton { get { return _examplesUI.ExampleButton02; } }
		private ExampleButton ConnectButton { get { return _examplesUI.ExampleButton03; } }


		//  Fields ----------------------------------------
		[SerializeField] private ExamplesUI _examplesUI = null;

		private ExampleSessionClient _exampleSessionClient = null;
		private IMatch _match = null;
		private ISocket _socket = null;
		private ISession _session = null;
		private IClient _client = null;


		//  Unity Methods   -------------------------------
		protected async void Start()
		{
			SendMatchStateButton.Button.onClick.AddListener(SendMatchStateButton_OnClicked);
			DisconnectButton.Button.onClick.AddListener(DisconnectButton_OnClicked);
			ConnectButton.Button.onClick.AddListener(ConnectButton_OnClicked);

			// Create Client
			_exampleSessionClient = new ExampleSessionClient();
			await _exampleSessionClient.Authenticate();

			// Store Common References
			_client = _exampleSessionClient.Client;
			_session = _exampleSessionClient.Session;

			// Create Socket
			_socket = _client.NewSocket(useMainThread:true);
			_socket.Closed += Socket_OnClosed;
			_socket.Connected += Socket_OnConnected;
			_socket.ReceivedError += Socket_OnReceivedError;
			_socket.ReceivedMatchState += Socket_OnReceivedMatchState;

			RefreshUI();
			ConnectButton_OnClicked();

		}


		//  Other Methods ---------------------------------
		private void RefreshUI()
		{
			// Refresh button interactivity
			bool isConnected = _socket.IsConnected;
			SendMatchStateButton.Button.interactable = isConnected;
			DisconnectButton.Button.interactable = isConnected;
			ConnectButton.Button.interactable = !isConnected;

			// Display text info
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time = {DateTime.Now.ToLocalTime()}");
			if (isConnected)
			{
				stringBuilder.AppendLine($"Session.UserId = {_session.UserId}");
				stringBuilder.AppendLine($"Session.Username = {_session.Username}");
			}

			stringBuilder.AppendLine($"Socket.IsConnected = {isConnected}");

			if (_match != null)
			{
				stringBuilder.AppendLine($"Match.Id = {_match.Id}");
				stringBuilder.AppendLine($"Match.Authoritative = {_match.Authoritative}");
				stringBuilder.AppendLine($"Match.Presences.Count = {_match.Presences.Count()}");
			}
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


		public void SendMatchStateMessage(MyMatchStateType opCode, MyPlayerMoveMatchState message)
		{
			try
			{
				// Packing MatchMessage object to json
				string json = JsonWriter.ToJson(message);

				// Sending match state json along with opCode needed for unpacking message to server.
				// Then server sends it to other players
				Debug.Log($"SendMatchStateAsync() _matchId = {_match.Id}, (long)opCode = {(long)opCode}, json = {json}");

				_socket.SendMatchStateAsync(_match.Id, (long)opCode, json, _match.Presences);
			}
			catch (Exception e)
			{
				Debug.LogError($"Error while sending match state. message = '{e.Message}'.");
			}
		}

		private void ProcessReceivedMatchState(IMatchState matchState)
		{
			Debug.Log($"Socket_OnReceivedMatchState() UserId = {matchState.UserPresence.UserId}");

			string messageJson = Encoding.UTF8.GetString(matchState.State);

			// Display text info
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time = {DateTime.Now.ToLocalTime()}");
			stringBuilder.AppendLine($"Socket_OnReceivedMatchState()");
			stringBuilder.AppendLine($"\tJson = {messageJson}");

			// Choosing which event should be invoked basing on opCode, then
			// parsing json to MatchMessage class and firing event
			switch ((MyMatchStateType)matchState.OpCode)
			{
				case MyMatchStateType.MyPlayerMoveMatchStateType:
					MyPlayerMoveMatchState myPlayerMoveMessage = JsonParser.FromJson<MyPlayerMoveMatchState>(messageJson);
					stringBuilder.AppendLine($"\tParsed C# = {myPlayerMoveMessage}");
					break;
			}

			SetBodyText(stringBuilder.ToString());
		}

		//  Event Handlers --------------------------------
		private async void ConnectButton_OnClicked()
		{
			RefreshUI();

			// Connect Socket
			await _socket.ConnectAsync(_session);

			// Create Match
			IMatch createMatchAsyncMatch = await _socket.CreateMatchAsync();

			// Join Match without a full matchmaking process
			_match = await _socket.JoinMatchAsync(createMatchAsyncMatch.Id);

			RefreshUI();
		}


		private void SendMatchStateButton_OnClicked()
		{
			SetBodyText($"SendMoveButton_OnClicked()");

			// Assume the user moves to a new world position of Vector (1,2,3)
			SendMatchStateMessage(MyMatchStateType.MyPlayerMoveMatchStateType,
				new MyPlayerMoveMatchState(1, 2, 3));
		}


		private async void DisconnectButton_OnClicked()
		{
			SetBodyText($"DisconnectButton_OnClicked()");

			RefreshUI();
			await _socket.CloseAsync();
			RefreshUI();
		}


		private void Socket_OnReceivedMatchState(IMatchState matchState)
		{
			ProcessReceivedMatchState(matchState);
		}

		private void Socket_OnReceivedError(Exception exception)
		{
			SetBodyText($"Socket_OnReceivedError() exception = {exception}");
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
