using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Addons;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Logic;
using OtterGui;

namespace InventoryTools.Ui
{
    public class CraftsWindow : Window
    {
        public override bool SaveState => true;

        public static string AsKey => "crafts";
        public override string Name { get; } = "Allagan Tools - Crafts";
        public override string Key => AsKey;
        public override Vector2 Size => new Vector2(600, 600);
        public override Vector2 MaxSize => new Vector2(5000, 5000);
        public override Vector2 MinSize => new Vector2(300, 300);
        public override bool DestroyOnClose => false;
        private int _selectedFilterTab = 0;
        private bool _settingsActive = false;
        private bool _costingBarOpen = false;
        private TextureWrap _settingsIcon => PluginService.IconStorage.LoadIcon(66319);
        private TextureWrap _addIcon => PluginService.IconStorage.LoadIcon(66315);
        private TextureWrap _addEphemeralIcon => PluginService.IconStorage.LoadIcon(66317);
        private TextureWrap _closeSettingsIcon => PluginService.IconStorage.LoadIcon(66311);
        private TextureWrap _csvIcon => PluginService.IconStorage.LoadIcon(47);
        private TextureWrap _clearIcon => PluginService.IconStorage.LoadIcon(66308);
        private TextureWrap _gilIcon => PluginService.IconStorage.LoadIcon(26001);
        private List<FilterConfiguration>? _filters;

        private List<FilterConfiguration> Filters
        {
            get
            {
                if (_filters == null)
                {
                    _filters = PluginService.FilterService.FiltersList.Where(c => c.FilterType == FilterType.CraftFilter).ToList();
                }

                return _filters;
            }
        }

        public void FocusFilter(FilterConfiguration filterConfiguration, bool showSettings = false)
        {
            var filterConfigurations = Filters;
            if (filterConfigurations.Contains(filterConfiguration))
            {
                _selectedFilterTab = filterConfigurations.IndexOf(filterConfiguration);
                if (showSettings)
                {
                    _settingsActive = true;
                }
            }
        }
        public override void Draw()
        {
            var isWindowFocused = ImGui.IsWindowFocused();
            var filterConfigurations = Filters;
            if (ImGui.BeginChild("###craftsList", new Vector2(180, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (ImGui.BeginChild("CraftList", new Vector2(0, -28) * ImGui.GetIO().FontGlobalScale, false))
                {
                    for (var index = 0; index < filterConfigurations.Count; index++)
                    {
                        var filterConfiguration = filterConfigurations[index];
                        if (ImGui.Selectable(filterConfiguration.Name + "###fl" + index, index == _selectedFilterTab))
                        {
                            _selectedFilterTab = index;
                            if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                                ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key &&
                                ConfigurationManager.Config.ActiveUiFilter != null)
                            {
                                PluginLog.Log(filterConfiguration.Key);
                                PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                            }
                        }
                    }
                    ImGui.EndChild();
                }

                if (ImGui.BeginChild("ListCommands", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, false))
                {
                    float height = ImGui.GetWindowSize().Y;
                    ImGui.SetCursorPosY((height - 24)* ImGui.GetIO().FontGlobalScale);
                    if (ImGui.ImageButton(_addIcon.ImGuiHandle, new Vector2(20, 20)* ImGui.GetIO().FontGlobalScale, new Vector2(0,0), new Vector2(1,1 ),2))
                    {
                        PluginService.PluginLogic.AddNewCraftFilter();
                    }
                    ImGuiUtil.HoverTooltip("Add a new craft list.");
                    /*ImGui.SameLine();
                    if (ImGui.ImageButton(_addEphemeralIcon.ImGuiHandle, new Vector2(20, 20)* ImGui.GetIO().FontGlobalScale, new Vector2(0,0), new Vector2(1,1 ),2))
                    {
                                
                    }
                    ImGuiUtil.HoverTooltip("Add a new ephemeral craft list.");*/
                    
                    var width = ImGui.GetWindowSize().X;
                    width -= 28;
                    ImGui.SetCursorPosX(width * ImGui.GetIO().FontGlobalScale);
                    ImGui.SetCursorPosY((height - 24) * ImGui.GetIO().FontGlobalScale);
                    if (ImGui.ImageButton(_settingsIcon.ImGuiHandle,
                            new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                            new Vector2(1, 1), 2))
                    {
                        PluginService.WindowService.ToggleConfigurationWindow();
                    }
                    ImGuiUtil.HoverTooltip("Open the configuration window.");
                    ImGui.EndChild();
                }

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("###craftMainWindow", new Vector2(_costingBarOpen ? -200 : -1, -1) * ImGui.GetIO().FontGlobalScale, false, ImGuiWindowFlags.HorizontalScrollbar))
            {
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
                                PluginLog.Log(filterConfiguration.Key);
                                PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                            }
                        }
                        if (_settingsActive)
                        {
                            if (ImGui.BeginChild("Content", new Vector2(0, -44) * ImGui.GetIO().FontGlobalScale, true))
                            {
                                var filterName = filterConfiguration.Name;
                                var labelName = "##" + filterConfiguration.Key;
                                if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
                                {
                                    ImGui.SetNextItemWidth(100);
                                    ImGui.LabelText(labelName + "FilterNameLabel", "Name: ");
                                    ImGui.SameLine();
                                    ImGui.InputText(labelName + "FilterName", ref filterName, 100);
                                    if (filterName != filterConfiguration.Name)
                                    {
                                        filterConfiguration.Name = filterName;
                                    }

                                    ImGui.NewLine();
                                    if (ImGui.Button("Export Configuration to Clipboard"))
                                    {
                                        var base64 = filterConfiguration.ExportBase64();
                                        ImGui.SetClipboardText(base64);
                                        ChatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
                                    }

                                    var filterType = filterConfiguration.FormattedFilterType;
                                    ImGui.SetNextItemWidth(100);
                                    ImGui.LabelText(labelName + "FilterTypeLabel", "Filter Type: ");
                                    ImGui.SameLine();
                                    ImGui.TextDisabled(filterType);

                                    ImGui.SetNextItemWidth(150);
                                    ImGui.LabelText(labelName + "DisplayInTabs", "Display in Tab List: ");
                                    ImGui.SameLine();
                                    var displayInTabs = filterConfiguration.DisplayInTabs;
                                    if (ImGui.Checkbox(labelName + "DisplayInTabsCheckbox", ref displayInTabs))
                                    {
                                        if (displayInTabs != filterConfiguration.DisplayInTabs)
                                        {
                                            filterConfiguration.DisplayInTabs = displayInTabs;
                                        }
                                    }
                                }

                                if (ImGui.BeginTabBar("###FilterConfigTabs", ImGuiTabBarFlags.FittingPolicyScroll))
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

                                        if (hasValuesSet)
                                        {
                                            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                                        }

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
                                            (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                                             filterConfiguration.FilterType.HasFlag(FilterType
                                                 .GameItemFilter)));
                                        if (hasValues && ImGui.BeginTabItem(group.Key.ToString()))
                                        {
                                            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
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
                                                     (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                                                      filterConfiguration.FilterType.HasFlag(FilterType.GameItemFilter))
                                                    ))
                                                {
                                                    filter.Draw(filterConfiguration);
                                                }
                                            }

                                            ImGui.PopStyleColor();
                                            ImGui.EndTabItem();
                                        }

                                        if (hasValuesSet)
                                        {
                                            ImGui.PopStyleColor();
                                        }
                                    }

                                    ImGui.EndTabBar();
                                }

                                ImGui.EndChild();
                            }

                            if (ImGui.BeginChild("BottomBar", new Vector2(0, 0), true, ImGuiWindowFlags.None))
                            {
                                float width = ImGui.GetWindowSize().X;
                                ImGui.SetCursorPosX((width - 42) * ImGui.GetIO().FontGlobalScale);
                                if (ImGui.ImageButton(_closeSettingsIcon.ImGuiHandle,
                                        new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                                        new Vector2(1, 1), 2))
                                {
                                    _settingsActive = false;
                                }
                                ImGuiUtil.HoverTooltip("Return to the craft list.");
                                ImGui.EndChild();
                            }
                        }
                        else
                        {
                            var itemTable = PluginService.FilterService.GetFilterTable(filterConfiguration.Key);
                            if (itemTable != null)
                            {
                                if (ImGui.BeginChild("TopBar", new Vector2(0, 40) * ImGui.GetIO().FontGlobalScale, true))
                                {
                                    var highlightItems = itemTable.HighlightItems;
                                    ImGui.Checkbox("Highlight?" + "###" + itemTable.Key + "VisibilityCheckbox",ref highlightItems);
                                    if (highlightItems != itemTable.HighlightItems)
                                    {
                                        PluginService.FilterService.ToggleActiveUiFilter(itemTable.FilterConfiguration);
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.ImageButton(_clearIcon.ImGuiHandle,
                                            new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                                            new Vector2(1, 1), 2))
                                    {
                                        itemTable.ClearFilters();
                                    }

                                    ImGuiUtil.HoverTooltip("Clear the current search.");
                                    
                                    /*ImGui.SameLine();
                                    float width = ImGui.GetWindowSize().X;
                                    ImGui.SetCursorPosX((width - 42) * ImGui.GetIO().FontGlobalScale);
                                    if (ImGui.ImageButton(_gilIcon.ImGuiHandle,
                                            new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),new Vector2(1, 1), 2))
                                    {
                                        _costingBarOpen = !_costingBarOpen;
                                    }
                                    ImGuiUtil.HoverTooltip("Toggles the costing bar.");*/

                                    ImGui.EndChild();
                                }

                                if (ImGui.BeginChild("Content", new Vector2(0, -44) * ImGui.GetIO().FontGlobalScale, true))
                                {
                                    var craftTable = PluginService.FilterService.GetCraftTable(filterConfiguration.Key);
                                    craftTable?.Draw(new Vector2(0, -400));
                                    itemTable.Draw(new Vector2(0, 0));
                                    ImGui.EndChild();
                                }

                                //Need to have these buttons be determined dynamically or moved elsewhere
                                if (ImGui.BeginChild("BottomBar", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, true))
                                {
                                    ImGui.SameLine();
                                    if (ImGui.ImageButton(_csvIcon.ImGuiHandle,
                                            new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                                            new Vector2(1, 1), 2))
                                    {
                                        PluginService.FileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv", (b, s) =>
                                        {
                                            SaveCallback(itemTable, b, s);
                                        }, null, true);

                                    }
                                    ImGuiUtil.HoverTooltip("Export to CSV");
                                    ImGui.SameLine();
                                    unsafe
                                    {
                                        var subMarinePartsMenu = PluginService.GameUi.GetWindow("SubmarinePartsMenu");
                                        if (subMarinePartsMenu != null)
                                        {
                                            if (ImGui.Button("Add Submarine Parts to Craft"))
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
                                                            filterConfiguration.CraftList.AddCraftItem(itemRequired,
                                                                (uint)amountLeft, ItemFlags.None);
                                                            filterConfiguration.NeedsRefresh = true;
                                                            filterConfiguration.StartRefresh();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    ImGui.SameLine();
                                    ImGui.SmallButton("PR: " + Universalis.QueuedCount);
                                    ImGuiUtil.HoverTooltip("The current amount of pending market requests(this data is sourced from universalis).");

                                    var craftTable = PluginService.FilterService.GetCraftTable(filterConfiguration.Key);
                                    craftTable?.DrawFooterItems();
                                    itemTable.DrawFooterItems();
                                    ImGui.SameLine();
                                    float width = ImGui.GetWindowSize().X;
                                    ImGui.SetCursorPosX((width - 42) * ImGui.GetIO().FontGlobalScale);
                                    if (ImGui.ImageButton(_settingsIcon.ImGuiHandle,
                                            new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                                            new Vector2(1, 1), 2))
                                    {
                                        _settingsActive = true;
                                    }

                                    ImGuiUtil.HoverTooltip("Open the settings dialog for this craft list.");
                                    ImGui.EndChild();
                                }
                            }

                        }
                    }
                }
                ImGui.EndChild();
            }
            ImGui.SameLine();
            if (_costingBarOpen && ImGui.BeginChild("###craftsCostingList", new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                for (var index = 0; index < filterConfigurations.Count; index++)
                {
                    if (_selectedFilterTab == index)
                    {
                        var filterConfiguration = filterConfigurations[index];
                        if (filterConfiguration.FilterType == FilterType.CraftFilter)
                        {
                            ImGui.Text("Costings");
                            ImGui.Separator();
                            ImGui.Text("Gil");
                            ImGui.Text("NQ: " + filterConfiguration.CraftList.MinimumNQCost);
                            ImGui.Text("HQ: " + filterConfiguration.CraftList.MinimumHQCost);

                        }
                    }
                }

                ImGui.EndChild();
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
            
        }
    }
}