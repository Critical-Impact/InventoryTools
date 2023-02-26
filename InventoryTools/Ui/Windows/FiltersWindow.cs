using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Logic;
using InventoryTools.Ui.Widgets;
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
        public override Vector2 MaxSize { get; } = new(2000, 2000);
        public override Vector2 MinSize { get; } = new(200, 200);
        public override Vector2 DefaultSize { get; } = new(600, 600);
        public override bool DestroyOnClose => false;
        private static TextureWrap _settingsIcon => PluginService.IconStorage.LoadIcon(66319);
        private static TextureWrap _tetrisIcon => PluginService.IconStorage.LoadIcon(76955);
        private static TextureWrap _craftIcon => PluginService.IconStorage.LoadIcon(114060);
        private static TextureWrap _csvIcon => PluginService.IconStorage.LoadIcon(47);
        private static TextureWrap _clearIcon => PluginService.IconStorage.LoadIcon(66308);
        private static TextureWrap _marketIcon => PluginService.IconStorage.LoadIcon(90003);
        private static TextureWrap _helpIcon => PluginService.IconStorage.LoadIcon(66313);
        private static TextureWrap _dutyIcon => PluginService.IconStorage.LoadIcon(61801);
        private static TextureWrap _mobIcon => PluginService.IconStorage.LoadIcon(60041);
        
        
        private List<FilterConfiguration>? _filters;
        private PopupMenu _addFilterMenu;

        public FiltersWindow(string name = "Allagan Tools - Filters") : base(name)
        {
            SetupWindow();
        }

        public FiltersWindow() : base("Allagan Tools - Filters")
        {
            SetupWindow();
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
                        new PopupMenu.PopupMenuItemSelectable("Move Left", "mu_" + configuration.Key, MoveFilterUp, "Move the filter left."),
                        new PopupMenu.PopupMenuItemSelectable("Move Right", "md_" + configuration.Key, MoveFilterDown, "Move the filter right."),
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
                PluginService.FilterService.MoveFilterDown(existingFilter);
            }
        }

        private void MoveFilterUp(string id)
        {
            id = id.Replace("mu_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                PluginService.FilterService.MoveFilterUp(existingFilter);
            }
        }

        private void DuplicateFilter(string filterName, string id)
        {
            id = id.Replace("df_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                PluginService.FilterService.DuplicateFilter(existingFilter, filterName);
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
                    new PopupMenu.PopupMenuItemSelectableAskName("Game Item Filter", "af3", "New Game Item Filter", AddGameItemFilter, "This will create a filter that lets you search for all items in the game.")
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

        public override void Draw()
        {
            if (ImGui.BeginTabBar("###InventoryTabs" + _tabLayout, ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton))
            {
                var filterConfigurations = Filters;
                for (var index = 0; index < filterConfigurations.Count; index++)
                {
                    var filterConfiguration = filterConfigurations[index];
                    var itemTable = PluginService.FilterService.GetFilterTable(filterConfiguration);
                    if (itemTable == null)
                    {
                        continue;
                    }

                    if (filterConfiguration.DisplayInTabs && ImGui.BeginTabItem(itemTable.Name + "##" + filterConfiguration.Key))
                    {
                        GetFilterMenu(filterConfiguration).Draw();

                        var activeFilter = DrawFilter(itemTable, filterConfiguration, _activeFilter);
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

                        ImGui.EndTabItem();
                    }
                }
                
                if (ConfigurationManager.Config.ShowFilterTab && ImGui.BeginTabItem("Filters"))
                {
                    ImGui.BeginChild("###monitorLeft", new Vector2(100, -1) * ImGui.GetIO().FontGlobalScale, true);
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

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("###monitorRight", new Vector2(-1, -1), true,
                        ImGuiWindowFlags.HorizontalScrollbar);
                    for (var index = 0; index < filterConfigurations.Count; index++)
                    {
                        if (_selectedFilterTab == index)
                        {
                            var filterConfiguration = filterConfigurations[index];
                            var table = PluginService.FilterService.GetFilterTable(filterConfiguration.Key);
                            if (table != null)
                            {
                                var activeFilter = DrawFilter(table, filterConfiguration, _activeFilter);
                                if (_activeFilter != activeFilter)
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

                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
                {
                }
                
                _addFilterMenu.Draw();

                ImGui.EndTabBar();
            }
        }

        public static unsafe string DrawFilter(FilterTable itemTable, FilterConfiguration filterConfiguration,string activeFilter)
        {
            ImGui.BeginChild("TopBar", new Vector2(0, 40) * ImGui.GetIO().FontGlobalScale, true,
                ImGuiWindowFlags.NoScrollbar);
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
            if (ImGui.ImageButton(_clearIcon.ImGuiHandle,
                    new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                    new Vector2(1, 1), 0))
            {
                itemTable.ClearFilters();
            }

            ImGuiUtil.HoverTooltip("Clear the current search.");

            ImGui.EndChild();

            ImGui.BeginChild("Content", new Vector2(0, -40) * ImGui.GetIO().FontGlobalScale, true,
                ImGuiWindowFlags.NoScrollbar);
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

            activeFilter = filterConfiguration.Key;

            ImGui.EndChild();

            //Need to have these buttons be determined dynamically or moved elsewhere
            ImGui.BeginChild("BottomBar", new Vector2(0, 0), true, ImGuiWindowFlags.NoScrollbar);
            UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.ImageButton(_marketIcon.ImGuiHandle,
                    new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                    new Vector2(1, 1), 2))
            {
                foreach (var item in itemTable.RenderSortedItems)
                {
                    PluginService.Universalis.QueuePriceCheck(item.InventoryItem.ItemId);
                }

                foreach (var item in itemTable.RenderItems)
                {
                    PluginService.Universalis.QueuePriceCheck(item.RowId);
                }
            }

            ImGuiUtil.HoverTooltip("Refresh Market Prices");
            ImGui.SameLine();
            UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.ImageButton(_csvIcon.ImGuiHandle,
                    new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                    new Vector2(1, 1), 2))
            {
                PluginService.FileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv",
                    (b, s) => { SaveCallback(itemTable, b, s); }, null, true);
            }

            ImGuiUtil.HoverTooltip("Export to CSV");
            if (filterConfiguration.FilterType == FilterType.CraftFilter && PluginService.GameUi.IsWindowVisible(CriticalCommonLib.Services.Ui.WindowName.SubmarinePartsMenu))
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
                ImGui.Text("Total Cost NQ: " + filterConfiguration.CraftList.MinimumNQCost);
                ImGui.SameLine();
                ImGui.Text("Total Cost HQ: " + filterConfiguration.CraftList.MinimumHQCost);
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
            UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
            ImGui.SetCursorPosX(width);
            if (ImGui.ImageButton(_settingsIcon.ImGuiHandle,
                    new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                    new Vector2(1, 1), 2))
            {
                PluginService.WindowService.ToggleConfigurationWindow();
            }

            ImGuiUtil.HoverTooltip("Open the configuration window.");

            ImGui.SetCursorPosY(0);
            width -= 30 * ImGui.GetIO().FontGlobalScale;
            ImGui.SetCursorPosX(width);
            UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.ImageButton(_craftIcon.ImGuiHandle,
                    new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                    new Vector2(1, 1), 2))
            {
                PluginService.WindowService.ToggleCraftsWindow();
            }

            ImGuiUtil.HoverTooltip("Open the craft window.");

            width -= 30 * ImGui.GetIO().FontGlobalScale;
            ImGui.SetCursorPosX(width);
            UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.ImageButton(_mobIcon.ImGuiHandle,
                    new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                    new Vector2(1, 1), 2))
            {
                PluginService.WindowService.ToggleMobWindow();
            }

            ImGuiUtil.HoverTooltip("Open the mob window.");


            width -= 30 * ImGui.GetIO().FontGlobalScale;
            ImGui.SetCursorPosX(width);
            UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.ImageButton(_dutyIcon.ImGuiHandle,
                    new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                    new Vector2(1, 1), 2))
            {
                PluginService.WindowService.ToggleDutiesWindow();
            }

            ImGuiUtil.HoverTooltip("Open the duty window.");
            
            width -= 30 * ImGui.GetIO().FontGlobalScale;
            ImGui.SetCursorPosX(width);
            UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.ImageButton(_helpIcon.ImGuiHandle,
                    new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                    new Vector2(1, 1), 2))
            {
                PluginService.WindowService.ToggleHelpWindow();
            }

            ImGuiUtil.HoverTooltip("Open the help window.");

            if (ConfigurationManager.Config.TetrisEnabled)
            {
                width -= 30 * ImGui.GetIO().FontGlobalScale;
                ImGui.SetCursorPosX(width);
                UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                if (ImGui.ImageButton(_tetrisIcon.ImGuiHandle,
                        new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                        new Vector2(1, 1), 2))
                {
                    PluginService.WindowService.ToggleTetrisWindow();
                }

                ImGuiUtil.HoverTooltip("Open the tetris input window.");
            }


            ImGui.EndChild();

            return activeFilter;
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
            _filters = null;
            _tabLayout = Utils.GenerateRandomId();
        }
    }
}