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
        private Vector2 menuSize = new Vector2(400, 500);
        private Vector2 minMenuSize = new Vector2(250, 200);
        internal float uiScale = 1.0f;
        private bool isDragging = false;
        private Vector2 dragOffset;
        private bool isResizing = false;
        private ResizeEdge resizeEdge = ResizeEdge.None;
        private float resizeEdgeThickness = 10f;
        private Rect logWindowRect = new Rect(10, 100, 300, 400);
        private string selectedCategory = "None";
        private bool isDraggingLogWindow = false;
        private Vector2 dragOffsetLogWindow;
        private Vector2 menuCommandScrollPosition = Vector2.zero;
        private Vector2 tabScrollPosition = Vector2.zero;

        private enum ResizeEdge
        {
            None,
            Left,
            Right,
            Top,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        internal bool showKeybindText = true;
        internal bool showMenuButton = true;
        internal bool showButtonKeybindText = true;
        public CursorLockMode previousCursorLockState;
        public bool previousCursorVisible;
        internal bool persistentSettings = true;


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
            uiScale = Config.Bind("Display Settings", "UI Scale", 1.0f, "UI scale factor (0.5 - 2.0)").Value;
            uiScale = Mathf.Clamp(uiScale, 0.5f, 2.0f);
            
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

        private void OnGUI()
        {
            UpdateCursor();
            
            if (showKeybindText)
            {
                GUIStyle scaledMenuStyle = new GUIStyle(menuStyle);
                scaledMenuStyle.fontSize = (int)(20 * uiScale);
                GUI.Label(new Rect(10, 10, 300 * uiScale, 30 * uiScale), $"<color=red>Press {GetToggleKey()} to toggle Mod Menu</color>", scaledMenuStyle);
            }

            if (showMenuButton && !showMenu)
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = (int)(13 * uiScale);
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.8f, 0.2f, 0.2f, 0.9f), 5);
                buttonStyle.hover.background = MakeRoundedTex(16, 16, new Color(1f, 0.3f, 0.3f, 1f), 5);
                buttonStyle.border = new RectOffset(5, 5, 5, 5);
                
                Rect buttonRect = new Rect(10, 50, 200 * uiScale, 32 * uiScale);
                SetHoverCursor(buttonRect);
                
                if (GUI.Button(buttonRect, "<b><color=white>Open Mod Menu</color></b>", buttonStyle))
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

        private void ExecuteCommand(CustomCommand command)
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

        internal void SaveConfig()
        {
            Config.Bind("Display Settings", "UI Scale", 1.0f).Value = uiScale;
            Config.Save();
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