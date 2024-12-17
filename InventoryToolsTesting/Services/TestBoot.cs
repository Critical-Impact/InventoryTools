using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Core.DI;
using DalaMock.Core.Mocks;
using DalaMock.Core.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Hosting;
using Veldrid.Sdl2;

namespace InventoryToolsTesting.Services
{
    public class TestBoot
    {
        public IHost CreateHost()
        {
            var mockContainer = new MockContainer(new MockDalamudConfiguration()
            {
                GamePath = new DirectoryInfo("/home/blair/.xlcore/ffxiv/game/sqpack"),
                PluginSavePath = new DirectoryInfo("/home/blair/.xlcore/DalaMockConfigs/"),
            }, builder =>
            {
                builder.RegisterType<MediatorService>();
            },new Dictionary<Type, Type>()
            {
                {typeof(IKeyState), typeof(TestKeyState)},
                {typeof(ITextureProvider), typeof(TestTextureProvider)}
            });
            var pluginLoader = mockContainer.GetPluginLoader();
            var mockPlugin = pluginLoader.AddPlugin(typeof(InventoryToolsTestingPlugin));
            pluginLoader.StartPlugin(mockPlugin, new PluginLoadSettings(new DirectoryInfo(Environment.CurrentDirectory), new FileInfo(Path.Combine(Environment.CurrentDirectory, "test.json"))));
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