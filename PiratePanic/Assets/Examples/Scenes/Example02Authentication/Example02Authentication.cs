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

using System;
using System.Text;
using UnityEngine;

namespace Nakama.Examples.Example02Authentication
{
	/// <summary>
	/// This example showcases user authentication with Nakama server.
	///
	/// See <a href="https://heroiclabs.com/docs/unity-client-guide/#authenticate">Nakama Docs</a> for more info.
	/// 
	///  -------------------------------------------
	///  NOTE: To Change The Session Expiration...
	///		* Open "./ServerModules/local.yml" 
	///		* Set "token_expiry_sec" to new value
	///		* Stop Nakama server
	///		* Start Nakama server
	///		* Rerun this example scene
	///  -------------------------------------------
	/// 
	/// </summary>
	public class Example02Authentication : MonoBehaviour
	{
		//  Properties ------------------------------------
		private ExampleButton _EndSessionButton { get { return _examplesUI.ExampleButton01; } }
		private ExampleButton _ReauthenticateButton { get { return _examplesUI.ExampleButton02; } }
		private ExampleButton _AuthenticateButton { get { return _examplesUI.ExampleButton03; } }


		//  Fields ----------------------------------------
		[SerializeField] private ExamplesUI _examplesUI = null;

		private const string _DeviceIdKey = "MyDeviceIdKey";
		private const string _AuthTokenKey = "MyAuthTokenKey";
		private ISession _session = null;
		private IClient _client = null;


		//  Unity Methods   -------------------------------
		protected void Start()
		{
			_EndSessionButton.Button.onClick.AddListener(EndSessionButton_OnClicked);
			_ReauthenticateButton.Button.onClick.AddListener(ReauthenticateButton_OnClicked);
			_AuthenticateButton.Button.onClick.AddListener(AuthenticateButton_OnClicked);
			RefreshUI();
			AuthenticateButton_OnClicked();
		}


		//  Other Methods ---------------------------------
		private void SetBodyText(string message)
		{
			_examplesUI.BodyText.text = message;
			Debug.Log(message);
		}


		private void RefreshUI()
		{
			// Refresh button interactivity
			bool isSessionActive = _session != null && !_session.IsExpired;

			_EndSessionButton.Button.interactable = isSessionActive;
			_ReauthenticateButton.Button.interactable = isSessionActive;
			_AuthenticateButton.Button.interactable = !isSessionActive;
		}

		//  Event Handlers --------------------------------
		private async void AuthenticateButton_OnClicked()
		{
			_AuthenticateButton.Button.interactable = false;

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time = {DateTime.Now.ToLocalTime()}");

			//  -------------------------------------------
			//  NOTE: Create Client
			//  -------------------------------------------
			_client = new Client("http", "127.0.0.1", 7350, "defaultkey");

			//  -------------------------------------------
			//  NOTE: Restore Session, if exists
			//  -------------------------------------------
			var deviceId = SystemInfo.deviceUniqueIdentifier;
			var sessionToken = PlayerPrefs.GetString(_AuthTokenKey);
			var currentDateTime = DateTime.UtcNow;
			_session = Session.Restore(sessionToken);

			//  -------------------------------------------
			//  NOTE: Authenticate Session, if needed
			//  -------------------------------------------
			if (_session == null || _session.HasExpired(currentDateTime))
			{
				_session = await _client.AuthenticateDeviceAsync(deviceId);
				PlayerPrefs.SetString(_DeviceIdKey, deviceId);
				PlayerPrefs.SetString(_AuthTokenKey, _session.AuthToken);

				stringBuilder.AppendLine($"Session Created At {DateTime.Now.ToLocalTime()}");
			}
			else
			{
				stringBuilder.AppendLine($"Session Restored At {DateTime.Now.ToLocalTime()}");
			}

			var epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var expirationDateTime = epochDateTime.AddSeconds(_session.ExpireTime).ToLocalTime();

			stringBuilder.AppendLine($"Session.UserId = {_session.UserId}");
			stringBuilder.AppendLine($"Session.Username = {_session.Username}");
			stringBuilder.AppendLine($"Session.IsExpired = {_session.IsExpired}");
			stringBuilder.AppendLine($"Session.ExpireTime = {_session.ExpireTime}"); // in seconds.
			stringBuilder.AppendLine($"Session Expires At = {expirationDateTime}");

			SetBodyText(stringBuilder.ToString());

			RefreshUI();
		}


		private void ReauthenticateButton_OnClicked()
		{
			var currentDateTime = DateTime.UtcNow;

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time = {DateTime.Now.ToLocalTime()}");

			//  -------------------------------------------
			//  NOTE: Reauthenticate Session, if needed
			//  -------------------------------------------
			if (_session == null || _session.HasExpired(currentDateTime))
			{
				stringBuilder.AppendLine("Reauthentication is needed.");
				AuthenticateButton_OnClicked();
			}
			else
			{
				var epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				var expirationDateTime = epochDateTime.AddSeconds(_session.ExpireTime).ToLocalTime();
				stringBuilder.AppendLine($"Reauthentication is NOT needed until {expirationDateTime}.");
			}

			SetBodyText(stringBuilder.ToString());

			RefreshUI();
		}


		private void EndSessionButton_OnClicked()
		{
			// Clear Session from RAM, if exists
			_session = null;

			//  -------------------------------------------
			//  NOTE: Clear PlayerPrefs, for fresh start
			//  -------------------------------------------
			PlayerPrefs.DeleteKey(_DeviceIdKey);
			PlayerPrefs.DeleteKey(_AuthTokenKey);

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time = {DateTime.Now.ToLocalTime()}");
			stringBuilder.AppendLine("Session Ended.");
			SetBodyText(stringBuilder.ToString());

			RefreshUI();
		}
	}
}
