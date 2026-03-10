using System.Linq;
using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu
{
public partial class CTDynamicModMenu
    {
        private void ShowPopupForUserInput()
        {
            EnableCursor();
            if (selectedCommand != null)
            {
                // Enhanced GUIStyle for the box
                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.border = new RectOffset(8, 8, 8, 8);
                boxStyle.normal.background = MakeRoundedTex(32, 32, new Color(0.1f, 0.1f, 0.15f, 0.95f), 8);
                boxStyle.fontSize = 14;
                boxStyle.fontStyle = FontStyle.Bold;
                boxStyle.alignment = TextAnchor.UpperCenter;
                boxStyle.padding = new RectOffset(10, 10, 10, 10);

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                
                // Calculate dynamic width based on command format length
                string argumentsText = selectedCommand.Format.Substring(selectedCommand.Format.Split(' ')[0].Length).Trim();
                string headerText = "Enter Arguments for Command";
                float minWidth = 300f * uiScale;
                float padding = 40f;
                
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = (int)(12 * uiScale);
                float headerWidth = labelStyle.CalcSize(new GUIContent(headerText)).x + padding;
                float argsWidth = labelStyle.CalcSize(new GUIContent("Arguments: " + argumentsText)).x + padding;
                float popupWidth = System.Math.Max(minWidth, System.Math.Max(headerWidth, argsWidth));
                float popupHeight = 220f * uiScale;
                
                Rect popupRect = new Rect(screenWidth / 2 - popupWidth / 2, screenHeight / 2 - popupHeight / 2, popupWidth, popupHeight);

                // Draw border
                DrawBorderFrame(popupRect);
                
                GUI.Box(popupRect, $"<b><color=#FF6B6B>{headerText}</color></b>\n<color=#E0E0E0>Arguments: {argumentsText}</color>", boxStyle);
                
                // Draw title bar separator line
                float titleBarHeight = 65f * uiScale;
                Rect separatorRect = new Rect(popupRect.x + 10, popupRect.y + titleBarHeight, popupWidth - 20, 2);
                GUI.DrawTexture(separatorRect, MakeTex(2, 2, new Color(0.5f, 0.5f, 0.7f, 0.6f)));

                // Styled text field
                GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
                textFieldStyle.fontSize = (int)(13 * uiScale);
                textFieldStyle.padding = new RectOffset(8, 8, 6, 6);
                textFieldStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.15f, 0.15f, 0.2f, 0.95f), 5);
                textFieldStyle.normal.textColor = Color.white;
                textFieldStyle.focused.background = MakeRoundedTex(16, 16, new Color(0.2f, 0.2f, 0.3f, 1f), 5);
                textFieldStyle.focused.textColor = Color.white;
                textFieldStyle.border = new RectOffset(5, 5, 5, 5);
                
                float contentWidth = popupWidth - 40;
                GUI.SetNextControlName("UserInputField");
                userInput = GUI.TextField(new Rect(popupRect.x + 20, popupRect.y + 80 * uiScale, contentWidth, 35f * uiScale), userInput, textFieldStyle);
                GUI.FocusControl("UserInputField");

                // Styled buttons
                GUIStyle cancelButtonStyle = new GUIStyle(GUI.skin.button);
                cancelButtonStyle.fontSize = (int)(13 * uiScale);
                cancelButtonStyle.fontStyle = FontStyle.Bold;
                cancelButtonStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.8f, 0.2f, 0.2f, 0.9f), 5);
                cancelButtonStyle.hover.background = MakeRoundedTex(16, 16, new Color(1f, 0.3f, 0.3f, 1f), 5);
                cancelButtonStyle.active.background = MakeRoundedTex(16, 16, new Color(0.6f, 0.15f, 0.15f, 1f), 5);
                cancelButtonStyle.border = new RectOffset(5, 5, 5, 5);
                
                GUIStyle confirmButtonStyle = new GUIStyle(GUI.skin.button);
                confirmButtonStyle.fontSize = (int)(13 * uiScale);
                confirmButtonStyle.fontStyle = FontStyle.Bold;
                confirmButtonStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.2f, 0.6f, 0.3f, 0.9f), 5);
                confirmButtonStyle.hover.background = MakeRoundedTex(16, 16, new Color(0.25f, 0.7f, 0.35f, 1f), 5);
                confirmButtonStyle.active.background = MakeRoundedTex(16, 16, new Color(0.15f, 0.5f, 0.25f, 1f), 5);
                confirmButtonStyle.border = new RectOffset(5, 5, 5, 5);

                float buttonWidth = 100f * uiScale;
                float buttonHeight = 32f * uiScale;
                float buttonY = popupRect.y + popupHeight - buttonHeight - 15;
                
                if (GUI.Button(new Rect(popupRect.x + popupWidth / 2 - buttonWidth - 10, buttonY, buttonWidth, buttonHeight), "<b><color=white>✕ Cancel</color></b>", cancelButtonStyle))
                {
                    showPopup = false;
                    showMenu = true;
                }

                if (GUI.Button(new Rect(popupRect.x + popupWidth / 2 + 10, buttonY, buttonWidth, buttonHeight), "<b><color=white>✓ Confirm</color></b>", confirmButtonStyle))
                {
                    string fullCommand = selectedCommand.Format.Split(' ')[0] + " " + userInput;
                    userInput = string.Empty;
                    showPopup = false;
                    showMenu = true;
                    try
                    {
                        selectedCommand.Execute(CommandInput.Parse(fullCommand));
                    }
                    catch (System.Exception e)
                    {
                        logger.LogError($"Error executing command {selectedCommand.Name}: {e.Message}");
                        lastDisplayedMessage = $"Error executing command {selectedCommand.Name}: {e.Message}";
                    }
                    selectedCommand = null;
                }
            }
        }
    }
}