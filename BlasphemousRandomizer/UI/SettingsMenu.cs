﻿using UnityEngine;
using UnityEngine.UI;
using Gameplay.UI.Others.MenuLogic;
using Gameplay.UI.Others;
using BlasphemousRandomizer.Config;

namespace BlasphemousRandomizer.UI
{
    public class SettingsMenu
    {
        private GameObject settingsMenu;
        private GameObject slotsMenu;

        private Camera camera;
        private SettingsElement[] buttons;
        private Text seedText;
        private Vector2 scaling;

        private bool menuActive;
        private bool waiting;
        private string currentSeed;
        private int currentSlot;

        public void update()
        {
            if (waiting)
            {
                waiting = false;
                return;
            }
            if (settingsMenu == null || !menuActive)
                return;

            // Check if a button was clicked
            if (Input.GetMouseButtonDown(0))
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (pointInsideRect(buttons[i].transform as RectTransform, Input.mousePosition))
                    {
                        buttons[i].onClick();
                        break;
                    }
                }
            }

            // Keyboard input
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) processKeyInput(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) processKeyInput(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) processKeyInput(3);
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) processKeyInput(4);
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) processKeyInput(5);
            else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) processKeyInput(6);
            else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) processKeyInput(7);
            else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) processKeyInput(8);
            else if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) processKeyInput(9);
            else if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) processKeyInput(0);
            else if (Input.GetKeyDown(KeyCode.Backspace)) processKeyInput(-1);
            else if (Input.GetKeyDown(KeyCode.Escape)) closeMenu();
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) beginGame();
        }

        private void processKeyInput(int num)
        {
            // Pressed backspace - delete last character
            if (num < 0)
            {
                if (currentSeed.Length > 0)
                    currentSeed = currentSeed.Substring(0, currentSeed.Length - 1);
            }
            // Pressed number 1-10 - add new character
            else if (int.TryParse(currentSeed + num.ToString(), out int x) && (currentSeed.Length > 0 || num != 0))
            {
                currentSeed += num.ToString();
            }

            // Update text
            seedText.text = "Seed: " + (currentSeed != "" ? currentSeed : "random");
            Main.Randomizer.playSoundEffect(2);
        }

        public void setConfigSettings(MainConfig config)
        {
            if (settingsMenu == null)
                return;

            // Load config into buttons
            ((SettingsCheckbox)buttons[0]).setSelected(config.general.teleportationAlwaysUnlocked);
            ((SettingsCheckbox)buttons[1]).setSelected(config.general.skipCutscenes);
            ((SettingsCheckbox)buttons[2]).setSelected(config.general.enablePenitence);
            ((SettingsCheckbox)buttons[3]).setSelected(config.items.lungDamage);
            ((SettingsCheckbox)buttons[4]).setSelected(config.items.disableNPCDeath);
            ((SettingsCheckbox)buttons[5]).setSelected(config.items.startWithWheel);
            ((SettingsCheckbox)buttons[6]).setSelected(config.items.shuffleReliquaries);
            ((SettingsCheckbox)buttons[7]).setSelected(config.enemies.maintainClass);
            ((SettingsCheckbox)buttons[8]).setSelected(config.enemies.areaScaling);
            ((SettingsCyclebox)buttons[9]).setOption(config.items.type);
            ((SettingsCyclebox)buttons[10]).setOption(config.enemies.type);

            // Load config into seed
            currentSeed = config.general.customSeed > 0 ? config.general.customSeed.ToString() : "";
            seedText.text = "Seed: " + (currentSeed != "" ? currentSeed : "random");
        }

        public MainConfig getConfigSettings()
        {
            if (settingsMenu == null)
                return MainConfig.Default();

            // Load config from buttons
            MainConfig config = MainConfig.Default();
            config.general.teleportationAlwaysUnlocked = ((SettingsCheckbox)buttons[0]).getSelected();
            config.general.skipCutscenes = ((SettingsCheckbox)buttons[1]).getSelected();
            config.general.enablePenitence = ((SettingsCheckbox)buttons[2]).getSelected();
            config.items.lungDamage = ((SettingsCheckbox)buttons[3]).getSelected();
            config.items.disableNPCDeath = ((SettingsCheckbox)buttons[4]).getSelected();
            config.items.startWithWheel = ((SettingsCheckbox)buttons[5]).getSelected();
            config.items.shuffleReliquaries = ((SettingsCheckbox)buttons[6]).getSelected();
            config.enemies.maintainClass = ((SettingsCheckbox)buttons[7]).getSelected();
            config.enemies.areaScaling = ((SettingsCheckbox)buttons[8]).getSelected();
            config.items.type = ((SettingsCyclebox)buttons[9]).getOption();
            config.enemies.type = ((SettingsCyclebox)buttons[10]).getOption();

            // Load config from seed
            config.general.customSeed = currentSeed != "" ? int.Parse(currentSeed) : 0;
            return config;
        }

        private void showSettingsMenu(bool value)
        {
            if (settingsMenu == null)
                createSettingsMenu();
            
            Main.Randomizer.Log("Showing settings menu: " + value);
            settingsMenu.SetActive(value);
            slotsMenu.SetActive(!value);
            Cursor.visible = value;
            menuActive = value;
        }

        public void beginGame()
        {
            if (!menuActive || waiting) return;

            Main.Randomizer.playSoundEffect(0);
            showSettingsMenu(false);
            waiting = true;
            Object.FindObjectOfType<SelectSaveSlots>().OnAcceptSlots(999 + currentSlot);
        }

        public void openMenu(int slot)
        {
            if (menuActive || waiting) return;

            currentSlot = slot;
            waiting = true;
            showSettingsMenu(true);
            setConfigSettings(MainConfig.Default());
        }

        public void closeMenu()
        {
            if (!menuActive || waiting) return;

            Main.Randomizer.playSoundEffect(1);
            showSettingsMenu(false);
        }

        private bool pointInsideRect(RectTransform rect, Vector2 point)
        {
            Vector2 position = camera.WorldToScreenPoint(rect.position);
            position = new Vector2(position.x * scaling.x, position.y * scaling.y);
            Vector2 size = new Vector2(rect.rect.width * scaling.x / 2, rect.rect.height * scaling.y / 2);

            return point.x >= position.x - size.x && point.x <= position.x + size.x && point.y >= position.y - size.y && point.y <= position.y + size.y;
        }

        private void createSettingsMenu()
        {
            // Calculate scaling
            scaling = new Vector2(Screen.width / 640, Screen.height / 360);

            // Find proper camera
            foreach (Camera cam in Object.FindObjectsOfType<Camera>())
                if (cam.name == "UICamera")
                    camera = cam;

            // Find main menu root
            Transform menu = Object.FindObjectOfType<NewMainMenu>().transform;
            slotsMenu = menu.GetChild(2).gameObject;

            // Duplicate slot menu
            settingsMenu = Object.Instantiate(slotsMenu, menu);
            settingsMenu.name = "Settings Menu";
            Object.Destroy(settingsMenu.GetComponent<SelectSaveSlots>());
            Object.Destroy(settingsMenu.GetComponent<KeepFocus>());
            Object.Destroy(settingsMenu.GetComponent<CanvasGroup>());
            int childrenCount = settingsMenu.transform.childCount;
            for (int i = 2; i < childrenCount; i++)
                Object.Destroy(settingsMenu.transform.GetChild(i).gameObject);
            
            // Set rect of settings menu
            RectTransform rect = settingsMenu.GetComponent<RectTransform>(); // Is this necessary ??
            rect.SetParent(menu, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            
            // Set header text
            Text headerText = settingsMenu.transform.GetChild(0).GetChild(0).GetComponent<Text>();
            headerText.text = "CHOOSE SETTINGS";
            Font font = headerText.font;
            
            // Create main section
            int width = 630, height = 260;
            RectTransform mainSection = getNewRect("Main Section", settingsMenu.transform);
            mainSection.pivot = new Vector2(0.5f, 0);
            mainSection.anchorMin = new Vector2(0.5f, 0);
            mainSection.anchorMax = new Vector2(0.5f, 0);
            mainSection.sizeDelta = new Vector2(width, height);
            mainSection.anchoredPosition = new Vector2(0, 185);
            
            // Create dividers
            //RectTransform divider1 = getNewImage("Divider 1", mainSection);
            //divider1.anchoredPosition = new Vector2(-0.25f * width, 20);
            //divider1.sizeDelta = new Vector2(1, 200);
            //divider1.GetComponent<Image>().color = new Color(1, 0.75f, 0);

            //RectTransform divider2 = getNewImage("Divider 2", mainSection);
            //divider2.anchoredPosition = new Vector2(0, 20);
            //divider2.sizeDelta = new Vector2(1, 200);
            //divider2.GetComponent<Image>().color = new Color(1, 0.75f, 0);

            //RectTransform divider3 = getNewImage("Divider 3", mainSection);
            //divider3.anchoredPosition = new Vector2(0.25f * width, 20);
            //divider3.sizeDelta = new Vector2(1, 200);
            //divider3.GetComponent<Image>().color = new Color(1, 0.75f, 0);

            buttons = new SettingsElement[13];
            int top = height / 2 - 5;
            int left = -60;

            // General section

            RectTransform generalSection = getNewRect("General Section", mainSection);
            generalSection.sizeDelta = new Vector2(width / 4, height);
            generalSection.anchoredPosition = new Vector2(-0.375f * width, 0);

            RectTransform generalTitle = getNewText("General Title", generalSection, "General:", font, 16, Color.white, TextAnchor.MiddleCenter);
            generalTitle.anchoredPosition = new Vector2(0, top - 5);

            RectTransform teleportOption = getNewCheckbox("Teleport", generalSection, "Teleportation", font, 15, 16);
            teleportOption.anchoredPosition = new Vector2(left, top - 40);
            buttons[0] = teleportOption.GetComponent<SettingsCheckbox>();

            RectTransform cutscenesOption = getNewCheckbox("Cutscenes", generalSection, "Skip Cutscenes", font, 15, 16);
            cutscenesOption.anchoredPosition = new Vector2(left, top - 70);
            buttons[1] = cutscenesOption.GetComponent<SettingsCheckbox>();

            RectTransform penitenceOption = getNewCheckbox("Penitence", generalSection, "Allow Penitence", font, 15, 16);
            penitenceOption.anchoredPosition = new Vector2(left, top - 100);
            buttons[2] = penitenceOption.GetComponent<SettingsCheckbox>();
            
            // Items section

            RectTransform itemsSection = getNewRect("Items Section", mainSection);
            itemsSection.sizeDelta = new Vector2(width / 4, height);
            itemsSection.anchoredPosition = new Vector2(-0.125f * width, 0);

            RectTransform itemsTitle = getNewText("Items Title", itemsSection, "Item Shuffle:", font, 16, Color.white, TextAnchor.MiddleCenter);
            itemsTitle.anchoredPosition = new Vector2(0, top - 5);

            RectTransform lungOption = getNewCheckbox("Lung", itemsSection, "Lung Damage", font, 15, 16);
            lungOption.anchoredPosition = new Vector2(left, top - 70);
            buttons[3] = lungOption.GetComponent<SettingsCheckbox>();

            RectTransform deathOption = getNewCheckbox("Death", itemsSection, "Disable NPC Death", font, 15, 16);
            deathOption.anchoredPosition = new Vector2(left, top - 100);
            buttons[4] = deathOption.GetComponent<SettingsCheckbox>();

            RectTransform wheelOption = getNewCheckbox("Wheel", itemsSection, "Start with Wheel", font, 15, 16);
            wheelOption.anchoredPosition = new Vector2(left, top - 130);
            buttons[5] = wheelOption.GetComponent<SettingsCheckbox>();

            RectTransform reliqOption = getNewCheckbox("Reliquaries", itemsSection, "Shuffle Reliquaries", font, 15, 16);
            reliqOption.anchoredPosition = new Vector2(left, top - 160);
            buttons[6] = reliqOption.GetComponent<SettingsCheckbox>();

            RectTransform itemsType = getNewCyclebox("Type", itemsSection, font, 15, 16, new string[] { "Disabled", "Enabled" }, new SettingsElement[] { buttons[3], buttons[4], buttons[5], buttons[6] });
            itemsType.anchoredPosition = new Vector2(left + 10, top - 25);
            buttons[9] = itemsType.GetComponent<SettingsCyclebox>();

            // Enemies section

            RectTransform enemiesSection = getNewRect("Enemies Section", mainSection);
            enemiesSection.sizeDelta = new Vector2(width / 4, height);
            enemiesSection.anchoredPosition = new Vector2(0.125f * width, 0);

            RectTransform enemiesTitle = getNewText("Enemies Title", enemiesSection, "Enemy Shuffle:", font, 16, Color.white, TextAnchor.MiddleCenter);
            enemiesTitle.anchoredPosition = new Vector2(0, top - 5);

            RectTransform classOption = getNewCheckbox("Class", enemiesSection, "Maintain Class", font, 15, 16);
            classOption.anchoredPosition = new Vector2(left, top - 70);
            buttons[7] = classOption.GetComponent<SettingsCheckbox>();

            RectTransform scalingOption = getNewCheckbox("Scaling", enemiesSection, "Area Scaling", font, 15, 16);
            scalingOption.anchoredPosition = new Vector2(left, top - 100);
            buttons[8] = scalingOption.GetComponent<SettingsCheckbox>();

            RectTransform enemiesType = getNewCyclebox("Type", enemiesSection, font, 15, 16, new string[] { "Disabled", "Simple", "Full" }, new SettingsElement[] { buttons[7], buttons[8] });
            enemiesType.anchoredPosition = new Vector2(left + 10, top - 25);
            buttons[10] = enemiesType.GetComponent<SettingsCyclebox>();

            // Doors section

            RectTransform doorsSection = getNewRect("Doors Section", mainSection);
            doorsSection.sizeDelta = new Vector2(width / 4, height);
            doorsSection.anchoredPosition = new Vector2(0.375f * width, 0);

            RectTransform doorsTitle = getNewText("Doors Title", doorsSection, "Door Shuffle:", font, 16, Color.white, TextAnchor.MiddleCenter);
            doorsTitle.anchoredPosition = new Vector2(0, top - 5);

            RectTransform comingSoon = getNewText("Doors Title", doorsSection, "Coming Soon!", font, 16, Color.white, TextAnchor.MiddleCenter);
            comingSoon.anchoredPosition = new Vector2(0, top - 40);

            // Create buttons
            RectTransform beginButton = getNewButtonbox("Begin", doorsSection, "Begin", font, 15, 16, 0);
            beginButton.anchoredPosition = new Vector2(-20, top - 170);
            buttons[11] = beginButton.GetComponent<SettingsButtonbox>();

            RectTransform cancelButton = getNewButtonbox("Cancel", doorsSection, "Cancel", font, 15, 16, 1);
            cancelButton.anchoredPosition = new Vector2(-20, top - 200);
            buttons[12] = cancelButton.GetComponent<SettingsButtonbox>();

            // Create seed text
            RectTransform seed = getNewText("Seed", mainSection, "Seed: ", font, 16, Color.white, TextAnchor.MiddleLeft);
            seed.anchoredPosition = new Vector2(0, top - 220);
            seedText = seed.GetComponent<Text>();
            
            // Hide menu
            Main.Randomizer.Log("Settings menu has been created");
            settingsMenu.SetActive(false);

            RectTransform getNewRect(string name, Transform parent)
            {
                GameObject obj = new GameObject(name, typeof(RectTransform));
                RectTransform rect = obj.GetComponent<RectTransform>();
                rect.gameObject.layer = LayerMask.NameToLayer("UI");
                rect.SetParent(parent, false);
                return rect;
            }

            RectTransform getNewImage(string name, Transform parent)
            {
                RectTransform rect = getNewRect(name, parent);
                rect.gameObject.AddComponent<Image>();
                return rect;
            }

            RectTransform getNewText(string name, Transform parent, string text, Font font, int size, Color color, TextAnchor alignment)
            {
                RectTransform rect = getNewRect(name, parent);
                Text display = rect.gameObject.AddComponent<Text>();
                display.text = text;
                display.font = font;
                display.color = color;
                display.fontSize = size;
                display.alignment = alignment;
                display.horizontalOverflow = HorizontalWrapMode.Overflow;
                return rect;
            }

            RectTransform getNewInteractable(string name, Transform parent, string label, Font font, Color color, int boxSize, int fontSize)
            {
                RectTransform rect = getNewImage(name, parent);
                rect.sizeDelta = new Vector2(boxSize, boxSize);

                RectTransform text = getNewText("Label", rect, label, font, fontSize, color, TextAnchor.MiddleLeft);
                text.anchoredPosition = new Vector2(boxSize + 50, 0);

                return rect;
            }

            RectTransform getNewCheckbox(string name, Transform parent, string label, Font font, int boxSize, int fontSize)
            {
                RectTransform rect = getNewInteractable(name, parent, label, font, Color.white, boxSize, fontSize);

                SettingsCheckbox check = rect.gameObject.AddComponent<SettingsCheckbox>();
                check.onStart();
                return rect;
            }

            RectTransform getNewCyclebox(string name, Transform parent, Font font, int boxSize, int fontSize, string[] options, SettingsElement[] boxes)
            {
                RectTransform rect = getNewInteractable(name, parent, "", font, Color.yellow, boxSize, fontSize);

                SettingsCyclebox cycle = rect.gameObject.AddComponent<SettingsCyclebox>();
                cycle.onStart(options, boxes);
                return rect;
            }

            RectTransform getNewButtonbox(string name, Transform parent, string label, Font font, int boxSize, int fontSize, int id)
            {
                RectTransform rect = getNewInteractable(name, parent, label, font, Color.yellow, boxSize, fontSize);

                SettingsButtonbox button = rect.gameObject.AddComponent<SettingsButtonbox>();
                button.onStart(id);
                return rect;
            }
        }

        public static string displayHierarchy(Transform transform, string output, int level, bool components)
        {
            // Indent
            for (int i = 0; i < level; i++)
                output += "\t";

            // Add this object
            output += transform.name;

            // Add components
            if (components)
            {
                output += " (";
                foreach (Component c in transform.GetComponents<Component>())
                    output += c.ToString() + ", ";
                output = output.Substring(0, output.Length - 2) + ")";
            }
            output += "\n";

            // Add children
            for (int i = 0; i < transform.childCount; i++)
                output = displayHierarchy(transform.GetChild(i), output, level + 1, components);

            // Return output
            return output;
        }
    }
}
