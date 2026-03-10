using UnityEngine;

namespace CTDynamicModMenu
{
    public partial class CTDynamicModMenu
    {
        private bool isHoveringInteractable = false;

        private void SetHoverCursor(Rect rect)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                isHoveringInteractable = true;
            }
        }

        private void UpdateCursor()
        {
            // Reset hover state at the beginning of each frame
            if (Event.current.type == EventType.Repaint)
            {
                isHoveringInteractable = false;
            }
        }

        private void HandleDragging(Rect menuRect)
        {
            if (!isResizing)
            {
                Rect titleBarRect = new Rect(menuRect.x, menuRect.y, menuRect.width, 40 * uiScale);
                
                if (titleBarRect.Contains(Event.current.mousePosition))
                {
                    isHoveringInteractable = true;
                }
                
                if (Event.current.type == EventType.MouseDown && titleBarRect.Contains(Event.current.mousePosition))
                {
                    isDragging = true;
                    dragOffset = Event.current.mousePosition - new Vector2(menuRect.x, menuRect.y);
                    Event.current.Use();
                }
                if (Event.current.type == EventType.MouseDrag && isDragging)
                {
                    menuPosition = (Event.current.mousePosition - dragOffset) / uiScale;
                    Event.current.Use();
                }
                if (Event.current.type == EventType.MouseUp)
                {
                    isDragging = false;
                }
            }
        }

        private void HandleResize(Rect rect)
        {
            Vector2 mousePos = Event.current.mousePosition;
            
            if (Event.current.type == EventType.MouseDown)
            {
                resizeEdge = DetectResizeEdge(rect, mousePos);
                if (resizeEdge != ResizeEdge.None)
                {
                    isResizing = true;
                    Event.current.Use();
                }
            }
            else if (Event.current.type == EventType.MouseDrag && isResizing)
            {
                ResizeMenu(resizeEdge, mousePos);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                isResizing = false;
                resizeEdge = ResizeEdge.None;
            }
            
            // Show hover cursor on resize edges
            if (!isResizing && DetectResizeEdge(rect, mousePos) != ResizeEdge.None)
            {
                isHoveringInteractable = true;
            }
        }

        private ResizeEdge DetectResizeEdge(Rect rect, Vector2 mousePos)
        {
            float edge = resizeEdgeThickness;
            bool left = mousePos.x >= rect.x && mousePos.x <= rect.x + edge;
            bool right = mousePos.x >= rect.x + rect.width - edge && mousePos.x <= rect.x + rect.width;
            bool top = mousePos.y >= rect.y && mousePos.y <= rect.y + edge;
            bool bottom = mousePos.y >= rect.y + rect.height - edge && mousePos.y <= rect.y + rect.height;
            
            if (top && left) return ResizeEdge.TopLeft;
            if (top && right) return ResizeEdge.TopRight;
            if (bottom && left) return ResizeEdge.BottomLeft;
            if (bottom && right) return ResizeEdge.BottomRight;
            if (left) return ResizeEdge.Left;
            if (right) return ResizeEdge.Right;
            if (top) return ResizeEdge.Top;
            if (bottom) return ResizeEdge.Bottom;
            
            return ResizeEdge.None;
        }

        private void ResizeMenu(ResizeEdge edge, Vector2 mousePos)
        {
            // Convert mouse position to unscaled coordinates
            Vector2 scaledMenuPos = menuPosition * uiScale;
            Vector2 scaledMenuSize = menuSize * uiScale;
            
            switch (edge)
            {
                case ResizeEdge.Right:
                    menuSize.x = Mathf.Max(minMenuSize.x, (mousePos.x - scaledMenuPos.x) / uiScale);
                    break;
                case ResizeEdge.Bottom:
                    menuSize.y = Mathf.Max(minMenuSize.y, (mousePos.y - scaledMenuPos.y) / uiScale);
                    break;
                case ResizeEdge.Left:
                    float newWidth = (scaledMenuPos.x + scaledMenuSize.x - mousePos.x) / uiScale;
                    if (newWidth >= minMenuSize.x)
                    {
                        menuPosition.x = mousePos.x / uiScale;
                        menuSize.x = newWidth;
                    }
                    break;
                case ResizeEdge.Top:
                    float newHeight = (scaledMenuPos.y + scaledMenuSize.y - mousePos.y) / uiScale;
                    if (newHeight >= minMenuSize.y)
                    {
                        menuPosition.y = mousePos.y / uiScale;
                        menuSize.y = newHeight;
                    }
                    break;
                case ResizeEdge.BottomRight:
                    menuSize.x = Mathf.Max(minMenuSize.x, (mousePos.x - scaledMenuPos.x) / uiScale);
                    menuSize.y = Mathf.Max(minMenuSize.y, (mousePos.y - scaledMenuPos.y) / uiScale);
                    break;
                case ResizeEdge.BottomLeft:
                    newWidth = (scaledMenuPos.x + scaledMenuSize.x - mousePos.x) / uiScale;
                    if (newWidth >= minMenuSize.x)
                    {
                        menuPosition.x = mousePos.x / uiScale;
                        menuSize.x = newWidth;
                    }
                    menuSize.y = Mathf.Max(minMenuSize.y, (mousePos.y - scaledMenuPos.y) / uiScale);
                    break;
                case ResizeEdge.TopRight:
                    menuSize.x = Mathf.Max(minMenuSize.x, (mousePos.x - scaledMenuPos.x) / uiScale);
                    newHeight = (scaledMenuPos.y + scaledMenuSize.y - mousePos.y) / uiScale;
                    if (newHeight >= minMenuSize.y)
                    {
                        menuPosition.y = mousePos.y / uiScale;
                        menuSize.y = newHeight;
                    }
                    break;
                case ResizeEdge.TopLeft:
                    newWidth = (scaledMenuPos.x + scaledMenuSize.x - mousePos.x) / uiScale;
                    newHeight = (scaledMenuPos.y + scaledMenuSize.y - mousePos.y) / uiScale;
                    if (newWidth >= minMenuSize.x)
                    {
                        menuPosition.x = mousePos.x / uiScale;
                        menuSize.x = newWidth;
                    }
                    if (newHeight >= minMenuSize.y)
                    {
                        menuPosition.y = mousePos.y / uiScale;
                        menuSize.y = newHeight;
                    }
                    break;
            }
        }
    }
}
