using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Core.DI;
using DalaMock.Core.Mocks;
using DalaMock.Core.Plugin;
using DalaMock.Host.Mediator;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Veldrid.Sdl2;

namespace InventoryToolsTesting.Services
{
    public class TestBoot
    {
        private Logger seriLog;

        public IHost CreateHost()
        {
            Log.Logger = this.seriLog = new LoggerConfiguration()
                .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Verbose))
                .CreateLogger();

            var exdDataDir = Environment.GetEnvironmentVariable("EXD_DATA_DIR");
            if (exdDataDir is not null)
            {
                seriLog.Information("Attempting to use EXD_DATA_DIR environment variable.");
                if (Path.Exists(exdDataDir))
                {
                    seriLog.Information("EXD_DATA_DIR environment variable set to " + exdDataDir);
                    var dataPath = new DirectoryInfo(exdDataDir);
                    seriLog.Information($"DataPath.FullName = '{dataPath.FullName}'");
                    seriLog.Information($"DataPath.Name     = '{dataPath.Name}'");
                    seriLog.Information($"Chars in Name     = {string.Join(",", dataPath.Name.Select(c => (int)c))}");
                }
            }
            else
            {
                seriLog.Information("No EXD_DATA_DIR environment variable set.");
            }
            seriLog.Information(new DirectoryInfo("/home/runner/work/InventoryTools/InventoryTools/exd-data/sqpack").FullName);
            var mockContainer = new MockContainer(new MockDalamudConfiguration()
            {
                CreateWindow = false,
            }, builder =>
            {
                builder.RegisterType<MediatorService>();
            },new Dictionary<Type, Type>()
            {
                {typeof(IKeyState), typeof(TestKeyState)},
                {typeof(ITextureProvider), typeof(TestTextureProvider)}
            }, false);
            var pluginLoader = mockContainer.GetPluginLoader();
            var mockPlugin = pluginLoader.AddPlugin(typeof(InventoryToolsTestingPlugin));
            var pluginLoadSettings = new PluginLoadSettings(new DirectoryInfo(Environment.CurrentDirectory), new FileInfo(Path.Combine(Environment.CurrentDirectory, "test.json")));
            pluginLoadSettings.AssemblyLocation = this.GetType().Assembly.Location;
            pluginLoader.StartPlugin(mockPlugin, pluginLoadSettings);
            if (mockPlugin.Container == null)
            {
                throw new Exception("Container was not built.");
            }

            var inventoryToolsTestingPlugin = mockPlugin.DalamudPlugin as InventoryToolsTestingPlugin;
            if (inventoryToolsTestingPlugin == null)
            {
                throw new Exception("Plugin was not built.");
            }
            return inventoryToolsTestingPlugin.Host;
        }
    }
}