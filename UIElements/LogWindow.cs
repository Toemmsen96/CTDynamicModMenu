using UnityEngine;

namespace CTDynamicModMenu
{
public partial class CTDynamicModMenu
    {
        private void DrawLogWindow(){
            // Define a custom GUIStyle for the box
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.border = new RectOffset(3, 3, 3, 3); // Set the border thickness
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f)); // Set a semi-transparent background

            Rect windowRect = new Rect(logWindowRect.x, logWindowRect.y, logWindowRect.width, logWindowRect.height);
            GUI.Box(windowRect, "<b><color=red>Log Window</color></b>", boxStyle);

            float buttonHeight = 30f;
            float buttonWidth = 100f;
            float padding = 10f;

            // Create scrollable area for log messages
            float scrollViewHeight = windowRect.height - (3 * buttonHeight + 2 * padding);
            Rect scrollViewRect = new Rect(windowRect.x + padding, windowRect.y + padding + buttonHeight, 
                                          windowRect.width - 2 * padding, scrollViewHeight);
            
            // Calculate content height based on number of messages
            GUIStyle labelStyle = GUI.skin.label;
            float contentWidth = scrollViewRect.width - 20;
            
            // Calculate total content height by measuring each message
            float contentHeight = 10;
            foreach (string msg in logMessages)
            {
                GUIContent content = new GUIContent(msg);
                float msgHeight = labelStyle.CalcHeight(content, contentWidth);
                contentHeight += msgHeight;
            }
            
            Rect contentRect = new Rect(0, 0, contentWidth, contentHeight);
            
            // Draw scroll view
            logScrollPosition = GUI.BeginScrollView(scrollViewRect, logScrollPosition, contentRect);
            
            float yPos = 0;
            for (int i = logMessages.Count - 1; i >= 0; i--) // Show newest first
            {
                GUIContent content = new GUIContent(logMessages[i]);
                float msgHeight = labelStyle.CalcHeight(content, contentWidth);
                GUI.Label(new Rect(0, yPos, contentWidth, msgHeight), logMessages[i]);
                yPos += msgHeight;
            }
            
            GUI.EndScrollView();

            // Close button
            if (GUI.Button(new Rect(windowRect.x + (windowRect.width - buttonWidth) / 2, windowRect.y + windowRect.height - buttonHeight - padding, buttonWidth, buttonHeight), "<b><color=red>Close</color></b>"))
            {
                showLogWindow = false;
            }

            // Handle dragging
            if (Event.current.type == EventType.MouseDown && windowRect.Contains(Event.current.mousePosition))
            {
                isDraggingLogWindow = true;
                dragOffsetLogWindow = Event.current.mousePosition - new Vector2(windowRect.x, windowRect.y);
                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseDrag && isDraggingLogWindow)
            {
                logWindowRect.position = Event.current.mousePosition - dragOffsetLogWindow;
                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseUp)
            {
                isDraggingLogWindow = false;
            }
        }
    }
}