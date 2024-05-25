using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Widgets;
using Dalamud.Interface.Utility.Raii;
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
        private readonly IIconService _iconService;
        private readonly IListService _listService;
        private readonly IFilterService _filterService;
        private readonly TableService _tableService;
        private readonly IChatUtilities _chatUtilities;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IUniversalis _universalis;
        private readonly FileDialogManager _fileDialogManager;
        private readonly IGameUiManager _gameUiManager;
        private readonly InventoryHistory _inventoryHistory;
        private readonly ListImportExportService _importExportService;
        private readonly InventoryToolsConfiguration _configuration;

        public FiltersWindow(ILogger<FiltersWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, IIconService iconService, IListService listService, IFilterService filterService, TableService tableService, IChatUtilities chatUtilities, ICharacterMonitor characterMonitor, IUniversalis universalis, FileDialogManager fileDialogManager, IGameUiManager gameUiManager, HostedInventoryHistory inventoryHistory, ListImportExportService importExportService) : base(logger, mediator, imGuiService, configuration, "Filters Window")
        {
            _iconService = iconService;
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
            _configuration = configuration;
            _editIcon = new(_iconService.LoadImage("edit"),  new Vector2(22, 22));
            _settingsIcon = new(_iconService.LoadIcon(66319),  new Vector2(22, 22));
            _craftIcon = new(_iconService.LoadImage("craft"),  new Vector2(22, 22));
            _csvIcon = new(_iconService.LoadImage("export2"),  new Vector2(22,22));
            _clipboardIcon = new(_iconService.LoadImage("clipboard"),  new Vector2(22,22));
            _clearIcon = new(_iconService.LoadIcon(66308),  new Vector2(22, 22));
            _closeSettingsIcon = new(_iconService.LoadIcon(66311),  new Vector2(22, 22));
            _marketIcon = new(_iconService.LoadImage("refresh-web"),  new Vector2(22, 22));
            _addIcon = new(_iconService.LoadIcon(66315),  new Vector2(22, 22));
            _menuIcon = new(_iconService.LoadImage("menu"),  new Vector2(22, 22));
        }
        public override void Initialize()
        {
            Key = "filters";
            WindowName = "Items";
            _settingsMenu  = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", OpenMobsWindow,"Open the mobs window."),
                    new PopupMenu.PopupMenuItemSelectable("Npcs Window", "npcs", OpenNpcsWindow,"Open the npcs window."),
                    new PopupMenu.PopupMenuItemSelectable("Duties Window", "duties", OpenDutiesWindow,"Open the duties window."),
                    new PopupMenu.PopupMenuItemSelectable("Airships Window", "airships", OpenAirshipsWindow,"Open the airships window."),
                    new PopupMenu.PopupMenuItemSelectable("Submarines Window", "submarines", OpenSubmarinesWindow,"Open the submarines window."),
                    new PopupMenu.PopupMenuItemSelectable("Retainer Ventures Window", "ventures", OpenRetainerVenturesWindow,"Open the retainer ventures window."),
                    new PopupMenu.PopupMenuItemSelectable("Tetris", "tetris", OpenTetrisWindow,"Open the tetris window.", () => _configuration.TetrisEnabled),
                    new PopupMenu.PopupMenuItemSeparator(),
                    new PopupMenu.PopupMenuItemSelectable("Help", "help", OpenHelpWindow,"Open the help window."),
                });
            
            _tabLayout = Utils.GenerateRandomId();
            _addFilterMenu = new PopupMenu("addFilter", PopupMenu.PopupMenuButtons.LeftRight,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectableAskName("Search Filter", "adf1", "New Search Filter", AddSearchFilter, "This will create a new filter that let's you search for specific items within your characters and retainers inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Sort Filter", "af2", "New Sort Filter", AddSortFilter, "This will create a new filter that let's you search for specific items within your characters and retainers inventories then determine where they should be moved to."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Game Item Filter", "af3", "New Game Item Filter", AddGameItemFilter, "This will create a filter that lets you search for all items in the game."),
                    new PopupMenu.PopupMenuItemSelectableAskName("History Filter", "af4", "New History Item Filter", AddHistoryFilter, "This will create a filter that lets you view historical data of how your inventory has changed."),
                });
            MediatorService.Subscribe<ListInvalidatedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRepositionedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListAddedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRemovedMessage>(this, _ => Invalidate());
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
        private HoverButton _editIcon { get; }
        private HoverButton _settingsIcon { get; }
        private HoverButton _craftIcon { get; }
        private HoverButton _csvIcon { get; }
        private HoverButton _clipboardIcon { get; }
        private HoverButton _clearIcon { get; }
        private HoverButton _closeSettingsIcon { get; }
        private HoverButton _marketIcon { get; }
        private HoverButton _addIcon { get; }
        private HoverButton _menuIcon { get; }

        
        private List<FilterConfiguration>? _filters;
        private PopupMenu _addFilterMenu = null!;

        private PopupMenu _settingsMenu;

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
                        new PopupMenu.PopupMenuItemSelectable("Edit", "ef_" + configuration.Key, EditFilter, "Edit the filter."),
                        new PopupMenu.PopupMenuItemSelectableAskName("Duplicate", "df_" + configuration.Key, configuration.Name, DuplicateFilter, "Duplicate the filter."),
                        new PopupMenu.PopupMenuItemSelectable(layout == WindowLayout.Tabs ? "Move Left" : "Move Up", "mu_" + configuration.Key, MoveFilterUp, layout == WindowLayout.Tabs ? "Move the filter left." : "Move the filter up."),
                        new PopupMenu.PopupMenuItemSelectable(layout == WindowLayout.Tabs ? "Move Right" : "Move Down", "md_" + configuration.Key, MoveFilterDown, layout == WindowLayout.Tabs ? "Move the filter right." : "Move the filter down."),
                        new PopupMenu.PopupMenuItemSelectableConfirm("Remove", "rf_" + configuration.Key, "Are you sure you want to remove this filter?", RemoveFilter, "Remove the filter."),
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
            MediatorService.Publish(new ConfigurationWindowEditFilter(filterConfiguration));
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
        }
        
        private void AddHistoryFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.HistoryFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            Invalidate();
            MediatorService.Publish(new ConfigurationWindowEditFilter(filterConfiguration));
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));

        }

        private void AddGameItemFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.GameItemFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            Invalidate();
            MediatorService.Publish(new ConfigurationWindowEditFilter(filterConfiguration));
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
        }

        private void AddSortFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.SortingFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            Invalidate();
            MediatorService.Publish(new ConfigurationWindowEditFilter(filterConfiguration));
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));

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

        private bool SwitchNewTab => _newTab != null && _applyNewTabTime != null && _applyNewTabTime.Value <= DateTime.Now;

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
            if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.HistoryFilter && !_configuration.HasSeenNotification(NotificationPopup.HistoryNotice) && ImGui.IsWindowFocused())
            {
                ImGui.OpenPopup("historynotice");
                _configuration.MarkNotificationSeen(NotificationPopup.HistoryNotice);
            }
            var choice = InventoryTools.Ui.Widgets.ImGuiUtil.ConfirmPopup("historynotice", new Vector2(800,340) * ImGui.GetIO().FontGlobalScale, () =>
            {
                ImGui.TextUnformatted("History Filter Notice");
                ImGui.Separator();
                ImGui.NewLine();

                ImGui.PushTextWrapPos();
                ImGui.Bullet();
                ImGui.Text("This is a new module that helps you track changes to your inventory.");
                ImGui.PopTextWrapPos();

                ImGui.BulletText("By default it will track the following events:");

                ImGui.Indent();
                ImGui.BulletText("Items added");
                ImGui.BulletText("Items removed");
                ImGui.BulletText("Items moved");
                ImGui.BulletText("Items quantities changing");
                ImGui.BulletText("Retainer sale item price changes");
                ImGui.Unindent();

                ImGui.BulletText("It is not limited to tracking just these events and can track most changes to individual items.");

                ImGui.BulletText("To change what is tracking, check out the History tab inside the main configuration section(gear icon).");
                ImGui.BulletText("Please note that this module is experimental so it may sometimes track single events as 2 events.");
                ImGui.NewLine();
                ImGui.Text("By default the history module is turned off, would you like to turn it on?");
            });
            if (choice != null)
            {
                _configuration.HistoryEnabled = choice.Value;
            }
            
            if (_configuration.FiltersLayout == WindowLayout.Sidebar)
            {
                DrawSidebar();
                DrawMainWindow();
            }
            else
            {
                DrawTabBar();
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
                            if (_addIcon.Draw("cb_af"))
                            {
                            }

                            _addFilterMenu.Draw();

                            ImGuiUtil.HoverTooltip("Add a new filter.");
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
                
                ImGuiUtil.HoverTooltip("Add a new filter");
                
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
                            ImGui.SetClipboardText(base64);
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
                                            if (group.Key == FilterCategory.CraftColumns)
                                            {
                                                using (var craftColumns = ImRaii.Child("craftColumns", new (0, -200)))
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
                                                        foreach (var filter in group.Value.Where(c => c is not CraftColumnsFilter and ColumnsFilter))
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
                    if (_closeSettingsIcon.Draw("bb_settings"))
                    {
                        _settingsActive = false;
                    }

                    ImGuiUtil.HoverTooltip("Return to the filter.");
                }
            }
        }

        public unsafe string DrawFilter(FilterTable itemTable, FilterConfiguration filterConfiguration)
        {
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
                    if(_clearIcon.Draw("clearSearch"))
                    {
                        itemTable.ClearFilters();
                    }

                    ImGuiUtil.HoverTooltip("Clear the current search.");
                    
                    ImGui.SameLine();
                    float width = ImGui.GetWindowSize().X;
                    
                    ImGui.SameLine();
                    width -= 28 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    if (_editIcon.Draw("tb_edit"))
                    {
                        _settingsActive = !_settingsActive;
                    }

                    ImGuiUtil.HoverTooltip("Edit the filter's configuration.");
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
                    if(_marketIcon.Draw("refreshMarket"))
                    {
                        var activeCharacter = _characterMonitor.ActiveCharacter;
                        if (activeCharacter != null)
                        {
                            foreach (var item in itemTable.RenderSortedItems)
                            {
                                _universalis.QueuePriceCheck(item.InventoryItem.ItemId, activeCharacter.WorldId);
                            }

                            foreach (var item in itemTable.RenderItems)
                            {
                                _universalis.QueuePriceCheck(item.RowId, activeCharacter.WorldId);
                            }

                            foreach (var item in itemTable.InventoryChanges)
                            {
                                _universalis.QueuePriceCheck(item.InventoryItem.ItemId, activeCharacter.WorldId);
                            }
                        }
                    }

                    ImGuiUtil.HoverTooltip("Refresh Market Prices");
                    ImGui.SameLine();
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_csvIcon.Draw("exportCsv"))
                    {
                        _fileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv",
                            (b, s) => { SaveCallback(itemTable, b, s); }, null, true);
                    }

                    ImGuiUtil.HoverTooltip("Export to CSV");
                    ImGui.SameLine();
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_clipboardIcon.Draw("copyJson"))
                    {
                        itemTable.ExportToJson().ToClipboard();
                    }

                    ImGuiUtil.HoverTooltip("Copy JSON to clipboard");
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
                    if (_menuIcon.Draw("openMenu"))
                    {
                    }
                    _settingsMenu.Draw();
                    
                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    ImGui.SetCursorPosX(width);
                    if (_settingsIcon.Draw("openConfig"))
                    {
                        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(ConfigurationWindow)));
                    }

                    ImGuiUtil.HoverTooltip("Open the configuration window.");

                    ImGui.SetCursorPosY(0);
                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_craftIcon.Draw("openCraft"))
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
                        if (_clearIcon.Draw("clearHistory"))
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

                    var totalItems =  itemTable.RenderSortedItems.Count + " items";

                    if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.GameItemFilter)
                    {
                        totalItems =  itemTable.RenderItems.Count + " items";
                    }
                    
                    if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.HistoryFilter)
                    {
                        if (_configuration.HistoryEnabled)
                        {
                            totalItems = itemTable.InventoryChanges.Count + " historical records";
                        }
                        else
                        {
                            totalItems = "History tracking is currently disabled";
                        }
                    }

                    var calcTextSize = ImGui.CalcTextSize(totalItems);
                    width -= calcTextSize.X + 15;
                    ImGui.SetCursorPosX(width);
                    ImGuiService.VerticalCenter(totalItems);
                }
            }

            return filterConfiguration.Key;
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
    }
}