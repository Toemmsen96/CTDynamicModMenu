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
                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.border = new RectOffset(3, 3, 3, 3);
                boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));
                boxStyle.fontSize = SI(14);
                boxStyle.richText = true;

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;

                string argumentsText = selectedCommand.Format.Substring(selectedCommand.Format.Split(' ')[0].Length).Trim();
                string headerText = "Enter Arguments for Command";
                float minWidth = S(300f);
                float padding = S(40f);

                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = SI(14);
                float headerWidth = labelStyle.CalcSize(new GUIContent(headerText)).x + padding;
                float argsWidth = labelStyle.CalcSize(new GUIContent("Arguments: " + argumentsText)).x + padding;
                float popupWidth = System.Math.Max(minWidth, System.Math.Max(headerWidth, argsWidth));
                float popupHeight = S(200f);

                Rect popupRect = new Rect(screenWidth / 2 - popupWidth / 2, screenHeight / 2 - popupHeight / 2, popupWidth, popupHeight);

                GUI.Box(popupRect, "<b><color=red>" + headerText + "</color></b>\nArguments: " + argumentsText, boxStyle);

                float contentWidth = popupWidth - S(40);
                float inputHeight = S(30f);
                GUIStyle inputStyle = new GUIStyle(GUI.skin.textField);
                inputStyle.fontSize = SI(14);
                GUI.SetNextControlName("UserInputField");
                userInput = GUI.TextField(new Rect(popupRect.x + S(20), popupRect.y + S(80), contentWidth, inputHeight), userInput, inputStyle);
                GUI.FocusControl("UserInputField");

                float buttonWidth = S(100f);
                float buttonHeight = S(30f);
                float buttonY = popupRect.y + popupHeight - buttonHeight - S(20);

                GUIStyle cancelStyle = new GUIStyle(GUI.skin.button);
                cancelStyle.fontSize = SI(14);
                cancelStyle.richText = true;
                GUIStyle confirmStyle = new GUIStyle(GUI.skin.button);
                confirmStyle.fontSize = SI(14);
                confirmStyle.richText = true;

                if (GUI.Button(new Rect(popupRect.x + popupWidth / 2 - buttonWidth - S(10), buttonY, buttonWidth, buttonHeight), "<b><color=red>Cancel</color></b>", cancelStyle))
                {
                    showPopup = false;
                    showMenu = true;
                }

                if (GUI.Button(new Rect(popupRect.x + popupWidth / 2 + S(10), buttonY, buttonWidth, buttonHeight), "<b><color=green>Confirm</color></b>", confirmStyle))
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
