using UnityEngine;

namespace CTDynamicModMenu
{
    public partial class CTDynamicModMenu
    {
        // Helper method to create a texture
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        // Helper method to create a texture with rounded corners and anti-aliasing
        private Texture2D MakeRoundedTex(int width, int height, Color col, int cornerRadius)
        {
            Color[] pix = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    
                    // Check if pixel is in corner region
                    bool inTopLeft = x < cornerRadius && y < cornerRadius;
                    bool inTopRight = x >= width - cornerRadius && y < cornerRadius;
                    bool inBottomLeft = x < cornerRadius && y >= height - cornerRadius;
                    bool inBottomRight = x >= width - cornerRadius && y >= height - cornerRadius;
                    
                    if (inTopLeft)
                    {
                        float dx = cornerRadius - x - 0.5f;
                        float dy = cornerRadius - y - 0.5f;
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        float alpha = Mathf.Clamp01(cornerRadius - distance);
                        pix[i] = new Color(col.r, col.g, col.b, col.a * alpha);
                    }
                    else if (inTopRight)
                    {
                        float dx = x - (width - cornerRadius - 0.5f);
                        float dy = cornerRadius - y - 0.5f;
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        float alpha = Mathf.Clamp01(cornerRadius - distance);
                        pix[i] = new Color(col.r, col.g, col.b, col.a * alpha);
                    }
                    else if (inBottomLeft)
                    {
                        float dx = cornerRadius - x - 0.5f;
                        float dy = y - (height - cornerRadius - 0.5f);
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        float alpha = Mathf.Clamp01(cornerRadius - distance);
                        pix[i] = new Color(col.r, col.g, col.b, col.a * alpha);
                    }
                    else if (inBottomRight)
                    {
                        float dx = x - (width - cornerRadius - 0.5f);
                        float dy = y - (height - cornerRadius - 0.5f);
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        float alpha = Mathf.Clamp01(cornerRadius - distance);
                        pix[i] = new Color(col.r, col.g, col.b, col.a * alpha);
                    }
                    else
                    {
                        pix[i] = col;
                    }
                }
            }
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        // Helper method to create modern styled scrollbar
        private GUIStyle CreateScrollbarStyle()
        {
            GUIStyle scrollbarStyle = new GUIStyle();
            scrollbarStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.25f, 0.25f, 0.35f, 0.85f), 6);
            scrollbarStyle.fixedWidth = 14f;
            scrollbarStyle.border = new RectOffset(6, 6, 6, 6);
            scrollbarStyle.padding = new RectOffset(2, 2, 2, 2);
            scrollbarStyle.margin = new RectOffset(0, 0, 0, 0);
            return scrollbarStyle;
        }

        private GUIStyle CreateScrollbarThumbStyle()
        {
            GUIStyle thumbStyle = new GUIStyle();
            thumbStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.6f, 0.5f, 0.8f, 1f), 6);
            thumbStyle.hover.background = MakeRoundedTex(16, 16, new Color(0.7f, 0.6f, 0.9f, 1f), 6);
            thumbStyle.active.background = MakeRoundedTex(16, 16, new Color(0.5f, 0.4f, 0.7f, 1f), 6);
            thumbStyle.border = new RectOffset(6, 6, 6, 6);
            thumbStyle.padding = new RectOffset(0, 0, 0, 0);
            thumbStyle.fixedWidth = 14f;
            return thumbStyle;
        }

        private GUIStyle CreateHorizontalScrollbarStyle()
        {
            GUIStyle scrollbarStyle = new GUIStyle();
            scrollbarStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.25f, 0.25f, 0.35f, 0.85f), 6);
            scrollbarStyle.fixedHeight = 14f;
            scrollbarStyle.border = new RectOffset(6, 6, 6, 6);
            scrollbarStyle.padding = new RectOffset(2, 2, 2, 2);
            scrollbarStyle.margin = new RectOffset(0, 0, 0, 0);
            return scrollbarStyle;
        }

        private GUIStyle CreateHorizontalScrollbarThumbStyle()
        {
            GUIStyle thumbStyle = new GUIStyle();
            thumbStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.6f, 0.5f, 0.8f, 1f), 6);
            thumbStyle.hover.background = MakeRoundedTex(16, 16, new Color(0.7f, 0.6f, 0.9f, 1f), 6);
            thumbStyle.active.background = MakeRoundedTex(16, 16, new Color(0.5f, 0.4f, 0.7f, 1f), 6);
            thumbStyle.border = new RectOffset(6, 6, 6, 6);
            thumbStyle.padding = new RectOffset(0, 0, 0, 0);
            thumbStyle.fixedHeight = 14f;
            return thumbStyle;
        }

        private GUIStyle CreateNormalTabStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.normal.background = MakeRoundedTex(16, 16, new Color(0.2f, 0.2f, 0.25f, 0.9f), 5);
            style.hover.background = MakeRoundedTex(16, 16, new Color(0.3f, 0.3f, 0.35f, 1f), 5);
            style.normal.textColor = new Color(0.8f, 0.8f, 0.85f);
            style.hover.textColor = Color.white;
            style.fontSize = (int)(12 * uiScale);
            style.fontStyle = FontStyle.Bold;
            style.border = new RectOffset(5, 5, 5, 5);
            style.padding = new RectOffset(6, 6, 6, 6);
            return style;
        }

        private GUIStyle CreateSelectedTabStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.normal.background = MakeRoundedTex(16, 16, new Color(0.2f, 0.6f, 0.3f, 1f), 5);
            style.hover.background = MakeRoundedTex(16, 16, new Color(0.25f, 0.7f, 0.35f, 1f), 5);
            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.fontSize = (int)(12 * uiScale);
            style.fontStyle = FontStyle.Bold;
            style.border = new RectOffset(5, 5, 5, 5);
            style.padding = new RectOffset(6, 6, 6, 6);
            return style;
        }

        private GUIStyle CreateCommandButtonStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = (int)(13 * uiScale);
            style.fontStyle = FontStyle.Bold;
            style.border = new RectOffset(5, 5, 5, 5);
            style.padding = new RectOffset(8, 8, 6, 6);
            style.normal.background = MakeRoundedTex(16, 16, new Color(0.15f, 0.15f, 0.2f, 0.95f), 5);
            style.hover.background = MakeRoundedTex(16, 16, new Color(0.25f, 0.25f, 0.3f, 1f), 5);
            style.active.background = MakeRoundedTex(16, 16, new Color(0.3f, 0.3f, 0.35f, 1f), 5);
            return style;
        }

        private GUIStyle CreateCloseButtonStyle()
        {
            GUIStyle closeButtonStyle = new GUIStyle(GUI.skin.button);
            closeButtonStyle.fontSize = (int)(14 * uiScale);
            closeButtonStyle.fontStyle = FontStyle.Bold;
            closeButtonStyle.normal.background = MakeRoundedTex(16, 16, new Color(0.8f, 0.2f, 0.2f, 0.9f), 5);
            closeButtonStyle.hover.background = MakeRoundedTex(16, 16, new Color(1f, 0.3f, 0.3f, 1f), 5);
            closeButtonStyle.active.background = MakeRoundedTex(16, 16, new Color(0.6f, 0.15f, 0.15f, 1f), 5);
            closeButtonStyle.border = new RectOffset(5, 5, 5, 5);
            return closeButtonStyle;
        }

        private void DrawBorderFrame(Rect rect)
        {
            // Draw gradient border effect with rounded corners
            int cornerRadius = 8;
            Color borderColor1 = new Color(0.4f, 0.4f, 0.8f, 0.8f);
            
            // Create rounded border texture with gradient
            Texture2D borderTex = MakeRoundedTex(32, 32, borderColor1, cornerRadius);
            
            // Draw rounded border
            GUI.DrawTexture(rect, borderTex);
        }

        private void DrawResizeHandles(Rect rect)
        {
            float handleSize = 12f;
            Color handleColor = new Color(0.6f, 0.6f, 0.8f, 0.9f);
            Texture2D handleTex = MakeRoundedTex(16, 16, handleColor, 3);
            
            // Corner handles with rounded appearance
            GUI.DrawTexture(new Rect(rect.x, rect.y, handleSize, handleSize), handleTex); // Top-left
            GUI.DrawTexture(new Rect(rect.x + rect.width - handleSize, rect.y, handleSize, handleSize), handleTex); // Top-right
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - handleSize, handleSize, handleSize), handleTex); // Bottom-left
            GUI.DrawTexture(new Rect(rect.x + rect.width - handleSize, rect.y + rect.height - handleSize, handleSize, handleSize), handleTex); // Bottom-right
        }
    }
}
