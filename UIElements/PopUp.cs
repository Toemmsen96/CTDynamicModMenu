using System.Linq;
using CTDynamicModMenu.Commands;
using UnityEngine;

namespace CTDynamicModMenu
{
public partial class CTDynamicModMenu
    {
        private void ShowPopupForUserInput()
        {
            if (selectedCommand != null)
            {
                // Define a custom GUIStyle for the box
                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.border = new RectOffset(3, 3, 3, 3); // Set the border thickness
                boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f)); // Set a semi-transparent background

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                
                // Calculate dynamic width based on command format length
                string argumentsText = selectedCommand.Format.Substring(selectedCommand.Format.Split(' ')[0].Length).Trim();
                string headerText = "Enter Arguments for Command";
                float minWidth = 300f;
                float padding = 40f;
                
                GUIStyle labelStyle = GUI.skin.label;
                float headerWidth = labelStyle.CalcSize(new GUIContent(headerText)).x + padding;
                float argsWidth = labelStyle.CalcSize(new GUIContent("Arguments: " + argumentsText)).x + padding;
                float popupWidth = System.Math.Max(minWidth, System.Math.Max(headerWidth, argsWidth));
                float popupHeight = 200f;
                
                Rect popupRect = new Rect(screenWidth / 2 - popupWidth / 2, screenHeight / 2 - popupHeight / 2, popupWidth, popupHeight);

                GUI.Box(popupRect, "<b><color=red>" + headerText + "</color></b>\nArguments: " + argumentsText, boxStyle);

                float contentWidth = popupWidth - 40;
                GUI.SetNextControlName("UserInputField");
                userInput = GUI.TextField(new Rect(popupRect.x + 20, popupRect.y + 80, contentWidth, 30), userInput);
                GUI.FocusControl("UserInputField");

                float buttonWidth = 100f;
                float buttonHeight = 30f;
                float buttonY = popupRect.y + popupHeight - buttonHeight - 20;
                
                if (GUI.Button(new Rect(popupRect.x + popupWidth / 2 - buttonWidth - 10, buttonY, buttonWidth, buttonHeight), "<b><color=red>Cancel</color></b>"))
                {
                    showPopup = false;
                    showMenu = true;
                }

                if (GUI.Button(new Rect(popupRect.x + popupWidth / 2 + 10, buttonY, buttonWidth, buttonHeight), "<b><color=green>Confirm</color></b>"))
                {
                    string fullCommand = selectedCommand.Format.Split(' ')[0] + " " + userInput; //add command to front of arguments, not ideal
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