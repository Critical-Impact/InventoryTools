using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.Interface.Widgets;
using AllaganLib.Shared.Extensions;
using Autofac;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.MenuItems;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using InventoryTools.Ui.Widgets;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Extensions;
using InventoryTools.Logic.Features;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.ConfigLayouts;
using InventoryTools.Ui.Config.Layouts;
using InventoryTools.Ui.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui
{
    public class ConfigurationWindow : GenericWindow, IMenuWindow
    {
        private readonly IPluginLog _pluginLog;
        private readonly ConfigurationWizardService _configurationWizardService;
        private readonly IChatUtilities _chatUtilities;
        private readonly PluginLogic _pluginLogic;
        private readonly IListService _listService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly LayoutConfigPage.Factory _layoutPageFactory;
        private readonly SettingCoverageService _settingCoverageService;
        private readonly ConfigNavigationState _configNavigationState;
        private readonly ConfigSearchService _configSearchService = new();
        private string _searchQuery = string.Empty;
        private readonly FilterConfiguration.Factory _filterConfigurationFactory;
        private readonly IEnumerable<ISampleFilter> _sampleFilters;
        private readonly Func<Type, IConfigPage> _configPageFactory;
        private readonly Func<FilterConfiguration, FilterPage> _filterPageFactory;
        private readonly IComponentContext _context;
        private readonly InventoryToolsConfiguration _configuration;
        private readonly VerticalSplitter _verticalSplitter;
        private readonly IFont _font;
        private IEnumerable<IMenuWindow>? _menuWindows;
        private FilterConfiguration? _nextFilter;

        public ConfigurationWindow(ILogger<ConfigurationWindow> logger,
            IPluginLog pluginLog,
            MediatorService mediator,
            ImGuiService imGuiService,
            InventoryToolsConfiguration configuration,
            ConfigurationWizardService configurationWizardService,
            IChatUtilities chatUtilities,
            PluginLogic pluginLogic,
            IListService listService,
            IServiceScopeFactory serviceScopeFactory,
            Func<Type, IConfigPage> configPageFactory,
            Func<FilterConfiguration, FilterPage> filterPageFactory,
            LayoutConfigPage.Factory layoutPageFactory,
            SettingCoverageService settingCoverageService,
            ConfigNavigationState configNavigationState,
            FilterConfiguration.Factory filterConfigurationFactory,
            IEnumerable<ISampleFilter> sampleFilters,
            IComponentContext context,
            IFont font) : base(logger,
            mediator,
            imGuiService,
            configuration,
            "Configuration Window")
        {
            _pluginLog = pluginLog;
            _configurationWizardService = configurationWizardService;
            _chatUtilities = chatUtilities;
            _pluginLogic = pluginLogic;
            _listService = listService;
            _serviceScopeFactory = serviceScopeFactory;
            _layoutPageFactory = layoutPageFactory;
            _settingCoverageService = settingCoverageService;
            _configNavigationState = configNavigationState;
            _filterConfigurationFactory = filterConfigurationFactory;
            _sampleFilters = sampleFilters;
            _configPageFactory = configPageFactory;
            _filterPageFactory = filterPageFactory;
            _context = context;
            _configuration = configuration;
            _verticalSplitter = new VerticalSplitter(250, new Vector2(200, 400));
            _font = font;
            this.Flags = ImGuiWindowFlags.MenuBar;
        }

        public override void Initialize()
        {
            WindowName = "Configuration";
            Key = "configuration";
            #if DEBUG
            _settingCoverageService.Report();
            #endif
            _configPages = new List<IConfigPage>();
            _configPages.Add(new SeparatorPageItem("Settings"));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<GeneralLayout>()));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<WindowsAndListsLayout>()));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<ItemIconsLayout>()));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<HighlightingLayout>()));
            _configPages.Add(new SeparatorPageItem("Modules", true));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<MarketBoardLayout>()));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<TooltipsLayout>()));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<ContextMenuLayout>()));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<HotkeysLayout>()));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<CraftOverlayLayout>()));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<EquipmentRecommendationLayout>()));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<HistoryLayout>()));
            _configPages.Add(new SeparatorPageItem(null, true));
            _configPages.Add(_layoutPageFactory.Invoke(_context.Resolve<TroubleshootingLayout>()));
            _configPages.Add(new SeparatorPageItem("Data", true));
            _configPages.Add(_configPageFactory.Invoke(typeof(ListsPage)));
            _configPages.Add(_configPageFactory.Invoke(typeof(CharacterRetainerPage)));

            _addFilterMenu = new PopupMenu("addFilter", PopupMenu.PopupMenuButtons.LeftRight,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectableAskName("Search List", "adf1", "New Search List", AddSearchFilter, "This will create a new list that let's you search for specific items within your characters and retainers inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Sort List", "af2", "New Sort Filter", AddSortFilter, "This will create a new list that let's you search for specific items within your characters and retainers inventories then determine where they should be moved to."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Game Item List", "af3", "New Game Item List", AddGameItemFilter, "This will create a list that lets you search for all items in the game."),
                    new PopupMenu.PopupMenuItemSelectableAskName("History List", "af4", "New History Item List", AddHistoryFilter, "This will create a list that lets you view historical data of how your inventory has changed."),
                });

            _addSampleMenu = new PopupMenu("addSampleFilter", PopupMenu.PopupMenuButtons.LeftRight, []);

            var sampleId = 0;
            foreach (var sampleFilter in _sampleFilters)
            {
                if (sampleFilter.SampleFilterType == SampleFilterType.Default)
                {
                    _addSampleMenu.Items.Add(new PopupMenu.PopupMenuItemSelectableAskName(sampleFilter.Name,
                        $"sf{sampleId}", sampleFilter.SampleDefaultName, (newName, id) =>
                        {
                            var createdFilter = sampleFilter.AddFilter();
                            createdFilter.Name = newName;
                        }, sampleFilter.SampleDescription));
                    sampleId++;
                }
            }

            _addSampleMenu.Items.Add(new PopupMenu.PopupMenuItemSeparator());

            foreach (var sampleFilter in _sampleFilters)
            {
                if (sampleFilter.SampleFilterType == SampleFilterType.Sample)
                {
                    _addSampleMenu.Items.Add(new PopupMenu.PopupMenuItemSelectableAskName(sampleFilter.Name,
                        $"sf{sampleId}", sampleFilter.SampleDefaultName, (newName, id) =>
                        {
                            var createdFilter = sampleFilter.AddFilter();
                            createdFilter.Name = newName;
                        }, sampleFilter.SampleDescription));
                    sampleId++;
                }
            }

            _settingsMenu = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectable("Items Window", "filters", OpenFiltersWindow,"Open the items window."),
                    new PopupMenu.PopupMenuItemSelectable("Craft Window", "crafts", OpenCraftsWindow,"Open the crafts window."),
                    new PopupMenu.PopupMenuItemSeparator(),
                    new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", OpenMobsWindow,"Open the mobs window."),
                    new PopupMenu.PopupMenuItemSelectable("Npcs Window", "npcs", OpenNpcsWindow,"Open the npcs window."),
                    new PopupMenu.PopupMenuItemSelectable("Duties Window", "duties", OpenDutiesWindow,"Open the duties window."),
                    new PopupMenu.PopupMenuItemSelectable("Airships Window", "airships", OpenAirshipsWindow,"Open the airships window."),
                    new PopupMenu.PopupMenuItemSelectable("Submarines Window", "submarines", OpenSubmarinesWindow,"Open the submarines window."),
                    new PopupMenu.PopupMenuItemSelectable("Retainer Ventures Window", "ventures", OpenRetainerVenturesWindow,"Open the retainer ventures window."),
                    new PopupMenu.PopupMenuItemSeparator(),
                    new PopupMenu.PopupMenuItemSelectable("Help", "help", OpenHelpWindow,"Open the help window."),
                });

            _wizardMenu = new PopupMenu("wizardMenu", PopupMenu.PopupMenuButtons.All,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectable("Configure new settings", "configureNew", ConfigureNewSettings,"Configure new settings."),
                    new PopupMenu.PopupMenuItemSelectable("Configure all settings", "configureAll", ConfigureAllSettings,"Configure all settings."),
                });
            _menuWindows = _context.Resolve<IEnumerable<IMenuWindow>>().OrderBy(c => c.GenericName).Where(c => c.GetType() != this.GetType());

            GenerateFilterPages();
            CheckDuplicateKeys();
            RebuildSearchIndex();
            MediatorService.Subscribe<ListInvalidatedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRepositionedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListAddedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRemovedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ConfigurationWindowEditFilter>(this,  message =>
            {
                Invalidate();
                SetActiveFilter(message.filter);
            });
            MediatorService.Subscribe<ListInvalidatedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRepositionedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListAddedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRemovedMessage>(this, _ => Invalidate());
        }

        private void ListInvalidated(ListInvalidatedMessage obj)
        {
            Invalidate();
        }



        private PopupMenu _wizardMenu = null!;

        private void ConfigureAllSettings(string obj)
        {
            _configurationWizardService.ClearFeaturesSeen();
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
        }

        private void ConfigureNewSettings(string obj)
        {
            if (_configurationWizardService.HasNewFeatures)
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
            }
            else
            {
                _chatUtilities.Print("There are no new settings available to configure.");
            }
        }

        private PopupMenu _addFilterMenu = null!;
        private PopupMenu _addSampleMenu = null!;
        private PopupMenu _settingsMenu = null!;

        private void OpenCraftsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
        }

        private void OpenFiltersWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
        }

        private void OpenHelpWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(HelpWindow)));
        }

        private void OpenDutiesWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(DutiesWindow)));
        }

        private void OpenAirshipsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(AirshipsWindow)));
        }

        private void OpenSubmarinesWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(SubmarinesWindow)));
        }

        private void OpenRetainerVenturesWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(RetainerTasksWindow)));
        }

        private void OpenMobsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(BNpcsWindow)));
        }

        private void OpenNpcsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ENpcsWindow)));
        }

        private Dictionary<FilterConfiguration, PopupMenu> _popupMenus = new();
        public PopupMenu GetFilterMenu(FilterConfiguration configuration)
        {
            if (!_popupMenus.ContainsKey(configuration))
            {
                _popupMenus[configuration] = new PopupMenu("fm" + configuration.Key, PopupMenu.PopupMenuButtons.Right,
                    new List<PopupMenu.IPopupMenuItem>()
                    {
                        new PopupMenu.PopupMenuItemSelectableAskName("Duplicate", "df_" + configuration.Key, configuration.Name, DuplicateFilter, "Duplicate the filter."),
                        new PopupMenu.PopupMenuItemSelectable("Move Up", "mu_" + configuration.Key, MoveFilterUp, "Move the filter up."),
                        new PopupMenu.PopupMenuItemSelectable("Move Down", "md_" + configuration.Key, MoveFilterDown, "Move the filter down."),
                        new PopupMenu.PopupMenuItemSelectableConfirm("Remove", "rf_" + configuration.Key, "Are you sure you want to remove this filter?", RemoveFilter, "Remove the filter."),
                    }
                );
            }

            return _popupMenus[configuration];
        }

        private void RemoveFilter(string id, bool confirmed)
        {
            if (confirmed)
            {
                id = id.Replace("rf_", "");
                var existingFilter = _listService.GetListByKey(id);
                if (existingFilter != null)
                {
                    if (_filterPages.TryGetValue(existingFilter.Key, out var removedPage)
                        && ConfigSelectedConfigurationPageKey == removedPage.Key)
                    {
                        ConfigSelectedConfigurationPageKey = null;
                    }

                    _listService.RemoveList(existingFilter);
                }
            }
        }

        private void MoveFilterDown(string id)
        {
            id = id.Replace("md_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.MoveListDown(existingFilter);
            }
        }

        private void MoveFilterUp(string id)
        {
            id = id.Replace("mu_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.MoveListUp(existingFilter);
            }
        }

        private void DuplicateFilter(string filterName, string id)
        {
            id = id.Replace("df_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                var duplicatedFilter = _listService.DuplicateList(existingFilter, filterName);
                SetNewFilterActive(duplicatedFilter);
            }
        }

        private void AddSearchFilter(string newName, string id)
        {
            var filterConfiguration = _filterConfigurationFactory.Invoke();
            filterConfiguration.Name = newName;
            filterConfiguration.FilterType = FilterType.SearchFilter;
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive(filterConfiguration);
        }

        private void AddHistoryFilter(string newName, string id)
        {
            var filterConfiguration = _filterConfigurationFactory.Invoke();
            filterConfiguration.Name = newName;
            filterConfiguration.FilterType = FilterType.HistoryFilter;
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive(filterConfiguration);
        }

        private void AddGameItemFilter(string newName, string id)
        {
            var filterConfiguration = _filterConfigurationFactory.Invoke();
            filterConfiguration.Name = newName;
            filterConfiguration.FilterType = FilterType.GameItemFilter;
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive(filterConfiguration);
        }

        private void AddSortFilter(string newName, string id)
        {
            var filterConfiguration = _filterConfigurationFactory.Invoke();
            filterConfiguration.Name = newName;
            filterConfiguration.FilterType = FilterType.SortingFilter;
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive(filterConfiguration);
        }

        private void CheckDuplicateKeys()
        {
            var seen = new HashSet<string>();
            foreach (var page in SelectablePages())
            {
                if (!seen.Add(page.Key))
                {
                    Logger.LogError(
                        "Two configuration pages share the key {Key}; the second ({Name}) cannot be navigated to.",
                        page.Key,
                        page.Name);
                }
            }
        }

        private string? ConfigSelectedConfigurationPageKey
        {
            get => _configuration.SelectedConfigurationPageKey;
            set => _configuration.SelectedConfigurationPageKey = value;
        }

        private IEnumerable<IConfigPage> SelectablePages()
        {
            foreach (var configPage in _configPages)
            {
                if (configPage.IsMenuItem)
                {
                    continue;
                }

                if (configPage.ChildPages != null)
                {
                    foreach (var childPage in configPage.ChildPages)
                    {
                        yield return childPage;
                    }
                }
                else
                {
                    yield return configPage;
                }
            }

            foreach (var filterPage in _filterPages.Values)
            {
                yield return filterPage;
            }
        }

        private IConfigPage? SelectedPage()
        {
            IConfigPage? first = null;
            foreach (var page in SelectablePages())
            {
                first ??= page;
                if (page.Key == ConfigSelectedConfigurationPageKey)
                {
                    return page;
                }
            }

            return first;
        }

        public void SetActiveFilter(FilterConfiguration configuration)
        {
            if (_filterPages.ContainsKey(configuration.Key))
            {
                _nextFilter = configuration;
            }
        }

        public void GenerateFilterPages()
        {
            var filterConfigurations = _listService.Lists.Where(c => c.FilterType != FilterType.CraftFilter);
            var filterPages = new Dictionary<string, IConfigPage>();
            foreach (var filter in filterConfigurations)
            {
                if (!filterPages.ContainsKey(filter.Key))
                {
                    filterPages.Add(filter.Key, _filterPageFactory.Invoke(filter));
                }
            }

            _filterPages = filterPages;
        }

        public override bool SaveState => true;
        public override Vector2? DefaultSize { get; } = new(700, 700);
        public override Vector2? MaxSize { get; } = new(2000, 2000);
        public override Vector2? MinSize { get; } = new(200, 200);
        public override string GenericKey => "configuration";
        public override string GenericName => "Configuration";
        public override bool DestroyOnClose => true;
        private List<IConfigPage> _configPages = null!;
        public Dictionary<string, IConfigPage> _filterPages = new Dictionary<string,IConfigPage>();


        private void SetNewFilterActive(FilterConfiguration filterConfiguration)
        {
            _nextFilter = filterConfiguration;
        }

        private void DrawMenuBar()
        {
            using (var menuBar = ImRaii.MenuBar())
            {
                if (menuBar)
                {
                    using (var menu = ImRaii.Menu("File"))
                    {
                        if (menu)
                        {
                            if (ImGui.MenuItem("Report a Issue"))
                            {
                                "https://github.com/Critical-Impact/AllaganMarket".OpenBrowser();
                            }

                            if (ImGui.MenuItem("Changelog"))
                            {
                                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ChangelogWindow)));
                            }

                            if (ImGui.MenuItem("Help"))
                            {
                                MediatorService.Publish(new OpenGenericWindowMessage(typeof(HelpWindow)));
                            }

                            if (ImGui.MenuItem("Enable Verbose Logging", "",
                                    this._pluginLog.MinimumLogLevel == LogEventLevel.Verbose))
                            {
                                if (this._pluginLog.MinimumLogLevel == LogEventLevel.Verbose)
                                {
                                    this._pluginLog.MinimumLogLevel = LogEventLevel.Debug;
                                }
                                else
                                {
                                    this._pluginLog.MinimumLogLevel = LogEventLevel.Verbose;
                                }
                            }

                            if (ImGui.MenuItem("Generate Support Dump"))
                            {
                                this.MediatorService.Publish(new OpenGenericWindowMessage(typeof(SupportDumpWindow)));
                            }

                            if (ImGui.MenuItem("Ko-Fi"))
                            {
                                "https://ko-fi.com/critical_impact".OpenBrowser();
                            }

                            if (ImGui.MenuItem("Close"))
                            {
                                this.IsOpen = false;
                            }
                        }
                    }

                    using (var menu = ImRaii.Menu("Wizard"))
                    {
                        if (menu)
                        {
                            var hasNewFeatures = this._configurationWizardService.HasNewFeatures;
                            using var disabled = ImRaii.Disabled(!hasNewFeatures);
                            if (ImGui.MenuItem("Configure New Features"))
                            {
                                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
                            }

                            disabled.Dispose();

                            if (ImGui.MenuItem("Reconfigure All Features"))
                            {
                                this._configurationWizardService.ClearFeaturesSeen();
                                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
                            }
                        }
                    }

                    using (var menu = ImRaii.Menu("Windows"))
                    {
                        if (menu)
                        {
                            if (_menuWindows != null)
                            {
                                foreach (var window in _menuWindows)
                                {
                                    if (ImGui.MenuItem(window.GenericName))
                                    {
                                        MediatorService.Publish(new OpenGenericWindowMessage(window.GetType()));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void DrawWindow()
        {
            DrawMenuBar();
            DrawSearchBar();
            _verticalSplitter.Draw(DrawSideBar, DrawMainWindow);
        }

        /// <summary>
        /// Drawn outside the splitter so it stays pinned: the right-hand pane is a scrolling child,
        /// and anything drawn inside it scrolls away with the page content.
        /// </summary>
        private void DrawSearchBar()
        {
            ImGui.SetNextItemWidth(-60 * ImGui.GetIO().FontGlobalScale);
            ImGui.InputTextWithHint("##configSearch", "Search settings...", ref _searchQuery, 100);

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                ImGui.SameLine();
                if (ImGui.Button("Clear##configSearchClear"))
                {
                    _searchQuery = string.Empty;
                }
            }

            ImGui.Separator();
        }

        private void DrawSearchResults()
        {
            var results = _configSearchService.Search(_searchQuery);
            if (results.Count == 0)
            {
                ImGui.TextWrapped($"No settings match '{_searchQuery}'.");
                return;
            }

            foreach (var result in results)
            {
                using (ImRaii.PushId(result.SettingType.Name))
                {
                    if (ImGui.Selectable(result.DisplayName))
                    {
                        ConfigSelectedConfigurationPageKey = result.PageKey;
                        _configNavigationState.RequestScrollTo(result.SettingType);
                        _searchQuery = string.Empty;
                    }

                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
                    {
                        ImGui.TextUnformatted("    " + result.Breadcrumb);
                    }
                }

                ImGui.Separator();
            }
        }

        private void DrawMainWindow()
        {
            if (_nextFilter != null && _filterPages.TryGetValue(_nextFilter.Key, out var nextFilterPage))
            {
                ConfigSelectedConfigurationPageKey = nextFilterPage.Key;
                _nextFilter = null;
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                DrawSearchResults();
                return;
            }

            var currentConfigPage = SelectedPage();
            if (currentConfigPage != null)
            {
                MediatorService.Publish(currentConfigPage.Draw());
            }
        }

        private void DrawSideBar()
        {
            using (var menuChild = ImRaii.Child("Menu", new Vector2(0, -28) * ImGui.GetIO().FontGlobalScale,
                       false, ImGuiWindowFlags.NoSavedSettings))
            {
                if (menuChild.Success)
                {

                    for (var index = 0; index < _configPages.Count; index++)
                    {
                        var configPage = _configPages[index];
                        if (configPage.IsMenuItem)
                        {
                            MediatorService.Publish(configPage.Draw());
                        }
                        else
                        {
                            var hasChildren = configPage.ChildPages != null;
                            var isSelected = ConfigSelectedConfigurationPageKey == configPage.Key;
                            using (var node = ImRaii.TreeNode(configPage.Name, hasChildren ?  ImGuiTreeNodeFlags.None : isSelected ? ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.Leaf))
                            {
                                if (node)
                                {
                                    if (configPage.ChildPages != null)
                                    {
                                        foreach (var childPage in configPage.ChildPages)
                                        {
                                            isSelected = ConfigSelectedConfigurationPageKey == childPage.Key;

                                            using (var subNode = ImRaii.TreeNode(childPage.Name,
                                                       isSelected
                                                           ? ImGuiTreeNodeFlags.Selected |
                                                             ImGuiTreeNodeFlags.Bullet
                                                           : ImGuiTreeNodeFlags.Bullet))
                                            {
                                                if (subNode)
                                                {
                                                }
                                            }

                                            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                                            {
                                                ConfigSelectedConfigurationPageKey = childPage.Key;
                                            }
                                        }
                                    }
                                }
                            }

                            if (!hasChildren)
                            {
                                if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                                {
                                    ConfigSelectedConfigurationPageKey = configPage.Key;
                                }
                            }
                        }
                    }

                    ImGui.NewLine();
                    ImGui.TextUnformatted("Item Lists");
                    ImGui.Separator();

                    foreach (var item in _filterPages)
                    {
                        using (var subNode = ImRaii.TreeNode(item.Value.Name,
                                   ConfigSelectedConfigurationPageKey == item.Value.Key
                                       ? ImGuiTreeNodeFlags.Selected |
                                         ImGuiTreeNodeFlags.Leaf
                                       : ImGuiTreeNodeFlags.Leaf))
                        {
                            if (subNode)
                            {
                            }
                        }

                        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                        {
                            ConfigSelectedConfigurationPageKey = item.Value.Key;
                        }

                        var filter = _listService.GetListByKey(item.Key);
                        if (filter != null)
                        {
                            GetFilterMenu(filter).Draw();
                        }

                    }
                }
            }

            using (var commandBarChild = ImRaii.Child("CommandBar",
                       new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, false))
            {
                if (commandBarChild.Success)
                {

                    float height = ImGui.GetWindowSize().Y;
                    ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);

                    var cursorX = ImGui.GetCursorPosX();
                    if (ImGuiService.DrawIconButton(_font, FontAwesomeIcon.Plus, ref cursorX))
                    {

                    }

                    _addFilterMenu.Draw();
                    ImGuiUtil.HoverTooltip("Add a new list");

                    ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                    ImGui.SetCursorPosX(26 * ImGui.GetIO().FontGlobalScale);

                    if (ImGuiService.DrawIconButton(_font, FontAwesomeIcon.Lightbulb, ref cursorX))
                    {

                    }

                    _addSampleMenu.Draw();
                    ImGuiUtil.HoverTooltip("Add a sample filter");

                    var width = ImGui.GetCursorPosX();
                    width -= 24 * ImGui.GetIO().FontGlobalScale;

                    ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                    if (ImGuiService.DrawIconButton(_font, FontAwesomeIcon.Bars, ref width))
                    {

                    }

                    _settingsMenu.Draw();


                    width -= 26 * ImGui.GetIO().FontGlobalScale;

                    ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                    if (ImGuiService.DrawIconButton(_font, FontAwesomeIcon.WandMagicSparkles, ref width))
                    {
                        _wizardMenu.Open();
                    }
                    _wizardMenu.Draw();


                    ImGuiUtil.HoverTooltip("Start configuration wizard.");
                }
            }
        }

        public override void Invalidate()
        {
            GenerateFilterPages();
            RebuildSearchIndex();
        }

        private void RebuildSearchIndex()
        {
            _configSearchService.BuildIndex(SelectablePages());
        }

        public override FilterConfiguration? SelectedConfiguration => null;
    }
}