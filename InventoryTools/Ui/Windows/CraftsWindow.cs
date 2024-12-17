using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using Autofac;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Helpers;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;

using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Widgets;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Lists;
using InventoryTools.Logic.Filters;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;
using PopupMenu = InventoryTools.Ui.Widgets.PopupMenu;
using StringExtensions = InventoryTools.Extensions.StringExtensions;

namespace InventoryTools.Ui
{
    public class CraftsWindow : GenericWindow, IMenuWindow
    {
        private readonly TableService _tableService;
        private readonly InventoryToolsConfiguration _configuration;
        private readonly ConfigurationManagerService _configurationManagerService;
        private readonly IListService _listService;
        private readonly IFilterService _filterService;
        private readonly PluginLogic _pluginLogic;
        private readonly IUniversalis _universalis;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IFileDialogManager _fileDialogManager;
        private readonly IGameUiManager _gameUiManager;
        private readonly IChatUtilities _chatUtilities;
        private readonly ListImportExportService _importExportService;
        private readonly CraftWindowLayoutSetting _layoutSetting;
        private readonly IComponentContext _context;
        private readonly PopupService _popupService;
        private readonly IClipboardService _clipboardService;
        private readonly IKeyState _keyState;
        private readonly ItemSheet _itemSheet;
        private IEnumerable<IMenuWindow> _menuWindows;
        private ThrottleDispatcher _throttleDispatcher;

        public CraftsWindow(ILogger<CraftsWindow> logger, MediatorService mediator, ImGuiService imGuiService,
            InventoryToolsConfiguration configuration, TableService tableService,
            ConfigurationManagerService configurationManagerService, IListService listService,
            IFilterService filterService, PluginLogic pluginLogic, IUniversalis universalis,
            ICharacterMonitor characterMonitor, IFileDialogManager fileDialogManager, IGameUiManager gameUiManager,
            IChatUtilities chatUtilities, ListImportExportService importExportService,
            CraftWindowLayoutSetting layoutSetting, IComponentContext context, PopupService popupService,
            IClipboardService clipboardService, IKeyState keyState, ItemSheet itemSheet) : base(logger, mediator, imGuiService, configuration, "Crafts Window")
        {
            _tableService = tableService;
            _configuration = configuration;
            _configurationManagerService = configurationManagerService;
            _listService = listService;
            _filterService = filterService;
            _pluginLogic = pluginLogic;
            _universalis = universalis;
            _characterMonitor = characterMonitor;
            _fileDialogManager = fileDialogManager;
            _gameUiManager = gameUiManager;
            _chatUtilities = chatUtilities;
            _importExportService = importExportService;
            _layoutSetting = layoutSetting;
            _context = context;
            _popupService = popupService;
            _clipboardService = clipboardService;
            _keyState = keyState;
            _itemSheet = itemSheet;
            Flags = ImGuiWindowFlags.MenuBar;
        }
        public override void Initialize()
        {
            WindowName = "Crafts";
            Key = "crafts";
            _throttleDispatcher = new ThrottleDispatcher(5000, true);
            _splitter = new(_configuration.CraftWindowSplitterPosition, new(100, 100), true);
            _settingsMenu = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", OpenMobsWindow,
                        "Open the mobs window."),
                    new PopupMenu.PopupMenuItemSelectable("Npcs Window", "npcs", OpenNpcsWindow,
                        "Open the npcs window."),
                    new PopupMenu.PopupMenuItemSelectable("Duties Window", "duties", OpenDutiesWindow,
                        "Open the duties window."),
                    new PopupMenu.PopupMenuItemSelectable("Airships Window", "airships", OpenAirshipsWindow,
                        "Open the airships window."),
                    new PopupMenu.PopupMenuItemSelectable("Submarines Window", "submarines", OpenSubmarinesWindow,
                        "Open the submarines window."),
                    new PopupMenu.PopupMenuItemSelectable("Retainer Ventures Window", "ventures",
                        OpenRetainerVenturesWindow, "Open the retainer ventures window."),
                    new PopupMenu.PopupMenuItemSelectable("Tetris", "tetris", OpenTetrisWindow,
                        "Open the tetris window.", () => _configuration.TetrisEnabled),
                    new PopupMenu.PopupMenuItemSeparator(),
                    new PopupMenu.PopupMenuItemSelectable("Help", "help", OpenHelpWindow, "Open the help window."),
                });
            _menuWindows = _context.Resolve<IEnumerable<IMenuWindow>>().OrderBy(c => c.GenericName).Where(c => c.GetType() != this.GetType());
            MediatorService.Subscribe<ListInvalidatedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRepositionedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListAddedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRemovedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<MarketCacheUpdatedMessage>(this, _ => RefreshCraftList());
            MediatorService.Subscribe<TeamCraftDataImported>(this, ImportTeamcraftData);
            MediatorService.Subscribe<FocusListMessage>(this, FocusList);
        }

        private void FocusList(FocusListMessage message)
        {
            if (message.windowType == this.GetType())
            {
                FocusFilter(message.FilterConfiguration);
            }
        }

        private void ImportTeamcraftData(TeamCraftDataImported data)
        {
            if (SelectedConfiguration != null)
            {
                foreach (var item in data.listData)
                {
                    bool isHq = item.Item1 > 1000000;
                    var itemId = item.Item1 % 500000;
                    SelectedConfiguration.CraftList.AddCraftItem(itemId, item.Item2, isHq ? InventoryItem.ItemFlags.HighQuality : InventoryItem.ItemFlags.None);
                }
                SelectedConfiguration.NeedsRefresh = true;
            }
        }

        public override bool SaveState => true;


        public override Vector2? DefaultSize { get; } = new(600, 600);
        public override Vector2? MaxSize => new Vector2(5000, 5000);
        public override Vector2? MinSize => new Vector2(300, 300);
        public override string GenericKey => "crafts";
        public override string GenericName => "Crafts";
        public override bool DestroyOnClose => false;
        private int _selectedFilterTab;
        private bool _settingsActive;
        private bool _addItemBarOpen;

        private HoverButton _editIcon = new();
        private HoverButton _toggleIcon = new();
        private HoverButton _settingsIcon = new();
        private HoverButton _addIcon = new();
        private HoverButton _searchIcon = new();
        private HoverButton _closeSettingsIcon = new();
        private HoverButton _resetButton = new();
        private HoverButton _marketIcon = new();
        private HoverButton _clearIcon = new();
        private HoverButton _export2Icon = new();
        private HoverButton _clipboardIcon = new();
        private HoverButton _importTcIcon = new();
        private HoverButton _filtersIcon = new();
        private HoverButton _menuIcon = new();


        private TeamCraftImportWindow? _teamCraftImportWindow;
        private List<FilterConfiguration>? _filters;
        private FilterConfiguration? _defaultFilter;
        private Dictionary<FilterConfiguration, Widgets.PopupMenu> _popupMenus = new();

        private PopupMenu _settingsMenu = null!;

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

        private void OpenTetrisWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(TetrisWindow)));
        }

        private void RefreshCraftList()
        {
            _throttleDispatcher.ThrottleAsync(RequestRefresh);
        }

        private Task RequestRefresh()
        {
            if (SelectedConfiguration != null)
            {
                MediatorService.Publish(new RequestListUpdateMessage(SelectedConfiguration));
            }

            return Task.CompletedTask;
        }

        public Widgets.PopupMenu GetFilterMenu(FilterConfiguration configuration, WindowLayout layout)
        {
            if (!_popupMenus.ContainsKey(configuration))
            {
                _popupMenus[configuration] = new Widgets.PopupMenu("fm" + configuration.Key, Widgets.PopupMenu.PopupMenuButtons.Right,
                    new List<Widgets.PopupMenu.IPopupMenuItem>()
                    {
                        new Widgets.PopupMenu.PopupMenuItemSelectable("Edit", "ef_" + configuration.Key, EditFilter, "Edit the craft list."),
                        new Widgets.PopupMenu.PopupMenuItemSelectableAskName("Duplicate", "df_" + configuration.Key, configuration.Name, DuplicateFilter, "Duplicate the craft list."),
                        new Widgets.PopupMenu.PopupMenuItemSelectable(layout == WindowLayout.Tabs ? "Move Left" : "Move Up", "mu_" + configuration.Key, MoveFilterUp, layout == WindowLayout.Tabs ? "Move the craft list left." : "Move the craft list up."),
                        new Widgets.PopupMenu.PopupMenuItemSelectable(layout == WindowLayout.Tabs ? "Move Right" : "Move Down", "md_" + configuration.Key, MoveFilterDown, layout == WindowLayout.Tabs ? "Move the craft list right." : "Move the craft list down."),
                        new Widgets.PopupMenu.PopupMenuItemSelectableConfirm("Remove", "rf_" + configuration.Key, "Are you sure you want to remove this craft list?", RemoveFilter, "Remove the craft list."),
                    }
                );
            }

            return _popupMenus[configuration];
        }

        private void EditFilter(string id)
        {
            id = id.Replace("ef_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                FocusFilter(existingFilter, true);
            }
        }


        private void RemoveFilter(string id, bool confirmed)
        {
            if (confirmed)
            {
                id = id.Replace("rf_", "");
                var existingFilter = _listService.GetListByKey(id);
                if (existingFilter != null)
                {
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
                var currentFilter = this.SelectedConfiguration;
                _listService.MoveListDown(existingFilter);
                if (currentFilter != null)
                {
                    FocusFilter(currentFilter);
                }
            }
        }

        private void MoveFilterUp(string id)
        {
            id = id.Replace("mu_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                var currentFilter = this.SelectedConfiguration;
                _listService.MoveListUp(existingFilter);
                if (currentFilter != null)
                {
                    FocusFilter(currentFilter);
                }
            }
        }

        private void DuplicateFilter(string filterName, string id)
        {
            id = id.Replace("df_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                var newFilter = _listService.DuplicateList(existingFilter, filterName);
                FocusFilter(newFilter);
            }
        }


        private List<FilterConfiguration> Filters
        {
            get
            {
                if (_filters == null)
                {
                    _filters = _listService.Lists.Where(c => c.FilterType == FilterType.CraftFilter && c.CraftListDefault == false).ToList();
                }

                return _filters;
            }
        }

        private FilterConfiguration DefaultConfiguration
        {
            get
            {
                if (_defaultFilter == null)
                {
                    _defaultFilter = _listService.GetDefaultCraftList();
                }

                return _defaultFilter;
            }
        }

        public void FocusFilter(FilterConfiguration filterConfiguration, bool showSettings = false)
        {
            var filterConfigurations = Filters;
            if (filterConfigurations.Contains(filterConfiguration))
            {
                _selectedFilterTab = filterConfigurations.IndexOf(filterConfiguration);
                var filterIndex = Filters.Contains(filterConfiguration) ? Filters.IndexOf(filterConfiguration) : -1;
                if (filterIndex != -1)
                {
                    _newTab = filterIndex;
                }

                _applyNewTabTime = DateTime.Now + TimeSpan.FromMilliseconds(10);
                if (showSettings)
                {
                    _settingsActive = true;
                }
            }
        }

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Configuration"))
                    {
                        this.MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
                    }

                    if (ImGui.MenuItem("Help"))
                    {
                        this.MediatorService.Publish(new OpenGenericWindowMessage(typeof(HelpWindow)));
                    }

                    if (ImGui.MenuItem("Report a Issue"))
                    {
                        "https://github.com/Critical-Impact/InventoryTools".OpenBrowser();
                    }

                    if (ImGui.MenuItem("Ko-Fi"))
                    {
                        "https://ko-fi.com/critical_impact".OpenBrowser();
                    }

                    if (ImGui.MenuItem("Close"))
                    {
                        this.IsOpen = false;
                    }

                    ImGui.EndMenu();
                }

                if (this.SelectedConfiguration != null && ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Clear Search"))
                    {
                        _tableService.GetListTable(SelectedConfiguration).ClearFilters();
                    }

                    ImGui.Separator();

                    if (ImGui.BeginMenu("Copy List Contents"))
                    {
                        if (ImGui.MenuItem("Craft List (All)"))
                        {
                            var searchResults = SelectedConfiguration.CraftList.GetFlattenedMergedMaterials()
                                .ToList();
                            var tcString = _importExportService.ToTCString(searchResults);
                            _clipboardService.CopyToClipboard(tcString);
                            _chatUtilities.Print("The craft list's contents were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("Craft List (Outputs)"))
                        {
                            var searchResults = SelectedConfiguration.CraftList.GetFlattenedMergedMaterials()
                                .Where(c => c.IsOutputItem)
                                .ToList();

                            var tcString = _importExportService.ToTCString(searchResults);
                            _clipboardService.CopyToClipboard(tcString);
                            _chatUtilities.Print("The craft list's outputs were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("Craft List (Precrafts)"))
                        {
                            var searchResults = SelectedConfiguration.CraftList.GetFlattenedMergedMaterials()
                                .Where(c => c is { IsOutputItem: false, IngredientPreference.Type: IngredientPreferenceType.Crafting })
                                .ToList();

                            var tcString = _importExportService.ToTCString(searchResults);
                            _clipboardService.CopyToClipboard(tcString);
                            _chatUtilities.Print("The craft list's outputs were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("Craft List (Gatherables)"))
                        {
                            var searchResults = SelectedConfiguration.CraftList.GetFlattenedMergedMaterials()
                                .Where(c => c.Item.ObtainedGathering && !c.IsOutputItem)
                                .ToList();

                            var tcString = _importExportService.ToTCString(searchResults);
                            _clipboardService.CopyToClipboard(tcString);
                            _chatUtilities.Print("The craft list's gatherables were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("Craft List (Missing Gatherables)"))
                        {
                            var searchResults = SelectedConfiguration.CraftList.GetFlattenedMergedMaterials()
                                .Where(c => c.Item.ObtainedGathering && !c.IsOutputItem)
                                .ToList();

                            var tcString = _importExportService.ToTCString(searchResults, TCExportMode.Missing);
                            _clipboardService.CopyToClipboard(tcString);
                            _chatUtilities.Print("The craft list's gatherables were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("Retainer/Bag List"))
                        {
                            var searchResults = _tableService.GetListTable(SelectedConfiguration).SearchResults
                                .ToList();
                            var tcString = _importExportService.ToTCString(searchResults);
                            _clipboardService.CopyToClipboard(tcString);
                            _chatUtilities.Print("The retainer/bag were copied to your clipboard.");
                        }
                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Copy List Contents (JSON)"))
                    {
                        if (ImGui.MenuItem("Craft List (All)"))
                        {
                            var craftTable = _tableService.GetCraftTable(SelectedConfiguration);
                            var searchResults = craftTable.CraftItems
                                .ToList();
                            _clipboardService.CopyToClipboard(craftTable.ExportToJson(searchResults));
                            _chatUtilities.Print("The craft list's contents were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("Craft List (Outputs)"))
                        {
                            var craftTable = _tableService.GetCraftTable(SelectedConfiguration);
                            var searchResults = craftTable.CraftItems
                                .Where(c => c.CraftItem?.IsOutputItem ?? false)
                                .ToList();
                            _clipboardService.CopyToClipboard(craftTable.ExportToJson(searchResults));
                            _chatUtilities.Print("The craft list's outputs were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("Craft List (Precrafts)"))
                        {
                            var craftTable = _tableService.GetCraftTable(SelectedConfiguration);
                            var searchResults = craftTable.CraftItems
                                .Where(c => c.CraftItem is { IsOutputItem: false, IngredientPreference.Type: IngredientPreferenceType.Crafting })
                                .ToList();
                            _clipboardService.CopyToClipboard(craftTable.ExportToJson(searchResults));
                            _chatUtilities.Print("The craft list's outputs were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("Craft List (Gatherables)"))
                        {
                            var craftTable = _tableService.GetCraftTable(SelectedConfiguration);
                            var searchResults = craftTable.CraftItems
                                .Where(c => c.Item.ObtainedGathering && (c.CraftItem?.IsOutputItem ?? false))
                                .ToList();
                            _clipboardService.CopyToClipboard(craftTable.ExportToJson(searchResults));
                            _chatUtilities.Print("The craft list's gatherables were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("Retainer/Bag List"))
                        {
                            var itemTable = _tableService.GetListTable(SelectedConfiguration);
                            _clipboardService.CopyToClipboard(itemTable.ExportToJson());
                        }

                        ImGui.EndMenu();
                    }

                    if (ImGui.MenuItem("Paste List Contents"))
                    {
                        var importedList = _importExportService.FromTCString(_clipboardService.PasteFromClipboard(), false);
                        if (importedList == null)
                        {
                            _chatUtilities.PrintError("The contents of your clipboard could not be parsed.");
                        }
                        else
                        {
                            _chatUtilities.Print("The contents of your clipboard were imported.");
                            this.SelectedConfiguration.AddItemsToList(importedList);
                        }
                    }
                    if (ImGui.MenuItem("Clear List"))
                    {
                        _popupService.AddPopup(new ConfirmPopup(GetType(), "craftListDelete",
                            "Are you sure you want to clear your craft list?",
                            result =>
                            {
                                if (result)
                                {
                                    this.SelectedConfiguration.CraftList.CraftItems.Clear();
                                    this.SelectedConfiguration.CraftList.NeedsRefresh = true;
                                }
                            }));

                    }
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Add to Craft List"))
                    {
                        var craftLists = _listService.Lists
                            .Where(c => c.FilterType == FilterType.CraftFilter && c.CraftListDefault == false)
                            .OrderBy(c => c.Order)
                            .ToList();

                        foreach (var craft in craftLists)
                        {
                            if (ImGui.MenuItem(craft.Name))
                            {
                                var searchResults = _tableService.GetListTable(SelectedConfiguration).SearchResults;
                                foreach (var searchResult in searchResults)
                                {
                                    craft.CraftList.AddCraftItem(searchResult.ItemId, searchResult.Quantity,
                                        searchResult.Flags);
                                }
                                MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                                MediatorService.Publish(new FocusListMessage(typeof(CraftsWindow), craft));
                            }
                        }
                        if (craftLists.Count != 0)
                        {
                            ImGui.Separator();
                        }

                        if (ImGui.MenuItem("New Craft List"))
                        {
                            _popupService.AddPopup(new NamePopup(typeof(CraftsWindow), "newCraftList", "New Craft List",
                                result =>
                                {
                                    if (result.Item1)
                                    {
                                        var craftList = _listService.AddNewCraftList(result.Item2);
                                        var searchResults = _tableService.GetListTable(SelectedConfiguration).SearchResults;
                                        foreach (var searchResult in searchResults)
                                        {
                                            craftList.CraftList.AddCraftItem(searchResult.ItemId, searchResult.Quantity,
                                                searchResult.Flags);
                                        }
                                    }
                                }));
                        }

                        if (ImGui.MenuItem("New Craft List (Ephemeral)"))
                        {
                            _popupService.AddPopup(new NamePopup(typeof(CraftsWindow), "newCraftList", "New Craft List",
                                result =>
                                {
                                    if (result.Item1)
                                    {
                                        var craftList = _listService.AddNewCraftList(result.Item2, true);
                                        var searchResults = _tableService.GetListTable(SelectedConfiguration).SearchResults;
                                        foreach (var searchResult in searchResults)
                                        {
                                            craftList.CraftList.AddCraftItem(searchResult.ItemId, searchResult.Quantity,
                                                searchResult.Flags);
                                        }
                                    }
                                }));
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("Add to Curated List"))
                    {
                        var curatedLists = _listService.Lists
                            .Where(c => c.FilterType == FilterType.CuratedList)
                            .OrderBy(c => c.Order)
                            .ToList();

                        foreach (var curatedList in curatedLists)
                        {
                            if (ImGui.MenuItem(curatedList.Name))
                            {
                                var searchResults = _tableService.GetListTable(SelectedConfiguration).SearchResults;
                                foreach (var searchResult in searchResults)
                                {
                                    curatedList.AddCuratedItem(new CuratedItem(searchResult.ItemId, searchResult.Quantity,
                                        searchResult.Flags));
                                }
                            }
                        }

                        if (curatedLists.Count != 0)
                        {
                            ImGui.Separator();
                        }

                        if (ImGui.MenuItem("New Curated List"))
                        {
                            _popupService.AddPopup(new NamePopup(typeof(CraftsWindow), "newCuratedList", "New Curated List",
                                result =>
                                {
                                    if (result.Item1)
                                    {
                                        var curatedList = _listService.AddNewCuratedList(result.Item2);
                                        var searchResults = _tableService.GetListTable(SelectedConfiguration).SearchResults;
                                        foreach (var searchResult in searchResults)
                                        {
                                            curatedList.AddCuratedItem(new CuratedItem(searchResult.ItemId, searchResult.Quantity,
                                                searchResult.Flags));
                                        }
                                        this.MediatorService.Publish(new FocusListMessage(typeof(FiltersWindow), curatedList));
                                        curatedList.NeedsRefresh = true;
                                    }
                                }));
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }


                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.MenuItem("Tabs", "", _layoutSetting.CurrentValue(_configuration) == WindowLayout.Tabs))
                    {
                        _layoutSetting.UpdateFilterConfiguration(_configuration, WindowLayout.Tabs);
                    }
                    if (ImGui.MenuItem("Sidebar", "", _layoutSetting.CurrentValue(_configuration) == WindowLayout.Sidebar))
                    {
                        _layoutSetting.UpdateFilterConfiguration(_configuration, WindowLayout.Sidebar);
                    }
                    if (ImGui.MenuItem("Single", "", _layoutSetting.CurrentValue(_configuration) == WindowLayout.Single))
                    {
                        _layoutSetting.UpdateFilterConfiguration(_configuration, WindowLayout.Single);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Export"))
                {
                    if (ImGui.MenuItem("Craft List (CSV)"))
                    {
                        if (SelectedConfiguration != null)
                        {
                            _fileDialogManager.SaveFileDialog("Save to csv", "*.csv",
                                "export-craft-list.csv", ".csv",
                                (b, s) =>
                                {
                                    var craftTable = _tableService.GetCraftTable(SelectedConfiguration);
                                    SaveCraftCallback(craftTable, b, s);
                                }, null, true);
                        }
                    }
                    if (ImGui.MenuItem("Retainer/Bag List (CSV)"))
                    {
                        if (SelectedConfiguration != null)
                        {
                            var itemTable = _tableService.GetListTable(SelectedConfiguration);
                            _fileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv",
                                (b, s) => { SaveCallback(itemTable, b, s); }, null, true);
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Market"))
                {
                    if (ImGui.MenuItem("Refresh All Prices (Craft List)"))
                    {
                        var activeCharacter = _characterMonitor.ActiveCharacter;
                        if (activeCharacter != null && SelectedConfiguration != null)
                        {
                            var itemTable = _tableService.GetCraftTable(SelectedConfiguration);
                            foreach (var item in itemTable.CraftItems)
                            {
                                _universalis.QueuePriceCheck(item.Item.RowId, activeCharacter.WorldId);
                            }
                        }
                    }
                    if (ImGui.MenuItem("Refresh All Prices (Retainer/Bags)"))
                    {
                        var activeCharacter = _characterMonitor.ActiveCharacter;
                        if (activeCharacter != null && SelectedConfiguration != null)
                        {
                            var itemTable = _tableService.GetListTable(SelectedConfiguration);
                            foreach (var item in itemTable.RenderSearchResults)
                            {
                                _universalis.QueuePriceCheck(item.Item.RowId, activeCharacter.WorldId);
                            }
                        }
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Lists"))
                {
                    if (ImGui.BeginMenu("Add"))
                    {
                        if (ImGui.MenuItem("Craft List"))
                        {
                            _popupService.AddPopup(new NamePopup(GetType(),"addCraftList", "", result =>
                            {
                                if (result.Item1)
                                {
                                    AddCraftFilter(result.Item2);
                                }
                            }));
                        }
                        if (ImGui.MenuItem("Craft List (Ephemeral)"))
                        {
                            _popupService.AddPopup(new NamePopup(GetType(),"addCraftListEphemeral", "", result =>
                            {
                                if (result.Item1)
                                {
                                    AddCraftFilter(result.Item2);
                                }
                            }));
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.NewLine();

                    var windowGroups = _listService.Lists.GroupBy(c => c.FilterType).OrderBySequence([FilterType.CraftFilter, FilterType.SearchFilter, FilterType.SortingFilter, FilterType.GameItemFilter, FilterType.HistoryFilter, FilterType.CuratedList], grouping => grouping.Key).ToList();
                    for (var index = 0; index < windowGroups.Count; index++)
                    {
                        var windowGroup = windowGroups[index];
                        ImGui.Text(windowGroup.Key.FormattedName());
                        ImGui.Separator();
                        foreach (var window in windowGroup.OrderBy(c => c.CraftListDefault).ThenBy(c => c.Order))
                        {
                            if (ImGui.MenuItem(window.Name, "", SelectedConfiguration == window || (SelectedConfiguration == null && window.CraftListDefault)))
                            {
                                if (window.FilterType == FilterType.CraftFilter)
                                {
                                    if (_keyState[VirtualKey.CONTROL])
                                    {
                                        this.MediatorService.Publish(new OpenStringWindowMessage(typeof(FilterWindow), window.Key));
                                    }
                                    else
                                    {
                                        if (window.CraftListDefault)
                                        {
                                            _selectedFilterTab = Filters.Count + 1;
                                        }
                                        else
                                        {
                                            MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                                            MediatorService.Publish(new FocusListMessage(typeof(CraftsWindow), window));
                                        }
                                    }

                                }
                                else
                                {
                                    if (_keyState[VirtualKey.CONTROL])
                                    {
                                        this.MediatorService.Publish(new OpenStringWindowMessage(typeof(FilterWindow), window.Key));
                                    }
                                    else
                                    {
                                        MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
                                        MediatorService.Publish(new FocusListMessage(typeof(FiltersWindow), window));
                                    }
                                }
                            }
                            ImGuiUtil.HoverTooltip("[CTRL] to open in a new window.");
                        }

                        if (index != windowGroups.Count - 1)
                        {
                            ImGui.NewLine();
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Windows"))
                {
                    if (_menuWindows != null)
                    {
                        foreach (var window in _menuWindows)
                        {
                            if (ImGui.MenuItem(window.GenericName))
                            {
                                this.MediatorService.Publish(new OpenGenericWindowMessage(window.GetType()));
                            }
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("Toggle Crafting Overlay"))
                {
                    this.MediatorService.Publish(new ToggleGenericWindowMessage(typeof(CraftOverlayWindow)));
                }

                ImGui.EndMenuBar();
            }
        }

        public override unsafe void Draw()
        {
            DrawMenuBar();
            _popupService.Draw(GetType());
            if (!_configuration.HasSeenNotification(NotificationPopup.CraftNotice) && ImGui.IsWindowFocused())
            {
                ImGui.OpenPopup("notification");
                _configuration.MarkNotificationSeen(NotificationPopup.CraftNotice);
            }

            ImGuiUtil.HelpPopup("notification", new Vector2(750,340) * ImGui.GetIO().FontGlobalScale, () =>
            {
                ImGui.TextUnformatted("Craft System Notice");
                ImGui.Separator();
                ImGui.NewLine();
                ImGui.PushTextWrapPos();
                ImGui.Bullet();
                ImGui.Text("The craft system has received an update, and your default configuration has been reset. Please readjust it according to your preferences.");
                ImGui.PopTextWrapPos();

                ImGui.BulletText("You can now copy configurations between your craft lists.");

                ImGui.BulletText("Two new columns have been added to your craft lists: 'Next Step' and 'Settings'.");

                ImGui.Indent();
                ImGui.BulletText("The 'Next Step' column provides guidance on what you should do next.");
                ImGui.Unindent();

                ImGui.Indent();
                ImGui.BulletText("The 'Settings' column allows you to configure item sourcing, retainer settings, and recipes.");
                ImGui.Unindent();

                ImGui.BulletText("The update includes the following changes:");

                ImGui.Indent();
                ImGui.BulletText("You can now change groupings for crafts based on class or required crafting order.");
                ImGui.BulletText("Retrievable items can be prioritized in their own group.");
                ImGui.BulletText("Gatherable and purchasable items can be grouped by zone.");
                ImGui.BulletText("Improved handling of items that can be purchased with seals, poetics, and scrip currencies.");
                ImGui.Unindent();

                ImGui.BulletText("You can customize these options further by clicking the pencil icon in the top right corner of a list.");

            });

            if (_configuration.CraftWindowLayout == WindowLayout.Sidebar)
            {
                DrawSidebar();
                DrawMainWindow();
            }
            else if (_configuration.CraftWindowLayout == WindowLayout.Tabs)
            {
                DrawTabBar();
            }
            else
            {
                DrawMainWindow();
            }
        }

        private string _newCraftName = "";
        private bool openNewFilterNamePopup;
        private bool openNewTypePopup;
        private bool _ephemeralList;
        private unsafe void DrawTabBar()
        {
            if (openNewFilterNamePopup)
            {
                ImGui.OpenPopup("addCraftFilterName");
                openNewFilterNamePopup = false;
            }
            if (ImGuiUtil.OpenNameField("addCraftFilterName", ref _newCraftName))
            {
                Service.Framework.RunOnFrameworkThread(() =>
                {
                    AddCraftFilter(_newCraftName, _ephemeralList);
                    _newCraftName = "";
                });
            }
            if (openNewTypePopup)
            {
                ImGui.OpenPopup("addCraftFilterType");
                openNewTypePopup = false;
            }
            using(var popup = ImRaii.Popup("addCraftFilterType"))
            {
                if (popup.Success)
                {
                    if (ImGui.Selectable("Normal List"))
                    {
                        _ephemeralList = false;
                        openNewFilterNamePopup = true;
                    }
                    ImGuiUtil.HoverTooltip("Add a new craft list.");

                    if (ImGui.Selectable("Ephemeral List"))
                    {
                        _ephemeralList = true;
                        openNewFilterNamePopup = true;
                    }
                    ImGuiUtil.HoverTooltip("Add a new ephemeral craft list that will be deleted once all the items in it are completed.");
                }
            }

            using (var tabbar = ImRaii.TabBar("CraftTabs", ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton))
            {
                if (tabbar.Success)
                {
                    var filterConfigurations = Filters;
                    for (var index = 0; index < filterConfigurations.Count; index++)
                    {
                        var filterConfiguration = filterConfigurations[index];
                        using var id = ImRaii.PushId(index);
                        var imGuiTabItemFlags = _newTab == index && SwitchNewTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
                        fixed (byte* namePtr = filterConfiguration.NameAsBytes)
                        {
                            using (var tabItem = ImRaii.TabItem(namePtr, imGuiTabItemFlags))
                            {
                                if (SwitchNewTab && _newTab != null && _newTab == index)
                                {
                                    _newTab = null;
                                    _applyNewTabTime = null;
                                    _selectedFilterTab = index;
                                }
                                GetFilterMenu(filterConfiguration, WindowLayout.Tabs).Draw();

                                if (tabItem.Success)
                                {
                                    _selectedFilterTab = index;
                                    DrawMainWindow();
                                }
                            }
                        }
                    }
                    using (var tabItem = ImRaii.TabItem("Default Configuration"))
                    {
                        if (_filters != null && tabItem.Success)
                        {
                            _selectedFilterTab = filterConfigurations.Count + 1;
                            DrawMainWindow();
                        }
                    }
                    if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
                    {
                        openNewTypePopup = true;
                    }
                    ImGuiUtil.HoverTooltip("Add a new craft list");
                }
            }
        }

        private void AddCraftFilter(string newName, bool ephemeralList = false)
        {
            var filterConfiguration = _listService.AddNewCraftList(newName, ephemeralList);
            Invalidate();
            this.FocusFilter(filterConfiguration);
        }

        private int? _newTab;
        private DateTime? _applyNewTabTime;

        private bool SwitchNewTab => _newTab != null && _applyNewTabTime != null && _applyNewTabTime.Value <= DateTime.Now;

        private void DrawMainWindow()
        {
            var isWindowFocused = ImGui.IsWindowFocused();
            var filterConfigurations = Filters;
            using (var child = ImRaii.Child("Main",
                       new Vector2(_addItemBarOpen ? -250 : -1, -1) * ImGui.GetIO().FontGlobalScale, false,
                       ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (child.Success)
                {
                    if (filterConfigurations.Count == 0 && _selectedFilterTab == 0)
                    {
                        using (var contentChild = ImRaii.Child("Content", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, true))
                        {
                            if (contentChild.Success)
                            {
                                ImGui.TextUnformatted(
                                    "Get started by adding a craft list by hitting the + button on the bottom left.");
                            }
                        }
                    }

                    for (var index = 0; index < filterConfigurations.Count; index++)
                    {
                        var filterConfiguration = filterConfigurations[index];

                        if (_selectedFilterTab == index)
                        {

                            if (isWindowFocused)
                            {
                                if (filterConfiguration.Active != true)
                                {
                                    filterConfiguration.NeedsRefresh = true;
                                    filterConfiguration.Active = true;
                                }
                                if (_configuration.SwitchFiltersAutomatically &&
                                    _configuration.ActiveUiFilter != filterConfiguration.Key &&
                                    _configuration.ActiveUiFilter != null)
                                {
                                    Service.Framework.RunOnFrameworkThread(() =>
                                    {
                                        _listService.ToggleActiveUiList(filterConfiguration);
                                    });
                                }
                                if (_configuration.SwitchCraftListsAutomatically &&
                                    _configuration.ActiveCraftList != filterConfiguration.Key &&
                                    _configuration.ActiveCraftList != null && filterConfiguration.FilterType == FilterType.CraftFilter)
                                {
                                    Service.Framework.RunOnFrameworkThread(() =>
                                    {
                                        _listService.ToggleActiveCraftList(filterConfiguration);
                                    });
                                }
                            }

                            if (_settingsActive)
                            {
                                DrawSettingsPanel(filterConfiguration);
                            }
                            else
                            {
                                DrawCraftPanel(filterConfiguration);
                            }
                        }
                        else
                        {
                            if (isWindowFocused)
                            {
                                filterConfiguration.Active = false;
                            }
                        }
                    }

                    if (_selectedFilterTab == filterConfigurations.Count + 1)
                    {
                        DrawSettingsPanel(DefaultConfiguration);
                    }
                }
            }

            ImGui.SameLine();
            if (_addItemBarOpen)
            {
                using (var addItemChild = ImRaii.Child("AddItem", new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, true))
                {
                    if (addItemChild.Success)
                    {
                        for (var index = 0; index < filterConfigurations.Count; index++)
                        {
                            if (_selectedFilterTab == index)
                            {
                                var filterConfiguration = filterConfigurations[index];
                                if (filterConfiguration.FilterType == FilterType.CraftFilter)
                                {
                                    ImGui.TextUnformatted("Add new Item");
                                    var searchString = SearchString;
                                    ImGui.InputText("##ItemSearch", ref searchString, 50);
                                    if (_searchString != searchString)
                                    {
                                        SearchString = searchString;
                                    }

                                    ImGui.Separator();
                                    if (_searchString == "")
                                    {
                                        ImGui.TextUnformatted("Start typing to search...");
                                    }

                                    using var table = ImRaii.Table("", 2, ImGuiTableFlags.None);
                                    if (!table || !table.Success)
                                        return;

                                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, 200);
                                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.None, 16);

                                    foreach (var datum in SearchItems)
                                    {
                                        ImGui.TableNextRow();
                                        DrawSearchRow(filterConfiguration, datum);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawSidebar()
        {
            var filterConfigurations = Filters;
            using (var sideMenuChild = ImRaii.Child("SideMenu", new Vector2(180, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (sideMenuChild.Success)
                {
                    using (var craftListChild = ImRaii.Child("CraftList", new Vector2(0, -28) * ImGui.GetIO().FontGlobalScale, false))
                    {
                        if (craftListChild.Success)
                        {
                            for (var index = 0; index < filterConfigurations.Count; index++)
                            {
                                var filterConfiguration = filterConfigurations[index];
                                var actualName = filterConfiguration.Name;
                                if (filterConfiguration.IsEphemeralCraftList)
                                {
                                    actualName += " (*)";
                                }
                                if (ImGui.Selectable(actualName + "###fl" + filterConfiguration.Key,
                                        index == _selectedFilterTab))
                                {
                                    _selectedFilterTab = index;
                                    if (_configuration.SwitchFiltersAutomatically &&
                                        _configuration.ActiveUiFilter != filterConfiguration.Key &&
                                        _configuration.ActiveUiFilter != null)
                                    {
                                        Service.Framework.RunOnFrameworkThread(() =>
                                        {
                                            _listService.ToggleActiveUiList(filterConfiguration);
                                        });
                                    }
                                    if (_configuration.SwitchCraftListsAutomatically &&
                                        _configuration.ActiveCraftList != filterConfiguration.Key &&
                                        _configuration.ActiveCraftList != null && filterConfiguration.FilterType == FilterType.CraftFilter)
                                    {
                                        Service.Framework.RunOnFrameworkThread(() =>
                                        {
                                            _listService.ToggleActiveCraftList(filterConfiguration);
                                        });
                                    }
                                }

                                GetFilterMenu(filterConfiguration, WindowLayout.Sidebar).Draw();
                            }

                            if (filterConfigurations.Count == 0)
                            {
                                ImGui.TextUnformatted("No craft lists created.");
                            }

                            ImGui.Separator();
                            if (_filters != null && ImGui.Selectable("Default Configuration",
                                    filterConfigurations.Count + 1 == _selectedFilterTab))
                            {
                                _selectedFilterTab = filterConfigurations.Count + 1;
                            }
                        }
                    }

                    using (var commandBarChild = ImRaii.Child("CommandBar", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, false))
                    {
                        if (commandBarChild.Success)
                        {
                            float height = ImGui.GetWindowSize().Y;
                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            if (_addIcon.Draw(ImGuiService.GetIconTexture(66315).ImGuiHandle, "cb_acf"))
                            {
                                _pluginLogic.AddNewCraftFilter();
                            }

                            ImGuiUtil.HoverTooltip("Add a new craft list.");
                        }
                    }
                }
            }

            ImGui.SameLine();
        }

        private HorizontalSplitter _splitter;

        private unsafe void DrawCraftPanel(FilterConfiguration filterConfiguration)
        {
            var itemTable = _tableService.GetListTable(filterConfiguration);
            var craftTable = _tableService.GetCraftTable(filterConfiguration);
            using (var topBarChild = ImRaii.Child("TopBar", new Vector2(0, 40) * ImGui.GetIO().FontGlobalScale, true, ImGuiWindowFlags.NoScrollbar))
            {
                if (topBarChild.Success)
                {
                    var highlightItems = itemTable.HighlightItems;
                    ImGuiService.CenterElement(22 * ImGui.GetIO().FontGlobalScale);
                    ImGui.Checkbox("Highlight?" + "###" + itemTable.Key + "VisibilityCheckbox", ref highlightItems);
                    if (highlightItems != itemTable.HighlightItems)
                    {
                        Service.Framework.RunOnFrameworkThread(() =>
                        {
                            _listService.ToggleActiveUiList(itemTable.FilterConfiguration);
                        });
                    }
                    ImGuiUtil.HoverTooltip("When checked, any items you need to retrieve from external sources will be highlighted.");

                    ImGui.SameLine();
                    if (_clearIcon.Draw(ImGuiService.GetIconTexture(66308).ImGuiHandle, "tb_cf"))
                    {
                        itemTable.ClearFilters();
                    }

                    ImGuiUtil.HoverTooltip("Clear the current search.");

                    ImGui.SameLine();
                    ImGuiService.CenterElement(22 * ImGui.GetIO().FontGlobalScale);
                    var hideCompleted = filterConfiguration.CraftList.HideComplete;
                    ImGui.Checkbox("Hide Completed?" + "###" + itemTable.Key + "HideCompleted", ref hideCompleted);
                    if (hideCompleted != filterConfiguration.CraftList.HideComplete)
                    {
                        filterConfiguration.CraftList.HideComplete = hideCompleted;
                        filterConfiguration.NeedsRefresh = true;
                    }

                    ImGuiUtil.HoverTooltip("Hide any precrafts/gather/buy items once completed?");

                    ImGui.SameLine();
                    float width = ImGui.GetWindowSize().X;
                    width -= 28 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    if (_searchIcon.Draw(ImGuiService.GetIconTexture(66320).ImGuiHandle, "tb_oib"))
                    {
                        _addItemBarOpen = !_addItemBarOpen;
                    }

                    ImGuiUtil.HoverTooltip("Toggles the add item side bar.");

                    ImGui.SameLine();
                    width -= 28 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    if (_editIcon.Draw(ImGuiService.GetImageTexture("edit").ImGuiHandle, "tb_edit"))
                    {
                        _settingsActive = !_settingsActive;
                    }

                    ImGuiUtil.HoverTooltip("Edit the craft list's configuration.");

                    ImGui.SameLine();
                    width -= 28 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    if (_toggleIcon.Draw(ImGuiService.GetImageTexture("toggle").ImGuiHandle, "set_active"))
                    {
                        _listService.ToggleActiveCraftList(filterConfiguration);
                    }
                    ImGuiUtil.HoverTooltip("Toggle the current craft list.");

                    ImGui.SameLine();
                    width -= 156 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    ImGui.SetNextItemWidth(150);
                    var activeCraftList = _listService.GetActiveCraftList();
                    using (var combo = ImRaii.Combo("##ActiveCraftList",activeCraftList != null ? activeCraftList.Name : "None"))
                    {
                        if (combo.Success)
                        {
                            if (ImGui.Selectable("None"))
                            {
                                _listService.ClearActiveCraftList();
                            }
                            foreach (var filter in _listService.Lists.Where(c =>
                                         c.FilterType == FilterType.CraftFilter && !c.CraftListDefault))
                            {
                                if (ImGui.Selectable(filter.Name + "##" + filter.Key))
                                {
                                    _listService.SetActiveCraftList(filter);
                                }
                            }
                        }
                    }
                    ImGuiUtil.HoverTooltip("This is the craft list that finished crafts will count towards.");
                    ImGui.SameLine();
                    var textSize = ImGui.CalcTextSize("Active: ");
                    width -= textSize.X * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    ImGui.Text("Active: ");
                    if (SelectedConfiguration?.IsEphemeralCraftList ?? false)
                    {
                        ImGui.SameLine();
                        width -= 28 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        ImGui.Image(ImGuiService.GetImageTexture("recycle").ImGuiHandle,
                            new Vector2(22, 22));
                        ImGuiUtil.HoverTooltip("This is the ephemeral craft list, once all items in it are completed, the list will delete itself.");
                    }
                }
            }

            using (var contentChild = ImRaii.Child("Content", new Vector2(0, -44) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (contentChild.Success)
                {
                    var result = _splitter.Draw((shouldDraw) =>
                    {
                        MediatorService.Publish(craftTable.Draw(new Vector2(0,0), shouldDraw));
                    }, (shouldDraw) =>
                    {
                        MediatorService.Publish(itemTable.Draw(new Vector2(0, 0), shouldDraw));
                    }, "To Craft", "Items in Retainers/Bags");
                    if (result != null)
                    {
                        _configuration.CraftWindowSplitterPosition = (int)result.Value;
                        _configuration.IsDirty = true;
                    }
                }
            }


            //Need to have these buttons be determined dynamically or moved elsewhere
            using (var bottomBarChild = ImRaii.Child("BottomBar", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale,
                       true, ImGuiWindowFlags.NoScrollbar))
            {
                if (bottomBarChild.Success)
                {
                    if (_marketIcon.Draw(ImGuiService.GetImageTexture("refresh-web").ImGuiHandle, "bb_market"))
                    {
                        var activeCharacter = _characterMonitor.ActiveCharacter;
                        foreach (var item in itemTable.RenderSearchResults)
                        {
                            if (activeCharacter != null)
                            {
                                _universalis.QueuePriceCheck(item.Item.RowId, activeCharacter.WorldId);
                            }
                        }

                        foreach (var item in filterConfiguration.CraftList.GetFlattenedMergedMaterials())
                        {
                            var useActiveWorld = filterConfiguration.GetBooleanFilter("CraftWorldPriceUseActiveWorld");
                            var useHomeWorld = filterConfiguration.GetBooleanFilter("CraftWorldPriceUseHomeWorld");
                            var character = _characterMonitor.ActiveCharacter;
                            HashSet<uint> worldIds = new HashSet<uint>();

                            var marketItemWorldPreference = filterConfiguration.CraftList.GetMarketItemWorldPreference(item.ItemId);
                            if (marketItemWorldPreference != null)
                            {
                                worldIds.Add(marketItemWorldPreference.Value);
                            }

                            if (character != null)
                            {
                                if (useActiveWorld == true)
                                {
                                    worldIds.Add(character.ActiveWorldId);
                                }
                                if (useHomeWorld == true)
                                {
                                    worldIds.Add(character.WorldId);
                                }
                            }

                            foreach (var worldId in filterConfiguration.CraftList.WorldPricePreference)
                            {
                                worldIds.Add(worldId);
                            }

                            foreach (var worldId in worldIds)
                            {
                                _universalis.QueuePriceCheck(item.ItemId, worldId);
                            }
                        }
                    }

                    ImGuiUtil.HoverTooltip("Refresh Market Prices");
                    ImGui.SameLine();

                    if (_gameUiManager.IsWindowVisible(
                            CriticalCommonLib.Services.Ui.WindowName.SubmarinePartsMenu))
                    {
                        var subMarinePartsMenu = _gameUiManager.GetWindow("SubmarinePartsMenu");
                        if (subMarinePartsMenu != null)
                        {
                            if (ImGui.Button("Add Company Craft to List"))
                            {
                                var subAddon = (SubmarinePartsMenuAddon*)subMarinePartsMenu;
                                for (int i = 0; i < 6; i++)
                                {
                                    var itemRequired = subAddon->RequiredItemId(i);
                                    if (itemRequired != 0)
                                    {
                                        var amountHandedIn = subAddon->AmountHandedIn(i);
                                        var amountNeeded = subAddon->AmountNeeded(i);
                                        var amountLeft = Math.Max(
                                            (int)amountNeeded - (int)amountHandedIn,
                                            0);
                                        if (amountLeft > 0)
                                        {
                                            Service.Framework.RunOnFrameworkThread(() =>
                                            {
                                                filterConfiguration.CraftList.AddCraftItem(itemRequired,
                                                    (uint)amountLeft, InventoryItem.ItemFlags.None);
                                                filterConfiguration.NeedsRefresh = true;
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        ImGui.SameLine();
                    }

                    ImGuiService.VerticalCenter("Pending Market Requests: " + _universalis.QueuedCount);

                    if (_universalis.LastFailure != null)
                    {
                        ImGui.SameLine();
                        ImGui.Image(ImGuiService.GetIconTexture(Icons.ExclamationIcon).ImGuiHandle,
                            new Vector2(22, 22));
                        ImGuiUtil.HoverTooltip($"There was an error when contacting Universalis at {_universalis.LastFailure.Value.ToString(CultureInfo.CurrentCulture)}. This likely means Universalis is having issues. Allagan Tools will back off requests for 30 seconds whenever this happens.");
                    }

                    if (_universalis.TooManyRequests)
                    {
                        ImGui.SameLine();
                        ImGui.Image(ImGuiService.GetIconTexture(Icons.ExclamationIcon).ImGuiHandle,
                            new Vector2(22, 22));
                        ImGuiUtil.HoverTooltip($"It appears you are sending too many requests to Universalis, if you have multiple plugins requesting marketboard data, this is the most likely cause.");
                    }

                    craftTable?.DrawFooterItems();
                    itemTable.DrawFooterItems();
                    ImGui.SameLine();


                    var width = ImGui.GetWindowSize().X;

                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_menuIcon.Draw(ImGuiService.GetImageTexture("menu").ImGuiHandle, "openMenu"))
                    {
                    }
                    _settingsMenu.Draw();

                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    ImGui.SetCursorPosX(width);
                    if (_settingsIcon.Draw(ImGuiService.GetIconTexture(66319).ImGuiHandle, "bb_ocw"))
                    {
                        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(ConfigurationWindow)));
                    }

                    ImGuiUtil.HoverTooltip("Open the configuration window.");

                    ImGui.SetCursorPosY(0);
                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_filtersIcon.Draw(ImGuiService.GetImageTexture("filters").ImGuiHandle, "openFilters"))
                    {
                        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(FiltersWindow)));
                    }

                    ImGuiUtil.HoverTooltip("Open the items window.");

                    if (craftTable != null)
                    {
                        var totalItems =  itemTable.RenderSearchResults.Count + " items / " + craftTable.GetCraftListCount() + " craft items";
                        var calcTextSize = ImGui.CalcTextSize(totalItems);
                        width -= calcTextSize.X + 15;
                        ImGui.SetCursorPosX(width);
                        ImGuiService.VerticalCenter(totalItems);
                    }
                }
            }
        }

        private string? _newName;
        private void DrawSettingsPanel(FilterConfiguration filterConfiguration)
        {
            using (var contentChild = ImRaii.Child("Content", new Vector2(0, -44) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (contentChild.Success)
                {
                    var filterName = _newName ?? filterConfiguration.Name;
                    var labelName = "##" + filterConfiguration.Key;
                    if (ImGui.CollapsingHeader("General",
                            ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                    {
                        if (!filterConfiguration.CraftListDefault)
                        {
                            ImGui.SetNextItemWidth(100);
                            ImGui.LabelText(labelName + "FilterNameLabel", "Name: ");
                            ImGui.SameLine();
                            ImGui.InputText(labelName + "FilterName", ref filterName, 100);
                            if (filterName != _newName && filterName != filterConfiguration.Name)
                            {
                                _newName = filterName;
                            }

                            if (_newName != null)
                            {
                                ImGui.SameLine();
                                if (ImGui.Button("Save"))
                                {
                                    filterConfiguration.Name = _newName;
                                    Invalidate();
                                    _newName = null;
                                }
                            }

                            ImGui.NewLine();
                            if (ImGui.Button("Export Configuration to Clipboard"))
                            {
                                var base64 = _importExportService.ToBase64(filterConfiguration);
                                _clipboardService.CopyToClipboard(base64);
                                _chatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
                            }
                        }
                        else
                        {
                            ImGui.TextWrapped(
                                "This is the default configuration for new craft lists. Any new craft list will inherit this lists settings.");
                        }

                        var filterType = filterConfiguration.FormattedFilterType;
                        ImGui.SetNextItemWidth(100);
                        ImGui.LabelText(labelName + "FilterTypeLabel", "Filter Type: ");
                        ImGui.SameLine();
                        ImGui.TextDisabled(filterType);

                    }

                    using (var tabBar = ImRaii.TabBar("###FilterConfigTabs", ImGuiTabBarFlags.FittingPolicyScroll))
                    {
                        if (tabBar.Success)
                        {
                            foreach (var group in _filterService.GroupedFilters)
                            {
                                var hasValuesSet = false;
                                foreach (var filter in group.Value)
                                {
                                    if (filter.HasValueSet(filterConfiguration))
                                    {
                                        hasValuesSet = true;
                                        break;
                                    }
                                }

                                using var color = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen,
                                    hasValuesSet);

                                var hasValues = group.Value.Any(filter =>
                                    filter.AvailableIn.HasFlag(FilterType.SearchFilter) &&
                                    filterConfiguration.FilterType.HasFlag(
                                        FilterType.SearchFilter)
                                    ||
                                    (filter.AvailableIn.HasFlag(FilterType.SortingFilter) &&
                                     filterConfiguration.FilterType.HasFlag(FilterType
                                         .SortingFilter))
                                    ||
                                    (filter.AvailableIn.HasFlag(FilterType.CraftFilter) &&
                                     filterConfiguration.FilterType.HasFlag(FilterType
                                         .CraftFilter))
                                    ||
                                    (filter.AvailableIn.HasFlag(FilterType.HistoryFilter) &&
                                     filterConfiguration.FilterType.HasFlag(FilterType
                                         .HistoryFilter))
                                    ||
                                    (filter.AvailableIn.HasFlag(FilterType.CuratedList) &&
                                     filterConfiguration.FilterType.HasFlag(FilterType
                                         .CuratedList))
                                    ||
                                    (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                                     filterConfiguration.FilterType.HasFlag(FilterType
                                         .GameItemFilter)));
                                if (hasValues)
                                {
                                    using (var tabItem = ImRaii.TabItem(group.Key.ToString().ToSentence()))
                                    {
                                        if (!tabItem.Success) continue;
                                        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudWhite))
                                        {
                                            if (group.Key is FilterCategory.CraftColumns or FilterCategory.Columns)
                                            {
                                                using (var craftColumns = ImRaii.Child("craftColumns", new (0, -100 * ImGui.GetIO().FontGlobalScale)))
                                                {
                                                    if (craftColumns.Success)
                                                    {
                                                        group.Value.Single(c => c is CraftColumnsFilter or ColumnsFilter).Draw(filterConfiguration);
                                                    }
                                                }
                                                using (var otherFilters = ImRaii.Child("otherFilters", new (0, 0)))
                                                {
                                                    if (otherFilters.Success)
                                                    {
                                                        foreach (var filter in group.Value.Where(c => c is not CraftColumnsFilter && c is not ColumnsFilter))
                                                        {
                                                            if ((filter.AvailableIn.HasFlag(FilterType.SearchFilter) &&
                                                                 filterConfiguration.FilterType.HasFlag(FilterType
                                                                     .SearchFilter)
                                                                 ||
                                                                 (filter.AvailableIn.HasFlag(FilterType
                                                                      .SortingFilter) &&
                                                                  filterConfiguration.FilterType.HasFlag(FilterType
                                                                      .SortingFilter))
                                                                 ||
                                                                 (filter.AvailableIn.HasFlag(FilterType.CraftFilter) &&
                                                                  filterConfiguration.FilterType
                                                                      .HasFlag(FilterType.CraftFilter))
                                                                 ||
                                                                 (filter.AvailableIn.HasFlag(FilterType
                                                                      .HistoryFilter) &&
                                                                  filterConfiguration.FilterType.HasFlag(FilterType
                                                                      .HistoryFilter))
                                                                 ||
                                                                 (filter.AvailableIn.HasFlag(FilterType.CuratedList) &&
                                                                  filterConfiguration.FilterType.HasFlag(FilterType
                                                                      .CuratedList))
                                                                 ||
                                                                 (filter.AvailableIn.HasFlag(FilterType
                                                                      .GameItemFilter) &&
                                                                  filterConfiguration.FilterType.HasFlag(FilterType
                                                                      .GameItemFilter))
                                                                ))
                                                            {
                                                                filter.Draw(filterConfiguration);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                foreach (var filter in group.Value)
                                                {
                                                    if ((filter.AvailableIn.HasFlag(FilterType.SearchFilter) &&
                                                         filterConfiguration.FilterType.HasFlag(FilterType.SearchFilter)
                                                         ||
                                                         (filter.AvailableIn.HasFlag(FilterType.SortingFilter) &&
                                                          filterConfiguration.FilterType.HasFlag(FilterType
                                                              .SortingFilter))
                                                         ||
                                                         (filter.AvailableIn.HasFlag(FilterType.CraftFilter) &&
                                                          filterConfiguration.FilterType
                                                              .HasFlag(FilterType.CraftFilter))
                                                         ||
                                                         (filter.AvailableIn.HasFlag(FilterType.HistoryFilter) &&
                                                          filterConfiguration.FilterType.HasFlag(FilterType
                                                              .HistoryFilter))
                                                         ||
                                                         (filter.AvailableIn.HasFlag(FilterType.CuratedList) &&
                                                          filterConfiguration.FilterType.HasFlag(FilterType
                                                              .CuratedList))
                                                         ||
                                                         (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                                                          filterConfiguration.FilterType.HasFlag(FilterType
                                                              .GameItemFilter))
                                                        ))
                                                    {
                                                        filter.Draw(filterConfiguration);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            using (var bottomBarChild = ImRaii.Child("BottomBar", new Vector2(0, 0), true, ImGuiWindowFlags.NoScrollbar))
            {
                if (bottomBarChild.Success)
                {
                    if (filterConfiguration.CraftListDefault)
                    {
                        ImGuiService.VerticalCenter(
                            "You are currently editing default craft list configuration.");
                    }
                    else
                    {
                        ImGuiService.VerticalCenter(
                            "You are currently editing the craft list's configuration. Press the tick on the right hand side to save configuration.");
                    }
                    float width = ImGui.GetWindowSize().X;

                    if (!filterConfiguration.CraftListDefault)
                    {
                        ImGui.SameLine();
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_closeSettingsIcon.Draw(ImGuiService.GetIconTexture(66311).ImGuiHandle, "bb_settings"))
                        {
                            _settingsActive = false;
                        }
                        ImGuiUtil.HoverTooltip("Return to the craft list.");

                        ImGui.SameLine();
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_resetButton.Draw(ImGuiService.GetImageTexture("nuke").ImGuiHandle, "bb_reset"))
                        {
                            ImGui.OpenPopup("confirmReset");
                        }

                        var result = InventoryTools.Ui.Widgets.ImGuiUtil.ConfirmPopup("confirmReset", new Vector2(400, 100), () =>
                        {
                            ImGui.TextWrapped("Are you sure you want to reset your configuration to the default?");
                        });
                        if (result == true)
                        {
                            _listService.ResetFilter(_filterService.AvailableFilters, filterConfiguration);
                        }
                        ImGuiUtil.HoverTooltip("Reset craft list to default configuration (keeps items).");
                    }
                    else
                    {
                        ImGui.SameLine();
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_resetButton.Draw(ImGuiService.GetImageTexture("nuke").ImGuiHandle, "bb_reset"))
                        {
                            ImGui.OpenPopup("Reset the default craft list?##defaultReset");
                        }

                        ImGuiUtil.HoverTooltip("Reset to the default settings.");

                        using (var popup = ImRaii.Popup("Reset the default craft list?##defaultReset"))
                        {
                            if (popup.Success)
                            {
                                ImGui.TextUnformatted(
                                    "Are you sure you want to reset the default craft list?.\nThis operation cannot be undone!\n\n");
                                ImGui.Separator();

                                if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                                {
                                    _listService.ResetFilter(_filterService.AvailableFilters, DefaultConfiguration);
                                    ImGui.CloseCurrentPopup();
                                }

                                ImGui.SetItemDefaultFocus();
                                ImGui.SameLine();
                                if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                                {
                                    ImGui.CloseCurrentPopup();
                                }
                            }
                        }
                    }
                    ImGui.SameLine();
                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_clipboardIcon.Draw(ImGuiService.GetImageTexture("clipboard").ImGuiHandle, "copyFilterBtn"))
                    {
                        ImGui.OpenPopup("copyFilter");
                    }
                    ImGuiUtil.HoverTooltip("Copy existing filter's settings");

                    using (var popup = ImRaii.ContextPopup("copyFilter"))
                    {
                        if (popup.Success)
                        {
                            var filterConfigurations = Filters.Where(c => c != SelectedConfiguration).ToList();
                            foreach (var filter in filterConfigurations)
                            {
                                if (ImGui.Selectable("Copy configuration from '" + filter.Name + "'"))
                                {
                                    _listService.ResetFilter(_filterService.AvailableFilters, filterConfiguration, filter);
                                }
                            }

                            if (filterConfigurations.Count == 0)
                            {
                                ImGui.Text("No other configurations available to copy from.");
                            }
                        }
                    }
                }
            }
        }



        private void DrawSearchRow(FilterConfiguration filterConfiguration, ItemRow item)
        {
            ImGui.TableNextColumn();
            ImGui.TextWrapped( item.NameString);
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("RightClick" + item.RowId);
            }

            using (var popup = ImRaii.Popup("RightClick"+ item.RowId))
            {
                if (popup.Success)
                {
                    MediatorService.Publish(ImGuiService.ImGuiMenuService.DrawRightClickPopup(item));
                }
            }
            ImGui.TableNextColumn();
            using (ImRaii.PushId("s_" + item.RowId))
            {
                if (_addIcon.Draw(ImGuiService.GetIconTexture(66315).ImGuiHandle, "bbadd_" + item.RowId, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale))
                {
                    Service.Framework.RunOnFrameworkThread(() =>
                    {
                        filterConfiguration.CraftList.AddCraftItem(item.RowId, 1, InventoryItem.ItemFlags.None);
                        filterConfiguration.NeedsRefresh = true;
                    });
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
            }
        }

        private string _searchString = "";
        private List<ItemRow>? _searchItems;
        public List<ItemRow> SearchItems
        {
            get
            {
                if (SearchString == "")
                {
                    _searchItems = new List<ItemRow>();
                    return _searchItems;
                }
                if (_searchItems == null)
                {
                    _searchItems = _itemSheet.Where(c => c.NameString.ToLower().PassesFilter(SearchString.ToLower())).Take(100)
                        .Select(c => _itemSheet.GetRow(c.RowId)).ToList();
                }

                return _searchItems;
            }
        }

        public override FilterConfiguration? SelectedConfiguration
        {
            get
            {
                if (_selectedFilterTab >= 0 && _selectedFilterTab < Filters.Count) return Filters[_selectedFilterTab];
                return null;
            }
        }

        public string SearchString
        {
            get => _searchString;
            set
            {
                _searchString = value;
                _searchItems = null;
            }
        }

        private void SaveCallback(FilterTable filterTable, bool arg1, string arg2)
        {
            if (arg1)
            {
                filterTable.ExportToCsv(arg2);
            }
        }

        private void SaveCraftCallback(CraftItemTable craftItemTable, bool arg1, string arg2)
        {
            if (arg1)
            {
                craftItemTable.ExportToCsv(arg2);
            }
        }

        public override void Invalidate()
        {
            var selectedConfiguration = SelectedConfiguration;
            _filters = null;
            if (selectedConfiguration != null)
            {
                FocusFilter(selectedConfiguration);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _throttleDispatcher.Dispose();
        }

        public override void OnClose()
        {
            if (SelectedConfiguration != null)
            {
                SelectedConfiguration.Active = false;
            }
            foreach (var filter in Filters)
            {
                if (SelectedConfiguration == filter)
                {
                    filter.Active = false;
                }
            }
            base.OnClose();
        }
    }
}