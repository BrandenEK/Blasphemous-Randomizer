﻿using HarmonyLib;
using Framework.Managers;
using Framework.EditorScripts.EnemiesBalance;
using Framework.EditorScripts.BossesBalance;
using Gameplay.UI.Others.MenuLogic;
using Gameplay.UI.Widgets;
using UnityEngine;

namespace BlasphemousRandomizer.Patches
{
    // Allow console commands
    [HarmonyPatch(typeof(ConsoleWidget), "Update")]
    public class ConsoleWidget_Patch
    {
        public static void Postfix(ConsoleWidget __instance, bool ___isEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                __instance.SetEnabled(!___isEnabled);
            }
        }
    }

    // Change functionality of the Ossuary
    [HarmonyPatch(typeof(OssuaryManager), "CheckGroupCompletion")]
    public class OssuaryManager_Patch
    {
        public static bool Prefix(OssuaryManager __instance)
        {
            __instance.pendingRewards = 0;
            int collected = OssuaryManager.CountAlreadyRetrievedCollectibles();
            int alreadyClaimed = 0;

            for (int i = 0; i < 11; i++)
            {
                string id = "OSSUARY_REWARD_" + (i + 1);
                if (Core.Events.GetFlag(id))
                {
                    alreadyClaimed++;
                }
                else if (collected >= (i + 1) * 4)
                {
                    Core.Events.SetFlag(id, true, false);
                    __instance.pendingRewards++;
                }
            }
            __instance.alreadyClaimedRewards = alreadyClaimed;
            Core.Events.LaunchEvent(__instance.CheckRewardsEvent, string.Empty);
            return false;
        }
    }

    // Make enemies stay as ng
    [HarmonyPatch(typeof(GameModeManager), "GetCurrentEnemiesBalanceChart")]
    public class GameModeEnemies_Patch
    {
        public static bool Prefix(ref EnemiesBalanceChart __result, EnemiesBalanceChart ___newGameEnemiesBalanceChart)
        {
            __result = ___newGameEnemiesBalanceChart;
            return false;
        }
    }

    // Make bosses stay as ng
    [HarmonyPatch(typeof(GameModeManager), "GetCurrentBossesBalanceChart")]
    public class GameModeBosses_Patch
    {
        public static bool Prefix(ref BossesBalanceChart __result, BossesBalanceChart ___newGameBossesBalanceChart)
        {
            __result = ___newGameBossesBalanceChart;
            return false;
        }
    }

    // Call load data even if loading vanilla game
    [HarmonyPatch(typeof(PersistentManager), "LoadSnapShot")]
    public class PersistentManager_Patch
    {
        public static void Postfix(PersistentManager __instance, PersistentManager.SnapShot snapShot)
        {
            if (!snapShot.commonElements.ContainsKey(Main.Randomizer.GetPersistenID()))
            {
                Main.Randomizer.SetCurrentPersistentState(null, false, "");
            }
        }
    }

    // Set up new game when select new save file
    [HarmonyPatch(typeof(NewMainMenu), "InternalPlay")]
    public class NewMainMenu_Patch
    {
        public static void Postfix(bool ___isContinue)
        {
            if (!___isContinue)
            {
                Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.NEW_GAME_PLUS);
                Main.Randomizer.newGame();
            }
        }
    }

    // Don't allow ascending a save file
    [HarmonyPatch(typeof(SaveSlot), "SetData")]
    public class SaveSlot_Patch
    {
        public static void Prefix(ref bool canConvert)
        {
            canConvert = false;
        }
    }

    // Initialize Randomizer class
    [HarmonyPatch(typeof(AchievementsManager), "AllInitialized")]
    public class AchievementsManager_InitializePatch
    {
        public static void Postfix()
        {
            Main.Randomizer.Initialize();
        }
    }
    // Dispose Randomizer class
    [HarmonyPatch(typeof(AchievementsManager), "Dispose")]
    public class AchievementsManager_DisposePatch
    {
        public static void Postfix()
        {
            Main.Randomizer.Dispose();
        }
    }
}
