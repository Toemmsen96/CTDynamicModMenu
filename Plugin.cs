using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.Collections.Generic;
using CTDynamicMenuMod.Commands;
using BepInEx.Logging;

namespace CTDynamicMenuMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class CTDynamicMenuMod : BaseUnityPlugin
    {
        private const string modGUID = "CTMods.CTDynamicMenu";
        private const string modName = "CTDynamicMenu";
        private const string modVersion = "1.0.0";

        private ConfigEntry<KeyCode> toggleKey;
        private GUIStyle menuStyle;
        private bool showMenu = false;
        private bool showPopup = false;
        private string userInput = string.Empty;
        private string lastDisplayedMessage = "No message yet";
        private List<CustomCommand> registeredCommands = new List<CustomCommand>();
        private CustomCommand selectedCommand;
        private static CTDynamicMenuMod instance;
        private ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

        private Vector2 menuPosition = new Vector2(100, 100);
        private bool isDragging = false;
        private Vector2 dragOffset;

        private List<string> categories = new List<string> {};
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

            logger.LogInfo("Menu initialized");
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey.Value))
            {
                showMenu = !showMenu;
            }
        }

        private void DrawModMenu()
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
        
            float buttonHeight = 30f;
            float buttonSpacing = 8f;
            float categoryHeight = categories.Count * (buttonHeight + buttonSpacing);
            float commandHeight = 0f;
        
            foreach (var command in registeredCommands)
            {
                if (!categories.Contains(command.Category))
                {
                    categories.Add(command.Category);
                }
                if (command.Category == selectedCategory)
                {
                    commandHeight += buttonHeight + buttonSpacing;
                }
            }
        
            float totalHeight = categoryHeight + commandHeight + 100; // Additional space for padding and other elements
            Rect menuRect = new Rect(menuPosition.x, menuPosition.y, 250, totalHeight);
        
            GUI.Box(menuRect, "<b><color=red>Mod Menu</color></b>");
        
            float currentYPosition = menuPosition.y + 40; // Start position for category tabs
        
            // Draw category tabs at the top
            foreach (var category in categories)
            {
                if (GUI.Button(new Rect(menuPosition.x + 10, currentYPosition, 230, buttonHeight), category))
                {
                    selectedCategory = category;
                }
                currentYPosition += buttonHeight + buttonSpacing;
            }
        
            currentYPosition += 20; // Add some space between tabs and commands
        
            // Draw commands for the selected category below the tabs
            foreach (var command in registeredCommands)
            {
                if (command.Category == selectedCategory)
                {
                    if (command.Format.Split(' ').Length > 1)
                    {
                        if (GUI.Button(new Rect(menuPosition.x + 10, currentYPosition, 230, buttonHeight), command.Name))
                        {
                            showMenu = false;
                            showPopup = true;
                            selectedCommand = command;
                        }
                    }
                    else
                    {
                        if (GUI.Button(new Rect(menuPosition.x + 10, currentYPosition, 230, buttonHeight), command.Name))
                        {
                            command.Execute(null);
                        }
                    }
                    currentYPosition += buttonHeight + buttonSpacing;
                }
            }
        
            if (GUI.Button(new Rect(menuPosition.x + 10, currentYPosition, 230, buttonHeight), "<b><color=red>Close Menu</color></b>"))
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
            GUI.Label(new Rect(10, 40, 300, 60), $"<color=grey>Last DBGMsg: {lastDisplayedMessage}</color>", menuStyle);

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
            logger.LogInfo(message);
        }

        public static CTDynamicMenuMod Instance => instance; // Singleton pattern to access the mod instance
    }
}