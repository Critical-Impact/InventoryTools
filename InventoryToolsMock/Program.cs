using System.Numerics;
using System.Reflection;
using CriticalCommonLib;
using DalaMock;
using DalaMock.Configuration;
using DalaMock.Dalamud;
using DalaMock.Interfaces;
using DalaMock.Mock;
using Dalamud;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using InventoryTools.Logic;
using Lumina;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using ILogger = Lumina.ILogger;

namespace InventoryToolsMock
{
    class Program
    {
        private static MockProgram _program;
        private static MockPlugin? _mockPlugin;
        private static MockService? _mockService;
        private static MockSettingsWindow? _mockSettingsWindow;
        private static GameData? _gameData;

        public static MockPlugin? MockPlugin => _mockPlugin;

        static void Main(string[] args)
        {
            _program = new MockProgram(new Service());
            _mockPlugin = new MockPlugin();
            _program.SetPlugin(_mockPlugin);
            _mockSettingsWindow = new MockSettingsWindow(_program);
            

            if (AppSettings.Default.AutoStart)
            {
                _program.StartPlugin();
            }

            while (_program.PumpEvents(PreUpdate, PostUpdate))
            {
                
            }

            if (MockPlugin != null)
            {
                MockPlugin.Dispose();
                ConfigurationManager.Save();
            }

            _program.Dispose();
        }

        private static void PostUpdate()
        {
            MockPlugin?.Draw();
            _mockSettingsWindow?.Draw();
        }

        private static void PreUpdate()
        {
            MockPlugin?.MockFrameworkService?.FireUpdate();
        }

    }
}