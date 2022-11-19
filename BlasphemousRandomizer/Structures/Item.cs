﻿using Framework.Managers;
using Framework.Inventory;
using Gameplay.GameControllers.Entities;

namespace BlasphemousRandomizer.Structures
{
	[System.Serializable]
    public class Item
    {
		public string name;
        public int type;
        public int id;
		public bool progression;

		/*public Item(string name, int type, int id, bool progression)
		{
			this.name = name;
			this.type = type;
			this.id = id;
			this.progression = progression;
		}*/

		/*public virtual string getDescriptor()
		{
			switch (type)
			{
				case 0: return "RB" + id.ToString("00");
				case 1: return "PR" + id.ToString("00");
				case 2: return "RE" + id.ToString("00");
				case 3: return "HE" + id.ToString("00");
				case 4: return "CO";
				case 5: return "QI" + id.ToString("00");
				case 6: return "CH";
				case 7: return "LU";
				case 8: return "FU";
				case 9: return "SU";
				case 10:
					if (id >= 10000) return "TL";
					if (id <= 2000) return "TS";
					return "TM";
				default: return "X";
			}
		}*/

		public virtual void addToInventory()
        {
			InventoryManager inv = Core.InventoryManager;
			EntityStats stats = Core.Logic.Penitent.Stats;

			switch (type)
			{
				case 0:
					inv.AddBaseObjectOrTears(inv.GetBaseObject("RB" + id.ToString("00"), InventoryManager.ItemType.Bead)); return;
				case 1:
					inv.AddBaseObjectOrTears(inv.GetBaseObject("PR" + id.ToString("00"), InventoryManager.ItemType.Prayer)); return;
				case 2:
					inv.AddBaseObjectOrTears(inv.GetBaseObject("RE" + id.ToString("00"), InventoryManager.ItemType.Relic)); return;
				case 3:
					inv.AddBaseObjectOrTears(inv.GetBaseObject("HE" + id.ToString("00"), InventoryManager.ItemType.Sword)); return;
				case 4:
					inv.AddBaseObjectOrTears(inv.GetBaseObject("CO" + id.ToString("00"), InventoryManager.ItemType.Collectible)); return;
				case 5:
					inv.AddBaseObjectOrTears(inv.GetBaseObject("QI" + id.ToString("00"), InventoryManager.ItemType.Quest)); return;
				case 6:
					Core.Events.SetFlag("RESCUED_CHERUB_" + id.ToString("00"), true, false); return;
				case 7:
					stats.Life.Upgrade(); stats.Life.SetToCurrentMax(); return;
				case 8:
					stats.Fervour.Upgrade(); stats.Fervour.SetToCurrentMax(); return;
				case 9:
					stats.MeaCulpa.Upgrade(); stats.Strength.Upgrade(); return;
				case 10:
					stats.Purge.Current += id; return;
				default:
					return;
			}
		}

		public virtual RewardInfo getRewardInfo(bool upgraded)
		{
			InventoryManager inventoryManager = Core.InventoryManager;
			EntityStats stats = Core.Logic.Penitent.Stats;

			switch (type)
			{
				case 0:
					BaseInventoryObject baseObject = inventoryManager.GetBaseObject("RB" + id.ToString("00"), InventoryManager.ItemType.Bead);
					return new RewardInfo(baseObject.caption, baseObject.description, "New rosary bead obtained!", baseObject.picture);
				case 1:
					baseObject = inventoryManager.GetBaseObject("PR" + id.ToString("00"), InventoryManager.ItemType.Prayer);
					return new RewardInfo(baseObject.caption, baseObject.description, "New prayer obtained!", baseObject.picture);
				case 2:
					baseObject = inventoryManager.GetBaseObject("RE" + id.ToString("00"), InventoryManager.ItemType.Relic);
					return new RewardInfo(baseObject.caption, baseObject.description, "New relic obtained!", baseObject.picture);
				case 3:
					baseObject = inventoryManager.GetBaseObject("HE" + id.ToString("00"), InventoryManager.ItemType.Sword);
					return new RewardInfo(baseObject.caption, baseObject.description, "New sword heart obtained!", baseObject.picture);
				case 4:
					baseObject = inventoryManager.GetBaseObject("CO" + id.ToString("00"), InventoryManager.ItemType.Collectible);
					return new RewardInfo(baseObject.caption, baseObject.description, "New collectible obtained!", baseObject.picture);
				case 5:
					baseObject = inventoryManager.GetBaseObject("QI" + id.ToString("00"), InventoryManager.ItemType.Quest);
					return new RewardInfo(baseObject.caption, baseObject.description, "New quest item obtained!", baseObject.picture);
				case 6:
					return new RewardInfo("Cherub " + (CherubCaptorPersistentObject.CountRescuedCherubs() + (upgraded ? 1 : 0)) + "/38", "A little floating baby that you rescued from a cage.", "Cherub rescued!", Main.Randomizer.getImage(0));
				case 7:
					return new RewardInfo("Life Upgrade " + (stats.Life.GetUpgrades() + (upgraded ? 1 : 0)) + "/6", "An increase to your maximum health.", "Stat increased!", Main.Randomizer.getImage(1));
				case 8:
					return new RewardInfo("Fervour Upgrade " + (stats.Fervour.GetUpgrades() + (upgraded ? 1 : 0)) + "/6", "An increase to your maximum fervour.", "Stat increased!", Main.Randomizer.getImage(2));
				case 9:
					return new RewardInfo("Mea Culpa Upgrade " + (stats.MeaCulpa.GetUpgrades() + (upgraded ? 1 : 0)) + "/7", "An increase to the strength of your sword.", "Stat increased!", Main.Randomizer.getImage(3));
				case 10:
					TearsObject tearsGenericObject = inventoryManager.TearsGenericObject;
					return new RewardInfo(tearsGenericObject.caption, "A bundle of " + id + " tears.", id + " tears added!", tearsGenericObject.picture);
				default:
					return new RewardInfo("Error!", "You should not see this.", "You should not see this!", null);
			}
		}
	}
}
