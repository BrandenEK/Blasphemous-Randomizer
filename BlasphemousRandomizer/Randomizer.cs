﻿using UnityEngine;
using Gameplay.UI;
using BlasphemousRandomizer.Shufflers;
using BlasphemousRandomizer.Config;
using Framework.FrameworkCore;
using Framework.Managers;

namespace BlasphemousRandomizer
{
    public class Randomizer : PersistentInterface
    {
        // Shufflers
        public ItemShuffle itemShuffler;
        public EnemyShuffle enemyShuffler;
        public DoorShuffle doorShuffler;
        public HintShuffle hintShuffler;

        // Config
        public MainConfig gameConfig;
        private MainConfig fileConfig;

        // Save file info
        private int seed;
        public int itemsCollected { get; private set; }
        public int totalItems { get; private set; }
        public bool startedInRando { get; private set; }

        private bool inGame;
        private int lastLoadedSlot;

        public Randomizer()
        {
            // Create main shufflers
            itemShuffler = new ItemShuffle();
            enemyShuffler = new EnemyShuffle();
            doorShuffler = new DoorShuffle();
            hintShuffler = new HintShuffle();

            // Load config
            fileConfig = FileUtil.loadConfig();
            if (!isConfigVersionValid(fileConfig.versionCreated))
            {
                fileConfig = MainConfig.Default();
            }
            gameConfig = fileConfig;

            // Set up data
            Core.Persistence.AddPersistentManager(this);
            lastLoadedSlot = -1;
        }

        // When game is saved
        public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
        {
            return new RandomizerPersistenceData
            {
                seed = seed,
                itemsCollected = itemsCollected,
                startedInRando = startedInRando,
                config = gameConfig
            };
        }

        // When game is loaded
        public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
        {
            // Only reload data if coming from title screen and loading different save file
            if (inGame || PersistentManager.GetAutomaticSlot() == lastLoadedSlot)
            {
                return;
            }

            RandomizerPersistenceData randomizerPersistenceData = data == null ? null : (RandomizerPersistenceData)data;
            if (randomizerPersistenceData != null && randomizerPersistenceData.startedInRando && isConfigVersionValid(randomizerPersistenceData.config.versionCreated))
            {
                // Loaded a valid randomized game
                seed = randomizerPersistenceData.seed;
                itemsCollected = randomizerPersistenceData.itemsCollected;
                startedInRando = randomizerPersistenceData.startedInRando;
                gameConfig = randomizerPersistenceData.config;
                Randomize(false);
            }
            else
            {
                // Loaded a vanilla game or an outdated rando game
                seed = -1;
                itemsCollected = 0;
                totalItems = 0;
                startedInRando = false;
                gameConfig = fileConfig;
                //Display error message
                //Reset shufflers
            }

            inGame = true;
            lastLoadedSlot = PersistentManager.GetAutomaticSlot();
        }

        // Returned to title screen
        public void ResetPersistence()
        {
            inGame = false;
        }

        private void Randomize(bool newGame)
        {

        }

        public void newGame()
        {
            seed = generateSeed();
            itemsCollected = 0;
            startedInRando = true;
            gameConfig = fileConfig;
            Randomize(true);

            inGame = true;
            lastLoadedSlot = PersistentManager.GetAutomaticSlot();
        }

        private int generateSeed()
        {
            return fileConfig.general.customSeed > 0 ? fileConfig.general.customSeed : new System.Random().Next();
        }

        // Keyboard input
        public void update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                UIController.instance.ShowPopUp("Current seed: " + seed, "", 0, false);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                UIController.instance.ShowPopUp("Test", "", 0, false);
            }
        }

        // Log message to file
        public void Log(string message)
        {
            if (fileConfig.debug.type > 0)
                FileUtil.writeLine("log.txt", message + "\n");
        }

        private bool isConfigVersionValid(string configVersion)
        {
            string version = MyPluginInfo.PLUGIN_VERSION;
            return version.Substring(version.IndexOf('.') + 1, 1) == configVersion.Substring(configVersion.IndexOf('.') + 1, 1);
        }

        public string GetPersistenID()
        {
            return "ID_RANDOMIZER";
        }

        public int GetOrder()
        {
            return 0;
        }
    }
}
