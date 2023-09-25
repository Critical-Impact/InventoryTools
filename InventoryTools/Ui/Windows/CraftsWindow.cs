using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Widgets;
using Dalamud.Interface.Utility.Raii;
using ImGuiUtil = OtterGui.ImGuiUtil;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;
using PopupMenu = InventoryTools.Ui.Widgets.PopupMenu;

namespace InventoryTools.Ui
{
    public class CraftsWindow : Window
    {
        public override bool SaveState => true;

        public static string AsKey => "crafts";
        public override string Key => AsKey;
        
        public override Vector2? DefaultSize { get; } = new(600, 600);
        public override Vector2? MaxSize => new Vector2(5000, 5000);
        public override Vector2? MinSize => new Vector2(300, 300);
        public override bool DestroyOnClose => false;
        private int _selectedFilterTab = 0;
        private bool _settingsActive = false;
        private bool _addItemBarOpen = false;
        private bool _craftsExpanded = true;
        private bool _itemsExpanded = true;

        private HoverButton _editIcon { get; } = new(PluginService.IconStorage.LoadImage("edit"),  new Vector2(22, 22));
        private HoverButton _toggleIcon { get; } = new(PluginService.IconStorage.LoadImage("toggle"),  new Vector2(22, 22));
        private HoverButton _settingsIcon { get; } = new(PluginService.IconStorage.LoadIcon(66319),  new Vector2(22, 22));

        private HoverButton _addIcon { get; } = new(PluginService.IconStorage.LoadIcon(66315),  new Vector2(22, 22));

        private HoverButton _searchIcon { get; } = new(PluginService.IconStorage.LoadIcon(66320),  new Vector2(22, 22));

        private HoverButton _closeSettingsIcon { get; } = new(PluginService.IconStorage.LoadIcon(66311),  new Vector2(22, 22));
        private HoverButton _resetButton { get; } = new(PluginService.IconStorage.LoadImage("nuke"),  new Vector2(22, 22));

        private static HoverButton _marketIcon { get; } = new(PluginService.IconStorage.LoadImage("refresh-web"),  new Vector2(22, 22));

        private HoverButton _clearIcon { get; } = new(PluginService.IconStorage.LoadIcon(66308),  new Vector2(22, 22));

        private static HoverButton _export2Icon { get; } = new(PluginService.IconStorage.LoadImage("export2"),  new Vector2(22,22));
        private static HoverButton _clipboardIcon { get; } = new(PluginService.IconStorage.LoadImage("clipboard"),  new Vector2(22,22));
        private static HoverButton _importTcIcon { get; } = new(PluginService.IconStorage.LoadImage("import_tc"),  new Vector2(22,22));
        private static HoverButton _filtersIcon { get; } = new(PluginService.IconStorage.LoadImage("filters"),  new Vector2(22,22));
        
        private static HoverButton _menuIcon { get; } = new(PluginService.IconStorage.LoadImage("menu"),  new Vector2(22, 22));


        private TeamCraftImportWindow? _teamCraftImportWindow;
        private List<FilterConfiguration>? _filters;
        private FilterConfiguration? _defaultFilter;
        private Dictionary<FilterConfiguration, Widgets.PopupMenu> _popupMenus = new();
        
        private PopupMenu _settingsMenu = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
            new List<PopupMenu.IPopupMenuItem>()
            {
                new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", OpenMobsWindow,"Open the mobs window."),
                new PopupMenu.PopupMenuItemSelectable("Npcs Window", "npcs", OpenNpcsWindow,"Open the npcs window."),
                new PopupMenu.PopupMenuItemSelectable("Duties Window", "duties", OpenDutiesWindow,"Open the duties window."),
                new PopupMenu.PopupMenuItemSelectable("Airships Window", "airships", OpenAirshipsWindow,"Open the airships window."),
                new PopupMenu.PopupMenuItemSelectable("Submarines Window", "submarines", OpenSubmarinesWindow,"Open the submarines window."),
                new PopupMenu.PopupMenuItemSelectable("Retainer Ventures Window", "ventures", OpenRetainerVenturesWindow,"Open the retainer ventures window."),
                new PopupMenu.PopupMenuItemSelectable("Tetris", "tetris", OpenTetrisWindow,"Open the tetris window.", () => ConfigurationManager.Config.TetrisEnabled),
                new PopupMenu.PopupMenuItemSeparator(),
                new PopupMenu.PopupMenuItemSelectable("Help", "help", OpenHelpWindow,"Open the help window."),
            });
        
        private static void OpenHelpWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<HelpWindow>(HelpWindow.AsKey);
        }

        private static void OpenDutiesWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<DutiesWindow>(DutiesWindow.AsKey);
        }

        private static void OpenAirshipsWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<AirshipsWindow>(AirshipsWindow.AsKey);
        }

        private static void OpenSubmarinesWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<SubmarinesWindow>(SubmarinesWindow.AsKey);
        }
        
        private static void OpenRetainerVenturesWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<RetainerTasksWindow>(RetainerTasksWindow.AsKey);
        }

        private static void OpenMobsWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<BNpcWindow>(BNpcWindow.AsKey);
        }
        
        private static void OpenNpcsWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<ENpcsWindow>(ENpcsWindow.AsKey);
        }
        
        private static void OpenTetrisWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<TetrisWindow>(TetrisWindow.AsKey);
        }

        public CraftsWindow(string name = "Crafts") : base(name)
        {
            SetupWindow();
            _splitter = new(ConfigurationManager.Config.CraftWindowSplitterPosition, new(100, 100), true);
        }
        
        public CraftsWindow() : base("Crafts")
        {
            SetupWindow();
            _splitter = new(ConfigurationManager.Config.CraftWindowSplitterPosition, new(100, 100), true);
        }

        public void SetupWindow()
        {

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
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                SetActiveFilter(existingFilter);
                this._settingsActive = true;
            }
        }


        private void RemoveFilter(string id, bool confirmed)
        {
            if (confirmed)
            {
                id = id.Replace("rf_", "");
                var existingFilter = PluginService.FilterService.GetFilterByKey(id);
                if (existingFilter != null)
                {
                    PluginService.FilterService.RemoveFilter(existingFilter);
                }
            }
        }

        private void MoveFilterDown(string id)
        {
            id = id.Replace("md_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                var currentFilter = this.SelectedConfiguration;
                PluginService.FilterService.MoveFilterDown(existingFilter);
                if (currentFilter != null)
                {
                    SetActiveFilter(currentFilter);
                    FocusFilter(currentFilter);
                }
            }
        }

        private void MoveFilterUp(string id)
        {
            id = id.Replace("mu_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                var currentFilter = this.SelectedConfiguration;
                PluginService.FilterService.MoveFilterUp(existingFilter);
                if (currentFilter != null)
                {
                    SetActiveFilter(currentFilter);
                    FocusFilter(currentFilter);
                }
            }
        }

        private void DuplicateFilter(string filterName, string id)
        {
            id = id.Replace("df_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                var newFilter = PluginService.FilterService.DuplicateFilter(existingFilter, filterName);
                SetActiveFilter(newFilter);
            }
        }


        private List<FilterConfiguration> Filters
        {
            get
            {
                if (_filters == null)
                {
                    _filters = PluginService.FilterService.FiltersList.Where(c => c.FilterType == FilterType.CraftFilter && c.CraftListDefault == false).ToList();
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
                    _defaultFilter = PluginService.FilterService.GetDefaultCraftList();
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
        public override unsafe void Draw()
        {
            if (!ConfigurationManager.Config.HasSeenNotification(NotificationPopup.CraftNotice) && ImGui.IsWindowFocused())
            {
                ImGui.OpenPopup("notification");
                ConfigurationManager.Config.MarkNotificationSeen(NotificationPopup.CraftNotice);
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
            _teamCraftImportWindow?.Draw();
            if (_teamCraftImportWindow != null &&_teamCraftImportWindow.HasResult && _teamCraftImportWindow.ParseResult != null && SelectedConfiguration != null)
            {
                foreach (var item in _teamCraftImportWindow.ParseResult)
                {
                    SelectedConfiguration.CraftList.AddCraftItem(item.Item1, item.Item2);
                    SelectedConfiguration.NeedsRefresh = true;
                    SelectedConfiguration.StartRefresh();
                    _teamCraftImportWindow = null;
                }
            }
            if (ConfigurationManager.Config.CraftWindowLayout == WindowLayout.Sidebar)
            {
                DrawSidebar();
                DrawMainWindow();
            }
            else
            {
                DrawTabBar();
            }
        }

        private string _newCraftName = "";
        private bool openPopup = false;
        private unsafe void DrawTabBar()
        {
            if (openPopup)
            {
                ImGui.OpenPopup("addCraftFilterName");
                openPopup = false;
            }
            if (ImGuiUtil.OpenNameField("addCraftFilterName", ref _newCraftName))
            {
                PluginService.FrameworkService.RunOnFrameworkThread(() =>
                {
                    AddCraftFilter(_newCraftName);
                    _newCraftName = "";
                });
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
                        openPopup = true;
                    }
                    ImGuiUtil.HoverTooltip("Add a new craft list");
                }
            }
        }

        private void AddCraftFilter(string newName)
        {
            var filterConfiguration = PluginService.FilterService.AddNewCraftFilter();
            filterConfiguration.Name = newName;
            Invalidate();
            this.SetActiveFilter(filterConfiguration);
        }

        private int? _newTab;
        private DateTime? _applyNewTabTime;

        private bool SwitchNewTab => _newTab != null && _applyNewTabTime != null && _applyNewTabTime.Value <= DateTime.Now;

        public void SetActiveFilter(FilterConfiguration configuration)
        {
            var filterIndex = Filters.Contains(configuration) ? Filters.IndexOf(configuration) : -1;
            if (filterIndex != -1)
            {
                _newTab = filterIndex;
                _applyNewTabTime = DateTime.Now + TimeSpan.FromMilliseconds(10);
                //ImGui being shit workaround
            }
        }

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
                        if (_selectedFilterTab == index)
                        {
                            var filterConfiguration = filterConfigurations[index];
                            if (isWindowFocused)
                            {
                                if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                                    ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key &&
                                    ConfigurationManager.Config.ActiveUiFilter != null)
                                {
                                    PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                    {
                                        PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                                    });
                                }
                                if (ConfigurationManager.Config.SwitchCraftListsAutomatically &&
                                    ConfigurationManager.Config.ActiveCraftList != filterConfiguration.Key &&
                                    ConfigurationManager.Config.ActiveCraftList != null && filterConfiguration.FilterType == FilterType.CraftFilter)
                                {
                                    PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                    {
                                        PluginService.FilterService.ToggleActiveCraftList(filterConfiguration);
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
                                if (ImGui.Selectable(filterConfiguration.Name + "###fl" + filterConfiguration.Key,
                                        index == _selectedFilterTab))
                                {
                                    _selectedFilterTab = index;
                                    if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                                        ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key &&
                                        ConfigurationManager.Config.ActiveUiFilter != null)
                                    {
                                        PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                        {
                                            PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                                        });
                                    }
                                    if (ConfigurationManager.Config.SwitchCraftListsAutomatically &&
                                        ConfigurationManager.Config.ActiveCraftList != filterConfiguration.Key &&
                                        ConfigurationManager.Config.ActiveCraftList != null && filterConfiguration.FilterType == FilterType.CraftFilter)
                                    {
                                        PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                        {
                                            PluginService.FilterService.ToggleActiveCraftList(filterConfiguration);
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
                            if (_addIcon.Draw("cb_acf"))
                            {
                                PluginService.PluginLogic.AddNewCraftFilter();
                            }

                            ImGuiUtil.HoverTooltip("Add a new craft list.");
                        }
                    }
                }
            }

            ImGui.SameLine();
        }

        private readonly HorizontalSplitter _splitter;

        private unsafe void DrawCraftPanel(FilterConfiguration filterConfiguration)
        {
            var itemTable = PluginService.FilterService.GetFilterTable(filterConfiguration.Key);
            var craftTable = PluginService.FilterService.GetCraftTable(filterConfiguration.Key);
            if (itemTable != null)
            {
                using (var topBarChild = ImRaii.Child("TopBar", new Vector2(0, 40) * ImGui.GetIO().FontGlobalScale, true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (topBarChild.Success)
                    {
                        var highlightItems = itemTable.HighlightItems;
                        UiHelpers.CenterElement(22 * ImGui.GetIO().FontGlobalScale);
                        ImGui.Checkbox("Highlight?" + "###" + itemTable.Key + "VisibilityCheckbox", ref highlightItems);
                        if (highlightItems != itemTable.HighlightItems)
                        {
                            PluginService.FrameworkService.RunOnFrameworkThread(() =>
                            {
                                PluginService.FilterService.ToggleActiveUiFilter(itemTable.FilterConfiguration);
                            });
                        }
                        ImGuiUtil.HoverTooltip("When checked, any items you need to retrieve from external sources will be highlighted.");

                        ImGui.SameLine();
                        if (_clearIcon.Draw("tb_cf"))
                        {
                            itemTable.ClearFilters();
                        }

                        ImGuiUtil.HoverTooltip("Clear the current search.");

                        ImGui.SameLine();
                        UiHelpers.CenterElement(22 * ImGui.GetIO().FontGlobalScale);
                        var hideCompleted = filterConfiguration.CraftList.HideComplete;
                        ImGui.Checkbox("Hide Completed?" + "###" + itemTable.Key + "HideCompleted", ref hideCompleted);
                        if (hideCompleted != filterConfiguration.CraftList.HideComplete)
                        {
                            filterConfiguration.CraftList.HideComplete = hideCompleted;
                            filterConfiguration.NeedsRefresh = true;
                            filterConfiguration.StartRefresh();
                        }

                        ImGuiUtil.HoverTooltip("Hide any precrafts/gather/buy items once completed?");

                        ImGui.SameLine();
                        float width = ImGui.GetWindowSize().X;
                        width -= 28 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        if (_searchIcon.Draw("tb_oib"))
                        {
                            _addItemBarOpen = !_addItemBarOpen;
                        }

                        ImGuiUtil.HoverTooltip("Toggles the add item side bar.");

                        ImGui.SameLine();
                        width -= 28 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        if (_editIcon.Draw("tb_edit"))
                        {
                            _settingsActive = !_settingsActive;
                        }
                        
                        ImGuiUtil.HoverTooltip("Edit the craft list's configuration.");
                        
                        ImGui.SameLine();
                        width -= 28 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        if (_toggleIcon.Draw("set_active"))
                        {
                            PluginService.FilterService.ToggleActiveCraftList(filterConfiguration);
                        }
                        ImGuiUtil.HoverTooltip("Toggle the current craft list.");

                        ImGui.SameLine();
                        width -= 156 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        ImGui.SetNextItemWidth(150);
                        var activeCraftList = PluginService.FilterService.GetActiveCraftList();
                        using (var combo = ImRaii.Combo("##ActiveCraftList",activeCraftList != null ? activeCraftList.Name : "None"))
                        {
                            if (combo.Success)
                            {
                                if (ImGui.Selectable("None"))
                                {
                                    PluginService.FilterService.ClearActiveCraftList();
                                }
                                foreach (var filter in PluginService.FilterService.FiltersList.Where(c =>
                                             c.FilterType == FilterType.CraftFilter && !c.CraftListDefault))
                                {
                                    if (ImGui.Selectable(filter.Name + "##" + filter.Key))
                                    {
                                        PluginService.FilterService.SetActiveCraftList(filter);
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
                    }
                }

                using (var contentChild = ImRaii.Child("Content", new Vector2(0, -44) * ImGui.GetIO().FontGlobalScale, true))
                {
                    if (contentChild.Success)
                    {
                        var result = _splitter.Draw((shouldDraw) =>
                        {
                            _craftsExpanded = craftTable?.Draw(new Vector2(0,0), shouldDraw) ??
                                              true;
                        }, (shouldDraw) =>
                        {
                            _itemsExpanded = itemTable.Draw(new Vector2(0, 0), shouldDraw);
                        }, "To Craft", "Items in Retainers/Bags");
                        if (result != null)
                        {
                            ConfigurationManager.Config.CraftWindowSplitterPosition = (int)result.Value;
                            ConfigurationManager.SaveAsync();
                        }
                    }
                }

                
                //Need to have these buttons be determined dynamically or moved elsewhere
                using (var bottomBarChild = ImRaii.Child("BottomBar", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale,
                           true, ImGuiWindowFlags.NoScrollbar))
                {
                    if (bottomBarChild.Success)
                    {
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_marketIcon.Draw("bb_market"))
                        {
                            foreach (var item in itemTable.RenderSortedItems)
                            {
                                PluginService.Universalis.QueuePriceCheck(item.InventoryItem.ItemId);
                            }

                            foreach (var item in itemTable.RenderItems)
                            {
                                PluginService.Universalis.QueuePriceCheck(item.RowId);
                            }

                            if (craftTable != null)
                            {
                                foreach (var item in craftTable.CraftItems)
                                {
                                    PluginService.Universalis.QueuePriceCheck(item.ItemId);
                                }

                                foreach (var item in craftTable.RenderItems)
                                {
                                    PluginService.Universalis.QueuePriceCheck(item.RowId);
                                }
                            }
                        }

                        ImGuiUtil.HoverTooltip("Refresh Market Prices");
                        ImGui.SameLine();
                        //Export CSV
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_export2Icon.Draw("bb_csv"))
                        {
                            ImGui.OpenPopup("SaveToCsv");
                        }
                        
                        if (ImGui.BeginPopup("SaveToCsv"))
                        {
                            if (ImGui.Selectable("Export Craft List CSV"))
                            {
                                if (craftTable != null)
                                {
                                    PluginService.FileDialogManager.SaveFileDialog("Save to csv", "*.csv",
                                        "export-craft-list.csv", ".csv",
                                        (b, s) => { SaveCraftCallback(craftTable, b, s); }, null, true);
                                }
                            }

                            if (ImGui.Selectable("Export Retainer List CSV"))
                            {
                                PluginService.FileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv",
                                    (b, s) => { SaveCallback(itemTable, b, s); }, null, true);
                            }
                            ImGui.EndPopup();
                        }

                        ImGuiUtil.HoverTooltip("Export to CSV");
                        ImGui.SameLine();
                        //Export Json
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_clipboardIcon.Draw("bb_json"))
                        {
                            ImGui.OpenPopup("SaveToJson");
                        }
                        
                        if (ImGui.BeginPopup("SaveToJson"))
                        {
                            if (ImGui.Selectable("Copy Craft List"))
                            {
                                if (craftTable != null)
                                {
                                    craftTable.ExportToJson().ToClipboard();
                                }
                            }

                            if (ImGui.Selectable("Copy Retainer List"))
                            {
                                itemTable.ExportToJson().ToClipboard();
                            }
                            ImGui.EndPopup();
                        }

                        ImGuiUtil.HoverTooltip("Copy JSON to clipboard");
                        ImGui.SameLine();
                        
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_importTcIcon.Draw("bb_import_tc_json"))
                        {
                            if (craftTable != null)
                            {
                                if(_teamCraftImportWindow == null)
                                {
                                    _teamCraftImportWindow = new TeamCraftImportWindow("craftsTCImport");
                                    _teamCraftImportWindow.OpenImportWindow = true;
                                }
                                else
                                {
                                    _teamCraftImportWindow.OpenImportWindow = true;
                                }
                            }
                        }

                        ImGuiUtil.HoverTooltip("Import TC List");
                        ImGui.SameLine();

                        if (PluginService.GameUi.IsWindowVisible(
                                CriticalCommonLib.Services.Ui.WindowName.SubmarinePartsMenu))
                        {
                            var subMarinePartsMenu = PluginService.GameUi.GetWindow("SubmarinePartsMenu");
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
                                                PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                                {
                                                    filterConfiguration.CraftList.AddCraftItem(itemRequired,
                                                        (uint)amountLeft, InventoryItem.ItemFlags.None);
                                                    filterConfiguration.NeedsRefresh = true;
                                                    filterConfiguration.StartRefresh();
                                                });
                                            }
                                        }
                                    }
                                }
                            }

                            ImGui.SameLine();
                        }

                        UiHelpers.VerticalCenter("Pending Market Requests: " + PluginService.Universalis.QueuedCount);

                        craftTable?.DrawFooterItems();
                        itemTable.DrawFooterItems();
                        ImGui.SameLine();


                        var width = ImGui.GetWindowSize().X;
                        
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_menuIcon.Draw("openMenu"))
                        {
                        }
                        _settingsMenu.Draw();
                        
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        ImGui.SetCursorPosX(width);
                        if (_settingsIcon.Draw("bb_ocw"))
                        {
                            PluginService.WindowService.ToggleConfigurationWindow();
                        }

                        ImGuiUtil.HoverTooltip("Open the configuration window.");
                        
                        ImGui.SetCursorPosY(0);
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_filtersIcon.Draw("openFilters"))
                        {
                            PluginService.WindowService.ToggleFiltersWindow();
                        }

                        ImGuiUtil.HoverTooltip("Open the filters window.");

                        if (craftTable != null)
                        {
                            var totalItems =  itemTable.RenderSortedItems.Count + " items / " + craftTable.GetCraftListCount() + " craft items";
                            var calcTextSize = ImGui.CalcTextSize(totalItems);
                            width -= calcTextSize.X + 15;
                            ImGui.SetCursorPosX(width);
                            UiHelpers.VerticalCenter(totalItems);
                        }
                    }
                }
            }
        }

        private string? _newName = null;
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
                                var base64 = filterConfiguration.ExportBase64();
                                ImGui.SetClipboardText(base64);
                                PluginService.ChatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
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
                            foreach (var group in PluginService.PluginLogic.GroupedFilters)
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
                                            foreach (var filter in group.Value)
                                            {
                                                if ((filter.AvailableIn.HasFlag(FilterType.SearchFilter) &&
                                                     filterConfiguration.FilterType.HasFlag(FilterType.SearchFilter)
                                                     ||
                                                     (filter.AvailableIn.HasFlag(FilterType.SortingFilter) &&
                                                      filterConfiguration.FilterType.HasFlag(FilterType.SortingFilter))
                                                     ||
                                                     (filter.AvailableIn.HasFlag(FilterType.CraftFilter) &&
                                                      filterConfiguration.FilterType.HasFlag(FilterType.CraftFilter))
                                                     ||
                                                     (filter.AvailableIn.HasFlag(FilterType.HistoryFilter) &&
                                                      filterConfiguration.FilterType.HasFlag(FilterType.HistoryFilter))
                                                     ||
                                                     (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                                                      filterConfiguration.FilterType.HasFlag(FilterType.GameItemFilter))
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

            using (var bottomBarChild = ImRaii.Child("BottomBar", new Vector2(0, 0), true, ImGuiWindowFlags.NoScrollbar))
            {
                if (bottomBarChild.Success)
                {
                    if (filterConfiguration.CraftListDefault)
                    {
                        UiHelpers.VerticalCenter(
                            "You are currently editing default craft list configuration.");
                    }
                    else
                    {
                        UiHelpers.VerticalCenter(
                            "You are currently editing the craft list's configuration. Press the tick on the right hand side to save configuration.");
                    }
                    float width = ImGui.GetWindowSize().X;

                    if (!filterConfiguration.CraftListDefault)
                    {
                        ImGui.SameLine();
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_closeSettingsIcon.Draw("bb_settings"))
                        {
                            _settingsActive = false;
                        }
                        ImGuiUtil.HoverTooltip("Return to the craft list.");
                        
                        ImGui.SameLine();
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_resetButton.Draw("bb_reset"))
                        {
                            ImGui.OpenPopup("confirmReset");
                        }

                        var result = InventoryTools.Ui.Widgets.ImGuiUtil.ConfirmPopup("confirmReset", new Vector2(400, 100), () =>
                        {
                            ImGui.TextWrapped("Are you sure you want to reset your configuration to the default?");
                        });
                        if (result == true)
                        {
                            filterConfiguration.ResetCraftFilter();
                        }
                        ImGuiUtil.HoverTooltip("Reset craft list to default configuration (keeps items).");
                    }
                    else
                    {
                        ImGui.SameLine();
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                        if (_resetButton.Draw("bb_reset"))
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
                                    DefaultConfiguration.ResetCraftFilter();
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
                    UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_clipboardIcon.Draw("copyFilterBtn"))
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
                                    filterConfiguration.ResetFilter(filter);
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
                    item.DrawRightClickPopup();
                }
            }
            ImGui.TableNextColumn();
            using (ImRaii.PushId("s_" + item.RowId))
            {
                if (_addIcon.Draw("bbadd_" + item.RowId, new Vector2(16,16) * ImGui.GetIO().FontGlobalScale))
                {
                    PluginService.FrameworkService.RunOnFrameworkThread(() =>
                    {
                        filterConfiguration.CraftList.AddCraftItem(item.RowId, 1, InventoryItem.ItemFlags.None);
                        filterConfiguration.NeedsRefresh = true;
                        filterConfiguration.StartRefresh();
                    });
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
            }
        }

        private string _searchString = "";
        private List<ItemEx>? _searchItems = null;
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
                    _searchItems = Service.ExcelCache.ItemNamesById.Where(c => c.Value.ToLower().PassesFilter(SearchString.ToLower())).Take(100)
                        .Select(c => Service.ExcelCache.GetItemExSheet().GetRow(c.Key)!).ToList();
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
    }
}