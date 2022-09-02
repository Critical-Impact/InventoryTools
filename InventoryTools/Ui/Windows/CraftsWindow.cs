using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using OtterGui;
using OtterGui.Raii;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

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
        private bool _addItemBarOpen = false;
        private bool _craftsExpanded = true;
        private bool _itemsExpanded = true;
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
                        if (ImGui.Selectable(filterConfiguration.Name + "###fl" + filterConfiguration.Key, index == _selectedFilterTab))
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
                    ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
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
                    width -= 28 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale );
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

            if (ImGui.BeginChild("###craftMainWindow", new Vector2(_addItemBarOpen ? -250 : -1, -1) * ImGui.GetIO().FontGlobalScale, false, ImGuiWindowFlags.HorizontalScrollbar))
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
                                ImGui.SetCursorPosX(width - 42 * ImGui.GetIO().FontGlobalScale);
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
                                    
                                    ImGui.SameLine();
                                    float width = ImGui.GetWindowSize().X;
                                    ImGui.SetCursorPosX(width - 42 * ImGui.GetIO().FontGlobalScale);
                                    if (ImGui.ImageButton(_addIcon.ImGuiHandle,
                                            new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),new Vector2(1, 1), 2))
                                    {
                                        _addItemBarOpen = !_addItemBarOpen;
                                    }
                                    ImGuiUtil.HoverTooltip("Toggles the add item side bar.");

                                    ImGui.EndChild();
                                }

                                if (ImGui.BeginChild("Content", new Vector2(0, -44) * ImGui.GetIO().FontGlobalScale, true))
                                {
                                    //Move footer and header drawing to tables to allow each to bring extra detail
                                    var craftTable = PluginService.FilterService.GetCraftTable(filterConfiguration.Key);
                                    _craftsExpanded = craftTable?.Draw(new Vector2(0, _itemsExpanded ? -300 * ImGui.GetIO().FontGlobalScale : -50 * ImGui.GetIO().FontGlobalScale)) ?? true;
                                    _itemsExpanded = itemTable.Draw(new Vector2(0, 0));
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
                                                                (uint)amountLeft, InventoryItem.ItemFlags.None);
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
                                    ImGui.SetCursorPosX(width - 42 * ImGui.GetIO().FontGlobalScale);
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
            if (_addItemBarOpen && ImGui.BeginChild("###craftsAddItem", new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                for (var index = 0; index < filterConfigurations.Count; index++)
                {
                    if (_selectedFilterTab == index)
                    {
                        var filterConfiguration = filterConfigurations[index];
                        if (filterConfiguration.FilterType == FilterType.CraftFilter)
                        {
                            ImGui.Text("Add new Item");
                            var searchString = SearchString;
                            ImGui.InputText("##ItemSearch", ref searchString, 50);
                            if (_searchString != searchString)
                            {
                                SearchString = searchString;
                            }
                            ImGui.Separator();
                            using var table = ImRaii.Table("", 2, ImGuiTableFlags.None);
                            if (!table)
                                return;

                            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, 200);
                            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.None, 16);

                            foreach (var datum in SearchItems)
                            {
                                ImGui.TableNextRow();
                                DrawSearchRow(filterConfiguration, datum);
                            }
                            if (_searchString == "")
                            {
                                ImGui.Text("Start typing to search...");
                            }

                        }
                    }
                }

                ImGui.EndChild();
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
                    
            if (ImGui.BeginPopup("RightClick"+ item.RowId))
            {
                item.DrawRightClickPopup();
                ImGui.EndPopup();
            }
            ImGui.TableNextColumn();
            ImGui.PushID("s_" + item.RowId);
            if (ImGui.ImageButton(_addIcon.ImGuiHandle, new Vector2(16, 16), new Vector2(0,0), new Vector2(1,1), 0))
            {
                filterConfiguration.CraftList.AddCraftItem(item.RowId, 1, InventoryItem.ItemFlags.None);
                filterConfiguration.NeedsRefresh = true;
                filterConfiguration.StartRefresh();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
            ImGui.PopID();
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
                    _searchItems = CraftItemsByName.Where(c => c.Value.Contains(SearchString.ToLower())).Take(100)
                        .Select(c => Service.ExcelCache.GetItemExSheet().GetRow(c.Key)!).ToList();
                }

                return _searchItems;
            }
        }

        public Dictionary<uint, string>? _craftItemsByName = null;
        
        public Dictionary<uint, string> CraftItemsByName
        {
            get
            {
                if (_craftItemsByName == null)
                {
                    _craftItemsByName = Service.ExcelCache.GetItemExSheet().Where(c => c.CanBeCrafted).ToDictionary(c => c.RowId, c => c.NameString.ToLower());
                }
                return _craftItemsByName;
            }
            set => _craftItemsByName = value;
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

        public override void Invalidate()
        {
            _filters = null;
        }
    }
}