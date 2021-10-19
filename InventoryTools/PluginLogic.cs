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

            WatchFilterChanges();
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
            foreach(var filterConfiguration in _filterConfigurations)
            {
                filterConfiguration.ConfigurationChanged += FilterConfigurationOnConfigurationChanged;
            }
        }
        
        private void UnwatchFilterChanges()
        {
            foreach(var filterConfiguration in _filterConfigurations)
            {
                filterConfiguration.ConfigurationChanged -= FilterConfigurationOnConfigurationChanged;
            }
        }

        private void FilterConfigurationOnConfigurationChanged(FilterConfiguration filterconfiguration)
        {
            //Do some sort of debouncing
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
                    }
                }
                else
                {
                    shouldHighlight = true;
                }
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
            var inventoryGrid0 = _gameUi.GetPrimaryInventoryGrid(0);
            var inventoryGrid1 = _gameUi.GetPrimaryInventoryGrid(1);
            var inventoryGrid2 = _gameUi.GetPrimaryInventoryGrid(2);
            var inventoryGrid3 = _gameUi.GetPrimaryInventoryGrid(3);
            if (inventoryGrid0 != null && inventoryGrid1 != null && inventoryGrid2 != null &&
                inventoryGrid3 != null)
            {
                inventoryGrid0.ClearColors();
                inventoryGrid1.ClearColors();
                inventoryGrid2.ClearColors();
                inventoryGrid3.ClearColors();
                if (shouldHighlight && filteredList != null)
                {
                    for (var index = 0; index < filteredList.Value.SortedItems.Count; index++)
                    {
                        var item = filteredList.Value.SortedItems[index];
                        if (activeFilter.FilterType == FilterType.SearchFilter && item.SourceRetainerId == _clientState.LocalContentId || item.SourceRetainerId == _clientState.LocalContentId && (_currentRetainerId == 0 ||
                            (_currentRetainerId != 0 &&
                             item.DestinationRetainerId ==
                             _currentRetainerId)))
                        {
                            if (item.SourceBag == InventoryType.Bag0)
                            {
                                inventoryGrid0.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                            }

                            if (item.SourceBag == InventoryType.Bag1)
                            {
                                inventoryGrid1.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                            }

                            if (item.SourceBag == InventoryType.Bag2)
                            {
                                inventoryGrid2.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                            }

                            if (item.SourceBag == InventoryType.Bag3)
                            {
                                inventoryGrid3.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                            }
                        }
                    }
                }
            }
            var smallInventoryGrid0 = _gameUi.GetNormalInventoryGrid(0);
            var smallInventoryGrid1 = _gameUi.GetNormalInventoryGrid(1);
            var smallInventoryGrid2 = _gameUi.GetNormalInventoryGrid(2);
            var smallInventoryGrid3 = _gameUi.GetNormalInventoryGrid(3);
            if (smallInventoryGrid0 != null || smallInventoryGrid1 != null || smallInventoryGrid2 != null ||
                smallInventoryGrid3 != null)
            {
                smallInventoryGrid0?.ClearColors();
                smallInventoryGrid1?.ClearColors();
                smallInventoryGrid2?.ClearColors();
                smallInventoryGrid3?.ClearColors();
                if (shouldHighlight && filteredList != null)
                {
                    for (var index = 0; index < filteredList.Value.SortedItems.Count; index++)
                    {
                        var item = filteredList.Value.SortedItems[index];
                        if (activeFilter.FilterType == FilterType.SearchFilter && item.SourceRetainerId == _clientState.LocalContentId || item.SourceRetainerId == _clientState.LocalContentId && (_currentRetainerId == 0 ||
                            (_currentRetainerId != 0 &&
                             item.DestinationRetainerId ==
                             _currentRetainerId)))
                        {
                            if (item.SourceBag == InventoryType.Bag0)
                            {
                                smallInventoryGrid0?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                smallInventoryGrid0?.SetTabColor(0, _config.HighlightColor);
                                smallInventoryGrid1?.SetTabColor(0, _config.HighlightColor);
                                smallInventoryGrid2?.SetTabColor(0, _config.HighlightColor);
                                smallInventoryGrid3?.SetTabColor(0, _config.HighlightColor);
                            }

                            if (item.SourceBag == InventoryType.Bag1)
                            {
                                smallInventoryGrid1?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                smallInventoryGrid0?.SetTabColor(1, _config.HighlightColor);
                                smallInventoryGrid1?.SetTabColor(1, _config.HighlightColor);
                                smallInventoryGrid2?.SetTabColor(1, _config.HighlightColor);
                                smallInventoryGrid3?.SetTabColor(1, _config.HighlightColor);
                            }

                            if (item.SourceBag == InventoryType.Bag2)
                            {
                                smallInventoryGrid2?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                smallInventoryGrid0?.SetTabColor(2, _config.HighlightColor);
                                smallInventoryGrid1?.SetTabColor(2, _config.HighlightColor);
                                smallInventoryGrid2?.SetTabColor(2, _config.HighlightColor);
                                smallInventoryGrid3?.SetTabColor(2, _config.HighlightColor);
                            }

                            if (item.SourceBag == InventoryType.Bag3)
                            {
                                smallInventoryGrid3?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                smallInventoryGrid0?.SetTabColor(3, _config.HighlightColor);
                                smallInventoryGrid1?.SetTabColor(3, _config.HighlightColor);
                                smallInventoryGrid2?.SetTabColor(3, _config.HighlightColor);
                                smallInventoryGrid3?.SetTabColor(3, _config.HighlightColor);
                            }
                        }
                    }
                }
            }
            
            var largeInventoryGrid0 = _gameUi.GetLargeInventoryGrid(0);
            var largeInventoryGrid1 = _gameUi.GetLargeInventoryGrid(1);
            var largeInventoryGrid2 = _gameUi.GetLargeInventoryGrid(2);
            var largeInventoryGrid3 = _gameUi.GetLargeInventoryGrid(3);
            if (largeInventoryGrid0 != null || largeInventoryGrid1 != null || largeInventoryGrid2 != null ||
                largeInventoryGrid3 != null)
            {
                largeInventoryGrid0?.ClearColors();
                largeInventoryGrid1?.ClearColors();
                largeInventoryGrid2?.ClearColors();
                largeInventoryGrid3?.ClearColors();
                if (shouldHighlight && filteredList != null)
                {
                    for (var index = 0; index < filteredList.Value.SortedItems.Count; index++)
                    {
                        var item = filteredList.Value.SortedItems[index];
                        if (activeFilter.FilterType == FilterType.SearchFilter && item.SourceRetainerId == _clientState.LocalContentId || item.SourceRetainerId == _clientState.LocalContentId && (_currentRetainerId == 0 ||
                            (_currentRetainerId != 0 &&
                             item.DestinationRetainerId ==
                             _currentRetainerId)))
                        {
                            if (item.SourceBag == InventoryType.Bag0)
                            {
                                largeInventoryGrid0?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                largeInventoryGrid0?.SetTabColor(0, _config.HighlightColor);
                                largeInventoryGrid1?.SetTabColor(0, _config.HighlightColor);
                                largeInventoryGrid2?.SetTabColor(0, _config.HighlightColor);
                                largeInventoryGrid3?.SetTabColor(0, _config.HighlightColor);
                            }
                            if (item.SourceBag == InventoryType.Bag1)
                            {
                                largeInventoryGrid1?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                largeInventoryGrid0?.SetTabColor(0, _config.HighlightColor);
                                largeInventoryGrid1?.SetTabColor(0, _config.HighlightColor);
                                largeInventoryGrid2?.SetTabColor(0, _config.HighlightColor);
                                largeInventoryGrid3?.SetTabColor(0, _config.HighlightColor);
                            }
                            if (item.SourceBag == InventoryType.Bag2)
                            {
                                largeInventoryGrid2?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                largeInventoryGrid0?.SetTabColor(1, _config.HighlightColor);
                                largeInventoryGrid1?.SetTabColor(1, _config.HighlightColor);
                                largeInventoryGrid2?.SetTabColor(1, _config.HighlightColor);
                                largeInventoryGrid3?.SetTabColor(1, _config.HighlightColor);
                            }
                            if (item.SourceBag == InventoryType.Bag3)
                            {
                                largeInventoryGrid3?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                largeInventoryGrid0?.SetTabColor(1, _config.HighlightColor);
                                largeInventoryGrid1?.SetTabColor(1, _config.HighlightColor);
                                largeInventoryGrid2?.SetTabColor(1, _config.HighlightColor);
                                largeInventoryGrid3?.SetTabColor(1, _config.HighlightColor);
                            }
                        }
                    }
                }
            }
            if (_currentRetainerId != 0)
            {
                var retainerGrid0 = _gameUi.GetRetainerGrid(0);
                var retainerGrid1 = _gameUi.GetRetainerGrid(1);
                var retainerGrid2 = _gameUi.GetRetainerGrid(2);
                var retainerGrid3 = _gameUi.GetRetainerGrid(3);
                var retainerGrid4 = _gameUi.GetRetainerGrid(4);
                var retainerTabGrid = _gameUi.GetLargeRetainerInventoryGrid();
                if (retainerGrid0 != null && retainerGrid1 != null && retainerGrid2 != null &&
                    retainerGrid3 != null && retainerGrid4 != null && retainerTabGrid != null)
                {
                    retainerGrid0.ClearColors();
                    retainerGrid1.ClearColors();
                    retainerGrid2.ClearColors();
                    retainerGrid3.ClearColors();
                    retainerGrid4.ClearColors();
                    retainerTabGrid.ClearColors();
                    if (shouldHighlight && filteredList != null)
                    {
                        for (var index = 0; index < filteredList.Value.SortedItems.Count; index++)
                        {
                            var item = filteredList.Value.SortedItems[index];
                            if (item.SourceRetainerId == _currentRetainerId)
                            {
                                if (item.SourceBag == InventoryType.RetainerBag0)
                                {
                                    retainerGrid0.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerTabGrid.SetTabColor(0, _config.HighlightColor);
                                }

                                if (item.SourceBag == InventoryType.RetainerBag1)
                                {
                                    retainerGrid1.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerTabGrid.SetTabColor(0, _config.HighlightColor);
                                }

                                if (item.SourceBag == InventoryType.RetainerBag2)
                                {
                                    retainerGrid2.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerTabGrid.SetTabColor(1, _config.HighlightColor);
                                }

                                if (item.SourceBag == InventoryType.RetainerBag3)
                                {
                                    retainerGrid3.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerTabGrid.SetTabColor(1, _config.HighlightColor);
                                }

                                if (item.SourceBag == InventoryType.RetainerBag4)
                                {
                                    retainerGrid4.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerTabGrid.SetTabColor(2, _config.HighlightColor);
                                }
                            }
                        }
                    }
                }
                
                
                var retainerInventoryGrid0 = _gameUi.GetNormalRetainerInventoryGrid(0);
                var retainerInventoryGrid1 = _gameUi.GetNormalRetainerInventoryGrid(1);
                var retainerInventoryGrid2 = _gameUi.GetNormalRetainerInventoryGrid(2);
                var retainerInventoryGrid3 = _gameUi.GetNormalRetainerInventoryGrid(3);
                var retainerInventoryGrid4 = _gameUi.GetNormalRetainerInventoryGrid(4);
                if (retainerInventoryGrid0 != null || retainerInventoryGrid1 != null || retainerInventoryGrid2 != null ||
                    retainerInventoryGrid3 != null ||
                    retainerInventoryGrid4 != null)
                {
                    PluginLog.Log("normal inventory grid changed");
                    retainerInventoryGrid0?.ClearColors();
                    retainerInventoryGrid1?.ClearColors();
                    retainerInventoryGrid2?.ClearColors();
                    retainerInventoryGrid3?.ClearColors();
                    retainerInventoryGrid4?.ClearColors();
                    if (shouldHighlight && filteredList != null)
                    {
                        for (var index = 0; index < filteredList.Value.SortedItems.Count; index++)
                        {
                            var item = filteredList.Value.SortedItems[index];
                            if (item.SourceRetainerId == _currentRetainerId)
                            {
                                if (item.SourceBag == InventoryType.RetainerBag0)
                                {
                                    retainerInventoryGrid0?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerInventoryGrid0?.SetTabColor(0, _config.HighlightColor);
                                    retainerInventoryGrid1?.SetTabColor(0, _config.HighlightColor);
                                    retainerInventoryGrid2?.SetTabColor(0, _config.HighlightColor);
                                    retainerInventoryGrid3?.SetTabColor(0, _config.HighlightColor);
                                    retainerInventoryGrid4?.SetTabColor(0, _config.HighlightColor);
                                }
                                if (item.SourceBag == InventoryType.RetainerBag1)
                                {
                                    retainerInventoryGrid1?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerInventoryGrid0?.SetTabColor(1, _config.HighlightColor);
                                    retainerInventoryGrid1?.SetTabColor(1, _config.HighlightColor);
                                    retainerInventoryGrid2?.SetTabColor(1, _config.HighlightColor);
                                    retainerInventoryGrid3?.SetTabColor(1, _config.HighlightColor);
                                    retainerInventoryGrid4?.SetTabColor(1, _config.HighlightColor);
                                }

                                if (item.SourceBag == InventoryType.RetainerBag2)
                                {
                                    retainerInventoryGrid2?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerInventoryGrid0?.SetTabColor(2, _config.HighlightColor);
                                    retainerInventoryGrid1?.SetTabColor(2, _config.HighlightColor);
                                    retainerInventoryGrid2?.SetTabColor(2, _config.HighlightColor);
                                    retainerInventoryGrid3?.SetTabColor(2, _config.HighlightColor);
                                    retainerInventoryGrid4?.SetTabColor(2, _config.HighlightColor);
                                }

                                if (item.SourceBag == InventoryType.RetainerBag3)
                                {
                                    retainerInventoryGrid3?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerInventoryGrid0?.SetTabColor(3, _config.HighlightColor);
                                    retainerInventoryGrid1?.SetTabColor(3, _config.HighlightColor);
                                    retainerInventoryGrid2?.SetTabColor(3, _config.HighlightColor);
                                    retainerInventoryGrid3?.SetTabColor(3, _config.HighlightColor);
                                    retainerInventoryGrid4?.SetTabColor(3, _config.HighlightColor);
                                }

                                if (item.SourceBag == InventoryType.RetainerBag4)
                                {
                                    retainerInventoryGrid4?.SetColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                    retainerInventoryGrid0?.SetTabColor(4, _config.HighlightColor);
                                    retainerInventoryGrid1?.SetTabColor(4, _config.HighlightColor);
                                    retainerInventoryGrid2?.SetTabColor(4, _config.HighlightColor);
                                    retainerInventoryGrid3?.SetTabColor(4, _config.HighlightColor);
                                    retainerInventoryGrid4?.SetTabColor(4, _config.HighlightColor);
                                }
                            }
                        }
                    }
                }
            }
            
            
            var saddleBagUi = _gameUi.GetChocoboSaddlebag() ?? _gameUi.GetChocoboSaddlebag2();
            if (saddleBagUi != null)
            {
                saddleBagUi.ClearColors();
                if (shouldHighlight && filteredList != null)
                {
                    for (var index = 0; index < filteredList.Value.SortedItems.Count; index++)
                    {
                        var item = filteredList.Value.SortedItems[index];
                        if (activeFilter.FilterType == FilterType.SearchFilter && item.SourceRetainerId == _clientState.LocalContentId || item.SourceRetainerId == _clientState.LocalContentId && (_currentRetainerId == 0 ||
                            (_currentRetainerId != 0 &&
                             item.DestinationRetainerId ==
                             _currentRetainerId)))
                        {
                            if (item.SourceBag == InventoryType.SaddleBag0 ||
                                item.SourceBag == InventoryType.SaddleBag1)
                            {
                                saddleBagUi.SetLeftTabColor(_config.HighlightColor);
                            }
                            else if (item.SourceBag == InventoryType.PremiumSaddleBag0 || item.SourceBag == InventoryType.PremiumSaddleBag1)
                            {
                                saddleBagUi.SetRightTabColor(_config.HighlightColor);
                            }

                            if (saddleBagUi.SaddleBagSelected == 0)
                            {
                                if (item.SourceBag == InventoryType.SaddleBag0)
                                {
                                    saddleBagUi.SetItemLeftColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                }

                                if (item.SourceBag == InventoryType.SaddleBag1)
                                {
                                    saddleBagUi.SetItemRightColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                }
                            }
                            else
                            {
                                if (item.SourceBag == InventoryType.PremiumSaddleBag0)
                                {
                                    saddleBagUi.SetItemLeftColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                }

                                if (item.SourceBag == InventoryType.PremiumSaddleBag1)
                                {
                                    saddleBagUi.SetItemRightColor(item.InventoryItem.SortedSlotIndex, _config.HighlightColor);
                                }
                            }
                        }
                    }
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
            
        }
    }
}