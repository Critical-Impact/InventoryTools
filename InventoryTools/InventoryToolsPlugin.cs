using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Util;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Ipc;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.Time;
using DalaMock.Host.Factories;
using DalaMock.Host.Hosting;
using DalaMock.Shared.Classes;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using InventoryTools.Commands;
using InventoryTools.Host;
using InventoryTools.Hotkeys;
using InventoryTools.IPC;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Logic.Editors;
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
using OtterGui.Log;
using Window = InventoryTools.Ui.Window;

namespace InventoryTools
{
    public class InventoryToolsPlugin : HostedPlugin
    {
        private readonly IPluginLog _pluginLog;
        private Service? _service;
        private IDalamudPluginInterface PluginInterface { get; set; }

        public InventoryToolsPlugin(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog,
            IAddonLifecycle addonLifecycle, IChatGui chatGui, IClientState clientState, ICommandManager commandManager,
            ICondition condition, IDataManager dataManager, IFramework framework, IGameGui gameGui,
            IGameInteropProvider gameInteropProvider, IKeyState keyState, IGameNetwork gameNetwork,
            IObjectTable objectTable, ITargetManager targetManager, ITextureProvider textureProvider,
            IToastGui toastGui, IContextMenu contextMenu, ITitleScreenMenu titleScreenMenu) : base(pluginInterface,
            pluginLog, addonLifecycle, chatGui, clientState, commandManager,
            condition, dataManager, framework, gameGui,
            gameInteropProvider, keyState, gameNetwork,
            objectTable, targetManager, textureProvider,
            toastGui, contextMenu, titleScreenMenu)
        {
            _pluginLog = pluginLog;
            PluginInterface = pluginInterface;
            _service = PluginInterface.Create<Service>()!;
            CreateHost();

            Start();

        }

        private List<Type> HostedServices { get; } = new()
        {
            typeof(ExcelCache),
            typeof(ConfigurationManagerService),
            typeof(ContextMenuService),
            typeof(ListService),
            typeof(OverlayService),
            typeof(HostedUniversalis),
            typeof(WotsitIpc),
            typeof(ListFilterService),
            typeof(MediatorService),
            typeof(MigrationManagerService),
            typeof(PluginCommandManager<PluginCommands>),
            typeof(PluginLogic),
            typeof(BootService),
            typeof(ServiceConfigurator),
            typeof(WindowService),
            typeof(TableService),
            typeof(InventoryToolsUi),
            typeof(TeleporterService),
            typeof(LaunchButtonService),
            typeof(HostedInventoryHistory),
            typeof(OdrScanner),
            typeof(IPCService),
            typeof(HostedCraftMonitor),
            typeof(ItemSearchService),

        };

        public List<Type> GetHostedServices()
        {
            var hostedServices = HostedServices.ToList();
            Dictionary<Type, Type> replacements = new();
            ReplaceHostedServices(replacements);
            foreach (var replacement in replacements)
            {
                hostedServices.Remove(replacement.Key);
                hostedServices.Add(replacement.Value);
            }

            return hostedServices;
        }

        public virtual void ReplaceHostedServices(Dictionary<Type,Type> replacements)
        {

        }

        public override void PreBuild(IHostBuilder hostBuilder)
        {
            hostBuilder
                .UseContentRoot(PluginInterface.ConfigDirectory.FullName)
                .ConfigureLogging(lb =>
                {
                    lb.ClearProviders();
                    lb.AddDalamudLogging();
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
                        typeof(IHotkey), typeof(BaseTooltip), typeof(IAtkOverlay), typeof(IColumn),
                        typeof(IGameOverlay), typeof(IFilter), typeof(ISetting), typeof(IColumnSetting),
                        typeof(IFeature)
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
                                builder.RegisterType(type).As(pair.Value).AsImplementedInterfaces().As(pair.Key).As(type).ExternallyOwned();
                            }
                        }

                        foreach (var pair in transientPairs)
                        {
                            if (pair.Key.IsAssignableFrom(type))
                            {
                                builder.RegisterType(type).AsSelf().InstancePerDependency().As(pair.Value).As(pair.Key)
                                    .As(type);
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

            //Hosted service registrations
            hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
            {
                foreach (var hostedService in GetHostedServices())
                {
                    builder.RegisterType(hostedService).AsSelf().AsImplementedInterfaces().As<IHostedService>().SingleInstance();
                }

                builder.RegisterType<QuestManagerService>().AsImplementedInterfaces().AsSelf().SingleInstance();
            });

            hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.Register(c => new HttpClient()).As<HttpClient>();
                builder.RegisterType<SeTime>().As<ISeTime>().SingleInstance();
                builder.RegisterType<ConfigurationWizardService>().SingleInstance();
                builder.RegisterType<FileDialogManager>().SingleInstance();
                builder.RegisterType<DalamudFileDialogManager>().As<IFileDialogManager>().SingleInstance();
                builder.RegisterType<BackgroundTaskQueue>().As<IBackgroundTaskQueue>();
                builder.RegisterType<CharacterMonitor>().As<ICharacterMonitor>().SingleInstance();
                builder.RegisterType<ChatUtilities>().As<IChatUtilities>().SingleInstance();
                builder.RegisterType<ConfigurationWizardService>().As<IConfigurationWizardService>().SingleInstance();
                builder.RegisterType<FilterService>().As<IFilterService>().SingleInstance();
                builder.RegisterType<Font>().As<IFont>().SingleInstance();
                builder.RegisterType<GameInterface>().As<IGameInterface>().SingleInstance();
                builder.RegisterType<GameUiManager>().As<IGameUiManager>().SingleInstance();
                builder.RegisterType<HotkeyService>().As<IHotkeyService>().SingleInstance();
                builder.RegisterType<InventoryMonitor>().As<IInventoryMonitor>().SingleInstance();
                builder.RegisterType<InventoryScanner>().As<IInventoryScanner>().SingleInstance();
                builder.RegisterType<MarketBoardService>().As<IMarketBoardService>().SingleInstance();
                builder.RegisterType<MarketCache>().As<IMarketCache>().SingleInstance();
                builder.RegisterType<MobTracker>().As<IMobTracker>().SingleInstance();
                builder.RegisterType<ClipboardService>().As<IClipboardService>().SingleInstance();
                builder.RegisterType<IPCService>().SingleInstance();
                builder.RegisterType<TeleporterIpc>().As<ITeleporterIpc>().SingleInstance();
                builder.RegisterType<TooltipService>().As<ITooltipService>().SingleInstance();
                builder.RegisterType<MarketboardTaskQueue>().SingleInstance();
                builder.RegisterType<ImGuiService>().SingleInstance();
                builder.RegisterType<InventoryHistory>().SingleInstance();
                builder.RegisterType<ListCategoryService>().SingleInstance();
                builder.RegisterType<Logger>().SingleInstance();
                builder.RegisterType<PopupService>().SingleInstance();

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
                builder.RegisterType<InventoryScopePicker>();
                builder.RegisterType<InventoryScopeCalculator>().SingleInstance();
                builder.RegisterType<GameInteropService>().As<IGameInteropService>().SingleInstance();
                builder.RegisterType<WindowSystemFactory>().As<IWindowSystemFactory>().SingleInstance();
                builder.RegisterType<DalamudWindowSystem>().As<IWindowSystem>();
                builder.RegisterType<HostedUniversalisConfiguration>().AsSelf().As<IHostedUniversalisConfiguration>()
                    .SingleInstance();

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

                builder.Register<Func<string, ColumnConfiguration?>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return typeName =>
                    {
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

                builder.Register<Func<string, IColumn?>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return typeName =>
                    {
                        var columns = context.Resolve<IEnumerable<IColumn>>();
                        var column = columns.FirstOrDefault(column => column.GetType().Name == typeName);

                        return column;
                    };
                });

                builder.Register<Func<Type, IColumn>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return type =>
                    {
                        var column = (IColumn)context.Resolve(type);
                        return column;
                    };
                });

                builder.Register<Func<FilterConfiguration, FilterTable>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return filterConfiguration =>
                    {
                        var filterTable = context.Resolve<FilterTable>();
                        filterTable.Initialize(filterConfiguration);
                        return filterTable;
                    };
                });
                builder.Register<Func<FilterConfiguration, CraftItemTable>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return filterConfiguration =>
                    {
                        var craftItemTable = context.Resolve<CraftItemTable>();
                        craftItemTable.Initialize(filterConfiguration);
                        return craftItemTable;
                    };
                });

                builder.Register<Func<Type, GenericWindow>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return type =>
                    {
                        var genericWindow = (GenericWindow)context.Resolve(type);
                        genericWindow.Initialize();
                        return genericWindow;
                    };
                });
                builder.Register<Func<SettingCategory, SettingPage>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return settingCategory =>
                    {
                        var settingPage = (SettingPage)context.Resolve(typeof(SettingPage));
                        settingPage.Initialize(settingCategory);
                        return settingPage;
                    };
                });
                builder.Register<Func<Type, uint, UintWindow>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return (type, id) =>
                    {
                        var uintWindow = (UintWindow)context.Resolve(type);
                        uintWindow.Initialize(id);
                        return uintWindow;
                    };
                });
                builder.Register<Func<Type, string, StringWindow>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return (type, id) =>
                    {
                        var stringWindow = (StringWindow)context.Resolve(type);
                        stringWindow.Initialize(id);
                        return stringWindow;
                    };
                });
                builder.Register<Func<FilterConfiguration, FilterState>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return filterConfiguration =>
                    {
                        var filterState = (FilterState)context.Resolve(typeof(FilterState));
                        filterState.Initialize(filterConfiguration);
                        return filterState;
                    };
                });
                builder.Register<Func<FilterConfiguration, FilterPage>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return filterConfiguration =>
                    {
                        var filterPage = (FilterPage)context.Resolve(typeof(FilterPage));
                        filterPage.Initialize(filterConfiguration);
                        return filterPage;
                    };
                });
                builder.Register<Func<Type, IConfigPage>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return type =>
                    {
                        var configPage = (IConfigPage)context.Resolve(type);
                        configPage.Initialize();
                        return configPage;
                    };
                });
                builder.Register<Func<Type, IFilter>>(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return type =>
                    {
                        var filter = (IFilter)context.Resolve(type);
                        return filter;
                    };
                });
                builder.Register<Func<int, IBackgroundTaskQueue>>(c =>
                {
                    return capacity =>
                    {
                        var filter = new BackgroundTaskQueue(capacity);
                        return filter;
                    };
                });
            });
        }

        public override void ConfigureContainer(ContainerBuilder containerBuilder)
        {

        }

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Service.Log.Debug("Starting dispose of InventoryToolsPlugin");
                _service?.Dispose();
                _service = null;
                PluginInterface = null;
            }
        }

        public new void Dispose()
        {
            Dispose(true);
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}