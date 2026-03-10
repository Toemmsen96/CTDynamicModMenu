using UnityEngine;
using System.Collections.Generic;
using CTDynamicModMenu.Commands;

namespace CTDynamicModMenu
{
    public partial class CTDynamicModMenu
    {
        private void DrawModMenu()
        {
            float buttonHeight = 32f * uiScale;
            float buttonSpacing = 10f * uiScale;
            float menuWidth = menuSize.x * uiScale;
            float menuHeight = menuSize.y * uiScale;
        
            Rect menuRect = new Rect(menuPosition.x * uiScale, menuPosition.y * uiScale, menuWidth, menuHeight);
        
            // Draw visual elements
            DrawBorderFrame(menuRect);
            DrawMenuBackground(menuRect);
            
            // Draw title bar separator line
            float titleBarHeight = 40f * uiScale;
            Rect separatorRect = new Rect(menuRect.x + 10, menuRect.y + titleBarHeight, menuRect.width - 20, 2);
            GUI.DrawTexture(separatorRect, MakeTex(2, 2, new Color(0.5f, 0.5f, 0.7f, 0.6f)));
        
            // Collect unique categories
            HashSet<string> uniqueCategories = new HashSet<string>();
            foreach (var command in registeredCommands)
            {
                uniqueCategories.Add(command.Category);
            }
        
            // Draw category tabs
            float tabsHeight = DrawCategoryTabs(uniqueCategories, new Vector2(menuRect.x, menuRect.y), menuWidth, buttonHeight, buttonSpacing);
        
            // Get commands for selected category
            List<CustomCommand> selectedCommands = GetCommandsForCategory(selectedCategory);
            List<List<CustomCommand>> commandRows = LayoutCommandRows(selectedCommands, menuWidth, buttonHeight, buttonSpacing);
        
            // Draw command buttons with scrolling
            DrawCommandButtons(commandRows, new Vector2(menuRect.x, menuRect.y), menuWidth, menuHeight, tabsHeight, buttonHeight, buttonSpacing);
        
            // Draw close button
            DrawCloseButton(new Vector2(menuRect.x, menuRect.y), menuWidth, menuHeight, buttonHeight);
        
            // Draw resize handles
            DrawResizeHandles(menuRect);
        
            // Handle resizing and dragging
            HandleResize(menuRect);
            HandleDragging(menuRect);
        }

        private void DrawMenuBackground(Rect menuRect)
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.border = new RectOffset(8, 8, 8, 8);
            boxStyle.normal.background = MakeRoundedTex(32, 32, new Color(0.1f, 0.1f, 0.15f, 0.95f), 8);
            boxStyle.fontSize = (int)(16 * uiScale);
            boxStyle.fontStyle = FontStyle.Bold;
            boxStyle.alignment = TextAnchor.UpperCenter;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);
            
            GUI.Box(menuRect, "<b><color=#FF6B6B> Mod Menu </color></b>", boxStyle);
        }

        private float DrawCategoryTabs(HashSet<string> categories, Vector2 position, float menuWidth, float buttonHeight, float spacing)
        {
            GUIStyle normalTabStyle = CreateNormalTabStyle();
            GUIStyle selectedTabStyle = CreateSelectedTabStyle();
            
            float tabAreaHeight = buttonHeight + 10;
            float tabY = position.y + 40;
            float availableWidth = menuWidth - (spacing * 2);
            
            // Create scrollable tab area if tabs exceed width
            Rect tabScrollRect = new Rect(position.x + spacing, tabY, availableWidth, tabAreaHeight);
            
            // Calculate total tab width
            float totalTabWidth = 0;
            foreach (var category in categories)
            {
                totalTabWidth += Mathf.Max(category.Length * 9 + 20, 80) + spacing;
            }
            
            Rect tabContentRect = new Rect(0, 0, Mathf.Max(totalTabWidth, availableWidth - 20), tabAreaHeight - 5);
            
            // Use scroll view with styled scrollbar if tabs exceed available width
            GUIStyle horizontalScrollbar = CreateHorizontalScrollbarStyle();
            GUIStyle horizontalScrollbarThumb = CreateHorizontalScrollbarThumbStyle();
            
            // Temporarily set scrollbar thumb style
            GUIStyle oldHThumb = GUI.skin.horizontalScrollbarThumb;
            GUI.skin.horizontalScrollbarThumb = horizontalScrollbarThumb;
            
            tabScrollPosition = GUI.BeginScrollView(tabScrollRect, tabScrollPosition, tabContentRect, 
                totalTabWidth > availableWidth, false, horizontalScrollbar, GUIStyle.none);
            
            float currentX = 0;
            foreach (var category in categories)
            {
                GUIStyle tabStyle = category == selectedCategory ? selectedTabStyle : normalTabStyle;
                float tabWidth = Mathf.Max(category.Length * 9 + 20, 80);
                
                Rect buttonRect = new Rect(currentX, 0, tabWidth, buttonHeight);
                SetHoverCursor(buttonRect);
                
                if (GUI.Button(buttonRect, category, tabStyle))
                {
                    selectedCategory = category;
                    menuCommandScrollPosition = Vector2.zero; // Reset scroll when changing category
                }
                currentX += tabWidth + spacing;
            }
            
            GUI.EndScrollView();
            
            // Restore original thumb style
            GUI.skin.horizontalScrollbarThumb = oldHThumb;
            
            return tabY + tabAreaHeight + 10; // Return Y position after tabs
        }

        private List<CustomCommand> GetCommandsForCategory(string category)
        {
            List<CustomCommand> commands = new List<CustomCommand>();
            foreach (var command in registeredCommands)
            {
                if (command.Category == category)
                {
                    commands.Add(command);
                }
            }
            return commands;
        }

        private List<List<CustomCommand>> LayoutCommandRows(List<CustomCommand> commands, float menuWidth, float buttonHeight, float spacing)
        {
            float commandWidth = Mathf.Min(230, menuWidth - 40);
            float maxRowWidth = menuWidth - 30;
            
            List<List<CustomCommand>> commandRows = new List<List<CustomCommand>>();
            List<CustomCommand> currentRow = new List<CustomCommand>();
            float currentRowWidth = 0;
        
            foreach (var command in commands)
            {
                float commandTotalWidth = commandWidth + spacing;
                if (currentRowWidth + commandTotalWidth > maxRowWidth && currentRow.Count > 0)
                {
                    commandRows.Add(currentRow);
                    currentRow = new List<CustomCommand>();
                    currentRowWidth = 0;
                }
                currentRow.Add(command);
                currentRowWidth += commandTotalWidth;
            }
            
            if (currentRow.Count > 0)
            {
                commandRows.Add(currentRow);
            }
            
            return commandRows;
        }

        private void DrawCommandButtons(List<List<CustomCommand>> commandRows, Vector2 position, float menuWidth, float menuHeight, float startY, float buttonHeight, float spacing)
        {
            GUIStyle commandButtonStyle = CreateCommandButtonStyle();
            
            float commandWidth = Mathf.Min(230, menuWidth - 40);
            float scrollViewHeight = menuHeight - (startY - position.y) - 80; // Reserve space for close button
            float contentHeight = commandRows.Count * (buttonHeight + spacing) + 20;
            
            Rect scrollViewRect = new Rect(position.x + 10, startY, menuWidth - 20, scrollViewHeight);
            Rect scrollContentRect = new Rect(0, 0, menuWidth - 40, contentHeight);
            
            // Apply styled scrollbar
            GUIStyle verticalScrollbar = this.CreateScrollbarStyle();
            GUIStyle verticalScrollbarThumb = this.CreateScrollbarThumbStyle();
            
            // Temporarily set scrollbar thumb style
            GUIStyle oldVThumb = GUI.skin.verticalScrollbarThumb;
            GUI.skin.verticalScrollbarThumb = verticalScrollbarThumb;
            
            menuCommandScrollPosition = GUI.BeginScrollView(scrollViewRect, menuCommandScrollPosition, scrollContentRect, 
                false, contentHeight > scrollViewHeight, GUIStyle.none, verticalScrollbar);
            
            float currentY = 0;
        
            foreach (var row in commandRows)
            {
                float rowWidth = row.Count * (commandWidth + spacing) - spacing;
                float startX = (menuWidth - rowWidth) / 2 - 10; // Account for scroll view offset
        
                foreach (var command in row)
                {
                    string color = command.IsToggle ? (command.IsEnabled ? "#4CAF50" : "#F44336") : "#E0E0E0";
                    string buttonText = CreateButtonText(command);
                    
                    Rect buttonRect = new Rect(startX, currentY, commandWidth, buttonHeight);
                    SetHoverCursor(buttonRect);
                    
                    if (GUI.Button(buttonRect, $"<color={color}>{buttonText}</color>", commandButtonStyle))
                    {
                        ExecuteCommand(command);
                    }
                    startX += commandWidth + spacing;
                }
                currentY += buttonHeight + spacing;
            }
        
            GUI.EndScrollView();
            
            // Restore original thumb style
            GUI.skin.verticalScrollbarThumb = oldVThumb;
        }

        private void DrawCloseButton(Vector2 position, float menuWidth, float menuHeight, float buttonHeight)
        {
            GUIStyle closeButtonStyle = CreateCloseButtonStyle();
        
            float closeButtonY = position.y + menuHeight - buttonHeight - 15 * uiScale;
            float closeButtonWidth = 200f * uiScale;
            
            Rect buttonRect = new Rect(position.x + (menuWidth - closeButtonWidth) / 2, closeButtonY, closeButtonWidth, buttonHeight);
            SetHoverCursor(buttonRect);
            
            if (GUI.Button(buttonRect, "<b><color=white>X Close Menu</color></b>", closeButtonStyle))
            {
                showMenu = false;
                RecoverCursorState();
            }
        }
    }
}
