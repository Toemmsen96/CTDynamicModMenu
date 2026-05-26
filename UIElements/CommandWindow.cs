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

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.border = new RectOffset(3, 3, 3, 3);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));
            boxStyle.fontSize = SI(14);
            boxStyle.richText = true;

            float windowWidth = S(400f);
            float windowHeight = S(300f);
            float windowX = Screen.width / 2 - windowWidth / 2;
            float windowY = Screen.height / 2 - windowHeight / 2;
            Rect windowRect = new Rect(windowX, windowY, windowWidth, windowHeight);

            GUI.Box(windowRect, "<b><color=red>Command Window</color></b>", boxStyle);

            float padding = S(10f);
            float buttonHeight = S(30f);
            float inputFieldY = windowRect.y + S(40f);

            GUIStyle inputStyle = new GUIStyle(GUI.skin.textField);
            inputStyle.fontSize = SI(14);

            GUI.SetNextControlName("CommandInputField");
            string previousInput = commandInput;
            commandInput = GUI.TextField(new Rect(windowRect.x + padding, inputFieldY, windowWidth - 2 * padding, buttonHeight), commandInput, inputStyle);

            if (showCommandWindow && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("CommandInputField");
            }

            if (commandInput != previousInput)
            {
                UpdateCommandSuggestions();
                selectedSuggestionIndex = -1;
            }

            if (Event.current.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == "CommandInputField")
            {
                if (Event.current.keyCode == KeyCode.Tab && commandSuggestions.Count > 0)
                {
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

            if (commandSuggestions.Count > 0)
            {
                float suggestionY = inputFieldY + buttonHeight + S(5f);
                float suggestionHeight = System.Math.Min(commandSuggestions.Count * S(25f), S(150f));
                Rect scrollViewRect = new Rect(windowRect.x + padding, suggestionY, windowWidth - 2 * padding, suggestionHeight);

                float contentHeight = commandSuggestions.Count * S(25f);
                Rect contentRect = new Rect(0, 0, windowWidth - 2 * padding - 20, contentHeight);

                commandScrollPosition = GUI.BeginScrollView(scrollViewRect, commandScrollPosition, contentRect);

                for (int i = 0; i < commandSuggestions.Count; i++)
                {
                    GUIStyle suggestionStyle = new GUIStyle(GUI.skin.label);
                    suggestionStyle.fontSize = SI(13);
                    if (i == selectedSuggestionIndex)
                    {
                        suggestionStyle.normal.textColor = Color.yellow;
                        suggestionStyle.fontStyle = FontStyle.Bold;
                    }

                    GUI.Label(new Rect(0, i * S(25f), contentRect.width, S(25f)), commandSuggestions[i], suggestionStyle);
                }

                GUI.EndScrollView();
            }

            float buttonWidth = S(100f);
            float buttonY = windowRect.y + windowHeight - buttonHeight - padding;

            GUIStyle executeStyle = new GUIStyle(GUI.skin.button);
            executeStyle.fontSize = SI(14);
            executeStyle.richText = true;
            GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
            closeStyle.fontSize = SI(14);
            closeStyle.richText = true;

            if (GUI.Button(new Rect(windowRect.x + windowWidth / 2 - buttonWidth - S(10), buttonY, buttonWidth, buttonHeight), "<b><color=green>Execute</color></b>", executeStyle))
            {
                ExecuteCommandInput();
            }

            if (GUI.Button(new Rect(windowRect.x + windowWidth / 2 + S(10), buttonY, buttonWidth, buttonHeight), "<b><color=red>Close</color></b>", closeStyle))
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
