using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.Shared.Time;
using Autofac;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;

using DalaMock.Core.Mocks;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using InventoryTools;
using InventoryTools.IPC;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using InventoryToolsMock;
using InventoryToolsTesting.Services;
using Lumina;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace InventoryToolsTesting;

public class InventoryToolsTestingPlugin : InventoryToolsPlugin
{
    public InventoryToolsTestingPlugin(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog,
        IAddonLifecycle addonLifecycle, IChatGui chatGui, IClientState clientState, ICommandManager commandManager,
        ICondition condition, IDataManager dataManager, IFramework framework, IGameGui gameGui,
        IGameInteropProvider gameInteropProvider, IKeyState keyState, IGameNetwork gameNetwork,
        IObjectTable objectTable, ITargetManager targetManager, ITextureProvider textureProvider, IToastGui toastGui,
        IContextMenu contextMenu, ITitleScreenMenu titleScreenMenu) : base(pluginInterface, pluginLog,
        addonLifecycle, chatGui, clientState, commandManager, condition, dataManager, framework, gameGui,
        gameInteropProvider, keyState, gameNetwork, objectTable, targetManager, textureProvider, toastGui, contextMenu,
        titleScreenMenu)
    {
    }

    public override void ReplaceHostedServices(Dictionary<Type, Type> replacements)
    {
        replacements.Add(typeof(WotsitIpc),typeof(MockWotsitIpc));
        replacements.Add(typeof(CraftMonitor),typeof(MockHostedCraftMonitor));
        replacements.Add(typeof(OdrScanner),typeof(MockOdrScanner));
    }

    public override void PreBuild(IHostBuilder hostBuilder)
    {
        base.PreBuild(hostBuilder);

        hostBuilder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterType<TestInventoryMonitor>().AsSelf().As<IInventoryMonitor>().SingleInstance();
                containerBuilder.RegisterType<TestMarketCache>().As<IMarketCache>().SingleInstance();
                containerBuilder.RegisterType<MockSeTime>().As<ISeTime>().SingleInstance();
                containerBuilder.RegisterType<MockWotsitIpc>().As<IWotsitIpc>().SingleInstance();
                containerBuilder.RegisterType<MockHostedCraftMonitor>().As<ICraftMonitor>().SingleInstance();
                containerBuilder.RegisterType<MockOdrScanner>().As<IOdrScanner>().SingleInstance();
            }
        );
    }

    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService(p => p.GetRequiredService<HostedInventoryHistory>());
    }
}