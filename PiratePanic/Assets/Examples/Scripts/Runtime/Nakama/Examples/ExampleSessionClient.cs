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
using System.Threading.Tasks;
using UnityEngine;

namespace Nakama.Examples
{
	public class ExampleSessionClient 
	{
		//  Properties ------------------------------------
		public IClient Client { get { return _client; } }
		public ISession Session { get { return _session; } }


		//  Fields ----------------------------------------
		private const string _DeviceIdKey = "MyDeviceIdKey";
		private const string _AuthTokenKey = "MyAuthTokenKey";
		private ISession _session = null;
		private IClient _client = null;


		//  Constructor   ---------------------------------
		public ExampleSessionClient()
		{

		}

		//  Other Methods ---------------------------------

		/// <summary>
		/// Returns a fake time to encourage expiration for the sake of the demo.
		/// </summary>
		private DateTime GetMockCurrentDateTime()
		{
			//TODO: What time to put here - srivello
			return DateTime.UtcNow.AddYears(1);
		}


		//  Event Handlers --------------------------------
		public async Task Authenticate()
		{
			//Create the Client
			//	Port: 7350 - Client()
			//	Port: 7351 - Console at http://localhost:7351/#/status
			_client = new Client("http", "127.0.0.1", 7350, "defaultkey");

			// Restore session from PlayerPrefs if possible.
			var deviceId = SystemInfo.deviceUniqueIdentifier;
			var sessionToken = PlayerPrefs.GetString(_AuthTokenKey);
			var currentDateTime = GetMockCurrentDateTime();
			_session = Nakama.Session.Restore(sessionToken);

			if (_session == null || _session.HasExpired(currentDateTime))
			{
				_session = await _client.AuthenticateDeviceAsync(deviceId);
				PlayerPrefs.SetString(_DeviceIdKey, deviceId);
				PlayerPrefs.SetString(_AuthTokenKey, _session.AuthToken);
			}
		}


		public async void Reauthenticate()
		{
			var currentDateTime = GetMockCurrentDateTime();

			if (_session == null || _session.HasExpired(currentDateTime))
			{
				await Authenticate();
			}
		}
	}
}
