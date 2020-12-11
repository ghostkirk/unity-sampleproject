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

namespace Nakama.Templates
{
	/// <summary>
	/// 
	/// </summary>
	public class TemplateMonoBehavior : MonoBehaviour
	{
		//  Properties ------------------------------------
		public GameObject SampleGameObject { get { return _sampleGameObject; } }

		//  Fields ----------------------------------------

		[SerializeField] private GameObject _sampleGameObject = null;

		//  Unity Methods   -------------------------------
		protected void Awake()
		{

		}

		protected void Start()
		{

		}

		//  Other Methods ---------------------------------
		private void SampleMethod ()
		{

		}

		//  Event Handlers --------------------------------
		private void OtherClass_OnEventHappenened()
		{

		}
	}
}
