using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Autofac;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.Sheets;
using DalaMock.Shared.Interfaces;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
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

namespace InventoryTools.Ui
{
    public class FiltersWindow : GenericWindow
    {
        private readonly IListService _listService;
        private readonly IFilterService _filterService;
        private readonly TableService _tableService;
        private readonly IChatUtilities _chatUtilities;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IUniversalis _universalis;
        private readonly IFileDialogManager _fileDialogManager;
        private readonly IGameUiManager _gameUiManager;
        private readonly InventoryHistory _inventoryHistory;
        private readonly ListImportExportService _importExportService;
        private readonly ExcelCache _excelCache;
        private readonly IComponentContext _context;
        private readonly FiltersWindowLayoutSetting _layoutSetting;
        private readonly IClipboardService _clipboardService;
        private readonly PopupService _popupService;
        private readonly IKeyState _keyState;
        private IEnumerable<IMenuWindow>? _menuWindows;
        private readonly InventoryToolsConfiguration _configuration;

        public FiltersWindow(ILogger<FiltersWindow> logger, MediatorService mediator, ImGuiService imGuiService,
            InventoryToolsConfiguration configuration, IListService listService, IFilterService filterService,
            TableService tableService, IChatUtilities chatUtilities, ICharacterMonitor characterMonitor,
            IUniversalis universalis, IFileDialogManager fileDialogManager, IGameUiManager gameUiManager,
            HostedInventoryHistory inventoryHistory, ListImportExportService importExportService,
            ExcelCache excelCache, IComponentContext context, FiltersWindowLayoutSetting layoutSetting,
            IClipboardService clipboardService, PopupService popupService, IKeyState keyState) : base(logger, mediator, imGuiService, configuration, "Filters Window")
        {
            _listService = listService;
            _filterService = filterService;
            _tableService = tableService;
            _chatUtilities = chatUtilities;
            _characterMonitor = characterMonitor;
            _universalis = universalis;
            _fileDialogManager = fileDialogManager;
            _gameUiManager = gameUiManager;
            _inventoryHistory = inventoryHistory;
            _importExportService = importExportService;
            _excelCache = excelCache;
            _context = context;
            _layoutSetting = layoutSetting;
            _clipboardService = clipboardService;
            _popupService = popupService;
            _keyState = keyState;
            _configuration = configuration;
            this.Flags = ImGuiWindowFlags.MenuBar;
        }

        public override void Initialize()
        {
            Key = "filters";
            WindowName = "Items";
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

            _tabLayout = Utils.GenerateRandomId();
            _addFilterMenu = new PopupMenu("addFilter", PopupMenu.PopupMenuButtons.LeftRight,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectableAskName("Search List", "adf1", "New Search List",
                        AddSearchFilter,
                        "This will create a new list that let's you search for specific items within your characters and retainers inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Sort List", "af2", "New Sort List", AddSortFilter,
                        "This will create a new list that let's you search for specific items within your characters and retainers inventories then determine where they should be moved to."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Game Item List", "af3", "New Game Item List",
                        AddGameItemFilter, "This will create a list that lets you search for all items in the game."),
                    new PopupMenu.PopupMenuItemSelectableAskName("History List", "af4", "New History List",
                        AddHistoryFilter,
                        "This will create a list that lets you view historical data of how your inventory has changed."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Curated List", "af5", "New Curated List",
                        AddCuratedFilter, "This will create a list that lets you add individual items to it manually."),
                });
            _menuWindows = _context.Resolve<IEnumerable<IMenuWindow>>().OrderBy(c => c.GenericName).Where(c => c.GetType() != this.GetType());
            MediatorService.Subscribe<ListInvalidatedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRepositionedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListAddedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRemovedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<TeamCraftDataImported>(this, ImportTeamcraftData);
            MediatorService.Subscribe<FocusListMessage>(this, FocusList);
        }

        private void FocusList(FocusListMessage obj)
        {
            if (obj.windowType == this.GetType())
            {
                this.FocusFilter(obj.FilterConfiguration);
            }
        }

        public override bool SaveState => true;

        private string _activeFilter = "";
        private string _tabLayout = "";
        private int _selectedFilterTab;
        private bool _settingsActive;
        public override Vector2? MaxSize { get; } = new(2000, 2000);
        public override Vector2? MinSize { get; } = new(200, 200);
        public override Vector2? DefaultSize { get; } = new(600, 600);
        public override string GenericKey => "filters";
        public override string GenericName => "Filters";
        public override bool DestroyOnClose => false;
        private HoverButton _editIcon = new();
        private HoverButton _settingsIcon = new();
        private HoverButton _craftIcon = new();
        private HoverButton _csvIcon = new();
        private HoverButton _clipboardIcon = new();
        private HoverButton _clearIcon = new();
        private HoverButton _closeSettingsIcon = new();
        private HoverButton _marketIcon = new();
        private HoverButton _addIcon = new();
        private HoverButton _menuIcon = new();
        private HoverButton _searchIcon = new();
        private bool _addItemBarOpen;

        public bool ShowAddItemBar =>
            SelectedConfiguration is { FilterType: FilterType.CuratedList } &&
            _addItemBarOpen;


        private List<FilterConfiguration>? _filters;
        private PopupMenu _addFilterMenu = null!;

        private PopupMenu _settingsMenu;

        private void PasteListContents(string obj)
        {
            if (SelectedConfiguration != null)
            {
                var importedList = _importExportService.FromTCString(_clipboardService.PasteFromClipboard());
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
        }


        private void CopyListContents(string obj)
        {
            if (SelectedConfiguration != null)
            {
                var tcString = _importExportService.ToTCString(SelectedConfiguration.CuratedItems?.ToList() ?? []);
                _clipboardService.CopyToClipboard(tcString);
                _chatUtilities.Print("The curated list's contents were copied to your clipboard.");
            }
        }

        private void ClearListContents(string arg1, bool arg2)
        {
            if (arg2)
            {
                if (this.SelectedConfiguration != null &&
                    this.SelectedConfiguration.FilterType == FilterType.CuratedList)
                {
                    this.SelectedConfiguration.CuratedItems = new List<CuratedItem>();
                    this.SelectedConfiguration.NeedsRefresh = true;
                }
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
                    SelectedConfiguration.AddCuratedItem(new CuratedItem(itemId, item.Item2,
                        isHq ? InventoryItem.ItemFlags.HighQuality : InventoryItem.ItemFlags.None));
                }

                SelectedConfiguration.NeedsRefresh = true;
            }
        }

        private void OpenHelpWindow(string obj)
        {
            MediatorService.Publish(new Mediator.OpenGenericWindowMessage(typeof(HelpWindow)));
        }

        private void OpenDutiesWindow(string obj)
        {
            MediatorService.Publish(new Mediator.OpenGenericWindowMessage(typeof(DutiesWindow)));
        }

        private void OpenAirshipsWindow(string obj)
        {
            MediatorService.Publish(new Mediator.OpenGenericWindowMessage(typeof(AirshipsWindow)));
        }

        private void OpenSubmarinesWindow(string obj)
        {
            MediatorService.Publish(new Mediator.OpenGenericWindowMessage(typeof(SubmarinesWindow)));
        }

        private void OpenRetainerVenturesWindow(string obj)
        {
            MediatorService.Publish(new Mediator.OpenGenericWindowMessage(typeof(RetainerTasksWindow)));
        }

        private void OpenTetrisWindow(string obj)
        {
            MediatorService.Publish(new Mediator.OpenGenericWindowMessage(typeof(TetrisWindow)));
        }

        private void OpenMobsWindow(string obj)
        {
            MediatorService.Publish(new Mediator.OpenGenericWindowMessage(typeof(BNpcsWindow)));
        }

        private void OpenNpcsWindow(string obj)
        {
            MediatorService.Publish(new Mediator.OpenGenericWindowMessage(typeof(ENpcsWindow)));
        }

        private Dictionary<FilterConfiguration, PopupMenu> _popupMenus = new();

        public PopupMenu GetFilterMenu(FilterConfiguration configuration, WindowLayout layout)
        {
            if (!_popupMenus.ContainsKey(configuration))
            {
                _popupMenus[configuration] = new PopupMenu("fm" + configuration.Key, PopupMenu.PopupMenuButtons.Right,
                    new List<PopupMenu.IPopupMenuItem>()
                    {
                        new PopupMenu.PopupMenuItemSelectable("Edit", "ef_" + configuration.Key, EditFilter,
                            "Edit the filter."),
                        new PopupMenu.PopupMenuItemSelectableAskName("Duplicate", "df_" + configuration.Key,
                            configuration.Name, DuplicateFilter, "Duplicate the filter."),
                        new PopupMenu.PopupMenuItemSelectable(layout == WindowLayout.Tabs ? "Move Left" : "Move Up",
                            "mu_" + configuration.Key, MoveFilterUp,
                            layout == WindowLayout.Tabs ? "Move the filter left." : "Move the filter up."),
                        new PopupMenu.PopupMenuItemSelectable(layout == WindowLayout.Tabs ? "Move Right" : "Move Down",
                            "md_" + configuration.Key, MoveFilterDown,
                            layout == WindowLayout.Tabs ? "Move the filter right." : "Move the filter down."),
                        new PopupMenu.PopupMenuItemSelectableConfirm("Remove", "rf_" + configuration.Key,
                            "Are you sure you want to remove this filter?", RemoveFilter, "Remove the filter."),
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
                FocusFilter(existingFilter);
                this._settingsActive = true;
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
                MediatorService.Publish(new ConfigurationWindowEditFilter(newFilter));
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
                FocusFilter(newFilter);
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

        private void AddSearchFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.SearchFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            Invalidate();
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
            MediatorService.Publish(new ConfigurationWindowEditFilter(filterConfiguration));
            FocusFilter(filterConfiguration);
        }

        private void AddHistoryFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.HistoryFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            Invalidate();
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
            MediatorService.Publish(new ConfigurationWindowEditFilter(filterConfiguration));
            FocusFilter(filterConfiguration);
        }

        private void AddCuratedFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.CuratedList);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            Invalidate();
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
            MediatorService.Publish(new ConfigurationWindowEditFilter(filterConfiguration));
            FocusFilter(filterConfiguration);
        }

        private void AddGameItemFilter(string newName, string id)
        {
            var filterConfiguration =
                new FilterConfiguration(newName, Guid.NewGuid().ToString("N"), FilterType.GameItemFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            Invalidate();
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
            MediatorService.Publish(new ConfigurationWindowEditFilter(filterConfiguration));
            FocusFilter(filterConfiguration);
        }

        private void AddSortFilter(string newName, string id)
        {
            var filterConfiguration =
                new FilterConfiguration(newName, Guid.NewGuid().ToString("N"), FilterType.SortingFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            Invalidate();
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
            MediatorService.Publish(new ConfigurationWindowEditFilter(filterConfiguration));
            FocusFilter(filterConfiguration);
        }

        private List<FilterConfiguration> Filters
        {
            get
            {
                if (_filters == null)
                {
                    _filters = _listService.Lists.Where(c => c.FilterType != FilterType.CraftFilter).ToList();
                }

                return _filters;
            }
        }

        private int? _newTab;
        private DateTime? _applyNewTabTime;

        private bool SwitchNewTab =>
            _newTab != null && _applyNewTabTime != null && _applyNewTabTime.Value <= DateTime.Now;

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

        public override unsafe void Draw()
        {
            foreach (var filter in Filters)
            {
                if (SelectedConfiguration == filter)
                {
                    if (filter.Active != true)
                    {
                        filter.NeedsRefresh = true;
                        filter.Active = true;
                    }
                }
                else
                {
                    filter.Active = false;
                }
            }

            DrawMenuBar();
            _popupService.Draw(GetType());
            if (_configuration.FiltersLayout == WindowLayout.Sidebar)
            {
                DrawSidebar();
                DrawMainWindow();
            }
            else if(_configuration.FiltersLayout == WindowLayout.Tabs)
            {
                DrawTabBar();
            }
            else
            {
                DrawMainWindow();
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

                if (ImGui.BeginMenu("Edit") && this.SelectedConfiguration != null)
                {
                    if (ImGui.MenuItem("Clear Search"))
                    {
                        _tableService.GetListTable(SelectedConfiguration).ClearFilters();
                    }

                    ImGui.Separator();

                    if (ImGui.BeginMenu("Copy List Contents"))
                    {
                        if (ImGui.MenuItem("Teamcraft Format"))
                        {
                            var searchResults = _tableService.GetListTable(SelectedConfiguration).SearchResults;
                            var tcString = _importExportService.ToTCString(searchResults);
                            _clipboardService.CopyToClipboard(tcString);
                            _chatUtilities.Print("The list's contents were copied to your clipboard.");
                        }
                        if (ImGui.MenuItem("JSON Format"))
                        {
                            var itemTable = _tableService.GetListTable(SelectedConfiguration);
                            _clipboardService.CopyToClipboard(itemTable.ExportToJson());
                        }
                        ImGui.EndMenu();
                    }

                    if (SelectedConfiguration.FilterType == FilterType.CuratedList && ImGui.MenuItem("Paste List Contents"))
                    {
                        var importedList = _importExportService.FromTCString(_clipboardService.PasteFromClipboard(), false);
                        if (importedList == null)
                        {
                            _chatUtilities.PrintError("The contents of your clipboard could not be parsed.");
                        }
                        else
                        {
                            _chatUtilities.Print("The contents of your clipboard were imported.");
                            SelectedConfiguration.AddItemsToList(importedList);
                        }
                    }
                    if (SelectedConfiguration.FilterType == FilterType.CuratedList && ImGui.MenuItem("Clear List"))
                    {
                        _popupService.AddPopup(new ConfirmPopup(GetType(), "craftListDelete",
                            "Are you sure you want to clear this curated list?",
                            result =>
                            {
                                if (result)
                                {
                                    SelectedConfiguration.ClearCuratedItems();
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
                            _popupService.AddPopup(new NamePopup(typeof(FiltersWindow), "newCraftList", "New Craft List",
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
                                        MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                                        this.MediatorService.Publish(new FocusListMessage(typeof(CraftsWindow), craftList));
                                    }
                                }));
                        }

                        if (ImGui.MenuItem("New Craft List (Ephemeral)"))
                        {
                            _popupService.AddPopup(new NamePopup(typeof(FiltersWindow), "newCraftList", "New Craft List",
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
                                        MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                                        this.MediatorService.Publish(new FocusListMessage(typeof(CraftsWindow), craftList));
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
                            _popupService.AddPopup(new NamePopup(typeof(FiltersWindow), "newCuratedList", "New Curated List",
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

                if (ImGui.MenuItem("Export"))
                {
                    if (SelectedConfiguration != null)
                    {
                        var itemTable = _tableService.GetListTable(SelectedConfiguration);
                        _fileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv",
                            (b, s) => { SaveCallback(itemTable, b, s); }, null, true);
                    }
                }

                if (ImGui.BeginMenu("Market"))
                {
                    if (ImGui.MenuItem("Refresh All Prices"))
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
                        if (ImGui.MenuItem("Search List"))
                        {
                            _popupService.AddPopup(new NamePopup(GetType(), "addSearchList", "", result =>
                            {
                                if (result.Item1)
                                {
                                    AddSearchFilter(result.Item2, "");
                                }
                            }));
                        }

                        if (ImGui.MenuItem("Sort List"))
                        {
                            _popupService.AddPopup(new NamePopup(GetType(), "addSortList", "", result =>
                            {
                                if (result.Item1)
                                {
                                    AddSortFilter(result.Item2, "");
                                }
                            }));
                        }

                        if (ImGui.MenuItem("Game Item List"))
                        {
                            _popupService.AddPopup(new NamePopup(GetType(), "addGameItemList", "", result =>
                            {
                                if (result.Item1)
                                {
                                    AddGameItemFilter(result.Item2, "");
                                }
                            }));
                        }

                        if (ImGui.MenuItem("Curated List"))
                        {
                            _popupService.AddPopup(new NamePopup(GetType(), "addCuratedList", "", result =>
                            {
                                if (result.Item1)
                                {
                                    AddCuratedFilter(result.Item2, "");
                                }
                            }));
                        }

                        if (ImGui.MenuItem("History List"))
                        {
                            _popupService.AddPopup(new NamePopup(GetType(), "addHistoryList", "", result =>
                            {
                                if (result.Item1)
                                {
                                    AddHistoryFilter(result.Item2, "");
                                }
                            }));
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.NewLine();

                    var windowGroups = _listService.Lists.GroupBy(c => c.FilterType).OrderBySequence([FilterType.SearchFilter, FilterType.SortingFilter, FilterType.GameItemFilter, FilterType.HistoryFilter, FilterType.CuratedList, FilterType.CraftFilter], grouping => grouping.Key).ToList();
                    for (var index = 0; index < windowGroups.Count; index++)
                    {
                        var windowGroup = windowGroups[index];
                        ImGui.Text(windowGroup.Key.FormattedName());
                        ImGui.Separator();
                        foreach (var window in windowGroup)
                        {
                            if (ImGui.MenuItem(window.Name, "", SelectedConfiguration == window))
                            {
                                if (window.FilterType == FilterType.CraftFilter)
                                {
                                    if (_keyState[VirtualKey.CONTROL])
                                    {
                                        this.MediatorService.Publish(new OpenStringWindowMessage(typeof(FilterWindow), window.Key));
                                    }
                                    else
                                    {
                                        MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                                        MediatorService.Publish(new FocusListMessage(typeof(CraftsWindow), window));
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
                                        FocusFilter(window);
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

                ImGui.EndMenuBar();
            }
        }

        private void DrawMainWindow()
        {
            using (var mainChild = ImRaii.Child("Main",new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, false,ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (mainChild.Success)
                {
                    var isWindowFocused = ImGui.IsWindowFocused();
                    var filterConfigurations = Filters;

                    if (filterConfigurations.Count == 0)
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
                        if (_selectedFilterTab == index)
                        {
                            var filterConfiguration = filterConfigurations[index];
                            if (isWindowFocused)
                            {
                                if (_configuration.SwitchFiltersAutomatically &&
                                    _configuration.ActiveUiFilter != filterConfiguration.Key &&
                                    _configuration.ActiveUiFilter != null)
                                {
                                    Service.Framework.RunOnFrameworkThread(() =>
                                    {
                                        _listService.ToggleActiveUiList(filterConfiguration);
                                    });
                                }
                            }

                            var itemTable = _tableService.GetListTable(filterConfiguration);

                            if (_settingsActive)
                            {
                                DrawSettingsPanel(filterConfiguration);
                            }
                            else
                            {
                                var activeFilter = DrawFilter(itemTable, filterConfiguration);
                                if (_activeFilter != activeFilter && ImGui.IsWindowFocused())
                                {
                                    if (_configuration.SwitchFiltersAutomatically &&
                                        _configuration.ActiveUiFilter != filterConfiguration.Key &&
                                        _configuration.ActiveUiFilter != null)
                                    {
                                        Service.Framework.RunOnFrameworkThread(() =>
                                        {
                                            _listService.ToggleActiveUiList(filterConfiguration);
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawAddItemBar()
        {
            if (ShowAddItemBar)
            {
                ImGui.SameLine();
                using (var addItemChild = ImRaii.Child("AddItem", new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, true))
                {
                    if (addItemChild.Success)
                    {
                        var filterConfiguration = SelectedConfiguration;
                        if (filterConfiguration is { FilterType: FilterType.CuratedList })
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
                                if (ImGui.Selectable(filterConfiguration.Name + "###fl" + filterConfiguration.Key,
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
                                }

                                GetFilterMenu(filterConfiguration, WindowLayout.Sidebar).Draw();
                            }
                        }
                    }

                    using (var commandBarChild = ImRaii.Child("CommandBar", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, false))
                    {
                        if (commandBarChild.Success)
                        {
                            float height = ImGui.GetWindowSize().Y;
                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            if (_addIcon.Draw(ImGuiService.GetIconTexture(66315).ImGuiHandle, "cb_af"))
                            {
                            }

                            _addFilterMenu.Draw();

                            ImGuiUtil.HoverTooltip("Add a new list.");
                        }
                    }
                }
            }

            ImGui.SameLine();
        }

        private unsafe void DrawTabBar()
        {
            using (var tabBar = ImRaii.TabBar("InventoryTabs" + _tabLayout, ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton))
            {
                if (!tabBar.Success) return;
                var filterConfigurations = Filters;
                for (var index = 0; index < filterConfigurations.Count; index++)
                {
                    var filterConfiguration = filterConfigurations[index];
                    var itemTable = _tableService.GetListTable(filterConfiguration);

                    if (filterConfiguration.DisplayInTabs)
                    {
                        var imGuiTabItemFlags = _newTab == index && SwitchNewTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
                        using var id = ImRaii.PushId(index);
                        fixed (byte* namePtr = filterConfiguration.NameAsBytes)
                        {
                            using (var tabItem = ImRaii.TabItem(namePtr, imGuiTabItemFlags))
                            {
                                GetFilterMenu(filterConfiguration, WindowLayout.Tabs).Draw();

                                if (SwitchNewTab && _newTab != null && _newTab == index)
                                {
                                    _newTab = null;
                                    _applyNewTabTime = null;
                                }
                                if (!tabItem.Success) continue;

                                _selectedFilterTab = index;
                                if (_settingsActive)
                                {
                                    DrawSettingsPanel(filterConfiguration);
                                }
                                else
                                {
                                    var activeFilter = DrawFilter(itemTable, filterConfiguration);
                                    if (_activeFilter != activeFilter && ImGui.IsWindowFocused())
                                    {
                                        if (_configuration.SwitchFiltersAutomatically &&
                                            _configuration.ActiveUiFilter != filterConfiguration.Key &&
                                            _configuration.ActiveUiFilter != null)
                                        {
                                            Service.Framework.RunOnFrameworkThread(() =>
                                            {
                                                _listService.ToggleActiveUiList(filterConfiguration);
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (_configuration.ShowFilterTab && ImGui.BeginTabItem("All Lists"))
                {
                    using (var child = ImRaii.Child("filterLeft", new Vector2(100, -1) * ImGui.GetIO().FontGlobalScale,
                               true))
                    {
                        if (child.Success)
                        {
                            for (var index = 0; index < filterConfigurations.Count; index++)
                            {
                                var filterConfiguration = filterConfigurations[index];
                                if (ImGui.Selectable(filterConfiguration.Name + "###fl" + filterConfiguration.Key,
                                        index == _selectedFilterTab))
                                {
                                    if (_configuration.SwitchFiltersAutomatically &&
                                        _configuration.ActiveUiFilter != filterConfiguration.Key)
                                    {
                                        Service.Framework.RunOnFrameworkThread(() =>
                                        {
                                            _listService.ToggleActiveUiList(filterConfiguration);
                                        });
                                    }

                                    _selectedFilterTab = index;
                                }
                            }
                        }
                    }
                    ImGui.SameLine();
                    using (var child = ImRaii.Child("filterRight", new Vector2(-1, -1), true,
                               ImGuiWindowFlags.HorizontalScrollbar))
                    {
                        if (child.Success)
                        {
                            for (var index = 0; index < filterConfigurations.Count; index++)
                            {
                                if (_selectedFilterTab == index)
                                {
                                    var filterConfiguration = filterConfigurations[index];
                                    var table = _tableService.GetListTable(filterConfiguration);
                                    var activeFilter = DrawFilter(table, filterConfiguration);
                                    if (_activeFilter != activeFilter)
                                    {
                                        if (_configuration.SwitchFiltersAutomatically &&
                                            _configuration.ActiveUiFilter != filterConfiguration.Key &&
                                            _configuration.ActiveUiFilter != null)
                                        {
                                            Service.Framework.RunOnFrameworkThread(() =>
                                            {
                                                _listService.ToggleActiveUiList(
                                                    filterConfiguration);
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
                {
                }

                ImGuiUtil.HoverTooltip("Add a new list");

                _addFilterMenu.Draw();
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

                        var filterType = filterConfiguration.FormattedFilterType;
                        ImGui.SetNextItemWidth(100);
                        ImGui.LabelText(labelName + "FilterTypeLabel", "Filter Type: ");
                        ImGui.SameLine();
                        ImGui.TextDisabled(filterType);

                    }

                    using (var tabBar = ImRaii.TabBar("ConfigTabs", ImGuiTabBarFlags.FittingPolicyScroll))
                    {
                        if (tabBar.Success)
                        {
                            foreach (var group in _filterService.GroupedFilters)
                            {
                                var hasValuesSet = false;
                                foreach (var filter in group.Value)
                                {
                                    if (filter.HasValueSet(filterConfiguration) && filter.AvailableIn.HasFlag(filterConfiguration.FilterType))
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
                                                using (var craftColumns = ImRaii.Child("craftColumns", new (0, -100)))
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
                                                            filter.Draw(filterConfiguration);
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
                    ImGuiService.VerticalCenter(
                        "You are currently editing the filter's configuration. Press the tick on the right hand side to save configuration.");

                    ImGui.SameLine();
                    float width = ImGui.GetWindowSize().X;
                    ImGui.SetCursorPosX(width - 42 * ImGui.GetIO().FontGlobalScale);
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_closeSettingsIcon.Draw(ImGuiService.GetIconTexture(66311).ImGuiHandle, "bb_settings"))
                    {
                        _settingsActive = false;
                    }

                    ImGuiUtil.HoverTooltip("Return to the filter.");
                }
            }
        }

        public unsafe string DrawFilter(FilterTable itemTable, FilterConfiguration filterConfiguration)
        {
            using var mainChild = ImRaii.Child("Main", new Vector2(filterConfiguration.FilterType == FilterType.CuratedList && _addItemBarOpen ? -250 : -1, -1) * ImGui.GetIO().FontGlobalScale, false,
                ImGuiWindowFlags.HorizontalScrollbar);
            if (!mainChild) return filterConfiguration.Key;

            using (var topBarChild = ImRaii.Child("TopBar", new Vector2(0, 40) * ImGui.GetIO().FontGlobalScale, true,
                       ImGuiWindowFlags.NoScrollbar))
            {
                if (topBarChild.Success)
                {
                    var highlightItems = itemTable.HighlightItems;
                    ImGuiService.CenterElement(20 * ImGui.GetIO().FontGlobalScale);
                    ImGui.Checkbox("Highlight?" + "###" + itemTable.Key + "VisibilityCheckbox",
                        ref highlightItems);
                    if (highlightItems != itemTable.HighlightItems)
                    {
                        Service.Framework.RunOnFrameworkThread(() =>
                        {
                            _listService.ToggleActiveUiList(itemTable.FilterConfiguration);
                        });
                    }

                    if ((filterConfiguration.HighlightWhen ?? _configuration.HighlightWhen) ==
                        "When Searching")
                    {
                        ImGuiUtil.HoverTooltip(
                            "When checked, any items matching the filter will be highlighted once you search in any of the columns.");
                    }
                    else
                    {
                        ImGuiUtil.HoverTooltip(
                            "When checked, any items matching the filter will be highlighted.");
                    }


                    ImGui.SameLine();
                    ImGuiService.CenterElement(20 * ImGui.GetIO().FontGlobalScale);
                    if(_clearIcon.Draw(ImGuiService.GetIconTexture(66308).ImGuiHandle, "clearSearch"))
                    {
                        itemTable.ClearFilters();
                    }

                    ImGuiUtil.HoverTooltip("Clear the current search.");

                    ImGui.SameLine();
                    float width = ImGui.GetWindowSize().X;

                    ImGui.SameLine();
                    width -= 28 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    if (_editIcon.Draw(ImGuiService.GetImageTexture("edit").ImGuiHandle, "tb_edit"))
                    {
                        _settingsActive = !_settingsActive;
                    }

                    ImGuiUtil.HoverTooltip("Edit the filter's configuration.");

                    if (SelectedConfiguration is { FilterType: FilterType.CuratedList })
                    {
                        ImGui.SameLine();
                        width -= 28 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        if (_searchIcon.Draw(ImGuiService.GetIconTexture(66320).ImGuiHandle, "tb_oib"))
                        {
                            _addItemBarOpen = !_addItemBarOpen;
                        }
                    }
                }
            }
            using (var contentChild = ImRaii.Child("Content", new Vector2(0, -40) * ImGui.GetIO().FontGlobalScale, true,
                       ImGuiWindowFlags.NoScrollbar))
            {
                if (contentChild.Success)
                {
                    if (filterConfiguration.FilterType == FilterType.CraftFilter)
                    {
                        var craftTable = _tableService.GetCraftTable(filterConfiguration);
                        MediatorService.Publish(craftTable.Draw(new Vector2(0, -400)));
                        MediatorService.Publish(itemTable.Draw(new Vector2(0, 0)));
                    }
                    else
                    {
                        MediatorService.Publish(itemTable.Draw(new Vector2(0, 0)));
                    }

                }
            }

            //Need to have these buttons be determined dynamically or moved elsewhere
            using (var bottomBarChild =
                   ImRaii.Child("BottomBar", new Vector2(0, 0), true, ImGuiWindowFlags.NoScrollbar))
            {
                if (bottomBarChild.Success)
                {
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if(_marketIcon.Draw(ImGuiService.GetImageTexture("refresh-web").ImGuiHandle, "refreshMarket"))
                    {
                        var activeCharacter = _characterMonitor.ActiveCharacter;
                        if (activeCharacter != null)
                        {
                            foreach (var item in itemTable.RenderSearchResults)
                            {
                                _universalis.QueuePriceCheck(item.Item.RowId, activeCharacter.WorldId);
                            }
                        }
                    }

                    ImGuiUtil.HoverTooltip("Refresh Market Prices");
                    ImGui.SameLine();

                    if (filterConfiguration.FilterType == FilterType.CraftFilter &&
                        _gameUiManager.IsWindowVisible(
                            CriticalCommonLib.Services.Ui.WindowName.SubmarinePartsMenu))
                    {
                        var subMarinePartsMenu = _gameUiManager.GetWindow("SubmarinePartsMenu");
                        if (subMarinePartsMenu != null)
                        {
                            ImGui.SameLine();
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
                                        var amountLeft = Math.Max((int)amountNeeded - (int)amountHandedIn,
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
                    }

                    ImGui.SameLine();
                    ImGuiService.VerticalCenter("Pending Market Requests: " + _universalis.QueuedCount);
                    if (filterConfiguration.FilterType == FilterType.CraftFilter)
                    {
                        ImGui.SameLine();
                        ImGui.TextUnformatted("Total Cost NQ: " + filterConfiguration.CraftList.MinimumNQCost);
                        ImGui.SameLine();
                        ImGui.TextUnformatted("Total Cost HQ: " + filterConfiguration.CraftList.MinimumHQCost);
                    }

                    if (filterConfiguration.FilterType == FilterType.CraftFilter)
                    {
                        var craftTable = _tableService.GetCraftTable(filterConfiguration);
                        craftTable.DrawFooterItems();
                        itemTable.DrawFooterItems();
                    }
                    else
                    {
                        itemTable.DrawFooterItems();
                    }

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
                    if (_settingsIcon.Draw(ImGuiService.GetIconTexture(66319).ImGuiHandle, "openConfig"))
                    {
                        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(ConfigurationWindow)));
                    }

                    ImGuiUtil.HoverTooltip("Open the configuration window.");

                    ImGui.SetCursorPosY(0);
                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_craftIcon.Draw(ImGuiService.GetImageTexture("craft").ImGuiHandle, "openCraft"))
                    {
                        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(CraftsWindow)));
                    }

                    ImGuiUtil.HoverTooltip("Open the craft window.");

                    if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.HistoryFilter)
                    {
                        ImGui.SetCursorPosY(0);
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_clearIcon.Draw(ImGuiService.GetIconTexture(66308).ImGuiHandle, "clearHistory"))
                        {
                            ImGui.OpenPopup("confirmHistoryDelete");
                        }

                        var result = InventoryTools.Ui.Widgets.ImGuiUtil.ConfirmPopup("confirmHistoryDelete", new Vector2(300, 100),
                            () =>
                            {
                                ImGui.TextWrapped("Are you sure you want to clear all your stored history?");
                            });
                        if (result == true)
                        {
                            _inventoryHistory.ClearHistory();
                        }

                        ImGuiUtil.HoverTooltip("Clear your history.");
                    }

                    var totalItems =  itemTable.RenderSearchResults.Count + " items";

                    if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.GameItemFilter)
                    {
                        totalItems =  itemTable.RenderSearchResults.Count + " items";
                    }

                    if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.HistoryFilter)
                    {
                        if (_configuration.HistoryEnabled)
                        {
                            totalItems = itemTable.RenderSearchResults.Count + " historical records";
                        }
                        else
                        {
                            totalItems = "History tracking is currently disabled";
                        }
                    }

                    if (this.Configuration.FiltersLayout == WindowLayout.Single)
                    {
                        var currentList = this.SelectedConfiguration?.Name ?? "No List";
                        currentList += " | ";
                        totalItems = currentList + totalItems;
                    }

                    var calcTextSize = ImGui.CalcTextSize(totalItems);
                    width -= calcTextSize.X + 15;
                    ImGui.SetCursorPosX(width);
                    ImGuiService.VerticalCenter(totalItems);


                }
            }

            mainChild.Dispose();
            if (filterConfiguration.FilterType == FilterType.CuratedList && _addItemBarOpen)
            {
                DrawAddItemBar();
            }

            return filterConfiguration.Key;
        }

        private void DrawSearchRow(FilterConfiguration filterConfiguration, ItemEx item)
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
                    MediatorService.Publish(ImGuiService.RightClickService.DrawRightClickPopup(item));
                }
            }
            ImGui.TableNextColumn();
            using (ImRaii.PushId("s_" + item.RowId))
            {
                if (_addIcon.Draw(ImGuiService.GetIconTexture(66315).ImGuiHandle, "bbadd_" + item.RowId, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale))
                {
                    Service.Framework.RunOnFrameworkThread(() =>
                    {
                        filterConfiguration.AddCuratedItem(new CuratedItem(item.RowId));
                        filterConfiguration.NeedsRefresh = true;
                    });
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
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

        private void SaveCallback(FilterTable filterTable, bool arg1, string arg2)
        {
            if (arg1)
            {
                filterTable.ExportToCsv(arg2);
            }
        }

        public override void Invalidate()
        {
            var selectedConfiguration = SelectedConfiguration;
            _filters = null;
            _tabLayout = Utils.GenerateRandomId();
            if (selectedConfiguration != null)
            {
                FocusFilter(selectedConfiguration);
            }
        }

        private string _searchString = "";
        private List<ItemEx>? _searchItems;
        public List<ItemEx> SearchItems
        {
            get
            {
                if (SearchString == "")
                {
                    _searchItems = new List<ItemEx>();
                    return _searchItems;
                }
                if (_searchItems == null)
                {
                    _searchItems = _excelCache.ItemNamesById.Where(c => c.Value.ToLower().PassesFilter(SearchString.ToLower())).Take(100)
                        .Select(c => _excelCache.GetItemExSheet().GetRow(c.Key)!).ToList();
                }

                return _searchItems;
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
    }
}