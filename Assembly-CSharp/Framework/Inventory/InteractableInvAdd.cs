using System;
using Framework.Managers;
using UnityEngine;

namespace Framework.Inventory
{
	public class InteractableInvAdd : MonoBehaviour
	{
		private void OnUsePost()
		{
			Core.Randomizer.Log("InteractableInvAdd (" + this.item + ")", 2);
			Core.Randomizer.giveReward(this.item, true);
		}

		public InventoryManager.ItemType itemType;

		public string item = string.Empty;

		public bool showMessage = true;
	}
}