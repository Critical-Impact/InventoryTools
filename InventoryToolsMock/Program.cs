using System.Numerics;
using ImGuiNET;
using InventoryTools.Ui;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace InventoryToolsMock
{
    class Program
    {
        private static Sdl2Window _window;
        public static GraphicsDevice _gd;
        private static CommandList _cl;
        public static ImGuiController _controller;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        private static MockPlugin _mockPlugin;

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Invalid arguments provided. Please provide the game directory, configuration directory and configuration file as arguments to the executable.");
                Console.ReadLine();
                return;
            }

            var gameLocation = args[0];
            if (!new DirectoryInfo(gameLocation).Exists)
            {
                Console.WriteLine("Game directory: " + gameLocation + " could not be found.");
                Console.ReadLine();
                return;
            }
            var configDirectory = args[1];
            if (!new DirectoryInfo(configDirectory).Exists)
            {
                Console.WriteLine("Config directory: " + configDirectory + " could not be found.");
                Console.ReadLine();
                return;
            }
            var configFile = args[2];
            if (!new FileInfo(configFile).Exists)
            {
                Console.WriteLine("Config file: " + configFile + " could not be found.");
                Console.ReadLine();
                return;
            }
            
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
            _mockPlugin = new MockPlugin(gameLocation, configDirectory, configFile);
            
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _controller.Update(1f / 60f, snapshot);

                _mockPlugin.Draw();

                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }
    }
}