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

using Facebook.Unity;
using Nakama;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{
    /// <summary>
    /// Adds all panels to the scene and handles navigation in menu.
    /// </summary>
    public class Scene01_MainMenuController : MonoBehaviour
    {
        [SerializeField] BattleMenuUI _battleMenuUI;
        [SerializeField] ClansMenuUI _clansMenuUI;
        [SerializeField] FriendsMenuUI _friendsMenuUI;
        [SerializeField] LeaderboardsMenuUI _leaderboardMenuUI;
        [SerializeField] ProfileMenuUI _profileMenuUI;
        [SerializeField] CardsMenuUI _cardsMenuUI;
        [SerializeField] ClanCreationPanel _clanCreationPanel;
        [SerializeField] LoadingMenu _loadingMenu;
        [SerializeField] ProfilePopup _profilePopup;
        [SerializeField] ProfileUpdatePanel _profileUpdatePanel;
        [SerializeField] NotificationPopup _notificationPopup;

        [SerializeField] Button _battleButton;
        [SerializeField] Button _cardsButton;
        [SerializeField] Button _clansButton;
        [SerializeField] Button _friendsButton;
        [SerializeField] Button _leaderboardsButton;
        [SerializeField] Button _profileButton;

        [SerializeField] private GameConnection _connection;

        private void Awake()
        {
            _battleButton.onClick.AddListener(() => _battleMenuUI.Show ());
            _cardsButton.onClick.AddListener(() => _cardsMenuUI.Show());
            _clansButton.onClick.AddListener(() => _clansMenuUI.Show());
            _friendsButton.onClick.AddListener(() => _friendsMenuUI.Show());
            _leaderboardsButton.onClick.AddListener(() => _leaderboardMenuUI.Show());
            _profileButton.onClick.AddListener(() => _profileMenuUI.Show());
        }

        private void Start()
        {
            InitializeGame();
        }

        private async void InitializeGame()
        {
            // Init PlayerPrefs
            bool willDeleteAllPlayerPrefsOnInit = GameConfigurationManager.Instance.GameConfiguration.WillDeleteAllPlayerPrefsOnInit;

            if (willDeleteAllPlayerPrefsOnInit)
            {
                PlayerPrefs.DeleteAll();
                Debug.LogWarning($"InitializeGame() willDeleteAllPlayerPrefsOnInit={willDeleteAllPlayerPrefsOnInit}");
			}

            string deviceId = GetDeviceId();

            if (!string.IsNullOrEmpty(deviceId))
            {
                PlayerPrefs.SetString(GameConstants.DeviceIdKey, deviceId);
            }

            if (_connection.Session == null)
            {
                _loadingMenu.Show(true);

                try
                {
                    FB.Init(() =>
                    {
                        FB.ActivateApp();
                    });
                }
                catch
                {
                    // not supported on mac
                }

                var client = new Client("http", "localhost", 7350, "defaultkey", UnityWebRequestAdapter.Instance);
                client.Timeout = 5;
                var socket = client.NewSocket(useMainThread: true);

                string storedToken = PlayerPrefs.GetString(GameConstants.AuthTokenKey, null);
                bool isStoredToken = !string.IsNullOrEmpty(storedToken);

                // Restoring authentication token from player prefs
                ISession session = null;

                if (isStoredToken)
                {
                    session = Nakama.Session.Restore(storedToken);
                }

                bool isExpiredToken = isStoredToken && session.HasExpired(DateTime.UtcNow);

                if (!isStoredToken || isExpiredToken)
                {
                    try
                    {
                        session = await client.AuthenticateDeviceAsync(deviceId);
                    }
                    catch (ApiResponseException e)
                    {
                        Debug.LogWarning("Error authenticating device: " + e.Message);
                        Application.Quit();
                        return;
                    }
                }

                try
                {
                    await socket.ConnectAsync(session);

                    if (!isStoredToken)
                    {
                        _profileUpdatePanel.Show();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error connecting socket: " + e.Message);
                }

                _loadingMenu.Hide(true);

                PlayerPrefs.SetString(GameConstants.AuthTokenKey, session.AuthToken);

                IApiAccount account;

                try
                {
                    account = await client.GetAccountAsync(session);
                }
                catch (ApiResponseException e)
                {
                    Debug.LogError("Error getting user account: " + e.Message);

                    if (e.StatusCode == 404)
                    {
                        Debug.LogWarning("invalid auth token. deleting... please restart application.");
                        PlayerPrefs.DeleteKey(GameConstants.AuthTokenKey);
                    }

                    Application.Quit();
                    return;
                }

                _connection.Init(client, socket, account, session);
            }

            _battleMenuUI.Init(_connection);
            _loadingMenu.Init(_connection);
            _notificationPopup.Init(_connection);
            _cardsMenuUI.Init(_connection);
            _clanCreationPanel.Init(_connection);
            _profilePopup.Init(_connection, _profileUpdatePanel);
            _profileUpdatePanel.Init(_connection, deviceId);
            _clansMenuUI.Init(_connection, _profilePopup);
            _friendsMenuUI.Init(_connection);
            _leaderboardMenuUI.Init(_connection, _profilePopup);
            _profileMenuUI.Init(_connection, _profileUpdatePanel);
        }

        private string GetDeviceId()
        {
            string deviceId = "";

			deviceId = PlayerPrefs.GetString(GameConstants.DeviceIdKey);

			if (string.IsNullOrWhiteSpace(deviceId))
			{
					// SystemInfo.deviceUniqueIdentifier is not supported in WebGL,
					// we generate a random one instead via System.Guid
#if UNITY_WEBGL && !UNITY_EDITOR
				deviceId = System.Guid.NewGuid().ToString();
#else
				deviceId = SystemInfo.deviceUniqueIdentifier;
#endif
			}

            return deviceId;
        }
    }
}
