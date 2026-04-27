using UnityEngine.Rendering;

namespace CTDynamicModMenu.Rendering
{
    internal interface IDearImGuiRenderBackend
    {
        GraphicsDeviceType BackendType { get; }
        string BackendName { get; }
        bool Initialize();
        void NewFrame(float deltaTime, int displayWidth, int displayHeight);
        void Render();
        bool ConsumePresentedFrame();
        void Shutdown();
    }
}
