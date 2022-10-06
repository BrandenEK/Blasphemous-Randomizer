using System;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic
{
	public class ImportGamepadClose : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetButtonDown("A") || Input.GetKeyDown(13))
			{
				this.menu.CloseImportMenu();
			}
		}

		public MainMenu menu;
	}
}
