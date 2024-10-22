using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.Collections.Generic;
using CTDynamicModMenu.Commands;
using BepInEx.Logging;
using UnityEngine.Windows;

namespace CTDynamicModMenu
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class CTDynamicModMenu : BaseUnityPlugin
    {
        private const string modGUID = "CTMods.CTDynamicModMenu";
        private const string modName = "CTDynamicModMenu";
        private const string modVersion = "1.0.0";

        private ConfigEntry<KeyCode> toggleKey;
        private GUIStyle menuStyle;
        private bool showMenu = false;
        private bool showPopup = false;
        internal bool showLogWindow = false;
        private string userInput = "";
        private string lastDisplayedMessage = "No message yet";
        private string fullMessageLog = "";
        private List<CustomCommand> registeredCommands = new List<CustomCommand>();
        private CustomCommand selectedCommand;
        private static CTDynamicModMenu instance;
        private ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        private Vector2 menuPosition = new Vector2(100, 100);
        private bool isDragging = false;
        private Vector2 dragOffset;
        private Rect logWindowRect = new Rect(10, 100, 300, 400);
        private Vector2 scrollPosition = Vector2.zero;        
        private string selectedCategory = "None";

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

        private void Update()
        {
            if (UnityInput.Current.GetKeyDown(toggleKey.Value))
            {
                showMenu = !showMenu;
            }
        }

        private void DrawModMenu()
        {
            float buttonHeight = 30f;
            float buttonSpacing = 8f;
            float baseMenuWidth = 250f;
            float menuWidth;
        
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
                totalCategoryWidth += category.Length * 10 + buttonSpacing;
            }
        
            // Adjust menu width based on total category width
            menuWidth = System.Math.Max(baseMenuWidth, totalCategoryWidth);
        
            // Define a custom GUIStyle for the box
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.border = new RectOffset(3, 3, 3, 3); // Set the border thickness
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f)); // Set a semi-transparent background
        
            // Calculate the height for category tabs
            float categoryTabsHeight = buttonHeight + 20; // 20 for padding
        
            // Collect commands for the selected category
            List<CustomCommand> commands = new List<CustomCommand>();
            foreach (var command in registeredCommands)
            {
                if (command.Category == selectedCategory)
                {
                    commands.Add(command);
                }
            }
        
            // Calculate the height for commands
            float commandWidth = 230;
            float commandSpacing = buttonSpacing;
            float maxRowWidth = menuWidth - 20; // Subtracting padding
        
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
        
            // Calculate the height for the close button
            float closeButtonHeight = buttonHeight + 20; // 20 for padding
        
            // Calculate total height
            float totalHeight = categoryTabsHeight + commandsHeight + closeButtonHeight + 40; // 40 for additional padding
        
            Rect menuRect = new Rect(menuPosition.x, menuPosition.y, menuWidth, totalHeight);
        
            GUI.Box(menuRect, "<b><color=red>Mod Menu</color></b>", boxStyle);
        
            float currentXPosition = menuPosition.x + buttonSpacing; // Start position for category tabs
        
            // Define GUI styles for tabs
            GUIStyle normalTabStyle = new GUIStyle(GUI.skin.button);
            GUIStyle selectedTabStyle = new GUIStyle(GUI.skin.button);
            selectedTabStyle.normal.textColor = Color.green;
            selectedTabStyle.fontStyle = FontStyle.Bold;
        
            // Draw category tabs side by side at the top
            foreach (var category in uniqueCategories)
            {
                GUIStyle tabStyle = category == selectedCategory ? selectedTabStyle : normalTabStyle;
                if (GUI.Button(new Rect(currentXPosition, menuPosition.y + 40, category.Length * 10, buttonHeight), category, tabStyle))
                {
                    selectedCategory = category;
                }
                currentXPosition += category.Length * 10 + buttonSpacing;
            }
        
            float currentYPosition = menuPosition.y + 40 + buttonHeight + 20; // Position for commands below the tabs
        
            foreach (var row in commandRows)
            {
                float rowWidth = row.Count * (commandWidth + commandSpacing) - commandSpacing;
                float startX = menuPosition.x + (menuWidth - rowWidth) / 2;
        
                foreach (var command in row)
                {
                    if (GUI.Button(new Rect(startX, currentYPosition, commandWidth, buttonHeight), command.Name))
                    {
                        if (command.Format.Split(' ').Length > 1)
                        {
                            showMenu = false;
                            showPopup = true;
                            selectedCommand = command;
                        }
                        else
                        {
                            command.Execute(null);
                        }
                    }
                    startX += commandWidth + commandSpacing;
                }
                currentYPosition += buttonHeight + commandSpacing;
            }
            if (GUI.Button(new Rect(menuPosition.x + (menuWidth - 230) / 2, currentYPosition, 230, buttonHeight), "<b><color=red>Close Menu</color></b>"))
            {
                showMenu = false;
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
            GUI.Label(new Rect(10, 10, 300, 30), $"<color=red>Press {toggleKey.Value} to toggle Mod Menu</color>", menuStyle);

            //Needs rework: log window is not compatible with old c#
            /*
            if (showLogWindow)
            {
                logWindowRect = GUILayout.Window(0, logWindowRect, (id) =>
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(280), GUILayout.Height(360));
                    GUILayout.TextArea(fullMessageLog);
                    GUILayout.EndScrollView();
                    GUI.DragWindow();
                }, "Log Window");
            }
            */
        
            if (showMenu)
            {
                DrawModMenu();
            }
        
            if (showPopup)
            {
                ShowPopupForUserInput();
            }
        }

        private void ShowPopupForUserInput()
        {
            if (selectedCommand != null)
            {
                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                Rect popupRect = new Rect(screenWidth / 2 - 100, screenHeight / 2 - 100, 200, 200);

                GUI.Box(popupRect, "Enter Arguments for Command");

                GUI.SetNextControlName("UserInputField");
                userInput = GUI.TextField(new Rect(screenWidth / 2 - 80, screenHeight / 2 - 60, 160, 30), userInput);
                GUI.FocusControl("UserInputField");

                if (GUI.Button(new Rect(screenWidth / 2 - 80, screenHeight / 2 - 20, 160, 30), "Confirm"))
                {
                    string fullCommand = selectedCommand.Format.Split(' ')[0] + " " + userInput; //add command to front of arguments, not ideal
                    userInput = string.Empty;
                    showPopup = false;
                    showMenu = true;
                    selectedCommand.Execute(CommandInput.Parse(fullCommand));
                    selectedCommand = null;
                }

                if (GUI.Button(new Rect(screenWidth / 2 - 80, screenHeight / 2 + 20, 160, 30), "Cancel"))
                {
                    showPopup = false;
                    showMenu = true;
                }
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

        public void RegisterCommand(CustomCommand command)
        {
            if (!registeredCommands.Contains(command))
            {
                registeredCommands.Add(command);
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

        public void DisplayMessage(string message)
        {
            lastDisplayedMessage = message;
            fullMessageLog = message + "\n";
            logger.LogInfo(message);
        }

        public static CTDynamicModMenu Instance => instance; // Singleton pattern to access the mod instance
    }
}