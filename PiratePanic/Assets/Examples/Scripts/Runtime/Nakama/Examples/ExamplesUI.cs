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
	/// User interface used in all examples. Encapsulates <see cref="Button"/>s and
	/// <see cref="Text"/>s for consistency in layout and C# API.
	/// 
	/// Some subclasses will directly use all 3 <see cref="ExampleButton"/>s, some use less.
	/// 
	/// </summary>
	[ExecuteInEditMode]
	public class ExamplesUI : MonoBehaviour
	{
		//  Properties ------------------------------------
		public Text TitleText { get { return _titleText; } }
		public Text BodyText { get { return _bodyText; } }
		public ExampleButton ExampleButton01 { get { return _exampleButton01; } }
		public ExampleButton ExampleButton02 { get { return _exampleButton02; } }
		public ExampleButton ExampleButton03 { get { return _exampleButton03; } }


		//  Fields ----------------------------------------
		[SerializeField] private string _titleTextMessage = "";
		[SerializeField] private Text _titleText = null;
		[SerializeField] private Text _bodyText = null;
		[SerializeField] private ExampleButton _exampleButton01 = null;
		[SerializeField] private ExampleButton _exampleButton02 = null;
		[SerializeField] private ExampleButton _exampleButton03 = null;


		//  Unity Methods   -------------------------------
		protected void OnValidate()
		{
			// Populate and enable the button label text *if* there is a text string
			TitleText.text = _titleTextMessage;
			BodyText.text = "";
			_exampleButton01.Validate();
			_exampleButton02.Validate();
			_exampleButton03.Validate();
		}
	}
}
