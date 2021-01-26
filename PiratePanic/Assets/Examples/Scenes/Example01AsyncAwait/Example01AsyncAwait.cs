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
using System.Threading.Tasks;
using UnityEngine;

namespace Nakama.Examples.Example01AsyncAwait
{
	/// <summary>
	/// This example showcases asynchronous programming with async and await.
	/// 
	/// See <a href="https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/">Microsoft Docs</a> for more info.
	///
	/// </summary>
	public class Example01AsyncAwait : MonoBehaviour
	{
		//  Properties ------------------------------------
		private ExampleButton CallMethod { get { return _examplesUI.ExampleButton03; } }

		//  Fields ----------------------------------------
		[SerializeField] private ExamplesUI _examplesUI = null;

		//  Unity Methods   -------------------------------
		protected void Start()
		{
			CallMethod.Button.interactable = true;
			CallMethod.Button.onClick.AddListener(CallMethod_OnClicked);
			CallMethod_OnClicked();
		}

		//  Other Methods ---------------------------------
		private void ClearBodyText()
		{
			_examplesUI.BodyText.text = "";
		}

		private void AddToBodyText(string message)
		{
			_examplesUI.BodyText.text += message + "\n";
			Debug.Log(message);
		}

		private async void Method01()
		{
			AddToBodyText("Method01() Begins ...");

			// Try / Catch the method
			// This is optional but recommended. Otherwise exceptions may not be found.
			try
			{
				// Call the method.
				// "await" is used. So the call is ASYNCHRONOUS. 
				int delayInMilliseconds = await Method02();

				AddToBodyText($"Method01() Ends. DelayInMilliseconds = {delayInMilliseconds}");

			}
			catch (Exception e)
			{
				Debug.LogError($"Method01() Exception={e}");
			}
		}

		/// <summary>
		/// Asynchronously returns an int value
		/// </summary>
		/// <returns></returns>
		private async Task<int> Method02()
		{
			CallMethod.Button.interactable = false;

			AddToBodyText("\tMethod02() Begin...");

			int delayInMilliseconds = 1000;

			await Task.Delay(delayInMilliseconds);

			AddToBodyText("\tMethod02() End.");

			CallMethod.Button.interactable = true;

			return delayInMilliseconds;
		}

		//  Event Handlers --------------------------------
		private void CallMethod_OnClicked()
		{
			ClearBodyText();

			// Call the method.
			// "await" is NOT used. So the call is SYNCHRONOUS. 
			// This is the default Unity behaviour.
			Method01();
		}
	}
}