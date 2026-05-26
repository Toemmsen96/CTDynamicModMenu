using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.Collections.Generic;
using CTDynamicModMenu.Commands;
using BepInEx.Logging;

namespace CTDynamicModMenu
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public partial class CTDynamicModMenu : BaseUnityPlugin
    {
        private const string modGUID = "Toemmsen96.CTDynamicModMenu";
        private const string modName = "CTDynamicModMenu";
        private const string modVersion = "1.2.0";

        private ConfigEntry<KeyCode>? toggleKey;
        private GUIStyle? menuStyle;
        private bool showMenu = false;
        private bool showPopup = false;
        internal bool showLogWindow = false;
        private string userInput = "";
        private string lastDisplayedMessage = "No message yet";
        private List<string> logMessages = new List<string>();
        private Vector2 logScrollPosition = Vector2.zero;
        private const int MAX_LOG_MESSAGES = 100;
        internal List<CustomCommand> registeredCommands = new List<CustomCommand>();
        private CustomCommand? selectedCommand;
        private static CTDynamicModMenu? instance;
        private ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        private Vector2 menuPosition = new Vector2(100, 100);
        private bool isDragging = false;
        private Vector2 dragOffset;
        private Rect logWindowRect = new Rect(10, 100, 300, 400);
        private string selectedCategory = "None";
        private bool isDraggingLogWindow = false;
        private Vector2 dragOffsetLogWindow;

        internal bool showKeybindText = true;
        internal bool showMenuButton = true;
        internal bool showButtonKeybindText = true;
        public CursorLockMode previousCursorLockState;
        public bool previousCursorVisible;
        internal bool persistentSettings = true;

        private ConfigEntry<float>? uiScaleConfig;
        private float uiScaleMultiplier = 1.0f;

        private float AutoScale => Screen.height / 1080f;
        private float EffectiveScale => AutoScale * uiScaleMultiplier;
        internal float S(float value) => value * EffectiveScale;
        internal int SI(float value) => Mathf.RoundToInt(value * EffectiveScale);


        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            InitMenu();
        }

        private void InitMenu()
        {
            toggleKey = Config.Bind<KeyCode>("Command Settings", "Toggle Key", KeyCode.F4, "Key to toggle the menu");
            showKeybindText = Config.Bind("Display Settings", "Show Keybind Text", true, "Show keybind text on screen").Value;
            showMenuButton = Config.Bind("Display Settings", "Show Menu Button", true, "Show menu button on screen").Value;
            uiScaleConfig = Config.Bind("Display Settings", "UI Scale Multiplier", 1.0f, "Manual UI scale multiplier (0.5 to 2.0), applied on top of automatic resolution scaling");
            uiScaleMultiplier = Mathf.Clamp(uiScaleConfig.Value, 0.5f, 2.0f);
            
            menuStyle = new GUIStyle
            {
                richText = true,
                fontSize = 20,
                normal = { textColor = Color.white }
            };

            

            foreach (var command in Settings.Settings.settingCommands)
            {
                RegisterCommand(command);
            }

            logger.LogInfo("Menu initialized");
        }

        private KeyCode GetToggleKey()
        {
            return toggleKey?.Value ?? KeyCode.F4;
        }

        private void Update()
        {
            if (UnityInput.Current.GetKeyDown(GetToggleKey()))
            {
                showMenu = !showMenu;
                if (showMenu)
                {
                    EnableCursor();
                }
                else
                {
                    RecoverCursorState();
                }
            }
            foreach (var command in registeredCommands)
            {
                if (command.IsKeybindPressed())
                {
                    if (command.IsToggle)
                    {
                        command.IsEnabled = !command.IsEnabled;
                        command.SaveConfig();
                    }
                    command.Execute(null);
                }
            }
        }

        private void DrawModMenu()
        {
            float buttonHeight = S(30f);
            float buttonSpacing = S(8f);
            float baseMenuWidth = S(250f);
            float menuWidth;

            GUIStyle scaledButton = new GUIStyle(GUI.skin.button);
            scaledButton.fontSize = SI(14);
            scaledButton.richText = true;

            GUIStyle scaledLabel = new GUIStyle(GUI.skin.label);
            scaledLabel.fontSize = SI(13);

            // Collect unique categories
            HashSet<string> uniqueCategories = new HashSet<string>();
            foreach (var command in registeredCommands)
            {
                uniqueCategories.Add(command.Category);
            }

            // Calculate total width required for category tabs
            float totalCategoryWidth = buttonSpacing;
            foreach (var category in uniqueCategories)
            {
                totalCategoryWidth += category.Length * S(10) + buttonSpacing;
            }

            // Adjust menu width based on total category width
            menuWidth = System.Math.Max(baseMenuWidth, totalCategoryWidth);

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.border = new RectOffset(3, 3, 3, 3);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));
            boxStyle.fontSize = SI(16);
            boxStyle.richText = true;

            float categoryTabsHeight = buttonHeight + S(20);

            // Collect commands for the selected category
            List<CustomCommand> commands = new List<CustomCommand>();
            foreach (var command in registeredCommands)
            {
                if (command.Category == selectedCategory)
                {
                    commands.Add(command);
                }
            }

            float commandWidth = S(230);
            float commandSpacing = buttonSpacing;
            float maxRowWidth = menuWidth - S(20);

            List<List<CustomCommand>> commandRows = new List<List<CustomCommand>>();
            List<CustomCommand> currentRow = new List<CustomCommand>();
            float currentRowWidth = 0;

            foreach (var command in commands)
            {
                float commandTotalWidth = commandWidth + commandSpacing;
                if (currentRowWidth + commandTotalWidth > maxRowWidth)
                {
                    commandRows.Add(currentRow);
                    currentRow = new List<CustomCommand>();
                    currentRowWidth = 0;
                }
                currentRow.Add(command);
                currentRowWidth += commandTotalWidth;
            }
            if (currentRow.Count > 0)
            {
                commandRows.Add(currentRow);
            }

            float commandsHeight = commandRows.Count * (buttonHeight + commandSpacing);

            // Scale slider section height
            float sliderSectionHeight = S(50f);

            float closeButtonHeight = buttonHeight + S(20);

            float totalHeight = categoryTabsHeight + commandsHeight + sliderSectionHeight + closeButtonHeight + S(40);

            Rect menuRect = new Rect(menuPosition.x, menuPosition.y, menuWidth, totalHeight);

            GUI.Box(menuRect, "<b><color=red>Mod Menu</color></b>", boxStyle);

            float currentXPosition = menuPosition.x + buttonSpacing;

            GUIStyle normalTabStyle = new GUIStyle(GUI.skin.button);
            normalTabStyle.fontSize = SI(13);
            GUIStyle selectedTabStyle = new GUIStyle(GUI.skin.button);
            selectedTabStyle.normal.textColor = Color.green;
            selectedTabStyle.fontStyle = FontStyle.Bold;
            selectedTabStyle.fontSize = SI(13);

            // Draw category tabs side by side at the top
            foreach (var category in uniqueCategories)
            {
                GUIStyle tabStyle = category == selectedCategory ? selectedTabStyle : normalTabStyle;
                if (GUI.Button(new Rect(currentXPosition, menuPosition.y + S(40), category.Length * S(10), buttonHeight), category, tabStyle))
                {
                    selectedCategory = category;
                }
                currentXPosition += category.Length * S(10) + buttonSpacing;
            }

            float currentYPosition = menuPosition.y + S(40) + buttonHeight + S(20);

            foreach (var row in commandRows)
            {
                float rowWidth = row.Count * (commandWidth + commandSpacing) - commandSpacing;
                float startX = menuPosition.x + (menuWidth - rowWidth) / 2;

                foreach (var command in row)
                {
                    string color = command.IsToggle ? (command.IsEnabled ? "green" : "red") : "white";
                    string buttontext = CreateButtonText(command);
                    if (GUI.Button(new Rect(startX, currentYPosition, commandWidth, buttonHeight), $"<color={color}>{buttontext}</color>", scaledButton))
                    {
                        if (command.Format.Split(' ').Length > 1)
                        {
                            showMenu = false;
                            showPopup = true;
                            selectedCommand = command;
                        }
                        else
                        {
                            try
                            {
                                if (command.IsToggle)
                                {
                                    command.IsEnabled = !command.IsEnabled;
                                    command.SaveConfig();
                                }
                                command.Execute(null);
                            }
                            catch (System.Exception e)
                            {
                                logger.LogError($"Error executing command {command.Name}: {e.Message}");
                                lastDisplayedMessage = $"Error executing command {command.Name}: {e.Message}";
                            }
                        }
                    }
                    startX += commandWidth + commandSpacing;
                }
                currentYPosition += buttonHeight + commandSpacing;
            }

            // UI Scale slider
            float sliderX = menuPosition.x + S(20);
            float sliderWidth = menuWidth - S(40);
            GUI.Label(
                new Rect(sliderX, currentYPosition + S(5), sliderWidth, S(20)),
                $"UI Scale: {uiScaleMultiplier:F2}x  (auto: {AutoScale:F2}x,  effective: {EffectiveScale:F2}x)",
                scaledLabel);
            float newMultiplier = GUI.HorizontalSlider(
                new Rect(sliderX, currentYPosition + S(27), sliderWidth, S(18)),
                uiScaleMultiplier, 0.5f, 2.0f);
            if (System.Math.Abs(newMultiplier - uiScaleMultiplier) > 0.001f)
            {
                uiScaleMultiplier = newMultiplier;
                if (uiScaleConfig != null) uiScaleConfig.Value = uiScaleMultiplier;
            }

            currentYPosition += sliderSectionHeight;

            if (GUI.Button(new Rect(menuPosition.x + (menuWidth - S(230)) / 2, currentYPosition, S(230), buttonHeight), "<b><color=red>Close Menu</color></b>", scaledButton))
            {
                showMenu = false;
                RecoverCursorState();
            }

            // Handle dragging
            if (Event.current.type == EventType.MouseDown && menuRect.Contains(Event.current.mousePosition))
            {
                isDragging = true;
                dragOffset = Event.current.mousePosition - new Vector2(menuRect.x, menuRect.y);
                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseDrag && isDragging)
            {
                menuPosition = Event.current.mousePosition - dragOffset;
                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseUp)
            {
                isDragging = false;
            }
        }

        private void OnGUI()
        {
            if (showKeybindText)
            {
                menuStyle!.fontSize = SI(20);
                GUI.Label(new Rect(10, 10, S(300), S(30)), $"<color=red>Press {GetToggleKey()} to toggle Mod Menu</color>", menuStyle);
            }

            if (showMenuButton && !showMenu)
            {
                GUIStyle menuBtnStyle = new GUIStyle(GUI.skin.button);
                menuBtnStyle.fontSize = SI(14);
                menuBtnStyle.fontStyle = FontStyle.Bold;
                menuBtnStyle.richText = true;
                if (GUI.Button(new Rect(10, S(50), S(200), S(30)), "<b><color=red>Open Mod Menu</color></b>", menuBtnStyle))
                {
                    showMenu = true;
                    EnableCursor();
                }
            }

            if (showLogWindow)
            {
                DrawLogWindow();
            }
        
            if (showMenu)
            {
                EnableCursor();
                DrawModMenu();
            } else
            {
                //RecoverCursorState();
            }
        
            if (showPopup)
            {
                ShowPopupForUserInput();
            }

            if (showCommandWindow)
            {
                DrawCommandWindow();
            }
        }

        // Helper method to create a texture
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void SaveCursorState()
        {
            previousCursorVisible = Cursor.visible;
            previousCursorLockState = Cursor.lockState;
        }

        public void RecoverCursorState()
        {
            Cursor.visible = previousCursorVisible;
            Cursor.lockState = previousCursorLockState;
        }

        public void EnableCursor()
        {
            if (!showCommandWindow)
            {
                SaveCursorState();
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void RegisterCommand(CustomCommand command)
        {
            if (!registeredCommands.Contains(command))
            {
                registeredCommands.Add(command);
                command.LoadConfig();
                logger.LogInfo($"Registered command: {command.Name}");
            }
            else
            {
                logger.LogWarning($"Command for {command.Name} is already registered.");
            }
        }

        public void UnregisterCommand(CustomCommand command)
        {
            if (registeredCommands.Contains(command))
            {
                registeredCommands.Remove(command);
                logger.LogInfo($"Unregistered command: {command.Name}");
            }
            else
            {
                logger.LogWarning($"Command for {command.Name} is not registered.");
            }
        }

        public string CreateButtonText(CustomCommand command)
        {
            if (showButtonKeybindText && command.Keybind != null)
            {
                return $"{command.Name} [{command.Keybind}]";
            }
            else
            {
                return command.Name;
            }
        }

#pragma warning disable CS8603 // Possible null reference return.
        public static CTDynamicModMenu Instance => instance; // Singleton pattern to access the mod instance
#pragma warning restore CS8603 // Possible null reference return.
    }
}