using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Addons;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Ui.Widgets;
using OtterGui.Raii;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui
{
    public class FilterWindow : Window
    {
        private HoverButton _editIcon { get; } = new(PluginService.IconStorage.LoadImage("edit"),  new Vector2(22, 22));
        private HoverButton _settingsIcon { get; } = new(PluginService.IconStorage.LoadIcon(66319),  new Vector2(22, 22));
        private HoverButton _craftIcon { get; } = new(PluginService.IconStorage.LoadImage("craft"),  new Vector2(22, 22));
        private HoverButton _csvIcon { get; } = new(PluginService.IconStorage.LoadImage("export2"),  new Vector2(22,22));
        private HoverButton _clearIcon { get; } = new(PluginService.IconStorage.LoadIcon(66308),  new Vector2(22, 22));
        private static HoverButton _marketIcon { get; } = new(PluginService.IconStorage.LoadImage("refresh-web"),  new Vector2(22, 22));
        private static HoverButton _menuIcon { get; } = new(PluginService.IconStorage.LoadImage("menu"),  new Vector2(22, 22));
        private static HoverButton _filtersIcon { get; } = new(PluginService.IconStorage.LoadImage("filters"),  new Vector2(22,22));
        
        private List<FilterConfiguration>? _filters;
        private PopupMenu _addFilterMenu;

        private PopupMenu _settingsMenu = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
            new List<PopupMenu.IPopupMenuItem>()
            {
                new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", OpenMobsWindow,"Open the mobs window."),
                new PopupMenu.PopupMenuItemSelectable("Duties Window", "duties", OpenDutiesWindow,"Open the duties window."),
                new PopupMenu.PopupMenuItemSelectable("Airships Window", "airships", OpenAirshipsWindow,"Open the airships window."),
                new PopupMenu.PopupMenuItemSelectable("Submarines Window", "submarines", OpenSubmarinesWindow,"Open the submarines window."),
                new PopupMenu.PopupMenuItemSelectable("Retainer Ventures Window", "ventures", OpenRetainerVenturesWindow,"Open the retainer ventures window."),
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
        
        public FilterWindow(string filterKey, string name = "Allagan Tools - Filter") : base(name)
        {
            _filterKey = filterKey;
            if (SelectedConfiguration != null)
            {
                WindowName = "Allagan Tools - " + SelectedConfiguration.Name;
            }
        }
        
        public override void Invalidate()
        {
            
        }

        private string _filterKey;
        private string _activeFilter;

        public static string AsKey(string filterKey)
        {
            return "filter_" + filterKey;
        }

        public override FilterConfiguration? SelectedConfiguration =>
            PluginService.FilterService.GetFilterByKey(_filterKey);
        
        public override string Key => AsKey(_filterKey);
        public override bool DestroyOnClose => true;
        public override bool SaveState => true;
        public override void Draw()
        {
            if (SelectedConfiguration != null)
            {
                if (ImGui.IsWindowFocused())
                {
                    if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                        ConfigurationManager.Config.ActiveUiFilter != SelectedConfiguration.Key &&
                        ConfigurationManager.Config.ActiveUiFilter != null)
                    {
                        PluginService.FrameworkService.RunOnFrameworkThread(() =>
                        {
                            PluginService.FilterService.ToggleActiveUiFilter(SelectedConfiguration);
                        });
                    }
                }
                var table = PluginService.FilterService.GetFilterTable(_filterKey);
                if (table != null)
                {
                    var activeFilter = DrawFilter(table, SelectedConfiguration);
                    if (_activeFilter != activeFilter && ImGui.IsWindowFocused())
                    {
                        if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                            ConfigurationManager.Config.ActiveUiFilter != SelectedConfiguration.Key &&
                            ConfigurationManager.Config.ActiveUiFilter != null)
                        {
                            PluginService.FrameworkService.RunOnFrameworkThread(() =>
                            {
                                PluginService.FilterService.ToggleActiveUiFilter(SelectedConfiguration);
                            });
                        }
                    }
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
                    if (_filtersIcon.Draw("openFilters"))
                    {
                        PluginService.WindowService.ToggleFiltersWindow();
                    }

                    ImGuiUtil.HoverTooltip("Open the filters window.");
                    
                    ImGui.SetCursorPosY(0);
                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_craftIcon.Draw("openCraft"))
                    {
                        PluginService.WindowService.ToggleCraftsWindow();
                    }

                    ImGuiUtil.HoverTooltip("Open the craft window.");
                    
                    var totalItems =  itemTable.RenderSortedItems.Count + " items";

                    if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.GameItemFilter)
                    {
                        totalItems =  itemTable.RenderItems.Count + " items";
                    }

                    var calcTextSize = ImGui.CalcTextSize(totalItems);
                    width -= calcTextSize.X + 15;
                    ImGui.SetCursorPosX(width);
                    UiHelpers.VerticalCenter(totalItems);
                }
            }

            return filterConfiguration.Key;
        }
        
        private static void SaveCallback(FilterTable filterTable, bool arg1, string arg2)
        {
            if (arg1)
            {
                filterTable.ExportToCsv(arg2);
            }
        }

        public override Vector2 DefaultSize => new Vector2(600, 500);
        public override Vector2 MaxSize => new Vector2(1500, 1500);
        public override Vector2 MinSize => new Vector2(200, 200);
    }
}