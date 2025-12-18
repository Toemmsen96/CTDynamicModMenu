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

                if (GUI.Button(new Rect(screenWidth / 2 - 80, screenHeight / 2 + 20, 160, 30), "Cancel"))
                {
                    showPopup = false;
                    showMenu = true;
                }
            }
        }
    }
}