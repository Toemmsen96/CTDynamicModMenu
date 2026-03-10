using UnityEngine;

namespace CTDynamicModMenu
{
public partial class CTDynamicModMenu
    {
        private void DrawLogWindow(){
            // Enhanced GUIStyle for the box
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.border = new RectOffset(8, 8, 8, 8);
            boxStyle.normal.background = MakeRoundedTex(32, 32, new Color(0.1f, 0.1f, 0.15f, 0.95f), 8);
            boxStyle.fontSize = (int)(14 * uiScale);
            boxStyle.fontStyle = FontStyle.Bold;
            boxStyle.alignment = TextAnchor.UpperCenter;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            Rect windowRect = new Rect(logWindowRect.x, logWindowRect.y, logWindowRect.width * uiScale, logWindowRect.height * uiScale);
            
            // Draw border
            DrawBorderFrame(windowRect);
            
            GUI.Box(windowRect, "<b><color=#FF6B6B>Log Window</color></b>", boxStyle);
            
            // Draw title bar separator line
            float titleBarHeight = 40f * uiScale;
            Rect separatorRect = new Rect(windowRect.x + 10, windowRect.y + titleBarHeight, windowRect.width - 20, 2);
            GUI.DrawTexture(separatorRect, MakeTex(2, 2, new Color(0.5f, 0.5f, 0.7f, 0.6f)));

            float buttonHeight = 32f * uiScale;
            float buttonWidth = 120f * uiScale;
            float padding = 10f * uiScale;

            // Create scrollable area for log messages
            float scrollViewHeight = windowRect.height - (buttonHeight + 60 * uiScale);
            Rect scrollViewRect = new Rect(windowRect.x + padding, windowRect.y + padding + 40 * uiScale, 
                                          windowRect.width - 2 * padding, scrollViewHeight);
            
            // Calculate content height based on number of messages
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = (int)(12 * uiScale);
            labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.95f);
            labelStyle.wordWrap = true;
            float contentWidth = scrollViewRect.width - 20;
            
            // Calculate total content height by measuring each message
            float contentHeight = 10;
            foreach (string msg in logMessages)
            {
                GUIContent content = new GUIContent(msg);
                float msgHeight = labelStyle.CalcHeight(content, contentWidth);
                contentHeight += msgHeight + 5;
            }
            
            Rect contentRect = new Rect(0, 0, contentWidth, contentHeight);
            
            // Styled scrollbar
            GUIStyle scrollbarStyle = CreateScrollbarStyle();
            GUIStyle scrollbarThumbStyle = CreateScrollbarThumbStyle();
            
            // Temporarily set scrollbar thumb style
            GUIStyle oldThumb = GUI.skin.verticalScrollbarThumb;
            GUI.skin.verticalScrollbarThumb = scrollbarThumbStyle;
            
            // Draw scroll view
            logScrollPosition = GUI.BeginScrollView(scrollViewRect, logScrollPosition, contentRect, false, true, GUIStyle.none, scrollbarStyle);
            
            float yPos = 0;
            for (int i = logMessages.Count - 1; i >= 0; i--) // Show newest first
            {
                GUIContent content = new GUIContent(logMessages[i]);
                float msgHeight = labelStyle.CalcHeight(content, contentWidth);
                GUI.Label(new Rect(5, yPos, contentWidth - 10, msgHeight), logMessages[i], labelStyle);
                yPos += msgHeight + 5;
            }
            
            GUI.EndScrollView();
            
            // Restore original thumb style
            GUI.skin.verticalScrollbarThumb = oldThumb;

            // Styled close button
            GUIStyle closeButtonStyle = new GUIStyle(GUI.skin.button);
            closeButtonStyle.fontSize = (int)(13 * uiScale);
            closeButtonStyle.fontStyle = FontStyle.Bold;
            closeButtonStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.8f, 0.2f, 0.2f, 0.9f), 5);
            closeButtonStyle.hover.background = MakeRoundedTex(16, 16, new Color(1f, 0.3f, 0.3f, 1f), 5);
            closeButtonStyle.active.background = MakeRoundedTex(16, 16, new Color(0.6f, 0.15f, 0.15f, 1f), 5);
            closeButtonStyle.border = new RectOffset(5, 5, 5, 5);
            
            if (GUI.Button(new Rect(windowRect.x + (windowRect.width - buttonWidth) / 2, windowRect.y + windowRect.height - buttonHeight - padding, buttonWidth, buttonHeight), "<b><color=white>✕ Close</color></b>", closeButtonStyle))
            {
                showLogWindow = false;
            }

            // Handle dragging
            Rect titleBarRect = new Rect(windowRect.x, windowRect.y, windowRect.width, 40 * uiScale);
            if (Event.current.type == EventType.MouseDown && titleBarRect.Contains(Event.current.mousePosition))
            {
                isDraggingLogWindow = true;
                dragOffsetLogWindow = Event.current.mousePosition - new Vector2(windowRect.x, windowRect.y);
                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseDrag && isDraggingLogWindow)
            {
                logWindowRect.position = (Event.current.mousePosition - dragOffsetLogWindow) / uiScale;
                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseUp)
            {
                isDraggingLogWindow = false;
            }
        }
    }
}