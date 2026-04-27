using System;
using System.Numerics;
using CTDynamicModMenu.Commands;
using ImGuiNET;

namespace CTDynamicModMenu
{
    public partial class CTDynamicModMenu
    {
        private bool dearImGuiActive;

        private void RenderDearImGuiFrame(float deltaTime)
        {
            if (dearImGuiBackend == null || !dearImGuiActive)
            {
                return;
            }

            dearImGuiBackend.NewFrame(deltaTime, UnityEngine.Screen.width, UnityEngine.Screen.height);
            DrawDearImGuiOverlay();
            dearImGuiBackend.Render();
        }

        private void DrawDearImGuiOverlay()
        {
            if (showKeybindText)
            {
                ImGui.SetNextWindowBgAlpha(0.35f);
                ImGui.SetNextWindowPos(new Vector2(10f, 10f), ImGuiCond.Always);
                ImGui.Begin("CTDynamicModMenu_Hud", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav);
                ImGui.TextColored(new Vector4(1f, 0.2f, 0.2f, 1f), $"Press {GetToggleKey()} to toggle Mod Menu");
                ImGui.End();
            }

            if (showMenuButton && !showMenu)
            {
                ImGui.SetNextWindowPos(new Vector2(10f, 50f), ImGuiCond.Always);
                ImGui.SetNextWindowBgAlpha(0.5f);
                ImGui.Begin("CTDynamicModMenu_OpenButton", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings);
                if (ImGui.Button("Open Mod Menu"))
                {
                    showMenu = true;
                    EnableCursor();
                }
                ImGui.End();
            }

            if (showLogWindow)
            {
                DrawDearImGuiLogWindow();
            }

            if (showMenu)
            {
                EnableCursor();
                DrawDearImGuiMenu();
            }

            if (showCommandWindow)
            {
                DrawDearImGuiCommandWindow();
            }

            if (showPopup)
            {
                DrawDearImGuiPopup();
            }
        }

        private void DrawDearImGuiMenu()
        {
            ImGui.SetNextWindowSize(new Vector2(780f, 520f), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin("Mod Menu##DearImGui", ref showMenu))
            {
                ImGui.End();
                return;
            }

            var categories = new System.Collections.Generic.HashSet<string>();
            foreach (var command in registeredCommands)
            {
                categories.Add(command.Category);
            }

            foreach (string category in categories)
            {
                bool selected = category == selectedCategory;
                if (selected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.7f, 0.2f, 1f));
                }

                if (ImGui.Button(category))
                {
                    selectedCategory = category;
                }

                if (selected)
                {
                    ImGui.PopStyleColor();
                }

                ImGui.SameLine();
            }
            ImGui.NewLine();
            ImGui.Separator();

            foreach (var command in registeredCommands)
            {
                if (command.Category != selectedCategory)
                {
                    continue;
                }

                string buttonText = CreateButtonText(command);
                if (command.IsToggle)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, command.IsEnabled ? new Vector4(0.1f, 0.6f, 0.2f, 1f) : new Vector4(0.6f, 0.1f, 0.1f, 1f));
                }

                if (ImGui.Button(buttonText, new Vector2(300f, 0f)))
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
                        catch (Exception e)
                        {
                            logger.LogError($"Error executing command {command.Name}: {e.Message}");
                            lastDisplayedMessage = $"Error executing command {command.Name}: {e.Message}";
                        }
                    }
                }

                if (command.IsToggle)
                {
                    ImGui.PopStyleColor();
                }
            }

            if (ImGui.Button("Close Menu", new Vector2(200f, 0f)))
            {
                showMenu = false;
                RecoverCursorState();
            }

            ImGui.End();
        }

        private void DrawDearImGuiLogWindow()
        {
            if (!ImGui.Begin("Log Window##DearImGui", ref showLogWindow))
            {
                ImGui.End();
                return;
            }

            if (ImGui.Button("Clear"))
            {
                logMessages.Clear();
            }
            ImGui.Separator();

            ImGui.BeginChild("LogMessages", new Vector2(0f, -34f));
            for (int i = logMessages.Count - 1; i >= 0; i--)
            {
                ImGui.TextWrapped(logMessages[i]);
            }
            ImGui.EndChild();

            ImGui.End();
        }

        private void DrawDearImGuiCommandWindow()
        {
            ImGui.SetNextWindowSize(new Vector2(520f, 360f), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin("Command Window##DearImGui", ref showCommandWindow))
            {
                ImGui.End();
                return;
            }

            ImGui.SetKeyboardFocusHere();

            string currentInput = commandInput;
            if (ImGui.InputText("Command", ref currentInput, 512, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                commandInput = currentInput;
                ExecuteCommandInput();
            }
            else if (currentInput != commandInput)
            {
                commandInput = currentInput;
                UpdateCommandSuggestions();
                selectedSuggestionIndex = -1;
            }

            if (ImGui.IsKeyPressed(ImGuiKey.Tab, false) && commandSuggestions.Count > 0)
            {
                int index = selectedSuggestionIndex >= 0 ? selectedSuggestionIndex : 0;
                commandInput = commandSuggestions[index];
                selectedSuggestionIndex = -1;
                UpdateCommandSuggestions();
            }

            if (ImGui.IsKeyPressed(ImGuiKey.DownArrow, false) && commandSuggestions.Count > 0)
            {
                selectedSuggestionIndex = (selectedSuggestionIndex + 1) % commandSuggestions.Count;
            }

            if (ImGui.IsKeyPressed(ImGuiKey.UpArrow, false) && commandSuggestions.Count > 0)
            {
                selectedSuggestionIndex = selectedSuggestionIndex <= 0 ? commandSuggestions.Count - 1 : selectedSuggestionIndex - 1;
            }

            if (commandSuggestions.Count > 0)
            {
                ImGui.Separator();
                ImGui.BeginChild("CommandSuggestions", new Vector2(0f, 170f));
                for (int i = 0; i < commandSuggestions.Count; i++)
                {
                    bool selected = i == selectedSuggestionIndex;
                    if (selected)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 0f, 1f));
                    }

                    if (ImGui.Selectable(commandSuggestions[i], selected))
                    {
                        commandInput = commandSuggestions[i];
                        selectedSuggestionIndex = i;
                    }

                    if (selected)
                    {
                        ImGui.PopStyleColor();
                    }
                }
                ImGui.EndChild();
            }

            if (ImGui.Button("Execute", new Vector2(120f, 0f)))
            {
                ExecuteCommandInput();
            }
            ImGui.SameLine();
            if (ImGui.Button("Close", new Vector2(120f, 0f)))
            {
                showCommandWindow = false;
                commandInput = string.Empty;
                commandSuggestions.Clear();
                selectedSuggestionIndex = -1;
                RecoverCursorState();
            }

            ImGui.End();
        }

        private void DrawDearImGuiPopup()
        {
            CustomCommand? command = selectedCommand;
            if (command == null)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(560f, 220f), ImGuiCond.FirstUseEver);
            bool popupVisible = showPopup;
            if (!ImGui.Begin("Enter Arguments##DearImGui", ref popupVisible, ImGuiWindowFlags.NoCollapse))
            {
                showPopup = popupVisible;
                ImGui.End();
                return;
            }

            string commandName = command.Format.Split(' ')[0];
            string arguments = command.Format.Substring(commandName.Length).Trim();

            ImGui.Text($"Arguments: {arguments}");
            ImGui.Separator();

            ImGui.InputText("Input", ref userInput, 512);

            if (ImGui.Button("Cancel", new Vector2(120f, 0f)))
            {
                showPopup = false;
                showMenu = true;
                selectedCommand = null;
                userInput = string.Empty;
            }

            ImGui.SameLine();
            if (ImGui.Button("Confirm", new Vector2(120f, 0f)))
            {
                string fullCommand = commandName + " " + userInput;
                userInput = string.Empty;
                showPopup = false;
                showMenu = true;
                try
                {
                    command.Execute(Commands.CommandInput.Parse(fullCommand));
                }
                catch (Exception e)
                {
                    logger.LogError($"Error executing command {command.Name}: {e.Message}");
                    lastDisplayedMessage = $"Error executing command {command.Name}: {e.Message}";
                }
                selectedCommand = null;
            }

            showPopup = popupVisible;
            ImGui.End();
        }
    }
}
