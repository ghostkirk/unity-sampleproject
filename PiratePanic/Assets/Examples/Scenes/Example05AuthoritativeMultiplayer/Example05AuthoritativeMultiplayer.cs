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
		private ExampleButton _SendMatchStateButton { get { return _examplesUI.ExampleButton01; } }
		private ExampleButton _DisconnectButton { get { return _examplesUI.ExampleButton02; } }
		private ExampleButton _ConnectButton { get { return _examplesUI.ExampleButton03; } }


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
			_SendMatchStateButton.Button.onClick.AddListener(SendMatchStateButton_OnClicked);
			_DisconnectButton.Button.onClick.AddListener(DisconnectButton_OnClicked);
			_ConnectButton.Button.onClick.AddListener(ConnectButton_OnClicked);

			//  -------------------------------------------
			//  NOTE: Create Client
			//  -------------------------------------------
			_exampleSessionClient = new ExampleSessionClient();
			await _exampleSessionClient.Authenticate();

			// Store Common References
			_client = _exampleSessionClient.Client;
			_session = _exampleSessionClient.Session;

			//  -------------------------------------------
			//  NOTE: Create The Socket
			//
			//	useMainThread	
			//		* If false, callbacks execute faster.
			//		* If true, callbacks execute safely. 
			//
			//	Some built-in Unity systems operate only on 
			//		the "main thread" of code execution of 
			//		Unity (e.g. UnityEngine.UI). 
			//  -------------------------------------------
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
			_SendMatchStateButton.Button.interactable = isConnected;
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
				//  -------------------------------------------
				//  NOTE: Covert the message object to Json
				//  -------------------------------------------
				string json = JsonWriter.ToJson(message);

				//  -------------------------------------------
				//  NOTE: Send object to all players in match
				//  -------------------------------------------
				_socket.SendMatchStateAsync(_match.Id, (long)opCode, json, _match.Presences);

				Debug.Log($"SendMatchStateAsync() _matchId = {_match.Id}, (long)opCode = {(long)opCode}, json = {json}");

			}
			catch (Exception e)
			{
				Debug.LogError($"Error while sending match state. message = '{e.Message}'.");
			}
		}

		private void ProcessReceivedMatchState(IMatchState matchState)
		{
			Debug.Log($"Socket_OnReceivedMatchState() UserId = {matchState.UserPresence.UserId}");

			//  -------------------------------------------
			//  NOTE: Covert the message object to Json
			//  -------------------------------------------
			string messageJson = Encoding.UTF8.GetString(matchState.State);

			// Display text info
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time = {DateTime.Now.ToLocalTime()}");
			stringBuilder.AppendLine($"Socket_OnReceivedMatchState()");
			stringBuilder.AppendLine($"\tJson = {messageJson}");

			//  -------------------------------------------
			//  NOTE: Depending on the OpCode...
			//  -------------------------------------------
			switch ((MyMatchStateType)matchState.OpCode)
			{
				case MyMatchStateType.MyPlayerMoveMatchStateType:

					//  -------------------------------------------
					//  NOTE: ...Covert the message Json to C#
					//  -------------------------------------------
					MyPlayerMoveMatchState myPlayerMoveMessage = JsonParser.FromJson<MyPlayerMoveMatchState>(messageJson);
					stringBuilder.AppendLine($"\tParsed C# = {myPlayerMoveMessage}");
					break;
			}

			SetBodyText(stringBuilder.ToString());
		}


		//  Event Handlers --------------------------------
		private async void ConnectButton_OnClicked()
		{
			// Empty UI
			RefreshUI();

			//  -------------------------------------------
			//  NOTE: Connect Socket
			//  -------------------------------------------
			await _socket.ConnectAsync(_session);

			//  -------------------------------------------
			//  NOTE: Create Match
			//  -------------------------------------------
			IMatch createMatchAsyncMatch = await _socket.CreateMatchAsync();

			//  -------------------------------------------
			//  NOTE: Join Match without a full matchmaking 
			//		  process
			//  -------------------------------------------
			_match = await _socket.JoinMatchAsync(createMatchAsyncMatch.Id);

			// Populate UI
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

			// Empty UI
			RefreshUI();

			await _socket.CloseAsync();

			// Populate UI
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
