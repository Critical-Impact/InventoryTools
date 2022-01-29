using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Dalamud.Plugin;
using InventoryTools.Logic;
using InventoryTools.MarketBoard;
using Lumina.Excel.GeneratedSheets;
using XivCommon;
using XivCommon.Functions.Tooltips;

namespace InventoryTools
{
    public partial class PluginLogic : IDisposable
    {
        private static InventoryToolsConfiguration _config;
        private static InventoryMonitor _inventoryMonitor;
        private static CharacterMonitor _characterMonitor;
        private static PluginLogic _pluginLogic;
        private static ChatGui _chatGui;
        private ClientState _clientState;
        private GameUi _gameUi;
        private Framework _framework;
        private List<FilterConfiguration> _filterConfigurations = new();
        private Dictionary<string, FilterTable> _filterTables = new();

        public static InventoryMonitor InventoryMonitor => _inventoryMonitor;
        public static CharacterMonitor CharacterMonitor => _characterMonitor;
        public static InventoryToolsConfiguration PluginConfiguration => _config;
        public static PluginLogic Instance => _pluginLogic;

        private ulong _currentRetainerId;
        private XivCommonBase _commonBase { get; set; }

        private DateTime? _nextSaveTime = null;

        public PluginLogic(InventoryToolsConfiguration inventoryToolsConfiguration, ClientState clientState, InventoryMonitor inventoryMonitor, CharacterMonitor characterMonitor, GameUi gameUi, ChatGui chatGui, Framework framework)
        {
            _pluginLogic = this;
            _config = inventoryToolsConfiguration;
            _clientState = clientState;
            _inventoryMonitor = inventoryMonitor;
            _characterMonitor = characterMonitor;
            _gameUi = gameUi;
            _chatGui = chatGui;
            _framework = framework;

            //Events we need to track, inventory updates, active retainer changes, player changes, 
            _inventoryMonitor.OnInventoryChanged += InventoryMonitorOnOnInventoryChanged;
            _characterMonitor.OnActiveRetainerChanged += CharacterMonitorOnOnActiveCharacterChanged;
            _characterMonitor.OnCharacterUpdated += CharacterMonitorOnOnCharacterUpdated;
            _config.ConfigurationChanged += ConfigOnConfigurationChanged;
            _framework.Update += FrameworkOnUpdate;

            _inventoryMonitor.LoadExistingData(_config.GetSavedInventory());
            _characterMonitor.LoadExistingRetainers(_config.GetSavedRetainers());

            _gameUi.WatchWindowState(GameUi.WindowName.RetainerGrid0);
            _gameUi.WatchWindowState(GameUi.WindowName.InventoryGrid0E);
            _gameUi.WatchWindowState(GameUi.WindowName.RetainerList);
            _gameUi.WatchWindowState(GameUi.WindowName.Inventory);
            _gameUi.WatchWindowState(GameUi.WindowName.InventoryLarge);
            _gameUi.WatchWindowState(GameUi.WindowName.InventoryRetainerLarge);
            _gameUi.WatchWindowState(GameUi.WindowName.InventoryRetainer);
            _gameUi.WatchWindowState(GameUi.WindowName.InventoryBuddy);
            _gameUi.WatchWindowState(GameUi.WindowName.InventoryBuddy2);
            _gameUi.UiVisibilityChanged += GameUiOnUiVisibilityChanged;

            LoadExistingData(_config.GetSavedFilters());
            if (_config.FirstRun)
            {
                LoadDefaultData();
                _config.FirstRun = false;
            }
            RunMigrations();


            WatchFilterChanges();

            this._commonBase = new XivCommonBase(Hooks.Tooltips);
            this._commonBase.Functions.Tooltips.OnItemTooltip += this.OnItemTooltip;
        }

        private void RunMigrations()
        {
            
            if (_config.InternalVersion == 0)
            {
                PluginLog.Log("Migrating to version 1");
                var highlight = _config.HighlightColor;
                if (highlight.W == 0.0f)
                {
                    highlight.W = 1;
                    _config.HighlightColor = highlight;
                }

                _config.TabHighlightColor = _config.HighlightColor;

                foreach (var filterConfig in _filterConfigurations)
                {
                    if (filterConfig.HighlightColor != null)
                    {
                        if (filterConfig.HighlightColor.Value.X == 0.0f && filterConfig.HighlightColor.Value.Y == 0.0f &&
                            filterConfig.HighlightColor.Value.Z == 0.0f && filterConfig.HighlightColor.Value.W == 0.0f)
                        {
                            filterConfig.HighlightColor = null;
                            filterConfig.TabHighlightColor = null;
                        }
                        else
                        {
                            var highlightColor = filterConfig.HighlightColor.Value;
                            highlightColor.W = 1;
                            filterConfig.TabHighlightColor = highlightColor;
                        }
                    }
                }

                _config.InternalVersion++;
            }
            if (_config.InternalVersion == 1)
            {
                PluginLog.Log("Migrating to version 2");
                _config.InvertTabHighlighting = _config.InvertHighlighting;

                foreach (var filterConfig in _filterConfigurations)
                {
                    if (filterConfig.InvertHighlighting != null)
                    {
                        filterConfig.InvertTabHighlighting = filterConfig.InvertHighlighting;
                    }
                }

                _config.InternalVersion++;
            }
        }

        private void FrameworkOnUpdate(Framework framework)
        {
            if (_config.AutoSave)
            {
                if (NextSaveTime == null && _config.AutoSaveMinutes != 0)
                {
                    _nextSaveTime = DateTime.Now.AddMinutes(_config.AutoSaveMinutes);
                }
                else
                {
                    if (DateTime.Now >= NextSaveTime)
                    {
                        _nextSaveTime = null;
                        _config.Save();
                    }
                }
            }
        }

        private void WatchFilterChanges()
        {
            foreach (var filterConfiguration in _filterConfigurations)
            {
                filterConfiguration.ConfigurationChanged += FilterConfigurationOnConfigurationChanged;
            }
        }

        private void UnwatchFilterChanges()
        {
            foreach (var filterConfiguration in _filterConfigurations)
            {
                filterConfiguration.ConfigurationChanged -= FilterConfigurationOnConfigurationChanged;
            }
        }

        private void FilterConfigurationOnConfigurationChanged(FilterConfiguration filterconfiguration)
        {
            //Do some sort of debouncing
            InvalidateFilters();
            ToggleHighlights();
            _config.Save();
        }


        private void ConfigOnConfigurationChanged()
        {
            InvalidateFilters();
            ToggleHighlights();
            _config.Save();
        }

        private void CharacterMonitorOnOnCharacterUpdated(Character character)
        {
            if (character != null)
            {
                InvalidateFilters();
            }
        }

        /// <summary>
        /// Returns the currently active filter determined by the main window state
        /// </summary>
        /// <returns>FilterConfiguration</returns>
        public FilterConfiguration GetActiveFilter()
        {
            if (PluginConfiguration.IsVisible)
            {
                if (PluginConfiguration.ActiveUiFilter != null)
                {
                    if (_filterConfigurations.Any(c => c.Key == PluginConfiguration.ActiveUiFilter))
                    {
                        return _filterConfigurations.First(c => c.Key == PluginConfiguration.ActiveUiFilter);
                    }
                }
            }
            else
            {
                if (PluginConfiguration.ActiveBackgroundFilter != null)
                {
                    if (_filterConfigurations.Any(c => c.Key == PluginConfiguration.ActiveBackgroundFilter))
                    {
                        return _filterConfigurations.First(c => c.Key == PluginConfiguration.ActiveBackgroundFilter);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the currently active UI filter regardless of window state
        /// </summary>
        /// <returns>FilterConfiguration</returns>
        public FilterConfiguration GetActiveUiFilter()
        {
            if (PluginConfiguration.ActiveUiFilter != null)
            {
                if (_filterConfigurations.Any(c => c.Key == PluginConfiguration.ActiveUiFilter))
                {
                    return _filterConfigurations.First(c => c.Key == PluginConfiguration.ActiveUiFilter);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the currently active background filter
        /// </summary>
        /// <returns>FilterConfiguration</returns>
        public FilterConfiguration GetActiveBackgroundFilter()
        {
            if (PluginConfiguration.ActiveBackgroundFilter != null)
            {
                if (_filterConfigurations.Any(c => c.Key == PluginConfiguration.ActiveBackgroundFilter))
                {
                    return _filterConfigurations.First(c => c.Key == PluginConfiguration.ActiveBackgroundFilter);
                }
            }

            return null;
        }

        public FilterTable GetFilterTable(string filterKey)
        {
            if (_filterTables.ContainsKey(filterKey))
            {
                return _filterTables[filterKey];
            }
            else
            {
                if (_filterConfigurations.Any(c => c.Key == filterKey))
                {
                    var filterConfig = _filterConfigurations.First(c => c.Key == filterKey);
                    FilterTable generateTable = filterConfig.GenerateTable();
                    generateTable.Refreshed += GenerateTableOnRefreshed;
                    _filterTables.Add(filterKey, generateTable);
                    return _filterTables[filterKey];
                }

                return null;
            }
        }

        private void GenerateTableOnRefreshed(FilterTable itemtable)
        {
            ToggleHighlights();
        }

        public void LoadDefaultData()
        {
            var allItemsFilter = new FilterConfiguration("All", "AllItemsFilter", FilterType.SearchFilter);
            allItemsFilter.DisplayInTabs = true;
            allItemsFilter.SourceAllCharacters = true;
            allItemsFilter.SourceAllRetainers = true;
            _filterConfigurations.Add(allItemsFilter);

            var retainerItemsFilter = new FilterConfiguration("Retainers", "RetainerItemsFilter", FilterType.SearchFilter);
            retainerItemsFilter.DisplayInTabs = true;
            retainerItemsFilter.SourceAllRetainers = true;
            _filterConfigurations.Add(retainerItemsFilter);

            var playerItemsFilter = new FilterConfiguration("Player", "PlayerItemsFilter", FilterType.SearchFilter);
            playerItemsFilter.DisplayInTabs = true;
            playerItemsFilter.SourceAllCharacters = true;
            _filterConfigurations.Add(playerItemsFilter);
        }

        public void AddSampleFilter100Gil()
        {
            var sampleFilter = new FilterConfiguration("100 gill or less", FilterType.SearchFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceAllCharacters = true;
            sampleFilter.SourceAllRetainers = true;
            sampleFilter.CanBeBought = true;
            sampleFilter.ShopBuyingPrice = "<=100";
            _filterConfigurations.Add(sampleFilter);
        }


        public void AddSampleFilterMaterials()
        {
            var sampleFilter = new FilterConfiguration("Put away materials", FilterType.SortingFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceAllCharacters = true;
            sampleFilter.DestinationAllRetainers = true;
            sampleFilter.FilterItemsInRetainers = true;
            var itemUiCategories = ExcelCache.GetAllItemUICategories();
            //I'm making assumptions about the names of these and one day I will try to support more than english
            var categories = new HashSet<string>() { "Bone", "Cloth", "Catalyst", "Crystal", "Ingredient", "Leather", "Lumber", "Metal", "Part", "Stone" };
            sampleFilter.ItemUiCategoryId = new List<uint>();
            foreach (var itemUiCategory in itemUiCategories)
            {
                if (categories.Contains(itemUiCategory.Value.Name))
                {
                    sampleFilter.ItemUiCategoryId.Add(itemUiCategory.Key);
                }
            }
            _filterConfigurations.Add(sampleFilter);
        }

        public void AddSampleFilterDuplicatedItems()
        {
            var sampleFilter = new FilterConfiguration("Duplicated Items", FilterType.SortingFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceAllCharacters = true;
            sampleFilter.SourceAllRetainers = true;
            sampleFilter.DestinationAllRetainers = true;
            sampleFilter.FilterItemsInRetainers = true;
            sampleFilter.DuplicatesOnly = true;
            _filterConfigurations.Add(sampleFilter);
        }

        public List<FilterConfiguration> FilterConfigurations => _filterConfigurations;

        public DateTime? NextSaveTime => _nextSaveTime;

        public void ClearAutoSave()
        {
            _nextSaveTime = null;
        }

        public void LoadExistingData(List<FilterConfiguration> filterConfigurations)
        {
            this._filterConfigurations = filterConfigurations;
        }

        public void RemoveFilter(FilterConfiguration filter)
        {
            if (_filterConfigurations.Contains(filter))
            {
                _filterConfigurations.Remove(filter);
                if (_filterTables.ContainsKey(filter.Key))
                {
                    var table = _filterTables[filter.Key];
                    table.Dispose();
                    filter.ConfigurationChanged -= FilterConfigurationOnConfigurationChanged;
                    _filterTables.Remove(filter.Key);
                    ToggleHighlights();
                }
            }
        }

        public string GetCharacterName(ulong characterId)
        {
            if (_characterMonitor.Characters.ContainsKey(characterId))
            {
                return _characterMonitor.Characters[characterId].Name;
            }
            return "Unknown";
        }

        public ulong GetCurrentCharacterId()
        {
            if (_clientState.IsLoggedIn && _clientState.LocalPlayer != null)
            {
                return _clientState.LocalContentId;
            }
            return 0;
        }

        public bool DisableActiveUiFilter()
        {
            PluginLog.Verbose("PluginLogic: Disabling active ui filter");
            _config.ActiveUiFilter = null;
            ToggleHighlights();
            return true;
        }

        public bool DisableActiveBackgroundFilter()
        {
            PluginLog.Verbose("PluginLogic: Disabling active background filter");
            _config.ActiveBackgroundFilter = null;
            ToggleHighlights();
            return true;
        }

        public bool ToggleActiveUiFilterByKey(string filterKey)
        {
            PluginLog.Verbose("PluginLogic: Switching active ui filter");
            if (filterKey == _config.ActiveUiFilter)
            {
                _config.ActiveUiFilter = null;
                ToggleHighlights();
                return true;
            }

            if (_filterConfigurations.Any(c => c.Key == filterKey))
            {
                _config.ActiveUiFilter = filterKey;
                ToggleHighlights();
                return true;
            }

            return false;
        }

        public bool ToggleActiveBackgroundFilterByKey(string filterKey)
        {
            PluginLog.Verbose("PluginLogic: Switching active background filter");
            if (filterKey == _config.ActiveBackgroundFilter)
            {
                _config.ActiveBackgroundFilter = null;
                ToggleHighlights();
                return true;
            }

            if (_filterConfigurations.Any(c => c.Key == filterKey))
            {
                _config.ActiveBackgroundFilter = filterKey;
                ToggleHighlights();
                return true;
            }

            return false;
        }


        public bool ToggleActiveUiFilterByName(string filterName)
        {
            PluginLog.Verbose("PluginLogic: Switching active ui filter");
            if (_filterConfigurations.Any(c => c.Name == filterName))
            {
                var filter = _filterConfigurations.First(c => c.Name == filterName);
                if (filter.Key == _config.ActiveUiFilter)
                {
                    _config.ActiveUiFilter = null;
                    ToggleHighlights();
                    return true;
                }
                _config.ActiveUiFilter = filterName;
                ToggleHighlights();
                return true;
            }

            return false;
        }

        public bool ToggleActiveBackgroundFilterByName(string filterName)
        {
            PluginLog.Verbose("PluginLogic: Switching active background filter");
            if (_filterConfigurations.Any(c => c.Name == filterName))
            {
                var filter = _filterConfigurations.First(c => c.Name == filterName);
                if (filter.Key == _config.ActiveBackgroundFilter)
                {
                    _chatGui.Print("Disabled filter: " + filterName);
                    _config.ActiveBackgroundFilter = null;
                    ToggleHighlights();
                    return true;
                }
                _chatGui.Print("Switched filter to: " + filterName);
                _config.ActiveBackgroundFilter = filter.Key;
                ToggleHighlights();
                return true;
            }
            _chatGui.Print("Failed to find filter with name: " + filterName);
            return false;
        }

        private void GameUiOnUiVisibilityChanged(GameUi.WindowName windowName, bool isWindowVisible)
        {
            if (isWindowVisible)
            {
                ToggleHighlights();
            }
        }

        private void CharacterMonitorOnOnActiveCharacterChanged(ulong retainerId)
        {
            PluginLog.Debug("Retainer changed.");
            PluginLog.Debug("Retainer ID: " + retainerId);
            _currentRetainerId = retainerId;
            RegenerateFilter();
        }

        private void InventoryMonitorOnOnInventoryChanged(Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> inventories)
        {
            RegenerateFilter();
            if (inventories != null)
            {
                PluginLog.Verbose("PluginLogic: Inventory changed, saving to config.");
                PluginConfiguration.SavedInventories = inventories;
            }
        }

        private void InvalidateFilters()
        {
            foreach (var filter in _filterConfigurations)
            {
                filter.NeedsRefresh = true;
                if (_filterTables.ContainsKey(filter.Key))
                {
                    _filterTables[filter.Key].NeedsRefresh = true;
                }
            }
        }

        private void RegenerateFilter()
        {
            InvalidateFilters();
            ToggleHighlights();
        }

        private void DisableHighlights()
        {
            var inventoryGrid0 = _gameUi.GetPrimaryInventoryGrid(0);
            var inventoryGrid1 = _gameUi.GetPrimaryInventoryGrid(1);
            var inventoryGrid2 = _gameUi.GetPrimaryInventoryGrid(2);
            var inventoryGrid3 = _gameUi.GetPrimaryInventoryGrid(3);
            inventoryGrid0?.ClearColors();
            inventoryGrid1?.ClearColors();
            inventoryGrid2?.ClearColors();
            inventoryGrid3?.ClearColors();

            var smallInventoryGrid0 = _gameUi.GetNormalInventoryGrid(0);
            var smallInventoryGrid1 = _gameUi.GetNormalInventoryGrid(1);
            var smallInventoryGrid2 = _gameUi.GetNormalInventoryGrid(2);
            var smallInventoryGrid3 = _gameUi.GetNormalInventoryGrid(3);
            smallInventoryGrid0?.ClearColors();
            smallInventoryGrid1?.ClearColors();
            smallInventoryGrid2?.ClearColors();
            smallInventoryGrid3?.ClearColors();

            var largeInventoryGrid0 = _gameUi.GetLargeInventoryGrid(0);
            var largeInventoryGrid1 = _gameUi.GetLargeInventoryGrid(1);
            var largeInventoryGrid2 = _gameUi.GetLargeInventoryGrid(2);
            var largeInventoryGrid3 = _gameUi.GetLargeInventoryGrid(3);
            largeInventoryGrid0?.ClearColors();
            largeInventoryGrid1?.ClearColors();
            largeInventoryGrid2?.ClearColors();
            largeInventoryGrid3?.ClearColors();

            if (_currentRetainerId != 0)
            {
                var retainerGrid0 = _gameUi.GetRetainerGrid(0);
                var retainerGrid1 = _gameUi.GetRetainerGrid(1);
                var retainerGrid2 = _gameUi.GetRetainerGrid(2);
                var retainerGrid3 = _gameUi.GetRetainerGrid(3);
                var retainerGrid4 = _gameUi.GetRetainerGrid(4);
                var retainerTabGrid = _gameUi.GetLargeRetainerInventoryGrid();
                retainerGrid0?.ClearColors();
                retainerGrid1?.ClearColors();
                retainerGrid2?.ClearColors();
                retainerGrid3?.ClearColors();
                retainerGrid4?.ClearColors();
                retainerTabGrid?.ClearColors();

                var retainerInventoryGrid0 = _gameUi.GetNormalRetainerInventoryGrid(0);
                var retainerInventoryGrid1 = _gameUi.GetNormalRetainerInventoryGrid(1);
                var retainerInventoryGrid2 = _gameUi.GetNormalRetainerInventoryGrid(2);
                var retainerInventoryGrid3 = _gameUi.GetNormalRetainerInventoryGrid(3);
                var retainerInventoryGrid4 = _gameUi.GetNormalRetainerInventoryGrid(4);
                retainerInventoryGrid0?.ClearColors();
                retainerInventoryGrid1?.ClearColors();
                retainerInventoryGrid2?.ClearColors();
                retainerInventoryGrid3?.ClearColors();
                retainerInventoryGrid4?.ClearColors();

            }

            var saddleBag = _gameUi.GetChocoboSaddlebag();
            saddleBag?.ClearColors();
            var saddleBag2 = _gameUi.GetChocoboSaddlebag2();
            saddleBag2?.ClearColors();

            var retainerList = _gameUi.GetRetainerList();
            retainerList?.ClearColors();
        }
        private void ToggleHighlights()
        {
            var activeFilter = GetActiveFilter();
            FilterTable activeTable = null;
            bool shouldHighlight = false;
            var invertHighlighting = false;
            var invertTabHighlighting = false;
            //Add in ability to turn off highlights
            if (activeFilter != null)
            {
                if (_config.IsVisible)
                {
                    activeTable = GetFilterTable(activeFilter.Key);
                    //Allow table to override highlight mode on filter
                    if (activeTable.HighlightItems)
                    {
                        shouldHighlight = activeTable.HighlightItems;
                        if (activeFilter.HighlightWhen is "When Searching" || activeFilter.HighlightWhen == null && _config.HighlightWhen == "When Searching")
                        {
                            if (!activeTable.IsSearching)
                            {
                                shouldHighlight = false;
                            }
                        }
                    }
                }
                else
                {
                    shouldHighlight = true;
                }
                invertHighlighting = activeFilter.InvertHighlighting ?? _config.InvertHighlighting;
                invertTabHighlighting = activeFilter.InvertTabHighlighting ?? _config.InvertTabHighlighting;
            }
            
            FilterResult? filteredList = null;
            if (activeTable != null && activeTable.Items != null)
            {
                filteredList = new FilterResult(activeTable.Items.ToList(), new List<InventoryItem>());
            }
            else if (activeFilter != null && activeFilter.FilterResult.HasValue)
            {
                filteredList = activeFilter.FilterResult.Value;
            }

            HashSet<int> grid0Highlights = new(); 
            HashSet<int> grid1Highlights = new(); 
            HashSet<int> grid2Highlights = new(); 
            HashSet<int> grid3Highlights = new(); 
            HashSet<int> tab2Highlights = new();
            HashSet<int> tab4Highlights = new();
            HashSet<int> saddleBag0Highlights = new();
            HashSet<int> saddleBag1Highlights = new();
            HashSet<int> pSaddleBag0Highlights = new();
            HashSet<int> pSaddleBag1Highlights = new();
            HashSet<int> saddleBagTabHighlights = new();
            
            var openAllGrid0 = _gameUi.GetPrimaryInventoryGrid(0);
            var openAllGrid1 = _gameUi.GetPrimaryInventoryGrid(1);
            var openAllGrid2 = _gameUi.GetPrimaryInventoryGrid(2);
            var openAllGrid3 = _gameUi.GetPrimaryInventoryGrid(3);
            var normalInventoryGrid0 = _gameUi.GetNormalInventoryGrid(0);
            var normalInventoryGrid1 = _gameUi.GetNormalInventoryGrid(1);
            var normalInventoryGrid2 = _gameUi.GetNormalInventoryGrid(2);
            var normalInventoryGrid3 = _gameUi.GetNormalInventoryGrid(3);
            var expandedInventoryGrid0 = _gameUi.GetLargeInventoryGrid(0);
            var expandedInventoryGrid1 = _gameUi.GetLargeInventoryGrid(1);
            var expandedInventoryGrid2 = _gameUi.GetLargeInventoryGrid(2);
            var expandedInventoryGrid3 = _gameUi.GetLargeInventoryGrid(3);
            var saddleBagUi = _gameUi.GetChocoboSaddlebag() ?? _gameUi.GetChocoboSaddlebag2();
            
            openAllGrid0?.ClearColors();
            openAllGrid1?.ClearColors();
            openAllGrid2?.ClearColors();
            openAllGrid3?.ClearColors();
            normalInventoryGrid0?.ClearColors();
            normalInventoryGrid1?.ClearColors();
            normalInventoryGrid2?.ClearColors();
            normalInventoryGrid3?.ClearColors();
            expandedInventoryGrid0?.ClearColors();
            expandedInventoryGrid1?.ClearColors();
            expandedInventoryGrid2?.ClearColors();
            expandedInventoryGrid3?.ClearColors();
            saddleBagUi?.ClearColors();
            
            //If invert highlighting is off then we need to make sure the empty items aren't used


            if (shouldHighlight && filteredList != null)
            {
                for (var index = 0; index < filteredList.Value.SortedItems.Count; index++)
                {
                    var item = filteredList.Value.SortedItems[index];
                    if (MatchesFilter(activeFilter, item, invertHighlighting))
                    {
                        if (item.SourceBag == InventoryType.Bag0)
                        {
                            if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                            {
                                tab4Highlights.Add(0);
                                tab2Highlights.Add(0);
                            }

                            if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                            {
                                grid0Highlights.Add(item.InventoryItem.SortedSlotIndex);
                            }

                        }

                        else if (item.SourceBag == InventoryType.Bag1)
                        {
                            if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                            {
                                tab4Highlights.Add(1);
                                tab2Highlights.Add(0);
                            }

                            if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                            {
                                grid1Highlights.Add(item.InventoryItem.SortedSlotIndex);
                            }
                        }

                        else if (item.SourceBag == InventoryType.Bag2)
                        {
                            if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                            {
                                tab4Highlights.Add(2);
                                tab2Highlights.Add(1);
                            }

                            if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                            {
                                grid2Highlights.Add(item.InventoryItem.SortedSlotIndex);
                            }
                        }

                        else if (item.SourceBag == InventoryType.Bag3)
                        {
                            if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                            {
                                tab4Highlights.Add(3);
                                tab2Highlights.Add(1);
                            }

                            if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                            {
                                grid3Highlights.Add(item.InventoryItem.SortedSlotIndex);
                            }
                        }
                        
                        else if (item.SourceBag == InventoryType.SaddleBag0)
                        {
                            if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                            {
                                saddleBagTabHighlights.Add(0);
                            }

                            if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                            {
                                saddleBag0Highlights.Add(item.InventoryItem.SortedSlotIndex);
                            }
                            
                        }
                        
                        else if (item.SourceBag == InventoryType.SaddleBag1)
                        {
                            if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                            {
                                saddleBagTabHighlights.Add(0);
                            }

                            if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                            {
                                saddleBag1Highlights.Add(item.InventoryItem.SortedSlotIndex);
                            }
                        }
                        
                        else if (item.SourceBag == InventoryType.PremiumSaddleBag0)
                        {
                            if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                            {
                                saddleBagTabHighlights.Add(1);
                            }

                            if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                            {
                                pSaddleBag0Highlights.Add(item.InventoryItem.SortedSlotIndex);
                            }

                        }
                        
                        else if (item.SourceBag == InventoryType.PremiumSaddleBag1)
                        {
                            if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                            {
                                saddleBagTabHighlights.Add(1);
                            }

                            if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                            {
                                pSaddleBag1Highlights.Add(item.InventoryItem.SortedSlotIndex);
                            }
                        }

                    }
                }
                
                openAllGrid0?.SetColors(grid0Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                openAllGrid1?.SetColors(grid1Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                openAllGrid2?.SetColors(grid2Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                openAllGrid3?.SetColors(grid3Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                normalInventoryGrid0?.SetColors(grid0Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                normalInventoryGrid1?.SetColors(grid1Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                normalInventoryGrid2?.SetColors(grid2Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                normalInventoryGrid3?.SetColors(grid3Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                normalInventoryGrid0?.SetTabColors(tab4Highlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                normalInventoryGrid1?.SetTabColors(tab4Highlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                normalInventoryGrid2?.SetTabColors(tab4Highlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                normalInventoryGrid3?.SetTabColors(tab4Highlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                expandedInventoryGrid0?.SetColors(grid0Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                expandedInventoryGrid1?.SetColors(grid1Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                expandedInventoryGrid2?.SetColors(grid2Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                expandedInventoryGrid3?.SetColors(grid3Highlights, activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                expandedInventoryGrid0?.SetTabColors(tab2Highlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                expandedInventoryGrid1?.SetTabColors(tab2Highlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                expandedInventoryGrid2?.SetTabColors(tab2Highlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                expandedInventoryGrid3?.SetTabColors(tab2Highlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                if (saddleBagUi != null)
                {
                    if (saddleBagUi.SaddleBagSelected == 0)
                    {
                        saddleBagUi.SetItemLeftColors(saddleBag0Highlights,
                            activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                        saddleBagUi.SetItemRightColors(saddleBag1Highlights,
                            activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                        saddleBagUi.SetTabColors(saddleBagTabHighlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    }
                    else
                    {
                        saddleBagUi.SetItemLeftColors(pSaddleBag0Highlights,
                            activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                        saddleBagUi.SetItemRightColors(pSaddleBag1Highlights,
                            activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                        saddleBagUi.SetTabColors(saddleBagTabHighlights, activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    }
                }
            }


            if (_currentRetainerId != 0)
            {
                var retainerExpandedGrid0 = _gameUi.GetRetainerGrid(0);
                var retainerExpandedGrid1 = _gameUi.GetRetainerGrid(1);
                var retainerExpandedGrid2 = _gameUi.GetRetainerGrid(2);
                var retainerExpandedGrid3 = _gameUi.GetRetainerGrid(3);
                var retainerExpandedGrid4 = _gameUi.GetRetainerGrid(4);
                var retainerExpandedTabs = _gameUi.GetLargeRetainerInventoryGrid();
                
                var retainerNormalGrid0 = _gameUi.GetNormalRetainerInventoryGrid(0);
                var retainerNormalGrid1 = _gameUi.GetNormalRetainerInventoryGrid(1);
                var retainerNormalGrid2 = _gameUi.GetNormalRetainerInventoryGrid(2);
                var retainerNormalGrid3 = _gameUi.GetNormalRetainerInventoryGrid(3);
                var retainerNormalGrid4 = _gameUi.GetNormalRetainerInventoryGrid(4);

                retainerExpandedGrid0?.ClearColors();
                retainerExpandedGrid1?.ClearColors();
                retainerExpandedGrid2?.ClearColors();
                retainerExpandedGrid3?.ClearColors();
                retainerExpandedGrid4?.ClearColors();
                retainerExpandedTabs?.ClearColors();
                retainerNormalGrid0?.ClearColors();
                retainerNormalGrid1?.ClearColors();
                retainerNormalGrid2?.ClearColors();
                retainerNormalGrid3?.ClearColors();
                retainerNormalGrid4?.ClearColors();
                if (shouldHighlight && filteredList != null)
                {
                    HashSet<int> retainerGrid0Highlights = new();
                    HashSet<int> retainerGrid1Highlights = new();
                    HashSet<int> retainerGrid2Highlights = new();
                    HashSet<int> retainerGrid3Highlights = new();
                    HashSet<int> retainerGrid4Highlights = new();
                    HashSet<int> retainerTab2Highlights = new();
                    HashSet<int> retainerTab5Highlights = new();
                    for (var index = 0; index < filteredList.Value.SortedItems.Count; index++)
                    {
                        var item = filteredList.Value.SortedItems[index];
                        if (MatchesRetainerFilter(activeFilter, item, invertHighlighting))
                        {
                            if (item.SourceBag == InventoryType.RetainerBag0)
                            {
                                if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                                {
                                    retainerTab2Highlights.Add(0);
                                    retainerTab5Highlights.Add(0);
                                }
                                if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                                {
                                    retainerGrid0Highlights.Add(item.InventoryItem.SortedSlotIndex);
                                }
                            }

                            if (item.SourceBag == InventoryType.RetainerBag1)
                            {
                                if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                                {
                                    retainerTab2Highlights.Add(0);
                                    retainerTab5Highlights.Add(1);
                                }
                                if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                                {
                                    retainerGrid1Highlights.Add(item.InventoryItem.SortedSlotIndex);
                                }
                            }

                            if (item.SourceBag == InventoryType.RetainerBag2)
                            {
                                if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                                {
                                    retainerTab2Highlights.Add(1);
                                    retainerTab5Highlights.Add(2);
                                }
                                if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                                {
                                    retainerGrid2Highlights.Add(item.InventoryItem.SortedSlotIndex);
                                }
                            }

                            if (item.SourceBag == InventoryType.RetainerBag3)
                            {
                                if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                                {
                                    retainerTab2Highlights.Add(1);
                                    retainerTab5Highlights.Add(3);
                                }
                                if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                                {
                                    retainerGrid3Highlights.Add(item.InventoryItem.SortedSlotIndex);
                                }
                            }

                            if (item.SourceBag == InventoryType.RetainerBag4)
                            {
                                if (!invertTabHighlighting && !item.InventoryItem.IsEmpty || invertTabHighlighting)
                                {
                                    retainerTab2Highlights.Add(2);
                                    retainerTab5Highlights.Add(4);
                                }
                                if (!invertHighlighting && !item.InventoryItem.IsEmpty || invertHighlighting)
                                {
                                    retainerGrid4Highlights.Add(item.InventoryItem.SortedSlotIndex);
                                }
                            }
                        }
                    }
                    
                    /***
                     * Retainer Interface
                     * Normal = retainerInventoryGrid
                     * Expanded = retainerGrid + retainerTabGrid
                     */

                    retainerExpandedGrid0?.SetColors(retainerGrid0Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerExpandedGrid1?.SetColors(retainerGrid1Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerExpandedGrid2?.SetColors(retainerGrid2Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerExpandedGrid3?.SetColors(retainerGrid3Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerExpandedGrid4?.SetColors(retainerGrid4Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerExpandedGrid0?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    retainerExpandedGrid1?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    retainerExpandedGrid2?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    retainerExpandedGrid3?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    retainerExpandedGrid4?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    
                    retainerExpandedTabs?.SetTabColors(retainerTab2Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);

                    retainerNormalGrid0?.SetColors(retainerGrid0Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerNormalGrid1?.SetColors(retainerGrid1Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerNormalGrid2?.SetColors(retainerGrid2Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerNormalGrid3?.SetColors(retainerGrid3Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerNormalGrid4?.SetColors(retainerGrid4Highlights,
                        activeFilter.HighlightColor ?? _config.HighlightColor, invertHighlighting);
                    retainerNormalGrid0?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    retainerNormalGrid1?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    retainerNormalGrid2?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    retainerNormalGrid3?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    retainerNormalGrid4?.SetTabColors(retainerTab5Highlights,
                        activeFilter.TabHighlightColor ?? _config.TabHighlightColor, invertTabHighlighting);
                    


                }
            }

            var retainerList = _gameUi.GetRetainerList();
            var currentCharacterId = _clientState.LocalContentId;
            if (retainerList != null)
            {
                retainerList.ClearColors();
                if (activeFilter != null)
                {
                    for (var index = 0; index < retainerList._sortedItems.Count; index++)
                    {
                        var listRetainer = retainerList._sortedItems[index];
                        var retainer =
                            _characterMonitor.GetCharacterByName(listRetainer.RetainerName, currentCharacterId);
                        if (retainer != null && filteredList != null)
                        {
                            if (activeFilter.FilterType == FilterType.SortingFilter)
                            {
                                var count = filteredList.Value.SortedItems.Count(c =>
                                    c.DestinationRetainerId == retainer.CharacterId &&
                                    c.SourceRetainerId == currentCharacterId);
                                if (count != 0)
                                {
                                    retainerList.SetTextAndColor(retainer.Name, retainer.Name + "(" + count + ")",
                                        "00FF00");
                                }
                            }
                            else
                            {
                                var count = filteredList.Value.SortedItems.Count(c =>
                                    c.SourceRetainerId == retainer.CharacterId);
                                if (count != 0)
                                {
                                    retainerList.SetTextAndColor(retainer.Name, retainer.Name + "(" + count + ")",
                                        "00FF00");
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool MatchesFilter(FilterConfiguration activeFilter, SortingResult item, bool invertHighlighting = false)
        {
            bool matches = activeFilter.FilterType == FilterType.SearchFilter &&
                           item.SourceRetainerId == _clientState.LocalContentId;
            
            if (item.SourceRetainerId == _clientState.LocalContentId && (_currentRetainerId == 0 ||
                                                                         _currentRetainerId != 0 &&
                                                                         item.DestinationRetainerId ==
                                                                         _currentRetainerId))
            {
                matches = true;
            }


            if (matches)
            {
                if (!item.InventoryItem.IsEmpty)
                {
                    return true;
                }
            }

            if (item.InventoryItem.IsEmpty && invertHighlighting)
            {
                return false;
            }

            return false;
        }

        private bool MatchesRetainerFilter(FilterConfiguration activeFilter, SortingResult item, bool invertHighlighting = false)
        {
            bool matches = activeFilter.FilterType == FilterType.SearchFilter && item.SourceRetainerId == _currentRetainerId;


            if (matches)
            {
                if (!item.InventoryItem.IsEmpty)
                {
                    return true;
                }
            }

            if (item.InventoryItem.IsEmpty && invertHighlighting)
            {
                return true;
            }

            return false;
        }


        private void OnItemTooltip(ItemTooltip tooltip, ulong itemId)
        {
            if (!tooltip.Fields.HasFlag(ItemTooltipFields.Description))
            {
                return;
            }

            if (itemId > 2000000 || itemId == 0)
            {
                return;
            }

            if (itemId > 1000000)
            {
                itemId -= 1000000;
            }

            var item = ExcelCache.GetItem((uint)itemId);
            if (item == null)
            {
                return;
            }

            var description = tooltip[ItemTooltipString.Description];
            const string indentation = "      ";

            if (_config.DisplayTooltip)
            {
                description += "\n\n";
                description += "[InventoryTools]\n";

                {
                    var ownedItems = InventoryMonitor.AllItems.Where(item => item.ItemId == itemId).ToList();
                    uint storageCount = 0;
                    List<string> locations = new List<string>();
                    foreach (var oItem in ownedItems)
                    {
                        storageCount += oItem.Quantity;
                        var name = CharacterMonitor.Characters[oItem.RetainerId]?.Name ?? "Unknown";
                        name = name.Trim().Length == 0 ? "Unknown" : name.Trim();

                        locations.Add($"{name} - {oItem.FormattedBagLocation}");
                    }


                    if (storageCount > 0)
                    {
                        description += $"Owned: {storageCount}\n";
                        description += $"Locations:\n";
                        foreach (var location in locations)
                        {
                            description += $"{indentation}{location}\n";
                        }
                    }
                }

                {
                    if (!ExcelCache.GetItem((uint)itemId).IsUntradable)
                    {
                        var marketData = Cache.GetData((uint)itemId, false);
                        if (marketData != null)
                        {
                            description += "Market Board Data:\n";

                            // no \t support?!
                            if (marketData.calculcatedPrice != null)
                            {
                                description += $"{indentation}Average Price: {marketData.calculcatedPrice}\n";
                            }

                            if (marketData.calculcatedPriceHQ != null)
                            {
                                description += $"{indentation}Average Price (HQ): {marketData.calculcatedPriceHQ}\n";
                            }

#if false // not really needed
                    description += $"{indentation}Max Price:              {marketData.maxPrice}\n";
                    description += $"{indentation}Average:                 {marketData.averagePrice}\n";
                    description += $"{indentation}Current Average:  {marketData.currentAveragePrice}\n";
                    description += $"{indentation}Min Price:               {marketData.minPrice}\n";
#endif
                        }
                    }
                }
            }

            tooltip[ItemTooltipString.Description] = description;
        }


        public void Dispose()
        {
            foreach (var filterTables in _filterTables)
            {
                filterTables.Value.Dispose();
            }

            UnwatchFilterChanges();

            DisableHighlights();
            _config.FilterConfigurations = FilterConfigurations;
            _config.SavedCharacters = _characterMonitor.Characters;
            _framework.Update -= FrameworkOnUpdate;
            _inventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
            _characterMonitor.OnActiveRetainerChanged -= CharacterMonitorOnOnActiveCharacterChanged;
            _characterMonitor.OnCharacterUpdated -= CharacterMonitorOnOnCharacterUpdated;
            _config.ConfigurationChanged -= ConfigOnConfigurationChanged;
            _gameUi.UiVisibilityChanged -= GameUiOnUiVisibilityChanged;

            _commonBase.Functions.Tooltips.OnItemTooltip -= this.OnItemTooltip;
            _commonBase.Dispose();
        }
    }
}