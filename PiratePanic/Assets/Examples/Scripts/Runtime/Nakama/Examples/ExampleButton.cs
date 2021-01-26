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

using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Examples
{
	/// <summary>
	/// Wrapper for <see cref="Button"/> which wraps common functionality
	/// used in Examples.
	/// </summary>
	[System.Serializable]
	public class ExampleButton
	{
		//  Properties ------------------------------------
		public Button Button { get { return _button; } }
		public Text ButtonText { get { return _button.GetComponentInChildren<Text>(); } }


		//  Fields ----------------------------------------
		[SerializeField] private Button _button = null;
		[SerializeField] private string _textMessage = "";


		//  Other Methods ---------------------------------
		public void Validate()
		{
			// Populate and enable the button label text *if* there is a text string
			ButtonText.text = _textMessage;
			_button.interactable = !string.IsNullOrEmpty(ButtonText.text);
		}
	}
}
