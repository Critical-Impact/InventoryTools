using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using InventoryTools.Commands;
using InventoryTools.Hotkeys;
using InventoryTools.IPC;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Logic.Features;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Misc;
using InventoryTools.Overlays;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Tooltips;
using InventoryTools.Ui;
using InventoryTools.Ui.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;
using OtterGui.Log;

namespace InventoryTools.Host;

public class PluginLoader : IDisposable
{
    private readonly IPluginInterfaceService _pluginInterfaceService;
    private readonly Service _service;
    public IHost? Host { get; private set; }

    public PluginLoader(IPluginInterfaceService pluginInterfaceService, Service service)
    {
        _pluginInterfaceService = pluginInterfaceService;
        _service = service;
    }

    public IHost Build()
    {
        if (!_pluginInterfaceService.ConfigDirectory.Exists)
        {
            _pluginInterfaceService.ConfigDirectory.Create();
        }
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
                    { typeof(CharacterRetainerPage), typeof(IConfigPage) },
                    { typeof(CraftFiltersPage), typeof(IConfigPage) },
                    { typeof(FiltersPage), typeof(IConfigPage) },
                    { typeof(ImportExportPage), typeof(IConfigPage) },
                };
                Dictionary<Type, Type> transientPairs = new Dictionary<Type, Type>()
                {
                    { typeof(SettingPage), typeof(IConfigPage) },
                    { typeof(FilterPage), typeof(IConfigPage) },
                };
                Dictionary<Type, Type> transientExternallyOwnedPairs = new Dictionary<Type, Type>()
                {
                    { typeof(GenericWindow), typeof(Window) },
                    { typeof(UintWindow), typeof(Window) },
                    { typeof(StringWindow), typeof(Window) },
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
                    foreach (var pair in transientExternallyOwnedPairs)
                    {
                        if (pair.Key.IsAssignableFrom(type))
                        {
                            builder.RegisterType(type).As(pair.Value).As(pair.Key).As(type).ExternallyOwned();
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
            
            //Dalamud service registrations
            hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.RegisterInstance(Service.AddonLifecycle).ExternallyOwned();
                builder.RegisterInstance(Service.Chat).ExternallyOwned();
                builder.RegisterInstance(Service.ClientState).ExternallyOwned();
                builder.RegisterInstance(Service.Commands).ExternallyOwned();
                builder.RegisterInstance(Service.Condition).ExternallyOwned();
                builder.RegisterInstance(Service.Data).ExternallyOwned();
                builder.RegisterInstance(Service.Framework).ExternallyOwned();
                builder.RegisterInstance(Service.GameGui).ExternallyOwned();
                builder.RegisterInstance(Service.GameInteropProvider).ExternallyOwned();
                builder.RegisterInstance(Service.Interface).ExternallyOwned();
                builder.RegisterInstance(Service.KeyState).ExternallyOwned();
                builder.RegisterInstance(Service.Log).ExternallyOwned();
                builder.RegisterInstance(Service.Network).ExternallyOwned();
                builder.RegisterInstance(Service.Objects).ExternallyOwned();
                builder.RegisterInstance(Service.Targets).ExternallyOwned();
                builder.RegisterInstance(Service.TextureProvider).ExternallyOwned();
                builder.RegisterInstance(Service.Toasts).ExternallyOwned();
                builder.RegisterInstance(Service.ContextMenu).ExternallyOwned();
                builder.RegisterInstance(Service.TitleScreenMenu).ExternallyOwned();
            });

            //Hosted service registrations
            hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
            {
                //Needs to be externally owned as the hosted service causes a second registration in the container which causes it to dispose twice even though it's only constructed once
                builder.RegisterType<ExcelCache>().SingleInstance().ExternallyOwned();
                builder.RegisterType<ConfigurationManagerService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<ContextMenuService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<ListService>().As<IListService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<OverlayService>().As<IOverlayService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<HostedUniversalis>().AsSelf().As<IUniversalis>().SingleInstance().ExternallyOwned();
                builder.RegisterType<WotsitIpc>().As<IWotsitIpc>().SingleInstance().ExternallyOwned();
                builder.RegisterType<ListFilterService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<MediatorService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<MigrationManagerService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<PluginCommandManager<PluginCommands>>().SingleInstance().ExternallyOwned();
                builder.RegisterType<PluginLogic>().SingleInstance().ExternallyOwned();
                builder.RegisterType<BootService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<ServiceConfigurator>().ExternallyOwned().SingleInstance();
                builder.RegisterType<WindowService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<TableService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<InventoryToolsUi>().SingleInstance().ExternallyOwned();
                builder.RegisterType<TeleporterService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<LaunchButtonService>().SingleInstance().ExternallyOwned();
                builder.RegisterType<HostedInventoryHistory>().SingleInstance().ExternallyOwned();
            });
            
            hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.Register(c => new HttpClient()).As<HttpClient>();
                builder.RegisterType<SeTime>().As<ISeTime>().SingleInstance();
                builder.RegisterType<ConfigurationWizardService>().SingleInstance();
                builder.RegisterType<FileDialogManager>().SingleInstance();
                builder.RegisterType<BackgroundTaskQueue>().As<IBackgroundTaskQueue>();
                builder.RegisterType<CharacterMonitor>().As<ICharacterMonitor>().SingleInstance();
                builder.RegisterType<ChatUtilities>().As<IChatUtilities>().SingleInstance();
                builder.RegisterType<ConfigurationWizardService>().As<IConfigurationWizardService>().SingleInstance();
                builder.RegisterType<HostedCraftMonitor>().AsSelf().As<ICraftMonitor>().SingleInstance();
                builder.RegisterType<FilterService>().As<IFilterService>().SingleInstance();
                builder.RegisterType<Font>().As<IFont>().SingleInstance();
                builder.RegisterType<GameInterface>().As<IGameInterface>().SingleInstance();
                builder.RegisterType<GameUiManager>().As<IGameUiManager>().SingleInstance();
                builder.RegisterType<HotkeyService>().As<IHotkeyService>().SingleInstance();
                builder.RegisterType<IconService>().As<IIconService>().SingleInstance();
                builder.RegisterType<InventoryMonitor>().As<IInventoryMonitor>().SingleInstance();
                builder.RegisterType<InventoryScanner>().As<IInventoryScanner>().SingleInstance();
                builder.RegisterType<MarketBoardService>().As<IMarketBoardService>().SingleInstance();
                builder.RegisterType<MarketCache>().As<IMarketCache>().SingleInstance();
                builder.RegisterType<MobTracker>().As<IMobTracker>().SingleInstance();
                builder.RegisterType<IPCService>().SingleInstance();
                builder.RegisterType<TeleporterIpc>().As<ITeleporterIpc>().SingleInstance();
                builder.RegisterType<TooltipService>().As<ITooltipService>().SingleInstance();
                builder.RegisterType<IconStorage>().SingleInstance();
                builder.RegisterType<MarketboardTaskQueue>().SingleInstance();
                builder.RegisterType<IconStorage>().SingleInstance();
                builder.RegisterType<ImGuiService>().SingleInstance();
                builder.RegisterType<InventoryHistory>().SingleInstance();
                builder.RegisterType<ListCategoryService>().SingleInstance();
                builder.RegisterType<Logger>().SingleInstance();
                builder.RegisterType<OdrScanner>().SingleInstance();
                builder.RegisterType<PluginCommands>().SingleInstance();
                builder.RegisterType<RightClickService>().SingleInstance();
                builder.RegisterType<TryOn>().SingleInstance();
                builder.RegisterType<TetrisGame>().SingleInstance();
                builder.RegisterType<WotsitIpc>().As<IWotsitIpc>().SingleInstance();
                builder.RegisterType<FilterTable>();
                builder.RegisterType<CraftItemTable>();
                builder.RegisterType<ListImportExportService>().SingleInstance();
                builder.RegisterType<VersionInfo>().SingleInstance();
                builder.RegisterType<CraftPricer>().SingleInstance();
                builder.RegisterType<HostedUniversalisConfiguration>().AsSelf().As<IHostedUniversalisConfiguration>().SingleInstance();

                //Transient
                builder.RegisterType<FilterState>();

                builder.Register(provider =>
                {
                    var configurationManager = provider.Resolve<ConfigurationManagerService>();
                    configurationManager.Load();
                    var configuration = configurationManager.GetConfig();
                    configuration.ClearDirtyFlags();
                    return configuration;
                }).As<InventoryToolsConfiguration>().SingleInstance();

                builder.Register<Func<string, ColumnConfiguration?>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return typeName => {
                        var columns = context.Resolve<IEnumerable<IColumn>>();
                        var column = columns.FirstOrDefault(column => column.GetType().Name == typeName);

                        if (column == null)
                        {
                            return null;
                        }

                        var columnConfiguration = new ColumnConfiguration(typeName);
                        columnConfiguration.Column = column;
                        return columnConfiguration;
                    };
                });
                
                builder.Register<Func<string,IColumn?>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return typeName =>
                    {
                        var columns = context.Resolve<IEnumerable<IColumn>>();
                        var column = columns.FirstOrDefault(column => column.GetType().Name == typeName);

                        return column;
                    };
                });
                
                builder.Register<Func<Type,IColumn>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return type =>
                    {
                        var column = (IColumn)context.Resolve(type);
                        return column;
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
                        genericWindow.Initialize();
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
                builder.Register<Func<Type, IFilter>>(c => {
                    var context = c.Resolve<IComponentContext>();
                    return type => { 
                        var filter = (IFilter)context.Resolve(type);
                        return filter;
                    };
                });
                builder.Register<Func<int, IBackgroundTaskQueue>>(c => {
                    return capacity => { 
                        var filter = new BackgroundTaskQueue(capacity);
                        return filter;
                    };
                });
            });
        hostBuilder
            .ConfigureServices(collection =>
            {
                collection.AddHostedService(p => p.GetRequiredService<ExcelCache>());
                collection.AddHostedService(p => p.GetRequiredService<MigrationManagerService>());
                collection.AddHostedService(p => p.GetRequiredService<ServiceConfigurator>());
                collection.AddHostedService(p => p.GetRequiredService<IListService>());
                collection.AddHostedService(p => p.GetRequiredService<ListFilterService>());
                collection.AddHostedService(p => p.GetRequiredService<MediatorService>());
                collection.AddHostedService(p => p.GetRequiredService<PluginLogic>());
                collection.AddHostedService(p => p.GetRequiredService<WindowService>());
                collection.AddHostedService(p => p.GetRequiredService<TableService>());
                collection.AddHostedService(p => p.GetRequiredService<IOverlayService>());
                collection.AddHostedService(p => p.GetRequiredService<IWotsitIpc>());
                collection.AddHostedService(p => p.GetRequiredService<TeleporterService>());
                collection.AddHostedService(p => p.GetRequiredService<ContextMenuService>());
                collection.AddHostedService(p => p.GetRequiredService<PluginCommandManager<PluginCommands>>());
                collection.AddHostedService(p => p.GetRequiredService<BootService>());
                collection.AddHostedService(p => p.GetRequiredService<ConfigurationManagerService>());
                collection.AddHostedService(p => p.GetRequiredService<InventoryToolsUi>());
                collection.AddHostedService(p => p.GetRequiredService<HostedUniversalis>());
                collection.AddHostedService(p => p.GetRequiredService<LaunchButtonService>());
                collection.AddHostedService(p => p.GetRequiredService<HostedInventoryHistory>());
                collection.AddHostedService(p => p.GetRequiredService<IPCService>());
                collection.AddHostedService(p => p.GetRequiredService<HostedCraftMonitor>());
            });
            PreBuild(hostBuilder);
            var builtHost = hostBuilder
                .Build();
            builtHost.StartAsync();
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
        Service.Log.Debug("Starting dispose of HostBuilder");
        Host?.StopAsync().GetAwaiter().GetResult();
        Host?.Dispose();
    }
}