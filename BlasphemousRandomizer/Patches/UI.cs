﻿using HarmonyLib;
using UnityEngine.UI;
using Tools.Playmaker2.Action;
using Framework.Achievements;
using Gameplay.UI;
using Gameplay.UI.Others.MenuLogic;
using BlasphemousRandomizer.Structures;

namespace BlasphemousRandomizer.Patches
{
    // Change version number on main menu to randomizer version
    [HarmonyPatch(typeof(VersionNumber), "Start")]
    public class VersionNumberPatch
    {
        public static bool Prefix(VersionNumber __instance)
        {
            __instance.GetComponent<Text>().text = "Randomizer v" + MyPluginInfo.PLUGIN_VERSION;
            return false;
        }
    }

    // Skip showing certain messages to the player
    [HarmonyPatch(typeof(ShowMessage), "OnEnter")]
    public class ShowMessage_Patch
    {
        public static bool Prefix(ShowMessage __instance)
        {
            if (__instance.textId != null && "MSG_0501,MSG_BAPTISMAL,MSG_1801,MSG_2101,MSG_10101".Contains(__instance.textId.Value))
            {
                __instance.Finish();
                return false;
            }
            return true;
        }
    }

    // Allow all achievement messages to go through
    [HarmonyPatch(typeof(UIController), "ShowPopupAchievement")]
    public class UIController_Patch
    {
        public static bool Prefix(Achievement achievement, PopupAchievementWidget ___popupAchievementWidget)
        {
            if (achievement.GetType() == typeof(RewardAchievement))
                ___popupAchievementWidget.ShowPopup(achievement);
            return false;
        }
    }

    // Change achievement message & time
    [HarmonyPatch(typeof(PopupAchievementWidget), "ShowPopupCorrutine")]
    public class PopupAchievementWidget_Patch
    {
        public static void Prefix()
        {
            Main.Randomizer.Log("Showing item notification!");
        }
    }

    // Can be used to close dialog
    [HarmonyPatch(typeof(PopUpWidget), "HideAreaPopup")]
    public class PopUpWidget_Patch
    {
        public static void Postfix(PopUpWidget __instance)
        {
            
        }
    }
}
