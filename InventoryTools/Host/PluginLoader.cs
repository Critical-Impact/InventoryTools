using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Ipc;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.Time;
using DalaMock.Shared.Classes;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Plugin.Services;
using InventoryTools.Commands;
using InventoryTools.GameUi;
using InventoryTools.Host;
using InventoryTools.Hotkeys;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Logic.Features;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Misc;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Test;
using InventoryTools.Tooltips;
using InventoryTools.Ui;
using InventoryTools.Ui.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;
using OtterGui.Log;

namespace InventoryTools;

public class PluginLoader : IDisposable
{
    private readonly IPluginInterfaceService _pluginInterfaceService;
    private readonly Service _service;
    private readonly CancellationTokenSource _pluginCts = new();
    private Task _hostBuilderRunTask;
    public IHost? Host { get; private set; }

    public PluginLoader(IPluginInterfaceService pluginInterfaceService, Service service)
    {
        _pluginInterfaceService = pluginInterfaceService;
        _service = service;
    }

    public IHost Build()
    {
        var hostBuilder = new HostBuilder();
        hostBuilder
            .UseContentRoot(_pluginInterfaceService.ConfigDirectory.FullName)
            .ConfigureLogging(lb =>
            {
                lb.ClearProviders();
                lb.AddDalamudLogging(Service.Log);
                lb.SetMinimumLevel(LogLevel.Trace);
            });
        //Load tooltips, hotkeys, filters
        hostBuilder
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                Dictionary<Type, Type> singletonPairs = new Dictionary<Type, Type>()
                {
                    { typeof(GenericWindow), typeof(Window) },
                    { typeof(CharacterRetainerPage), typeof(IConfigPage) },
                    { typeof(CraftFiltersPage), typeof(IConfigPage) },
                    { typeof(FiltersPage), typeof(IConfigPage) },
                    { typeof(ImportExportPage), typeof(IConfigPage) },
                };
                Dictionary<Type, Type> transientPairs = new Dictionary<Type, Type>()
                {
                    { typeof(UintWindow), typeof(Window) },
                    { typeof(StringWindow), typeof(Window) },
                    { typeof(SettingPage), typeof(IConfigPage) },
                    { typeof(FilterPage), typeof(IConfigPage) },
                };

                HashSet<Type> singletons = new HashSet<Type>()
                {
                    typeof(IHotkey), typeof(BaseTooltip), typeof(IAtkOverlay),typeof(IColumn),
                    typeof(IGameOverlay), typeof(IFilter), typeof(ISetting), typeof(IColumnSetting), typeof(IFeature)
                };

                HashSet<Type> transients = new HashSet<Type>()
                {
                };

                var loadableTypes = Assembly.GetExecutingAssembly().GetLoadableTypes().Where(c =>
                    c is { IsInterface: false, IsAbstract: false } &&
                    (!c.ContainsGenericParameters || c.IsConstructedGenericType)).ToList();
                foreach (var type in loadableTypes)
                {
                    foreach (var pair in singletonPairs)
                    {
                        if (pair.Key.IsAssignableFrom(type))
                        {
                            builder.RegisterType(type).As(pair.Value).As(pair.Key).As(type).SingleInstance();
                        }
                    }

                    foreach (var pair in transientPairs)
                    {
                        if (pair.Key.IsAssignableFrom(type))
                        {
                            builder.RegisterType(type).AsSelf().InstancePerDependency().As(pair.Value).As(pair.Key).As(type);
                        }
                    }

                    foreach (var singletonType in singletons)
                    {
                        if (singletonType.IsAssignableFrom(type))
                        {
                            builder.RegisterType(type).As(singletonType).As(type).SingleInstance();
                        }
                    }

                    foreach (var transient in transients)
                    {
                        if (transient.IsAssignableFrom(type))
                        {
                            builder.RegisterType(type).AsSelf().InstancePerDependency().As(transient);
                        }
                    }
                }
            });
            hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.RegisterType<AtkArmouryBoard>().As<IAtkOverlay>().As<AtkArmouryBoard>();
                builder.RegisterType<AtkCabinetWithdraw>().As<IAtkOverlay>().As<AtkCabinetWithdraw>();
                builder.RegisterType<AtkFreeCompanyChest>().As<IAtkOverlay>().As<AtkFreeCompanyChest>();
                builder.RegisterType<AtkInventoryBuddy>().As<IAtkOverlay>().As<AtkInventoryBuddy>();
                builder.RegisterType<AtkInventoryBuddy2>().As<IAtkOverlay>().As<AtkInventoryBuddy2>();
                builder.RegisterType<AtkInventoryExpansion>().As<IAtkOverlay>().As<AtkInventoryExpansion>();
                builder.RegisterType<AtkInventoryGrid>().As<IAtkOverlay>().As<AtkInventoryGrid>();
                builder.RegisterType<AtkInventoryLarge>().As<IAtkOverlay>().As<AtkInventoryLarge>();
                builder.RegisterType<AtkInventoryMiragePrismBox>().As<IAtkOverlay>().As<AtkInventoryMiragePrismBox>();
                builder.RegisterType<AtkInventoryRetainer>().As<IAtkOverlay>().As<AtkInventoryRetainer>();
                builder.RegisterType<AtkRetainerLarge>().As<IAtkOverlay>().As<AtkRetainerLarge>();
                builder.RegisterType<AtkRetainerList>().As<IAtkOverlay>().As<AtkRetainerList>();
                builder.RegisterType<AtkSelectIconString>().As<IAtkOverlay>().As<AtkSelectIconString>();
            });
            //Load Dalamud Services
            hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.RegisterInstance(Service.AddonLifecycle);
                builder.RegisterInstance(Service.Chat);
                builder.RegisterInstance(Service.ClientState);
                builder.RegisterInstance(Service.Commands);
                builder.RegisterInstance(Service.Condition);
                builder.RegisterInstance(Service.Data);
                builder.RegisterInstance(Service.Framework);
                builder.RegisterInstance(Service.GameGui);
                builder.RegisterInstance(Service.GameInteropProvider);
                builder.RegisterInstance(Service.Interface);
                builder.RegisterInstance(Service.KeyState);
                builder.RegisterInstance(Service.LibcFunction);
                builder.RegisterInstance(Service.Log);
                builder.RegisterInstance(Service.Network);
                builder.RegisterInstance(Service.Objects);
                builder.RegisterInstance(Service.SeTime);
                builder.RegisterInstance(Service.Targets);
                builder.RegisterInstance(Service.TextureProvider);
                builder.RegisterInstance(Service.Toasts);
            });
            //Load Services
            hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.RegisterType<ExcelCache>().SingleInstance();
                builder.RegisterType<ConfigurationManager>().SingleInstance();
                builder.RegisterType<ConfigurationWizardService>().SingleInstance();
                builder.RegisterType<ContextMenuService>().SingleInstance();
                builder.RegisterType<FileDialogManager>().SingleInstance();
                builder.RegisterType<BackgroundTaskQueue>().As<IBackgroundTaskQueue>();
                builder.RegisterType<CharacterMonitor>().As<ICharacterMonitor>().SingleInstance();
                builder.RegisterType<ChatUtilities>().As<IChatUtilities>().SingleInstance();
                builder.RegisterType<ConfigurationWizardService>().As<IConfigurationWizardService>().SingleInstance();
                builder.RegisterType<CraftMonitor>().As<ICraftMonitor>().SingleInstance();
                builder.RegisterType<FilterService>().As<IFilterService>().SingleInstance();
                builder.RegisterType<Font>().As<IFont>().SingleInstance();
                builder.RegisterType<GameInterface>().As<IGameInterface>().SingleInstance();
                builder.RegisterType<GameUiManager>().As<IGameUiManager>().SingleInstance();
                builder.RegisterType<GuiService>().As<IGuiService>().SingleInstance();
                builder.RegisterType<HotkeyService>().As<IHotkeyService>().SingleInstance();
                builder.RegisterType<IconService>().As<IIconService>().SingleInstance();
                builder.RegisterType<InventoryMonitor>().As<IInventoryMonitor>().SingleInstance();
                builder.RegisterType<InventoryScanner>().As<IInventoryScanner>().SingleInstance();
                builder.RegisterType<ListService>().As<IListService>().SingleInstance();
                builder.RegisterType<MarketBoardService>().As<IMarketBoardService>().SingleInstance();
                builder.RegisterType<MarketCache>().As<IMarketCache>().SingleInstance();
                builder.RegisterType<MobTracker>().As<IMobTracker>().SingleInstance();
                builder.RegisterType<OverlayService>().As<IOverlayService>().SingleInstance();
                builder.RegisterType<IPCService>().SingleInstance();
                builder.RegisterType<TeleporterIpc>().As<ITeleporterIpc>().SingleInstance();
                builder.RegisterType<TooltipService>().As<ITooltipService>().SingleInstance();
                builder.RegisterType<Universalis>().As<IUniversalis>().SingleInstance();
                builder.RegisterType<WotsitIpc>().As<IWotsitIpc>().SingleInstance();
                builder.RegisterType<IconStorage>().SingleInstance();
                builder.RegisterType<IconStorage>().SingleInstance();
                builder.RegisterType<ImGuiService>().SingleInstance();
                builder.RegisterType<InventoryHistory>().SingleInstance();
                builder.RegisterType<ListFilterService>().SingleInstance();
                builder.RegisterType<ListCategoryService>().SingleInstance();
                builder.RegisterType<Logger>().SingleInstance();
                builder.RegisterType<MediatorService>().SingleInstance();
                builder.RegisterType<MigrationManager>().SingleInstance();
                builder.RegisterType<OdrScanner>().SingleInstance();
                builder.RegisterType<PluginCommandManager<PluginCommands>>().SingleInstance();
                builder.RegisterType<PluginCommands>().SingleInstance();
                builder.RegisterType<PluginLogic>().SingleInstance();
                builder.RegisterType<PluginBoot>().SingleInstance();
                builder.RegisterType<RightClickService>().SingleInstance();
                builder.RegisterType<ServiceConfigurator>().SingleInstance();
                builder.RegisterType<TryOn>().SingleInstance();
                builder.RegisterType<TetrisGame>().SingleInstance();
                builder.RegisterType<WindowService>().SingleInstance();
                builder.RegisterType<TableService>().SingleInstance();
                builder.RegisterType<WotsitIpc>().As<IWotsitIpc>().SingleInstance();
                builder.RegisterType<FilterTable>();
                builder.RegisterType<CraftItemTable>();
                builder.RegisterType<InventoryToolsUi>().SingleInstance();

                //Transient
                builder.RegisterType<FilterState>();

                builder.Register(provider =>
                {
                    var configurationManager = provider.Resolve<ConfigurationManager>();
                    configurationManager.Load();
                    var configuration = configurationManager.GetConfig();
                    configuration.ClearDirtyFlags();
                    return configuration;
                }).As<InventoryToolsConfiguration>().SingleInstance();


                
                builder.Register<Func<string,IColumn?>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return typeName => { 
                        Type? type = Type.GetType($"InventoryTools.Logic.Columns." + typeName);
                        if (type == null)
                        {
                            return null;
                        }

                        var uintWindow = (IColumn)context.Resolve(type);
                        return uintWindow;
                    };
                });
                
                builder.Register<Func<FilterConfiguration,FilterTable>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return filterConfiguration => { 
                        var filterTable = context.Resolve<FilterTable>();
                        filterTable.Initialize(filterConfiguration);
                        return filterTable;
                    };
                });
                builder.Register<Func<FilterConfiguration,CraftItemTable>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return filterConfiguration => { 
                        var craftItemTable = context.Resolve<CraftItemTable>();
                        craftItemTable.Initialize(filterConfiguration);
                        return craftItemTable;
                    };
                });
                builder.Register<Func<Type,GenericWindow>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return type => { 
                        var genericWindow = (GenericWindow)context.Resolve(type);
                        return genericWindow;
                    };
                });
                builder.Register<Func<SettingCategory,SettingPage>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return settingCategory => { 
                        var settingPage = (SettingPage)context.Resolve(typeof(SettingPage));
                        settingPage.Initialize(settingCategory);
                        return settingPage;
                    };
                });
                builder.Register<Func<Type,uint, UintWindow>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return (type, id) => { 
                        var uintWindow = (UintWindow)context.Resolve(type);
                        uintWindow.Initialize(id);
                        return uintWindow;
                    };
                });
                builder.Register<Func<Type,string, StringWindow>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return (type, id) => { 
                        var stringWindow = (StringWindow)context.Resolve(type);
                        stringWindow.Initialize(id);
                        return stringWindow;
                    };
                });
                builder.Register<Func<FilterConfiguration,FilterState>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return filterConfiguration => { 
                        var filterState = (FilterState)context.Resolve(typeof(FilterState));
                        filterState.Initialize(filterConfiguration);
                        return filterState;
                    };
                });
                builder.Register<Func<FilterConfiguration,FilterPage>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return filterConfiguration => { 
                        var filterPage = (FilterPage)context.Resolve(typeof(FilterPage));
                        filterPage.Initialize(filterConfiguration);
                        return filterPage;
                    };
                });
                builder.Register<Func<Type,IConfigPage>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return type => { 
                        var configPage = (IConfigPage)context.Resolve(type);
                        configPage.Initialize();
                        return configPage;
                    };
                });
            });
        hostBuilder
            .ConfigureServices(collection =>
            {
                collection.AddHostedService(p => p.GetRequiredService<ExcelCache>());
                collection.AddHostedService(p => p.GetRequiredService<MigrationManager>());
                collection.AddHostedService(p => p.GetRequiredService<ServiceConfigurator>());
                collection.AddHostedService(p => p.GetRequiredService<IListService>());
                collection.AddHostedService(p => p.GetRequiredService<ListFilterService>());
                collection.AddHostedService(p => p.GetRequiredService<MediatorService>());
                collection.AddHostedService(p => p.GetRequiredService<PluginLogic>());
                collection.AddHostedService(p => p.GetRequiredService<PluginBoot>());
                collection.AddHostedService(p => p.GetRequiredService<WindowService>());
                collection.AddHostedService(p => p.GetRequiredService<TableService>());
                collection.AddHostedService(p => p.GetRequiredService<IWotsitIpc>());
                collection.AddHostedService(p => p.GetRequiredService<PluginCommandManager<PluginCommands>>());
                collection.AddHostedService(p => p.GetRequiredService<InventoryToolsUi>());
            });
            PreBuild(hostBuilder);
            var builtHost = hostBuilder
                .Build();
            _hostBuilderRunTask = builtHost
            .RunAsync(_pluginCts.Token);
            Host = builtHost;
            return builtHost;
    }

    /// <summary>
    /// Override this if you want to replace services before building
    /// </summary>
    /// <param name="hostBuilder"></param>
    public virtual void PreBuild(HostBuilder hostBuilder)
    {
        
    }

    public void Dispose()
    {
        _pluginCts.Cancel();
        _pluginCts.Dispose();
        _hostBuilderRunTask.Wait();
        Host?.Dispose();
    }
}