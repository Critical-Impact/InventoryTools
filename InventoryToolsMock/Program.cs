using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using ImGuiNET;
using InventoryTools.Logic;
using Lumina;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace InventoryToolsMock
{
    class Program
    {
        public static Sdl2Window _window;
        public static GraphicsDevice _gd;
        private static CommandList _cl;
        public static ImGuiController _controller;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        public static MockPlugin? _mockPlugin;
        public static MockSettingsWindow _mockSettingsWindow;
        public static GameData? GameData;

        static void Main(string[] args)
        {
            var field = typeof(ImGuiHelpers).GetProperty("GlobalScale", 
                BindingFlags.Static | 
                BindingFlags.Public);
            field.SetValue(null, 1);
            
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "Allagan Tools - Mocked"),
                new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                out _window,
                out _gd);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };
            
            _cl = _gd.ResourceFactory.CreateCommandList();

            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
            _mockSettingsWindow = new MockSettingsWindow();
            var property = typeof(ImGuiHelpers).GetProperty("MainViewport", 
                BindingFlags.Static | 
                BindingFlags.Public);
            property.SetValue(null, ImGui.GetMainViewport());
            if (AppSettings.Default.AutoStart)
            {
                StartPlugin();
            }
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _mockPlugin?._frameworkService.FireUpdate();
                _controller.Update(1f / 60f, snapshot);

                _mockPlugin?.Draw();
                _mockSettingsWindow.Draw();
                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            if (_mockPlugin != null)
            {
                _mockPlugin.Dispose();
                ConfigurationManager.Save();
            }

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }

        public static void StartPlugin()
        {
            var gameLocation = AppSettings.Default.GamePath;
            var configDirectory = AppSettings.Default.PluginConfigPath;

            if (gameLocation != null && configDirectory != null)
            {
                GameData = new GameData(gameLocation, new LuminaOptions()
                {
                    PanicOnSheetChecksumMismatch = false
                });
                _mockPlugin = new MockPlugin(GameData, configDirectory);
            }
        }

        public static void StopPlugin()
        {
            var mockPlugin = _mockPlugin;
            _mockPlugin = null;
            mockPlugin?.Dispose();
            GameData = null;
        }
    }
}