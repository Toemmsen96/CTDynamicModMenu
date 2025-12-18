using System.Collections.Generic;
using UnityEngine;

namespace CTDynamicModMenu
{
public partial class CTDynamicModMenu
    {
        public bool showCommandWindow = false;
        private string commandInput = "";
        private List<string> commandSuggestions = new List<string>();
        private int selectedSuggestionIndex = -1;
        private Vector2 commandScrollPosition = Vector2.zero;
        
        private void DrawCommandWindow()
        {
            // Define a custom GUIStyle for the box
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.border = new RectOffset(3, 3, 3, 3);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));

            float windowWidth = 400f;
            float windowHeight = 300f;
            float windowX = Screen.width / 2 - windowWidth / 2;
            float windowY = Screen.height / 2 - windowHeight / 2;
            Rect windowRect = new Rect(windowX, windowY, windowWidth, windowHeight);

            GUI.Box(windowRect, "<b><color=red>Command Window</color></b>", boxStyle);

            float padding = 10f;
            float buttonHeight = 30f;
            float inputFieldY = windowRect.y + 40f;
            
            // Command input field
            GUI.SetNextControlName("CommandInputField");
            string previousInput = commandInput;
            commandInput = GUI.TextField(new Rect(windowRect.x + padding, inputFieldY, windowWidth - 2 * padding, buttonHeight), commandInput);
            
            // Auto-focus the input field
            if (showCommandWindow && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("CommandInputField");
            }

            // Handle input changes and update suggestions
            if (commandInput != previousInput)
            {
                UpdateCommandSuggestions();
                selectedSuggestionIndex = -1;
            }

            // Handle keyboard events
            if (Event.current.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == "CommandInputField")
            {
                if (Event.current.keyCode == KeyCode.Tab && commandSuggestions.Count > 0)
                {
                    // Tab completion
                    int index = selectedSuggestionIndex >= 0 ? selectedSuggestionIndex : 0;
                    commandInput = commandSuggestions[index];
                    selectedSuggestionIndex = -1;
                    UpdateCommandSuggestions();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.DownArrow && commandSuggestions.Count > 0)
                {
                    selectedSuggestionIndex = (selectedSuggestionIndex + 1) % commandSuggestions.Count;
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.UpArrow && commandSuggestions.Count > 0)
                {
                    selectedSuggestionIndex = selectedSuggestionIndex <= 0 ? commandSuggestions.Count - 1 : selectedSuggestionIndex - 1;
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    ExecuteCommandInput();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    showCommandWindow = false;
                    Event.current.Use();
                }
            }

            // Display autocomplete suggestions
            if (commandSuggestions.Count > 0)
            {
                float suggestionY = inputFieldY + buttonHeight + 5f;
                float suggestionHeight = System.Math.Min(commandSuggestions.Count * 25f, 150f);
                Rect scrollViewRect = new Rect(windowRect.x + padding, suggestionY, windowWidth - 2 * padding, suggestionHeight);
                
                float contentHeight = commandSuggestions.Count * 25f;
                Rect contentRect = new Rect(0, 0, windowWidth - 2 * padding - 20, contentHeight);
                
                commandScrollPosition = GUI.BeginScrollView(scrollViewRect, commandScrollPosition, contentRect);
                
                for (int i = 0; i < commandSuggestions.Count; i++)
                {
                    GUIStyle suggestionStyle = new GUIStyle(GUI.skin.label);
                    if (i == selectedSuggestionIndex)
                    {
                        suggestionStyle.normal.textColor = Color.yellow;
                        suggestionStyle.fontStyle = FontStyle.Bold;
                    }
                    
                    GUI.Label(new Rect(0, i * 25f, contentRect.width, 25f), commandSuggestions[i], suggestionStyle);
                }
                
                GUI.EndScrollView();
            }

            // Execute and Close buttons
            float buttonWidth = 100f;
            float buttonY = windowRect.y + windowHeight - buttonHeight - padding;
            
            if (GUI.Button(new Rect(windowRect.x + windowWidth / 2 - buttonWidth - 10, buttonY, buttonWidth, buttonHeight), "<b><color=green>Execute</color></b>"))
            {
                ExecuteCommandInput();
            }

            if (GUI.Button(new Rect(windowRect.x + windowWidth / 2 + 10, buttonY, buttonWidth, buttonHeight), "<b><color=red>Close</color></b>"))
            {
                showCommandWindow = false;
                commandInput = "";
                commandSuggestions.Clear();
                selectedSuggestionIndex = -1;
            }
        }

        private void UpdateCommandSuggestions()
        {
            commandSuggestions.Clear();
            
            if (string.IsNullOrEmpty(commandInput))
            {
                return;
            }

            string input = commandInput.ToLower().Trim();
            
            foreach (var command in registeredCommands)
            {
                string commandName = command.Format.Split(' ')[0].ToLower();
                
                if (commandName.StartsWith(input))
                {
                    commandSuggestions.Add(command.Format);
                }
                else if (commandName.Contains(input))
                {
                    commandSuggestions.Add(command.Format);
                }
            }
        }

        private void ExecuteCommandInput()
        {
            if (string.IsNullOrEmpty(commandInput))
            {
                return;
            }

            try
            {
                bool commandExecuted = false;
                foreach (var command in registeredCommands)
                {
                    if (command.Handle(commandInput))
                    {
                        commandExecuted = true;
                        logger.LogInfo($"Executed command: {commandInput}");
                        lastDisplayedMessage = $"Executed: {commandInput}";
                        break;
                    }
                }

                if (!commandExecuted)
                {
                    logger.LogWarning($"Command not found: {commandInput}");
                    lastDisplayedMessage = $"Command not found: {commandInput}";
                }
            }
            catch (System.Exception e)
            {
                logger.LogError($"Error executing command: {e.Message}");
                lastDisplayedMessage = $"Error: {e.Message}";
            }

            commandInput = "";
            commandSuggestions.Clear();
            selectedSuggestionIndex = -1;
            showCommandWindow = false;
        }
    }
}