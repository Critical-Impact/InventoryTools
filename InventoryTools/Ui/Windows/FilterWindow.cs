using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;
using Dalamud.Interface.ImGuiFileDialog;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Ui.Widgets;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui
{
    public class FilterWindow : StringWindow
    {
        private readonly TableService _tableService;
        private readonly IIconService _iconService;
        private readonly IListService _listService;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IUniversalis _universalis;
        private readonly FileDialogManager _fileDialogManager;
        private readonly IGameUiManager _gameUiManager;
        private readonly InventoryToolsConfiguration _configuration;

        public FilterWindow(ILogger<FilterWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, TableService tableService, IIconService iconService, IListService listService, ICharacterMonitor characterMonitor, IUniversalis universalis, FileDialogManager fileDialogManager, IGameUiManager gameUiManager, string name = "Filter Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _tableService = tableService;
            _iconService = iconService;
            _listService = listService;
            _characterMonitor = characterMonitor;
            _universalis = universalis;
            _fileDialogManager = fileDialogManager;
            _gameUiManager = gameUiManager;
            _configuration = configuration;
        }
        public override void Initialize(string filterKey)
        {
            _filterKey = filterKey;
            if (SelectedConfiguration != null)
            {
                Key = "filter_" + filterKey;
                WindowName = "Allagan Tools - " + SelectedConfiguration.Name;
            }
            else
            {
                Key = "filter_invalid";
                WindowName = "Invalid List";
            }

            _settingsMenu = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", _ => MediatorService.Publish(new OpenGenericWindowMessage(typeof(BNpcsWindow))),
                        "Open the mobs window."),
                    new PopupMenu.PopupMenuItemSelectable("Npcs Window", "npcs", _ => MediatorService.Publish(new OpenGenericWindowMessage(typeof(ENpcsWindow))),
                        "Open the npcs window."),
                    new PopupMenu.PopupMenuItemSelectable("Duties Window", "duties", _ => MediatorService.Publish(new OpenGenericWindowMessage(typeof(DutiesWindow))),
                        "Open the duties window."),
                    new PopupMenu.PopupMenuItemSelectable("Airships Window", "airships", _ => MediatorService.Publish(new OpenGenericWindowMessage(typeof(AirshipsWindow))),
                        "Open the airships window."),
                    new PopupMenu.PopupMenuItemSelectable("Submarines Window", "submarines", _ => MediatorService.Publish(new OpenGenericWindowMessage(typeof(SubmarinesWindow))),
                        "Open the submarines window."),
                    new PopupMenu.PopupMenuItemSelectable("Retainer Ventures Window", "ventures",_ => MediatorService.Publish(new OpenGenericWindowMessage(typeof(RetainerTasksWindow))),
                        "Open the retainer ventures window."),
                    new PopupMenu.PopupMenuItemSelectable("Tetris", "tetris", _ => MediatorService.Publish(new OpenGenericWindowMessage(typeof(TetrisWindow))),
                        "Open the tetris window.", () => _configuration.TetrisEnabled),
                    new PopupMenu.PopupMenuItemSeparator(),
                    new PopupMenu.PopupMenuItemSelectable("Help", "help", _ => MediatorService.Publish(new OpenGenericWindowMessage(typeof(HelpWindow))), "Open the help window."),
                });
            
                _editIcon = new(_iconService.LoadImage("edit"),  new Vector2(22, 22));
                _settingsIcon = new(_iconService.LoadIcon(66319),  new Vector2(22, 22));
                _craftIcon = new(_iconService.LoadImage("craft"),  new Vector2(22, 22));
                _csvIcon = new(_iconService.LoadImage("export2"),  new Vector2(22,22));
                _clearIcon = new(_iconService.LoadIcon(66308),  new Vector2(22, 22));
                _marketIcon = new(_iconService.LoadImage("refresh-web"),  new Vector2(22, 22));
                _menuIcon = new(_iconService.LoadImage("menu"),  new Vector2(22, 22));
                _filtersIcon = new(_iconService.LoadImage("filters"),  new Vector2(22,22));
        }

        private HoverButton _editIcon;
        private HoverButton _settingsIcon;
        private HoverButton _craftIcon;
        private HoverButton _csvIcon;
        private HoverButton _clearIcon;
        private HoverButton _marketIcon;
        private HoverButton _menuIcon;
        private HoverButton _filtersIcon;

        private PopupMenu _settingsMenu;

        public override void Invalidate()
        {
            
        }

        private string _filterKey;

        public override FilterConfiguration? SelectedConfiguration =>
            _listService.GetListByKey(_filterKey);

        public override string GenericKey { get; } = "filter";
        public override string GenericName { get; } = "Filter";
        public override bool DestroyOnClose => true;
        public override bool SaveState => true;
        public override void Draw()
        {
            var filterConfiguration = SelectedConfiguration;
            if (filterConfiguration != null)
            {
                if (ImGui.IsWindowFocused())
                {
                    filterConfiguration.Active = true;
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
                else
                {
                    filterConfiguration.Active = false;
                }
                var table = _tableService.GetListTable(filterConfiguration);
                DrawFilter(table, filterConfiguration);
                if (ImGui.IsWindowFocused())
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

                    ImGui.SameLine();
                    ImGuiService.CenterElement(20 * ImGui.GetIO().FontGlobalScale);
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
                    if (_filtersIcon.Draw("openFilters"))
                    {
                        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(FiltersWindow)));
                    }

                    ImGuiUtil.HoverTooltip("Open the items window.");
                    
                    ImGui.SetCursorPosY(0);
                    width -= 30 * ImGui.GetIO().FontGlobalScale;
                    ImGui.SetCursorPosX(width);
                    ImGuiService.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
                    if (_craftIcon.Draw("openCraft"))
                    {
                        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(CraftsWindow)));
                    }

                    ImGuiUtil.HoverTooltip("Open the craft window.");
                    
                    var totalItems =  itemTable.RenderSortedItems.Count + " items";

                    if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.GameItemFilter)
                    {
                        totalItems =  itemTable.RenderItems.Count + " items";
                    }

                    if (SelectedConfiguration != null && SelectedConfiguration.FilterType == FilterType.HistoryFilter)
                    {
                        totalItems =  itemTable.InventoryChanges.Count + " historical records";
                    }

                    var calcTextSize = ImGui.CalcTextSize(totalItems);
                    width -= calcTextSize.X + 15;
                    ImGui.SetCursorPosX(width);
                    ImGuiService.VerticalCenter(totalItems);
                }
            }

            return filterConfiguration.Key;
        }
        
        private void SaveCallback(FilterTable filterTable, bool arg1, string arg2)
        {
            if (arg1)
            {
                filterTable.ExportToCsv(arg2);
            }
        }

        public override Vector2? DefaultSize => new Vector2(600, 500);
        public override Vector2? MaxSize => new Vector2(1500, 1500);
        public override Vector2? MinSize => new Vector2(200, 200);
    }
}