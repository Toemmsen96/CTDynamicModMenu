using System;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine.Rendering;

namespace CTDynamicModMenu.Rendering
{
    internal sealed class ReflectionDearImGuiRenderBackend : IDearImGuiRenderBackend
    {
        private readonly object backendInstance;
        private readonly MethodInfo initializeMethod;
        private readonly MethodInfo newFrameMethod;
        private readonly MethodInfo renderMethod;
        private readonly MethodInfo? consumePresentedFrameMethod;
        private readonly MethodInfo shutdownMethod;

        public GraphicsDeviceType BackendType { get; }
        public string BackendName { get; }

        private ReflectionDearImGuiRenderBackend(
            object backend,
            GraphicsDeviceType backendType,
            string backendName,
            MethodInfo initialize,
            MethodInfo newFrame,
            MethodInfo render,
            MethodInfo? consumePresentedFrame,
            MethodInfo shutdown)
        {
            backendInstance = backend;
            BackendType = backendType;
            BackendName = backendName;
            initializeMethod = initialize;
            newFrameMethod = newFrame;
            renderMethod = render;
            consumePresentedFrameMethod = consumePresentedFrame;
            shutdownMethod = shutdown;
        }

        public static IDearImGuiRenderBackend? TryCreate(GraphicsDeviceType backendType, ManualLogSource logger)
        {
            string expectedBackendToken = BackendToken(backendType);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] exportedTypes;
                try
                {
                    exportedTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    exportedTypes = e.Types.Where(t => t != null).Cast<Type>().ToArray();
                }

                foreach (var type in exportedTypes)
                {
                    if (type == null || type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    if (type.Name.IndexOf("DearImGui", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(expectedBackendToken) &&
                        type.Name.IndexOf(expectedBackendToken, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    MethodInfo? initialize = type.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    MethodInfo? newFrame = type.GetMethod("NewFrame", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(float), typeof(int), typeof(int) }, null);
                    MethodInfo? render = type.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    MethodInfo? consumePresentedFrame = type.GetMethod("ConsumePresentedFrame", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    MethodInfo? shutdown = type.GetMethod("Shutdown", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

                    if (initialize == null || newFrame == null || render == null || shutdown == null)
                    {
                        continue;
                    }

                    if (initialize.ReturnType != typeof(bool))
                    {
                        continue;
                    }

                    ConstructorInfo? ctor = type.GetConstructor(Type.EmptyTypes);
                    if (ctor == null)
                    {
                        continue;
                    }

                    object backendInstance = ctor.Invoke(null);
                    logger.LogInfo($"Found Dear ImGui backend provider: {type.FullName}");

                    return new ReflectionDearImGuiRenderBackend(
                        backendInstance,
                        backendType,
                        type.Name,
                        initialize,
                        newFrame,
                        render,
                        consumePresentedFrame,
                        shutdown);
                }
            }

            return null;
        }

        public bool Initialize()
        {
            return (bool)initializeMethod.Invoke(backendInstance, null);
        }

        public void NewFrame(float deltaTime, int displayWidth, int displayHeight)
        {
            newFrameMethod.Invoke(backendInstance, new object[] { deltaTime, displayWidth, displayHeight });
        }

        public void Render()
        {
            renderMethod.Invoke(backendInstance, null);
        }

        public bool ConsumePresentedFrame()
        {
            if (consumePresentedFrameMethod == null)
            {
                return true;
            }

            return (bool)consumePresentedFrameMethod.Invoke(backendInstance, null);
        }

        public void Shutdown()
        {
            shutdownMethod.Invoke(backendInstance, null);
        }

        private static string BackendToken(GraphicsDeviceType backendType)
        {
            switch (backendType)
            {
                case GraphicsDeviceType.Direct3D11:
                case GraphicsDeviceType.Direct3D12:
                    return "D3D";
                case GraphicsDeviceType.OpenGLCore:
                case GraphicsDeviceType.OpenGLES2:
                case GraphicsDeviceType.OpenGLES3:
                    return "OpenGL";
                case GraphicsDeviceType.Vulkan:
                    return "Vulkan";
                case GraphicsDeviceType.Metal:
                    return "Metal";
                default:
                    return string.Empty;
            }
        }
    }
}
