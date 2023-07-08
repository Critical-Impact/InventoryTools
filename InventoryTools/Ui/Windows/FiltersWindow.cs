using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Widgets;
using OtterGui.Raii;
using ImGuiUtil = OtterGui.ImGuiUtil;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Ui
{
    public class FiltersWindow : Window
    {
        public override bool SaveState => true;

        public static string AsKey = "filters";
        public override string Key => AsKey;
        private string _activeFilter = "";
        private string _tabLayout = "";
        private int _selectedFilterTab = 0;
        private bool _settingsActive = false;
        public override Vector2? MaxSize { get; } = new(2000, 2000);
        public override Vector2? MinSize { get; } = new(200, 200);
        public override Vector2? DefaultSize { get; } = new(600, 600);
        public override bool DestroyOnClose => false;
        private HoverButton _editIcon { get; } = new(PluginService.IconStorage.LoadImage("edit"),  new Vector2(22, 22));
        private HoverButton _settingsIcon { get; } = new(PluginService.IconStorage.LoadIcon(66319),  new Vector2(22, 22));
        private HoverButton _craftIcon { get; } = new(PluginService.IconStorage.LoadImage("craft"),  new Vector2(22, 22));
        private HoverButton _csvIcon { get; } = new(PluginService.IconStorage.LoadImage("export2"),  new Vector2(22,22));
        private HoverButton _clipboardIcon { get; } = new(PluginService.IconStorage.LoadImage("clipboard"),  new Vector2(22,22));
        private HoverButton _clearIcon { get; } = new(PluginService.IconStorage.LoadIcon(66308),  new Vector2(22, 22));
        private HoverButton _closeSettingsIcon { get; } = new(PluginService.IconStorage.LoadIcon(66311),  new Vector2(22, 22));
        private static HoverButton _marketIcon { get; } = new(PluginService.IconStorage.LoadImage("refresh-web"),  new Vector2(22, 22));
        private HoverButton _addIcon { get; } = new(PluginService.IconStorage.LoadIcon(66315),  new Vector2(22, 22));
        private static HoverButton _menuIcon { get; } = new(PluginService.IconStorage.LoadImage("menu"),  new Vector2(22, 22));

        
        private List<FilterConfiguration>? _filters;
        private PopupMenu _addFilterMenu = null!;

        private PopupMenu _settingsMenu = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
            new List<PopupMenu.IPopupMenuItem>()
            {
                new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", OpenMobsWindow,"Open the mobs window."),
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

        private static void OpenTetrisWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<TetrisWindow>(TetrisWindow.AsKey);
        }

        private static void OpenMobsWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<BNpcWindow>(BNpcWindow.AsKey);
        }        
        private static void OpenNPCsWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<ENpcsWindow>(ENpcsWindow.AsKey);
        }

        public FiltersWindow(string name = "Filters") : base(name)
        {
            SetupWindow();
        }

        public FiltersWindow() : base("Filters")
        {
            SetupWindow();
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
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
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
                var configWindow = PluginService.WindowService.GetWindow<ConfigurationWindow>(ConfigurationWindow.AsKey);
                configWindow.Open();
                configWindow.BringToFront();
                configWindow.SetActiveFilter(newFilter);
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


        public void SetupWindow()
        {
            _tabLayout = Utils.GenerateRandomId();
            _addFilterMenu = new PopupMenu("addFilter", PopupMenu.PopupMenuButtons.LeftRight,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectableAskName("Search Filter", "adf1", "New Search Filter", AddSearchFilter, "This will create a new filter that let's you search for specific items within your characters and retainers inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Sort Filter", "af2", "New Sort Filter", AddSortFilter, "This will create a new filter that let's you search for specific items within your characters and retainers inventories then determine where they should be moved to."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Game Item Filter", "af3", "New Game Item Filter", AddGameItemFilter, "This will create a filter that lets you search for all items in the game."),
                    new PopupMenu.PopupMenuItemSelectableAskName("History Filter", "af4", "New History Item Filter", AddHistoryFilter, "This will create a filter that lets you view historical data of how your inventory has changed."),
                });
        }
        
        private void AddSearchFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.SearchFilter);
            PluginService.FilterService.AddFilter(filterConfiguration);
            Invalidate();
            var configWindow = PluginService.WindowService.GetWindow<ConfigurationWindow>(ConfigurationWindow.AsKey);
            configWindow.Open();
            configWindow.BringToFront();
            configWindow.SetActiveFilter(filterConfiguration);
        }
        
        private void AddHistoryFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.HistoryFilter);
            PluginService.FilterService.AddFilter(filterConfiguration);
            Invalidate();
            var configWindow = PluginService.WindowService.GetWindow<ConfigurationWindow>(ConfigurationWindow.AsKey);
            configWindow.Open();
            configWindow.BringToFront();
            configWindow.SetActiveFilter(filterConfiguration);
        }

        private void AddGameItemFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.GameItemFilter);
            PluginService.FilterService.AddFilter(filterConfiguration);
            Invalidate();
            var configWindow = PluginService.WindowService.GetWindow<ConfigurationWindow>(ConfigurationWindow.AsKey);
            configWindow.Open();
            configWindow.BringToFront();
            configWindow.SetActiveFilter(filterConfiguration);
        }

        private void AddSortFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.SortingFilter);
            PluginService.FilterService.AddFilter(filterConfiguration);
            Invalidate();
            var configWindow = PluginService.WindowService.GetWindow<ConfigurationWindow>(ConfigurationWindow.AsKey);
            configWindow.Open();
            configWindow.BringToFront();
            configWindow.SetActiveFilter(filterConfiguration);
        }

        private List<FilterConfiguration> Filters
        {
            get
            {
                if (_filters == null)
                {
                    _filters = PluginService.FilterService.FiltersList.Where(c => c.FilterType != FilterType.CraftFilter).ToList();
                }

                return _filters;
            }
        }
        
        private int? _newTab;
        private DateTime? _applyNewTabTime;

        private bool SwitchNewTab => _newTab != null && _applyNewTabTime != null && _applyNewTabTime.Value <= DateTime.Now;
        
        public override unsafe void Draw()
        {
            if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.HistoryFilter && !ConfigurationManager.Config.HasSeenNotification(NotificationPopup.HistoryNotice) && ImGui.IsWindowFocused())
            {
                ImGui.OpenPopup("historynotice");
                ConfigurationManager.Config.MarkNotificationSeen(NotificationPopup.HistoryNotice);
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
                ConfigurationManager.Config.HistoryEnabled = choice.Value;
            }
            
            if (ConfigurationManager.Config.FiltersLayout == WindowLayout.Sidebar)
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
                                if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                                    ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key &&
                                    ConfigurationManager.Config.ActiveUiFilter != null)
                                {
                                    PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                    {
                                        PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                                    });
                                }
                            }

                            var itemTable = PluginService.FilterService.GetFilterTable(filterConfiguration);
                            if (itemTable == null)
                            {
                                continue;
                            }

                            if (_settingsActive)
                            {
                                DrawSettingsPanel(filterConfiguration);
                            }
                            else
                            {
                                var activeFilter = DrawFilter(itemTable, filterConfiguration);
                                if (_activeFilter != activeFilter && ImGui.IsWindowFocused())
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
                    var itemTable = PluginService.FilterService.GetFilterTable(filterConfiguration);
                    if (itemTable == null)
                    {
                        continue;
                    }

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
                                        if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                                            ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key &&
                                            ConfigurationManager.Config.ActiveUiFilter != null)
                                        {
                                            PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                            {
                                                PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (ConfigurationManager.Config.ShowFilterTab && ImGui.BeginTabItem("Filters"))
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
                                    if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                                        ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key)
                                    {
                                        PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                        {
                                            PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
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
                                    var table = PluginService.FilterService.GetFilterTable(filterConfiguration.Key);
                                    if (table != null)
                                    {
                                        var activeFilter = DrawFilter(table, filterConfiguration);
                                        if (_activeFilter != activeFilter)
                                        {
                                            if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                                                ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key &&
                                                ConfigurationManager.Config.ActiveUiFilter != null)
                                            {
                                                PluginService.FrameworkService.RunOnFrameworkThread(() =>
                                                {
                                                    PluginService.FilterService.ToggleActiveUiFilter(
                                                        filterConfiguration);
                                                });
                                            }
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
                    UiHelpers.VerticalCenter(
                        "You are currently editing the filter's configuration. Press the tick on the right hand side to save configuration.");

                    ImGui.SameLine();
                    float width = ImGui.GetWindowSize().X;
                    ImGui.SetCursorPosX(width - 42 * ImGui.GetIO().FontGlobalScale);
                    UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
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
                    UiHelpers.CenterElement(20 * ImGui.GetIO().FontGlobalScale);
                    ImGui.Checkbox("Highlight?" + "###" + itemTable.Key + "VisibilityCheckbox",
                        ref highlightItems);
                    if (highlightItems != itemTable.HighlightItems)
                    {
                        PluginService.FrameworkService.RunOnFrameworkThread(() =>
                        {
                            PluginService.FilterService.ToggleActiveUiFilter(itemTable.FilterConfiguration);
                        });
                    }

                    ImGui.SameLine();
                    UiHelpers.CenterElement(20 * ImGui.GetIO().FontGlobalScale);
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
                        var craftTable = PluginService.FilterService.GetCraftTable(filterConfiguration);
                        craftTable?.Draw(new Vector2(0, -400));
                        itemTable.Draw(new Vector2(0, 0));
                    }
                    else
                    {
                        itemTable.Draw(new Vector2(0, 0));
                    }

                }
            }

            //Need to have these buttons be determined dynamically or moved elsewhere
            using (var bottomBarChild =
                   ImRaii.Child("BottomBar", new Vector2(0, 0), true, ImGuiWindowFlags.NoScrollbar))
            {
                if (bottomBarChild.Success)
                {
                    UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if(_marketIcon.Draw("refreshMarket"))
                    {
                        foreach (var item in itemTable.RenderSortedItems)
                        {
                            PluginService.Universalis.QueuePriceCheck(item.InventoryItem.ItemId);
                        }

                        foreach (var item in itemTable.RenderItems)
                        {
                            PluginService.Universalis.QueuePriceCheck(item.RowId);
                        }

                        foreach (var item in itemTable.InventoryChanges)
                        {
                            PluginService.Universalis.QueuePriceCheck(item.InventoryItem.ItemId);
                        }
                    }

                    ImGuiUtil.HoverTooltip("Refresh Market Prices");
                    ImGui.SameLine();
                    UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_csvIcon.Draw("exportCsv"))
                    {
                        PluginService.FileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv",
                            (b, s) => { SaveCallback(itemTable, b, s); }, null, true);
                    }

                    ImGuiUtil.HoverTooltip("Export to CSV");
                    ImGui.SameLine();
                    UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_clipboardIcon.Draw("copyJson"))
                    {
                        itemTable.ExportToJson().ToClipboard();
                    }

                    ImGuiUtil.HoverTooltip("Copy JSON to clipboard");
                    if (filterConfiguration.FilterType == FilterType.CraftFilter &&
                        PluginService.GameUi.IsWindowVisible(
                            CriticalCommonLib.Services.Ui.WindowName.SubmarinePartsMenu))
                    {
                        var subMarinePartsMenu = PluginService.GameUi.GetWindow("SubmarinePartsMenu");
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
                    }

                    ImGui.SameLine();
                    UiHelpers.VerticalCenter("Pending Market Requests: " + PluginService.Universalis.QueuedCount);
                    if (filterConfiguration.FilterType == FilterType.CraftFilter)
                    {
                        ImGui.SameLine();
                        ImGui.TextUnformatted("Total Cost NQ: " + filterConfiguration.CraftList.MinimumNQCost);
                        ImGui.SameLine();
                        ImGui.TextUnformatted("Total Cost HQ: " + filterConfiguration.CraftList.MinimumHQCost);
                    }

                    if (filterConfiguration.FilterType == FilterType.CraftFilter)
                    {
                        var craftTable = PluginService.FilterService.GetCraftTable(filterConfiguration);
                        craftTable?.DrawFooterItems();
                        itemTable.DrawFooterItems();
                    }
                    else
                    {
                        itemTable.DrawFooterItems();
                    }

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
                    if (_settingsIcon.Draw("openConfig"))
                    {
                        PluginService.WindowService.ToggleConfigurationWindow();
                    }

                    ImGuiUtil.HoverTooltip("Open the configuration window.");

                    ImGui.SetCursorPosY(0);
                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_craftIcon.Draw("openCraft"))
                    {
                        PluginService.WindowService.ToggleCraftsWindow();
                    }

                    ImGuiUtil.HoverTooltip("Open the craft window.");

                    if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.HistoryFilter)
                    {
                        ImGui.SetCursorPosY(0);
                        width -= 30 * ImGui.GetIO().FontGlobalScale;
                        ImGui.SetCursorPosX(width);
                        UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
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
                            PluginService.InventoryHistory.ClearHistory();
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
                        if (ConfigurationManager.Config.HistoryEnabled)
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
                    UiHelpers.VerticalCenter(totalItems);
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

        private static void SaveCallback(FilterTable filterTable, bool arg1, string arg2)
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