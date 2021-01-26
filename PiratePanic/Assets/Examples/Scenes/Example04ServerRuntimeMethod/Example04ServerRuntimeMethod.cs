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
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Nakama.Examples.Example04ServerRuntimeMethod
{

	/// <summary>
	/// Request sent to the RPC method
	/// </summary>
	[Serializable]
	public class AddNumbersRequest
	{
		public float A;
		public float B;

		public AddNumbersRequest(float a, float b)
		{
			A = a;
			B = b;
		}

		public override string ToString()
		{
			return $"[ AddNumbersRequest (A={A}, B={B}) ]";
		}
	}

	/// <summary>
	/// Response sent from the RPC method
	/// </summary>
	[Serializable]
	public class AddNumbersResponse
	{
		public float Result;

		public override string ToString()
		{
			return $"[ AddNumbersResponse (Result={Result}) ]";
		}
	}

	/// <summary>
	/// This example showcases the fast embedded code runtime with Nakama server.
	/// 
	/// See <a href="https://heroiclabs.com/docs/runtime-code-basics/">Nakama Docs</a> for more info.
	///
	/// </summary>
	public class Example04ServerRuntimeMethod : MonoBehaviour
	{
		//  Properties ------------------------------------
		private ExampleButton AddNumbersButton { get { return _examplesUI.ExampleButton03; } }


		//  Fields ----------------------------------------
		[SerializeField] private ExamplesUI _examplesUI = null;
		private ExampleSessionClient _exampleSessionClient = null;


		//  Unity Methods   -------------------------------
		protected async void Start()
		{
			_exampleSessionClient = new ExampleSessionClient();
			await _exampleSessionClient.Authenticate();

			AddNumbersButton.Button.onClick.AddListener(AddNumbersButton_OnClicked);
			AddNumbersButton_OnClicked();
		}
		 

		//  Other Methods ---------------------------------
		private void SetBodyText(string message)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Time = {DateTime.Now.ToLocalTime()}");
			stringBuilder.AppendLine($"{message}");

			if (_examplesUI != null)
			{
				_examplesUI.BodyText.text = stringBuilder.ToString();
			}

			Debug.Log(stringBuilder.ToString());
		}


		//  Event Handlers --------------------------------
		private async void AddNumbersButton_OnClicked()
		{
			SetBodyText("");

			IClient client = _exampleSessionClient.Client;
			ISession session = _exampleSessionClient.Session;

			// Build the request
			AddNumbersRequest addNumbersRequest = new AddNumbersRequest(5, 10);
			string requestString = JsonWriter.ToJson(addNumbersRequest);

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"AddButton_OnClicked()");
			stringBuilder.AppendLine($"\nrequest...");
			stringBuilder.AppendLine($"\tJson = {requestString}");
			stringBuilder.AppendLine($"\tParsed C# = {addNumbersRequest}");
			SetBodyText(stringBuilder.ToString());

			// Send the request
			var rpc = await client.RpcAsync(session, "AddNumbers", requestString);

			// Handle the response
			AddNumbersResponse addNumbersResponse = JsonParser.FromJson<AddNumbersResponse>(rpc.Payload);
			stringBuilder.AppendLine($"\nresponse.Result...");
			stringBuilder.AppendLine($"\tJson = {rpc.Payload}");
			stringBuilder.AppendLine($"\tParsed C# = {addNumbersResponse}");
			
			SetBodyText(stringBuilder.ToString());

			// Validate the response
			float localSum = addNumbersRequest.A + addNumbersRequest.B;
			Assert.AreEqual(localSum, addNumbersResponse.Result);
		}
	}
}
