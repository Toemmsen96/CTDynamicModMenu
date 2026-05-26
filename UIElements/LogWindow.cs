using UnityEngine;

namespace CTDynamicModMenu
{
public partial class CTDynamicModMenu
    {
        private void DrawLogWindow(){
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.border = new RectOffset(3, 3, 3, 3);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));
            boxStyle.fontSize = SI(14);
            boxStyle.richText = true;

            float logWidth = S(300f);
            float logHeight = S(400f);
            Rect windowRect = new Rect(logWindowRect.x, logWindowRect.y, logWidth, logHeight);
            GUI.Box(windowRect, "<b><color=red>Log Window</color></b>", boxStyle);

            float buttonHeight = S(30f);
            float buttonWidth = S(100f);
            float padding = S(10f);

            float scrollViewHeight = windowRect.height - (3 * buttonHeight + 2 * padding);
            Rect scrollViewRect = new Rect(windowRect.x + padding, windowRect.y + padding + buttonHeight,
                                          windowRect.width - 2 * padding, scrollViewHeight);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = SI(12);
            float contentWidth = scrollViewRect.width - 20;

            float contentHeight = 10;
            foreach (string msg in logMessages)
            {
                GUIContent content = new GUIContent(msg);
                float msgHeight = labelStyle.CalcHeight(content, contentWidth);
                contentHeight += msgHeight;
            }

            Rect contentRect = new Rect(0, 0, contentWidth, contentHeight);

            logScrollPosition = GUI.BeginScrollView(scrollViewRect, logScrollPosition, contentRect);

            float yPos = 0;
            for (int i = logMessages.Count - 1; i >= 0; i--)
            {
                GUIContent content = new GUIContent(logMessages[i]);
                float msgHeight = labelStyle.CalcHeight(content, contentWidth);
                GUI.Label(new Rect(0, yPos, contentWidth, msgHeight), logMessages[i], labelStyle);
                yPos += msgHeight;
            }

            GUI.EndScrollView();

            GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
            closeStyle.fontSize = SI(14);
            closeStyle.richText = true;
            if (GUI.Button(new Rect(windowRect.x + (windowRect.width - buttonWidth) / 2, windowRect.y + windowRect.height - buttonHeight - padding, buttonWidth, buttonHeight), "<b><color=red>Close</color></b>", closeStyle))
            {
                showLogWindow = false;
            }

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
