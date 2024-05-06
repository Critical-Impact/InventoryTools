using System;
using System.IO;
using CriticalCommonLib;
using DalaMock.Dalamud;
using DalaMock.Mock;
using Dalamud;
using Lumina;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace InventoryToolsTesting.Services
{
    public class TestBoot
    {
        public IHost CreateHost()
        {
            var service = new Service();
            var mockProgram = new MockProgram(service, false);
            var pluginInterface = new MockPluginInterfaceService(mockProgram, new FileInfo(Path.Join(Environment.CurrentDirectory,"/","test.json")), new DirectoryInfo(Environment.CurrentDirectory));
            var mockFramework = new MockFramework();
            var logger = new LoggerConfiguration()
                .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Verbose))
                .CreateLogger();  
            var gameData = new Lumina.GameData( "C:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack", new LuminaOptions()
            {
                PanicOnSheetChecksumMismatch = false
            } );
            var mockService = new MockService(mockProgram, service, pluginInterface, mockFramework, gameData, ClientLanguage.English, logger);
            mockService.BuildMockServices();
            mockService.InjectMockServices();

            Service.Interface = pluginInterface;
            Service.KeyState = new TestKeyState();
            Service.TextureProvider = new TestTextureProvider();

            var loader = new TestPluginLoader(pluginInterface, service, mockFramework, logger);
            return loader.Build();
        }
    }
}