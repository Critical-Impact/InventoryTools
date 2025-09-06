using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using AllaganLib.Data.Service;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Modules;
using AllaganLib.Interface.Grid.ColumnFilters;
using AllaganLib.Monitors.Debuggers;
using AllaganLib.Monitors.Services;
using AllaganLib.Shared.Interfaces;
using AllaganLib.Shared.Services;
using AllaganLib.Shared.Time;
using AllaganLib.Shared.Windows;
using Autofac;
using CharacterTools.Logic.Editors;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Ipc;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;

using DalaMock.Host.Factories;
using DalaMock.Host.Hosting;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Classes;
using DalaMock.Shared.Extensions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using InventoryTools.Commands;
using InventoryTools.EquipmentSuggest;
using InventoryTools.Highlighting;
using InventoryTools.Host;
using InventoryTools.Hotkeys;
using InventoryTools.IPC;
using InventoryTools.Lists;
using InventoryTools.Localizers;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Features;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.GenericFilters;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Overlays;
using InventoryTools.ServiceConfigurations;
using InventoryTools.Services;
using InventoryTools.Tooltips;
using InventoryTools.Ui;
using InventoryTools.Ui.Pages;
using Lumina;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OtterGui.Log;
using Window = InventoryTools.Ui.Window;

namespace InventoryTools
{
    public class InventoryToolsPlugin : HostedPlugin
    {
        private readonly IPluginLog _pluginLog;
        private readonly IFramework _framework;
        private IDalamudPluginInterface? PluginInterface { get; set; }

        public InventoryToolsPlugin(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog,
            IAddonLifecycle addonLifecycle, IChatGui chatGui, IClientState clientState, ICommandManager commandManager,
            ICondition condition, IDataManager dataManager, IFramework framework, IGameGui gameGui,
            IGameInteropProvider gameInteropProvider, IKeyState keyState, IObjectTable objectTable, ITargetManager targetManager, ITextureProvider textureProvider,
            IToastGui toastGui, IContextMenu contextMenu, ITitleScreenMenu titleScreenMenu,
            IGameInventory gameInventory) : base(pluginInterface,
            pluginLog, addonLifecycle, chatGui, clientState, commandManager,
            condition, dataManager, framework, gameGui,
            gameInteropProvider, keyState, objectTable,
            targetManager, textureProvider,
            toastGui, contextMenu, titleScreenMenu,
            gameInventory)
        {
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();
            pluginLog.Verbose("Starting Allagan Tools.");
            _pluginLog = pluginLog;
            _framework = framework;
            PluginInterface = pluginInterface;
            this.Host = CreateHost();
            Start();
            this.Host.Services.GetRequiredService<MediatorService>().Publish(new PluginLoadedMessage());
            loadConfigStopwatch.Stop();
            pluginLog.Verbose("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to start Allagan Tools.");
        }

        public IHost Host { get; set; }

        public override HostedPluginOptions ConfigureOptions()
        {
            return new HostedPluginOptions()
            {
                UseMediatorService = true
            };
        }

        public override void ConfigureContainer(ContainerBuilder builder)
        {
            var dataAccess = Assembly.GetExecutingAssembly();
            var cclAssembly = typeof(CriticalCommonLib.Services.ICharacterMonitor).Assembly;

            builder.Register(c => new HttpClient()).As<HttpClient>();

            //Register all classes that are singletons and implement a particular interface/class
            builder.RegisterSingletonsSelfAndInterfaces<IHotkey>(dataAccess);
            builder.RegisterSingletonsSelfAndInterfaces<BaseTooltip>(dataAccess);
            builder.RegisterSingletonsSelfAndInterfaces<IAtkOverlay>(cclAssembly);
            builder.RegisterSingletonsSelfAndInterfaces<IGameOverlay>(dataAccess);
            builder.RegisterSingletonsSelfAndInterfaces<IColumn>(dataAccess);
            builder.RegisterSingletonsSelfAndInterfaces<ISetting>(dataAccess).AsImplementedInterfaces();
            builder.RegisterSingletonsSelfAndInterfaces<ISampleFilter>(dataAccess);
            builder.RegisterSingletonsSelfAndInterfaces<IFeature>(dataAccess);
            builder.RegisterSingletonsSelfAndInterfaces<IItemInfoRenderer>(dataAccess);
            builder.RegisterSingletonsSelfAndInterfaces<IDebugPane>(typeof(ShopMonitorDebugPane).Assembly); //Register AllaganLib.Monitor debug panes
            builder.RegisterSingletonsSelfAndInterfaces<IDebugPane>(dataAccess); //Register InventoryTools debug panes
            builder.RegisterSingletonsSelfAndInterfaces<IFilter>(dataAccess).Where(c => c.GetInterface("IGenericFilter") == null); //Generic filters should not be registered as IFilters as we don't want them to show up anywhere we want to iterate over all available filters

            //Register all classes that are transients and implement a particular interface/class
            builder.RegisterTransientsSelfAndInterfaces<IColumnSetting>(dataAccess);

            //Register all classes that are externally owned transients and implement a particular interface/class
            builder.RegisterExternalTransientsSelfAndInterfaces<GenericWindow>(dataAccess, typeof(Window));
            builder.RegisterExternalTransientsSelfAndInterfaces<UintWindow>(dataAccess, typeof(Window));
            builder.RegisterExternalTransientsSelfAndInterfaces<StringWindow>(dataAccess, typeof(Window));

            //Register our generic filters
            builder.RegisterTransientSelf<GenericHasSourceFilter>();
            builder.RegisterTransientSelf<GenericHasSourceCategoryFilter>();
            builder.RegisterTransientSelf<GenericHasUseFilter>();
            builder.RegisterTransientSelf<GenericHasUseCategoryFilter>();
            builder.RegisterTransientSelf<GenericBooleanFilter>();
            builder.RegisterTransientSelf<GenericIntegerFilter>();

            //Hosted service registrations
            this.RegisterHostedService(typeof(BootService));
            this.RegisterHostedService(typeof(Chat2Ipc));
            this.RegisterHostedService(typeof(ConfigurationManagerService));
            this.RegisterHostedService(typeof(ContextMenuService));
            this.RegisterHostedService(typeof(HostedCraftMonitor));
            this.RegisterHostedService(typeof(HostedInventoryHistory));
            this.RegisterHostedService(typeof(HostedUniversalis));
            this.RegisterHostedService(typeof(HotkeyService));
            this.RegisterHostedService(typeof(IPCService));
            this.RegisterHostedService(typeof(InventoryToolsUi));
            this.RegisterHostedService(typeof(ItemSearchService));
            this.RegisterHostedService(typeof(LaunchButtonService));
            this.RegisterHostedService(typeof(ListFilterService));
            this.RegisterHostedService(typeof(ListService));
            this.RegisterHostedService(typeof(MarketCache));
            this.RegisterHostedService(typeof(MarketRefreshService));
            this.RegisterHostedService(typeof(MigrationManagerService));
            this.RegisterHostedService(typeof(OdrScanner));
            this.RegisterHostedService(typeof(OverlayService));
            this.RegisterHostedService(typeof(PluginCommandManager<PluginCommands>));
            this.RegisterHostedService(typeof(PluginLogic));
            this.RegisterHostedService(typeof(ServiceConfigurator));
            this.RegisterHostedService(typeof(TableService));
            this.RegisterHostedService(typeof(TeleporterService));
            this.RegisterHostedService(typeof(WindowService));
            this.RegisterHostedService(typeof(WotsitIpc));
            this.RegisterHostedService(typeof(ShopMonitorService));
            this.RegisterHostedService(typeof(AcquisitionMonitorService));

            //AllaganLib modules
            builder.RegisterModule(new GameSheetManagerModule());
            builder.RegisterModule(new GameDataModule());

            //Service configuration
            builder.RegisterSingletonSelfAndInterfaces<AcquisitionMonitorConfiguration>();

            //Dalamud related services
            builder.Register<GameData>(c => c.Resolve<IDataManager>().GameData).SingleInstance().ExternallyOwned();

            //Singleton registrations
            builder.RegisterSingletonSelfAndInterfaces<AutofacResolver>();
            builder.RegisterSingletonSelfAndInterfaces<AllaganDebugWindow>();
            builder.RegisterSingletonSelfAndInterfaces<ChangelogService>();
            builder.RegisterSingletonSelfAndInterfaces<CharacterMonitor>();
            builder.RegisterSingletonSelfAndInterfaces<CharacterRetainerPage>();
            builder.RegisterSingletonSelfAndInterfaces<CharacterScopeCalculator>();
            builder.RegisterSingletonSelfAndInterfaces<ChatUtilities>();
            builder.RegisterSingletonSelfAndInterfaces<ClassJobService>();
            builder.RegisterSingletonSelfAndInterfaces<ClipboardService>();
            builder.RegisterSingletonSelfAndInterfaces<ConfigurationWizardService>();
            builder.RegisterSingletonSelfAndInterfaces<ConfigurationWizardService>();
            builder.RegisterSingletonSelfAndInterfaces<ContainerAwareCsvLoader>();
            builder.RegisterSingletonSelfAndInterfaces<CraftFiltersPage>();
            builder.RegisterSingletonSelfAndInterfaces<CraftGroupingLocalizer>();
            builder.RegisterSingletonSelfAndInterfaces<CraftItemLocalizer>();
            builder.RegisterSingletonSelfAndInterfaces<CraftPricer>();
            builder.RegisterSingletonSelfAndInterfaces<CraftingCache>();
            builder.RegisterSingletonSelfAndInterfaces<DalamudFileDialogManager>();
            builder.RegisterSingletonSelfAndInterfaces<DalamudWindowSystem>();
            builder.RegisterSingletonSelfAndInterfaces<FileDialogManager>();
            builder.RegisterSingletonSelfAndInterfaces<FilterService>();
            builder.RegisterSingletonSelfAndInterfaces<FiltersPage>();
            builder.RegisterSingletonSelfAndInterfaces<Font>();
            builder.RegisterSingletonSelfAndInterfaces<GameInterface>();
            builder.RegisterSingletonSelfAndInterfaces<GameInteropService>();
            builder.RegisterSingletonSelfAndInterfaces<GameUiManager>();
            builder.RegisterSingletonSelfAndInterfaces<HostedUniversalisConfiguration>();
            builder.RegisterSingletonSelfAndInterfaces<ImGuiMenuService>();
            builder.RegisterSingletonSelfAndInterfaces<ImGuiService>();
            builder.RegisterSingletonSelfAndInterfaces<ImGuiTooltipService>().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterSingletonSelfAndInterfaces<ImportExportPage>();
            builder.RegisterSingletonSelfAndInterfaces<IngredientPatchService>();
            builder.RegisterSingletonSelfAndInterfaces<IngredientPreferenceLocalizer>();
            builder.RegisterSingletonSelfAndInterfaces<InventoryHistory>();
            builder.RegisterSingletonSelfAndInterfaces<InventoryMonitor>();
            builder.RegisterSingletonSelfAndInterfaces<InventoryScanner>();
            builder.RegisterSingletonSelfAndInterfaces<InventoryScopeCalculator>();
            builder.RegisterSingletonSelfAndInterfaces<ItemInfoRenderService>();
            builder.RegisterSingletonSelfAndInterfaces<ItemLocalizer>();
            builder.RegisterSingletonSelfAndInterfaces<ListImportExportService>();
            builder.RegisterSingletonSelfAndInterfaces<Logger>();
            builder.RegisterSingletonSelfAndInterfaces<MarketBoardService>();
            builder.RegisterSingletonSelfAndInterfaces<MarketCacheConfiguration>();
            builder.RegisterSingletonSelfAndInterfaces<MarketOrderService>();
            builder.RegisterSingletonSelfAndInterfaces<MinifyResolver>();
            builder.RegisterSingletonSelfAndInterfaces<MobTracker>();
            builder.RegisterSingletonSelfAndInterfaces<PluginCommands>();
            builder.RegisterSingletonSelfAndInterfaces<PopupService>();
            builder.RegisterSingletonSelfAndInterfaces<QuestManagerService>();
            builder.RegisterSingletonSelfAndInterfaces<SeTime>();
            builder.RegisterSingletonSelfAndInterfaces<ShopHighlighting>();
            builder.RegisterSingletonSelfAndInterfaces<TeleporterIpc>();
            builder.RegisterSingletonSelfAndInterfaces<TooltipService>();
            builder.RegisterSingletonSelfAndInterfaces<TryOn>();
            builder.RegisterSingletonSelfAndInterfaces<UnlockTrackerService>();
            builder.RegisterSingletonSelfAndInterfaces<VersionInfo>();
            builder.RegisterSingletonSelfAndInterfaces<WindowSystemFactory>();
            builder.RegisterSingletonSelfAndInterfaces<CsvLoaderService>();
            builder.RegisterSingletonSelfAndInterfaces<BackgroundTaskCollector>();

            //Transient registrations
            builder.RegisterTransientSelfAndInterfaces<BackgroundTaskQueue>();
            builder.RegisterTransientSelfAndInterfaces<NamedBackgroundTaskQueue>();
            builder.RegisterTransientSelfAndInterfaces<InventoryScopePicker>();
            builder.RegisterTransientSelfAndInterfaces<FilterTable>();
            builder.RegisterTransientSelfAndInterfaces<CraftItemTable>();
            builder.RegisterTransientSelfAndInterfaces<CharacterScopePicker>();
            builder.RegisterTransientSelfAndInterfaces<StringColumnFilter>();
            builder.RegisterTransientSelfAndInterfaces<ChoiceColumnFilter>();
            builder.RegisterTransientSelfAndInterfaces<SettingPage>();
            builder.RegisterTransientSelfAndInterfaces<FilterPage>();
            builder.RegisterTransientSelfAndInterfaces<FilterState>();
            builder.RegisterTransientSelfAndInterfaces<Character>();
            builder.RegisterTransientSelfAndInterfaces<CraftList>();
            builder.RegisterTransientSelfAndInterfaces<CraftCalculator>();
            builder.RegisterTransientSelfAndInterfaces<FilterConfiguration>();
            builder.RegisterTransientSelfAndInterfaces<Inventory>();
            builder.RegisterTransientSelfAndInterfaces<InventoryChange>();
            builder.RegisterTransientSelfAndInterfaces<CraftItem>();
            builder.RegisterTransientSelfAndInterfaces<InventoryItem>();

            //Equipment Recommendation System
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestGrid>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestConfig>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestItem>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestSlotColumn>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestSelectedItemColumn>();
            builder.RegisterTransientSelfAndInterfaces<EquipmentSuggestSuggestionColumn>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestClassJobFormField>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestSourceTypeField>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestExcludeSourceTypeField>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestLevelFormField>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestFilterStatsField>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestToolModeCategorySetting>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestModeSetting>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestSelectedSecondaryItemColumn>();
            builder.RegisterSingletonSelfAndInterfaces<EquipmentSuggestService>();

            builder.Register<UniversalisUserAgent>(c =>
            {
                var pluginInterface = c.Resolve<IDalamudPluginInterface>();
                return new UniversalisUserAgent("AllaganTools",
                    pluginInterface.Manifest.AssemblyVersion.ToString());
            });

            builder.Register(provider =>
            {
                var configurationManager = provider.Resolve<ConfigurationManagerService>();
                configurationManager.Load();
                var configuration = configurationManager.GetConfig();
                configuration.ClearDirtyFlags();
                return configuration;
            }).As<InventoryToolsConfiguration>().SingleInstance();

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

            builder.Register<Func<Type, Dalamud.Interface.Windowing.Window>>(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return type =>
                {
                    var genericWindow = (Dalamud.Interface.Windowing.Window)context.Resolve(type);
                    return genericWindow;
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

            builder.Register<Func<ItemInfoType, GenericHasSourceFilter>>(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return type => context.Resolve<GenericHasSourceFilter>(new NamedParameter("itemType", type));
            });

            builder.Register<Func<ItemInfoType, GenericHasUseFilter>>(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return type => context.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", type));
            });

            builder.Register<Func<ItemInfoRenderCategory, GenericHasSourceCategoryFilter>>(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return renderCategory => context.Resolve<GenericHasSourceCategoryFilter>(new NamedParameter("renderCategory", renderCategory));
            });

            builder.Register<Func<ItemInfoRenderCategory, GenericHasUseCategoryFilter>>(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return renderCategory => context.Resolve<GenericHasUseCategoryFilter>(new NamedParameter("renderCategory", renderCategory));
            });
        }

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {


        }
    }
}