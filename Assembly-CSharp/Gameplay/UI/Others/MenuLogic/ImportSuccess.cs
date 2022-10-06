using System;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic
{
	public class ImportSuccess : MonoBehaviour
	{
		private void Start()
		{
		}

		private void Update()
		{
			if (Input.GetButtonDown("A") || Input.GetKeyDown(13))
			{
				this.menu.ExitSucessMenu();
			}
		}

		public MainMenu menu;
	}
}
