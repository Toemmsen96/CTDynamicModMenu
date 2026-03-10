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
            EnableCursor();

            // Enhanced GUIStyle for the box
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.border = new RectOffset(8, 8, 8, 8);
            boxStyle.normal.background = MakeRoundedTex(32, 32, new Color(0.1f, 0.1f, 0.15f, 0.95f), 8);
            boxStyle.fontSize = (int)(14 * uiScale);
            boxStyle.fontStyle = FontStyle.Bold;
            boxStyle.alignment = TextAnchor.UpperCenter;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            float windowWidth = 450f * uiScale;
            float windowHeight = 350f * uiScale;
            float windowX = Screen.width / 2 - windowWidth / 2;
            float windowY = Screen.height / 2 - windowHeight / 2;
            Rect windowRect = new Rect(windowX, windowY, windowWidth, windowHeight);

            // Draw border
            DrawBorderFrame(windowRect);
            
            GUI.Box(windowRect, "<b><color=#FF6B6B>Command Window</color></b>", boxStyle);
            
            // Draw title bar separator line
            float titleBarHeight = 40f * uiScale;
            Rect separatorRect = new Rect(windowRect.x + 10, windowRect.y + titleBarHeight, windowRect.width - 20, 2);
            GUI.DrawTexture(separatorRect, MakeTex(2, 2, new Color(0.5f, 0.5f, 0.7f, 0.6f)));

            float padding = 10f * uiScale;
            float buttonHeight = 32f * uiScale;
            float inputFieldY = windowRect.y + 45f * uiScale;
            
            // Styled command input field
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.fontSize = (int)(13 * uiScale);
            textFieldStyle.padding = new RectOffset(8, 8, 6, 6);
            textFieldStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.15f, 0.15f, 0.2f, 0.95f), 5);
            textFieldStyle.normal.textColor = Color.white;
            textFieldStyle.focused.background = MakeRoundedTex(16, 16, new Color(0.2f, 0.2f, 0.3f, 1f), 5);
            textFieldStyle.focused.textColor = Color.white;
            textFieldStyle.border = new RectOffset(5, 5, 5, 5);
            
            // Command input field
            GUI.SetNextControlName("CommandInputField");
            string previousInput = commandInput;
            commandInput = GUI.TextField(new Rect(windowRect.x + padding, inputFieldY, windowWidth - 2 * padding, buttonHeight), commandInput, textFieldStyle);
            
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

            // Display autocomplete suggestions with styled scrollbar
            if (commandSuggestions.Count > 0)
            {
                float suggestionY = inputFieldY + buttonHeight + 10f * uiScale;
                float suggestionHeight = System.Math.Min(commandSuggestions.Count * 25f * uiScale, 150f * uiScale);
                Rect scrollViewRect = new Rect(windowRect.x + padding, suggestionY, windowWidth - 2 * padding, suggestionHeight);
                
                float contentHeight = commandSuggestions.Count * 25f * uiScale;
                Rect contentRect = new Rect(0, 0, windowWidth - 2 * padding - 20, contentHeight);
                
                // Styled scrollbar
                GUIStyle scrollbarStyle = CreateScrollbarStyle();
                GUIStyle scrollbarThumbStyle = CreateScrollbarThumbStyle();
                
                // Temporarily set scrollbar thumb style
                GUIStyle oldThumb = GUI.skin.verticalScrollbarThumb;
                GUI.skin.verticalScrollbarThumb = scrollbarThumbStyle;
                
                commandScrollPosition = GUI.BeginScrollView(scrollViewRect, commandScrollPosition, contentRect, false, true, GUIStyle.none, scrollbarStyle);
                
                for (int i = 0; i < commandSuggestions.Count; i++)
                {
                    GUIStyle suggestionStyle = new GUIStyle(GUI.skin.label);
                    suggestionStyle.fontSize = (int)(12 * uiScale);
                    suggestionStyle.padding = new RectOffset(5, 5, 3, 3);
                    
                    if (i == selectedSuggestionIndex)
                    {
                        suggestionStyle.normal.textColor = new Color(1f, 0.9f, 0.3f); // Yellow
                        suggestionStyle.fontStyle = FontStyle.Bold;
                    }
                    else
                    {
                        suggestionStyle.normal.textColor = new Color(0.85f, 0.85f, 0.9f);
                    }
                    
                    GUI.Label(new Rect(5, i * 25f * uiScale, contentRect.width - 10, 25f * uiScale), commandSuggestions[i], suggestionStyle);
                }
                
                GUI.EndScrollView();
                
                // Restore original thumb style
                GUI.skin.verticalScrollbarThumb = oldThumb;
            }

            // Styled Execute and Close buttons
            GUIStyle executeButtonStyle = new GUIStyle(GUI.skin.button);
            executeButtonStyle.fontSize = (int)(13 * uiScale);
            executeButtonStyle.fontStyle = FontStyle.Bold;
            executeButtonStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.2f, 0.6f, 0.3f, 0.9f), 5);
            executeButtonStyle.hover.background = MakeRoundedTex(16, 16, new Color(0.25f, 0.7f, 0.35f, 1f), 5);
            executeButtonStyle.active.background = MakeRoundedTex(16, 16, new Color(0.15f, 0.5f, 0.25f, 1f), 5);
            executeButtonStyle.border = new RectOffset(5, 5, 5, 5);
            
            GUIStyle closeButtonStyle = new GUIStyle(GUI.skin.button);
            closeButtonStyle.fontSize = (int)(13 * uiScale);
            closeButtonStyle.fontStyle = FontStyle.Bold;
            closeButtonStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.8f, 0.2f, 0.2f, 0.9f), 5);
            closeButtonStyle.hover.background = MakeRoundedTex(16, 16, new Color(1f, 0.3f, 0.3f, 1f), 5);
            closeButtonStyle.active.background = MakeRoundedTex(16, 16, new Color(0.6f, 0.15f, 0.15f, 1f), 5);
            closeButtonStyle.border = new RectOffset(5, 5, 5, 5);
            
            float buttonWidth = 110f * uiScale;
            float buttonY = windowRect.y + windowHeight - buttonHeight - padding;
            
            if (GUI.Button(new Rect(windowRect.x + windowWidth / 2 - buttonWidth - 10, buttonY, buttonWidth, buttonHeight), "<b><color=white>▶ Execute</color></b>", executeButtonStyle))
            {
                ExecuteCommandInput();
            }

            if (GUI.Button(new Rect(windowRect.x + windowWidth / 2 + 10, buttonY, buttonWidth, buttonHeight), "<b><color=white>✕ Close</color></b>", closeButtonStyle))
            {
                showCommandWindow = false;
                commandInput = "";
                commandSuggestions.Clear();
                selectedSuggestionIndex = -1;
                RecoverCursorState();
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
            RecoverCursorState();
        }
    }
}