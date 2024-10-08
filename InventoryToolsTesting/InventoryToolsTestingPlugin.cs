using System;
using Autofac;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.Time;
using DalaMock.Core.Mocks;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Interfaces;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using InventoryTools;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using InventoryToolsMock;
using InventoryToolsTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace InventoryToolsTesting;

public class InventoryToolsTestingPlugin : HostedPlugin
{
    public IHost BuiltHost { get; set; }
    public InventoryToolsTestingPlugin(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog, IDataManager dataManager, IFramework framework) : base(pluginInterface, pluginLog, dataManager, framework)
    {
        BuiltHost = CreateHost();
        Start();
    }

    public override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<MockWindow>().SingleInstance();
        containerBuilder.RegisterType<MockFont>().As<IFont>().SingleInstance();
        containerBuilder.RegisterType<MockCharacterMonitor>().As<ICharacterMonitor>().SingleInstance();
        containerBuilder.RegisterType<MockPluginLog>().As<IPluginLog>().SingleInstance();
        containerBuilder.RegisterType<MockTeleporterIpc>().As<ITeleporterIpc>().SingleInstance();
        containerBuilder.RegisterType<MockChatUtilities>().As<IChatUtilities>().SingleInstance();
        containerBuilder.RegisterType<MockGameInterface>().As<IGameInterface>().SingleInstance();
        containerBuilder.RegisterType<MockWotsitIpc>().As<IWotsitIpc>().SingleInstance();
        containerBuilder.RegisterType<TestInventoryMonitor>().AsSelf().As<IInventoryMonitor>().SingleInstance();
        containerBuilder.RegisterType<MockCraftMonitor>().AsSelf().As<ICraftMonitor>().SingleInstance();
        containerBuilder.RegisterType<MockInventoryScanner>().AsSelf().As<IInventoryScanner>().SingleInstance();
        containerBuilder.RegisterType<MockSeTime>().AsSelf().As<ISeTime>().SingleInstance();
        containerBuilder.RegisterType<MediatorService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<ExcelCache>().AsSelf().SingleInstance().ExternallyOwned();
        containerBuilder.RegisterType<ListFilterService>().AsSelf().SingleInstance();
        containerBuilder.RegisterType<HostedInventoryHistory>().As<InventoryHistory>().AsSelf().SingleInstance().ExternallyOwned();
        containerBuilder.RegisterInstance(new InventoryToolsConfiguration()).AsSelf().SingleInstance();
        containerBuilder.RegisterType<BackgroundTaskQueue>().As<IBackgroundTaskQueue>();
        containerBuilder.RegisterType<TestMarketCache>().As<IMarketCache>().SingleInstance();
        containerBuilder.RegisterType<CraftPricer>().SingleInstance();
        containerBuilder.RegisterType<FilterService>().As<IFilterService>().SingleInstance();
        containerBuilder.RegisterType<ListService>().As<IListService>().SingleInstance();
        containerBuilder.RegisterType<ConfigurationManagerService>().SingleInstance();
        containerBuilder.RegisterType<WindowService>().SingleInstance();
        containerBuilder.RegisterType<MockGameUiManager>().As<IGameUiManager>().SingleInstance();
        containerBuilder.Register<Func<Type, uint, UintWindow>>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return (type, id) =>
            {
                var uintWindow = (UintWindow)context.Resolve(type);
                uintWindow.Initialize(id);
                return uintWindow;
            };
        });
        containerBuilder.Register<Func<Type, string, StringWindow>>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return (type, id) =>
            {
                var stringWindow = (StringWindow)context.Resolve(type);
                stringWindow.Initialize(id);
                return stringWindow;
            };
        });
        containerBuilder.Register<Func<Type, GenericWindow>>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return type =>
            {
                var genericWindow = (GenericWindow)context.Resolve(type);
                genericWindow.Initialize();
                return genericWindow;
            };
        });
        containerBuilder.RegisterType<FilterState>();
        var seriLog = new LoggerConfiguration()
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
            .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Verbose))
            .CreateLogger();
        containerBuilder.RegisterInstance(seriLog).As<ILogger>().SingleInstance();
    }

    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService(p => p.GetRequiredService<ExcelCache>());
        serviceCollection.AddHostedService(p => p.GetRequiredService<HostedInventoryHistory>());
    }
}