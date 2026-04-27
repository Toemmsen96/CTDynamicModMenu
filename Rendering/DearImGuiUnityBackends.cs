using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using ImGuiNET;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

namespace CTDynamicModMenu.Rendering
{
    // Backend type names intentionally include API tokens so the reflection selector can match them.
    internal sealed class DearImGuiD3DBackend : DearImGuiUnityBackendBase
    {
        public DearImGuiD3DBackend() : base(GraphicsDeviceType.Direct3D11, "DearImGuiD3DBackend") { }
    }

    internal sealed class DearImGuiOpenGLBackend : DearImGuiUnityBackendBase
    {
        public DearImGuiOpenGLBackend() : base(GraphicsDeviceType.OpenGLCore, "DearImGuiOpenGLBackend") { }
    }

    internal sealed class DearImGuiVulkanBackend : DearImGuiUnityBackendBase
    {
        public DearImGuiVulkanBackend() : base(GraphicsDeviceType.Vulkan, "DearImGuiVulkanBackend") { }
    }

    internal sealed class DearImGuiMetalBackend : DearImGuiUnityBackendBase
    {
        public DearImGuiMetalBackend() : base(GraphicsDeviceType.Metal, "DearImGuiMetalBackend") { }
    }

    internal abstract class DearImGuiUnityBackendBase : IDearImGuiRenderBackend
    {
        private static readonly MethodInfo? setScissorRectMethod = typeof(Graphics).GetMethod("SetScissorRect", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Rect) }, null);
        private static readonly MethodInfo? disableScissorRectMethod = typeof(Graphics).GetMethod("DisableScissorRect", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);

        private readonly GraphicsDeviceType backendType;
        private readonly string backendName;

        private IntPtr imGuiContext = IntPtr.Zero;
        private bool initialized;

        private Material? drawMaterial;
        private Texture2D? fontTexture;
        private Texture2D? fallbackTexture;

        private ImDrawDataPtr pendingDrawData;
        private bool hasPendingDrawData;
        private bool presentedFrameSinceLastCheck;
        private readonly List<char> pendingTextInput = new List<char>();
        private GameObject? overlayCameraGO;
        private Camera? overlayCamera;

        public GraphicsDeviceType BackendType => backendType;
        public string BackendName => backendName;

        protected DearImGuiUnityBackendBase(GraphicsDeviceType backendType, string backendName)
        {
            this.backendType = backendType;
            this.backendName = backendName;
        }

        public bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

            if (!CreateMaterial())
            {
                return false;
            }

            imGuiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imGuiContext);
            ImGui.StyleColorsDark();

            ImGuiIOPtr io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

            if (!CreateFontTexture(io))
            {
                return false;
            }

            overlayCameraGO = new GameObject("CTDynamicModMenu_ImGuiOverlay");
            overlayCameraGO.hideFlags = HideFlags.HideAndDontSave;
            overlayCamera = overlayCameraGO.AddComponent<Camera>();
            overlayCamera.clearFlags = CameraClearFlags.Nothing;
            overlayCamera.cullingMask = 0;
            overlayCamera.depth = 100;

            Camera.onPostRender += OnCameraPostRender;
            if (Keyboard.current != null)
                Keyboard.current.onTextInput += OnTextInput;
            InputSystem.onDeviceChange += OnInputDeviceChange;
            initialized = true;
            return true;
        }

        public void NewFrame(float deltaTime, int displayWidth, int displayHeight)
        {
            if (!initialized)
            {
                return;
            }

            ImGui.SetCurrentContext(imGuiContext);

            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(displayWidth, displayHeight);
            io.DeltaTime = Mathf.Max(deltaTime, 1f / 1000f);

            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 mousePos = mouse.position.ReadValue();
                io.MousePos = new System.Numerics.Vector2(mousePos.x, displayHeight - mousePos.y);
                io.MouseDown[0] = mouse.leftButton.isPressed;
                io.MouseDown[1] = mouse.rightButton.isPressed;
                io.MouseDown[2] = mouse.middleButton.isPressed;
                io.MouseWheel = mouse.scroll.ReadValue().y;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                io.KeyCtrl = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
                io.KeyAlt = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
                io.KeyShift = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                io.KeySuper = keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed;
            }

            foreach (char c in pendingTextInput)
                io.AddInputCharacter(c);
            pendingTextInput.Clear();

            ImGui.NewFrame();
        }

        public void Render()
        {
            if (!initialized)
            {
                return;
            }

            ImGui.SetCurrentContext(imGuiContext);
            ImGui.Render();
            pendingDrawData = ImGui.GetDrawData();
            hasPendingDrawData = pendingDrawData.CmdListsCount > 0;

            // Render directly if the overlay camera hasn't been created yet.
            if (hasPendingDrawData && overlayCamera == null)
            {
                RenderDrawData(pendingDrawData);
                hasPendingDrawData = false;
                presentedFrameSinceLastCheck = true;
            }
        }

        public bool ConsumePresentedFrame()
        {
            bool presented = presentedFrameSinceLastCheck;
            presentedFrameSinceLastCheck = false;
            return presented;
        }

        public void Shutdown()
        {
            if (!initialized)
            {
                return;
            }

            Camera.onPostRender -= OnCameraPostRender;
            InputSystem.onDeviceChange -= OnInputDeviceChange;
            if (Keyboard.current != null)
                Keyboard.current.onTextInput -= OnTextInput;
            hasPendingDrawData = false;

            if (overlayCameraGO != null)
            {
                UnityEngine.Object.Destroy(overlayCameraGO);
                overlayCameraGO = null;
                overlayCamera = null;
            }

            if (fontTexture != null)
            {
                UnityEngine.Object.Destroy(fontTexture);
                fontTexture = null;
            }

            if (fallbackTexture != null)
            {
                UnityEngine.Object.Destroy(fallbackTexture);
                fallbackTexture = null;
            }

            if (drawMaterial != null)
            {
                UnityEngine.Object.Destroy(drawMaterial);
                drawMaterial = null;
            }

            if (imGuiContext != IntPtr.Zero)
            {
                ImGui.DestroyContext(imGuiContext);
                imGuiContext = IntPtr.Zero;
            }

            initialized = false;
        }

        private void OnTextInput(char c)
        {
            pendingTextInput.Add(c);
        }

        private void OnInputDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is not Keyboard keyboard) return;
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
                keyboard.onTextInput += OnTextInput;
            else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
                keyboard.onTextInput -= OnTextInput;
        }

        private void OnCameraPostRender(Camera camera)
        {
            if (!hasPendingDrawData || camera != overlayCamera)
                return;

            RenderDrawData(pendingDrawData);
            hasPendingDrawData = false;
            presentedFrameSinceLastCheck = true;
        }

        private bool CreateMaterial()
        {
            Shader? shader = Shader.Find("UI/Default");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                return false;
            }

            drawMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            drawMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            drawMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            drawMaterial.SetInt("_Cull", (int)CullMode.Off);
            drawMaterial.SetInt("_ZWrite", 0);
            return true;
        }

        private bool CreateFontTexture(ImGuiIOPtr io)
        {
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);
            if (pixels == IntPtr.Zero || width <= 0 || height <= 0)
            {
                return false;
            }

            int byteCount = width * height * 4;
            byte[] pixelData = new byte[byteCount];
            Marshal.Copy(pixels, pixelData, 0, byteCount);

            fontTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            fontTexture.LoadRawTextureData(pixelData);
            fontTexture.Apply();

            fallbackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            fallbackTexture.SetPixel(0, 0, Color.white);
            fallbackTexture.Apply();

            io.Fonts.SetTexID(new IntPtr(1));
            io.Fonts.ClearTexData();
            return true;
        }

        private void RenderDrawData(ImDrawDataPtr drawData)
        {
            if (drawMaterial == null || drawData.CmdListsCount <= 0)
            {
                return;
            }

            float displayWidth = drawData.DisplaySize.X;
            float displayHeight = drawData.DisplaySize.Y;
            if (displayWidth <= 0f || displayHeight <= 0f)
            {
                return;
            }

            GL.PushMatrix();
            GL.LoadPixelMatrix(0f, displayWidth, displayHeight, 0f);

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];
                int indexOffset = 0;

                for (int cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex++)
                {
                    ImDrawCmdPtr drawCmd = cmdList.CmdBuffer[cmdIndex];
                    if (drawCmd.UserCallback != IntPtr.Zero)
                    {
                        indexOffset += (int)drawCmd.ElemCount;
                        continue;
                    }

                    if (!TryApplyScissor(drawData, drawCmd, displayHeight))
                    {
                        indexOffset += (int)drawCmd.ElemCount;
                        continue;
                    }

                    Texture texture = drawCmd.TextureId == new IntPtr(1)
                        ? (Texture)(fontTexture ?? fallbackTexture!)
                        : (Texture)(fontTexture ?? fallbackTexture!);

                    drawMaterial.mainTexture = texture;
                    drawMaterial.SetPass(0);

                    GL.Begin(GL.TRIANGLES);
                    for (int i = 0; i < drawCmd.ElemCount; i += 3)
                    {
                        DrawVertex(cmdList, indexOffset + i + 0);
                        DrawVertex(cmdList, indexOffset + i + 1);
                        DrawVertex(cmdList, indexOffset + i + 2);
                    }
                    GL.End();

                    DisableScissor();

                    indexOffset += (int)drawCmd.ElemCount;
                }
            }

            DisableScissor();
            GL.PopMatrix();
        }

        private static bool TryApplyScissor(ImDrawDataPtr drawData, ImDrawCmdPtr drawCmd, float displayHeight)
        {
            if (setScissorRectMethod == null || disableScissorRectMethod == null)
            {
                return true;
            }

            System.Numerics.Vector2 clipOffset = drawData.DisplayPos;
            System.Numerics.Vector2 clipScale = drawData.FramebufferScale;

            float clipX1 = (drawCmd.ClipRect.X - clipOffset.X) * clipScale.X;
            float clipY1 = (drawCmd.ClipRect.Y - clipOffset.Y) * clipScale.Y;
            float clipX2 = (drawCmd.ClipRect.Z - clipOffset.X) * clipScale.X;
            float clipY2 = (drawCmd.ClipRect.W - clipOffset.Y) * clipScale.Y;

            if (clipX1 < 0f) clipX1 = 0f;
            if (clipY1 < 0f) clipY1 = 0f;
            if (clipX2 < 0f) clipX2 = 0f;
            if (clipY2 < 0f) clipY2 = 0f;

            int x = Mathf.FloorToInt(clipX1);
            int yTop = Mathf.FloorToInt(clipY1);
            int x2 = Mathf.CeilToInt(clipX2);
            int y2 = Mathf.CeilToInt(clipY2);

            int width = x2 - x;
            int height = y2 - yTop;
            if (width <= 0 || height <= 0)
            {
                return false;
            }

            int y = Mathf.FloorToInt(displayHeight - y2);
            Rect scissorRect = new Rect(x, y, width, height);
            setScissorRectMethod.Invoke(null, new object[] { scissorRect });
            return true;
        }

        private static void DisableScissor()
        {
            if (disableScissorRectMethod == null)
            {
                return;
            }

            disableScissorRectMethod.Invoke(null, null);
        }

        private static void DrawVertex(ImDrawListPtr cmdList, int idxPosition)
        {
            ushort index = cmdList.IdxBuffer[idxPosition];
            ImDrawVertPtr vert = cmdList.VtxBuffer[index];

            uint col = vert.col;
            byte r = (byte)(col & 0xFF);
            byte g = (byte)((col >> 8) & 0xFF);
            byte b = (byte)((col >> 16) & 0xFF);
            byte a = (byte)((col >> 24) & 0xFF);

            GL.Color(new Color32(r, g, b, a));
            GL.TexCoord2(vert.uv.X, vert.uv.Y);
            GL.Vertex3(vert.pos.X, vert.pos.Y, 0f);
        }
    }
}
