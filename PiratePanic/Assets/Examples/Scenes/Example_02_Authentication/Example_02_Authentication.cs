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

namespace Nakama.Examples.Example_02_Authentication
{
	/// <summary>
	/// This example showcases user authentication with Nakama server.
	///
	/// See <a href="https://heroiclabs.com/docs/unity-client-guide/#authenticate">Nakama Docs</a> for more info.
	///
	/// </summary>
	public class Example_02_Authentication : MonoBehaviour
	{
		//  Properties ------------------------------------
		private ExampleButton ReauthenticateButton { get { return _examplesUI.ExampleButton02; } }
		private ExampleButton AuthenticateButton { get { return _examplesUI.ExampleButton03; } }


		//  Fields ----------------------------------------
		[SerializeField] private ExamplesUI _examplesUI = null;

		private const string DeviceIdKey = "MyDeviceIdKey";
		private const string AuthTokenKey = "MyAuthTokenKey";
		private ISession _session = null;
		private IClient _client = null;

		//  Unity Methods   -------------------------------
		protected void Start()
		{
			ReauthenticateButton.Button.onClick.AddListener(ReauthenticateButton_OnClicked);
			AuthenticateButton.Button.onClick.AddListener(AuthenticateButton_OnClicked);
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
			ReauthenticateButton.Button.interactable = isSessionActive;
			AuthenticateButton.Button.interactable = !isSessionActive;
		}


		/// <summary>
		/// Returns a fake time to encourage expiration for the sake of the demo.
		/// </summary>
		private DateTime GetMockCurrentDateTime()
		{
			return DateTime.UtcNow.AddYears(1);
		}


		//  Event Handlers --------------------------------
		private async void AuthenticateButton_OnClicked()
		{
			AuthenticateButton.Button.interactable = false;

			StringBuilder stringBuilder = new StringBuilder();

			// Create client
			_client = new Client("http", "127.0.0.1", 7350, "defaultkey");

			// Restore session from PlayerPrefs if possible.
			var deviceId = SystemInfo.deviceUniqueIdentifier;
			var sessionToken = PlayerPrefs.GetString(AuthTokenKey);
			var currentDateTime = GetMockCurrentDateTime();
			_session = Session.Restore(sessionToken);

			if (_session == null || _session.HasExpired(currentDateTime))
			{
				_session = await _client.AuthenticateDeviceAsync(deviceId);
				PlayerPrefs.SetString(DeviceIdKey, deviceId);
				PlayerPrefs.SetString(AuthTokenKey, _session.AuthToken);

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
			var epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var expirationDateTime = epochDateTime.AddSeconds(_session.ExpireTime).ToLocalTime();
			var currentDateTime = GetMockCurrentDateTime();

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time = {DateTime.Now.ToLocalTime()}");

			if (_session == null || _session.HasExpired(currentDateTime))
			{
				stringBuilder.AppendLine($"Reauthentication is NOT needed until {expirationDateTime}.");
			}
			else
			{
				stringBuilder.AppendLine("Reauthentication is needed.");
				AuthenticateButton_OnClicked();
			}

			SetBodyText(stringBuilder.ToString());

			RefreshUI();
		}
	}
}
