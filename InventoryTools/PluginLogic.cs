using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Extensions;
using InventoryTools.Images;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Filters;
using Lumina.Excel.GeneratedSheets;
using XivCommon;
using XivCommon.Functions.Tooltips;

namespace InventoryTools
{
    public partial class PluginLogic : IDisposable
    {
        private List<FilterConfiguration> _filterConfigurations = new();
        private Dictionary<string, FilterTable> _filterTables = new();
        private List<IFilter> _availableFilters = new();

        private Dictionary<int, InventoryMonitor.ItemChangesItem> _recentlyAddedSeen = new();

        public bool WasRecentlySeen(uint itemId)
        {
            if (_recentlyAddedSeen.ContainsKey((int) itemId))
            {
                return true;
            }
            return false;
        }

        public TimeSpan? GetLastSeenTime(uint itemId)
        {
            if (WasRecentlySeen(itemId))
            {
                return DateTime.Now - _recentlyAddedSeen[(int) itemId].Date;
            }
            return null;
        }
        public static InventoryToolsConfiguration PluginConfiguration => ConfigurationManager.Config;
        private ulong _currentRetainerId;
        private XivCommonBase CommonBase { get; set; }

        private DateTime? _nextSaveTime = null;

        private RightClickColumn _rightClickColumn = new();
        
        public readonly ConcurrentDictionary<ushort, TextureWrap> TextureDictionary = new ConcurrentDictionary<ushort, TextureWrap>();
        public readonly Dictionary<string, TextureWrap> UldTextureDictionary = new Dictionary<string, TextureWrap>();

        public PluginLogic()
        {
            
            AvailableFilters.Add(new CanBePurchasedFilter());
            AvailableFilters.Add(new CanCraftFilter());
            AvailableFilters.Add(new BuyFromVendorPriceFilter());
            AvailableFilters.Add(new SellToVendorPriceFilter());
            AvailableFilters.Add(new IsCollectibleFilter());
            AvailableFilters.Add(new IsHqFilter());
            AvailableFilters.Add(new ItemLevelFilter());
            AvailableFilters.Add(new ItemUiCategoryFilter());
            AvailableFilters.Add(new NameFilter());
            AvailableFilters.Add(new QuantityFilter());
            AvailableFilters.Add(new RequiredLevelFilter());
            AvailableFilters.Add(new SearchCategoryFilter());
            AvailableFilters.Add(new SpiritBondFilter());
            AvailableFilters.Add(new MarketBoardPriceFilter());
            AvailableFilters.Add(new MarketBoardTotalPriceFilter());
            AvailableFilters.Add(new AcquiredFilter());
            AvailableFilters.Add(new DuplicatesOnlyFilter());
            AvailableFilters.Add(new DisplayFilterInRetainersFilter());
            AvailableFilters.Add(new SourceAllCharactersFilter());
            AvailableFilters.Add(new SourceAllRetainersFilter());
            AvailableFilters.Add(new SourceInventoriesFilter());
            AvailableFilters.Add(new DestinationInventoriesFilter());
            AvailableFilters.Add(new DestinationAllCharactersFilter());
            AvailableFilters.Add(new DestinationAllRetainersFilter());
            AvailableFilters.Add(new IsTimedNodeFilter());
            AvailableFilters.Add(new ColumnsFilter());
            AvailableFilters.Add(new InvertHighlightingFilter());
            AvailableFilters.Add(new InvertTabHighlightingFilter());
            AvailableFilters.Add(new HighlightWhenFilter());
            AvailableFilters.Add(new HighlightColorFilter());
            AvailableFilters.Add(new TabHighlightColorFilter());
            AvailableFilters.Add(new RetainerListColorFilter());
            //Events we need to track, inventory updates, active retainer changes, player changes, 
            PluginService.InventoryMonitor.OnInventoryChanged += InventoryMonitorOnOnInventoryChanged;
            PluginService.CharacterMonitor.OnActiveRetainerChanged += CharacterMonitorOnOnActiveCharacterChanged;
            PluginService.CharacterMonitor.OnCharacterUpdated += CharacterMonitorOnOnCharacterUpdated;
            PluginConfiguration.ConfigurationChanged += ConfigOnConfigurationChanged;
            Service.Framework.Update += FrameworkOnUpdate;

            PluginService.InventoryMonitor.LoadExistingData(PluginConfiguration.GetSavedInventory());
            PluginService.CharacterMonitor.LoadExistingRetainers(PluginConfiguration.GetSavedRetainers());

            PluginService.GameUi.WatchWindowState(WindowName.RetainerGrid0);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryGrid0E);
            PluginService.GameUi.WatchWindowState(WindowName.RetainerList);
            PluginService.GameUi.WatchWindowState(WindowName.Inventory);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryLarge);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryRetainerLarge);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryRetainer);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryBuddy);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryBuddy2);
            PluginService.GameUi.UiVisibilityChanged += GameUiOnUiVisibilityChanged;
            
            GameInterface.AcquiredItemsUpdated += GameInterfaceOnAcquiredItemsUpdated;
            
            LoadExistingData(PluginConfiguration.GetSavedFilters());
            if (PluginConfiguration.FirstRun)
            {
                LoadDefaultData();
                PluginConfiguration.FirstRun = false;
            }
            RunMigrations();
            WatchFilterChanges();

            this.CommonBase = new XivCommonBase(Hooks.Tooltips);
            this.CommonBase.Functions.Tooltips.OnItemTooltip += this.OnItemTooltip;
        }

        private void GameInterfaceOnAcquiredItemsUpdated()
        {
            var activeCharacter = PluginService.CharacterMonitor.ActiveCharacter;
            if (activeCharacter != 0)
            {
                PluginConfiguration.AcquiredItems[activeCharacter] = GameInterface.AcquiredItems;
            }
        }

        private void RunMigrations()
        {
            
            if (PluginConfiguration.InternalVersion == 0)
            {
                PluginLog.Log("Migrating to version 1");
                var highlight = PluginConfiguration.HighlightColor;
                if (highlight.W == 0.0f)
                {
                    highlight.W = 1;
                    PluginConfiguration.HighlightColor = highlight;
                }
                PluginConfiguration.TabHighlightColor = PluginConfiguration.HighlightColor;

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

                PluginConfiguration.InternalVersion++;
            }
            if (PluginConfiguration.InternalVersion == 1)
            {
                PluginLog.Log("Migrating to version 2");
                PluginConfiguration.InvertTabHighlighting = PluginConfiguration.InvertHighlighting;

                foreach (var filterConfig in _filterConfigurations)
                {
                    if (filterConfig.InvertHighlighting != null)
                    {
                        filterConfig.InvertTabHighlighting = filterConfig.InvertHighlighting;
                    }
                }

                PluginConfiguration.InternalVersion++;
            }
            if (PluginConfiguration.InternalVersion == 2)
            {
                PluginLog.Log("Migrating to version 3");
                foreach (var filterConfig in _filterConfigurations)
                {
                    filterConfig.GenerateNewTableId();
                    filterConfig.Columns = new List<string>();
                    filterConfig.AddColumn("IconColumn");
                    filterConfig.AddColumn("NameColumn");
                    filterConfig.AddColumn("TypeColumn");
                    filterConfig.AddColumn("SourceColumn");
                    filterConfig.AddColumn("LocationColumn");
                    filterConfig.AddColumn("DestinationColumn");
                    filterConfig.AddColumn("QuantityColumn");
                    filterConfig.AddColumn("ItemILevelColumn");
                    filterConfig.AddColumn("SearchCategoryColumn");
                    filterConfig.AddColumn("MarketBoardPriceColumn");
                }
                Cache.ClearCache();
                PluginConfiguration.InternalVersion++;
            }
            if (PluginConfiguration.InternalVersion == 3)
            {
                PluginLog.Log("Migrating to version 4");
                
                foreach (var filterConfig in _filterConfigurations)
                {
                    new IsHqFilter().UpdateFilterConfiguration(filterConfig, filterConfig.IsHq);
                    new IsCollectibleFilter().UpdateFilterConfiguration(filterConfig, filterConfig.IsCollectible);
                    new NameFilter().UpdateFilterConfiguration(filterConfig, filterConfig.NameFilter);
                    new QuantityFilter().UpdateFilterConfiguration(filterConfig, filterConfig.Quantity);
                    new ItemLevelFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ILevel);
                    new SpiritBondFilter().UpdateFilterConfiguration(filterConfig, filterConfig.Spiritbond);
                    new SellToVendorPriceFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ShopSellingPrice);
                    new BuyFromVendorPriceFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ShopBuyingPrice);
                    new CanBePurchasedFilter().UpdateFilterConfiguration(filterConfig, filterConfig.CanBeBought);
                    new MarketBoardPriceFilter().UpdateFilterConfiguration(filterConfig, filterConfig.MarketAveragePrice);
                    new MarketBoardTotalPriceFilter().UpdateFilterConfiguration(filterConfig, filterConfig.MarketTotalAveragePrice);
                    new IsTimedNodeFilter().UpdateFilterConfiguration(filterConfig, filterConfig.IsAvailableAtTimedNode);
                    new ItemUiCategoryFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ItemUiCategoryId);
                    new SearchCategoryFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ItemSearchCategoryId);
                    filterConfig.FilterType++;
                }
                PluginConfiguration.InternalVersion++;
            }

            if (PluginConfiguration.InternalVersion == 4)
            {
                PluginConfiguration.RetainerListColor = ImGuiColors.HealerGreen;
                PluginConfiguration.InternalVersion++;
            }
        }

        private void FrameworkOnUpdate(Framework framework)
        {
            if (PluginConfiguration.AutoSave)
            {
                if (NextSaveTime == null && PluginConfiguration.AutoSaveMinutes != 0)
                {
                    _nextSaveTime = DateTime.Now.AddMinutes(PluginConfiguration.AutoSaveMinutes);
                }
                else
                {
                    if (DateTime.Now >= NextSaveTime)
                    {
                        _nextSaveTime = null;
                        ConfigurationManager.Save();
                    }
                }
            }
        }

        private void WatchFilterChanges()
        {
            foreach (var filterConfiguration in _filterConfigurations)
            {
                filterConfiguration.ConfigurationChanged += FilterConfigurationOnConfigurationChanged;
                filterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
            }
        }

        private void FilterConfigurationOnTableConfigurationChanged(FilterConfiguration filterconfiguration)
        {
            ConfigurationManager.Save();
        }

        private void UnwatchFilterChanges()
        {
            foreach (var filterConfiguration in _filterConfigurations)
            {
                filterConfiguration.ConfigurationChanged -= FilterConfigurationOnConfigurationChanged;
                filterConfiguration.TableConfigurationChanged -= FilterConfigurationOnTableConfigurationChanged;
            }
        }

        private void FilterConfigurationOnConfigurationChanged(FilterConfiguration filterconfiguration)
        {
            //Do some sort of debouncing
            InvalidateFilters();
            ToggleHighlights();
            ConfigurationManager.Save();
        }

        private void ConfigOnConfigurationChanged()
        {
            InvalidateFilters();
            ToggleHighlights();
            ConfigurationManager.Save();
        }

        private void CharacterMonitorOnOnCharacterUpdated(Character? character)
        {
            if (character != null)
            {
                InvalidateFilters();
                if (PluginConfiguration.AcquiredItems.ContainsKey(character.CharacterId))
                {
                    GameInterface.AcquiredItems = PluginConfiguration.AcquiredItems[character.CharacterId];
                }
            }
            else
            {
                GameInterface.AcquiredItems = new HashSet<uint>();
            }
        }
        /// <summary>
        /// Returns the currently active filter determined by the main window state
        /// </summary>
        /// <returns>FilterConfiguration</returns>
        public FilterConfiguration? GetActiveFilter()
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
        public FilterConfiguration? GetActiveUiFilter()
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
        public FilterConfiguration? GetActiveBackgroundFilter()
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

        public FilterTable? GetFilterTable(string filterKey)
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

        public void AddFilter(FilterConfiguration filterConfiguration)
        {
            if (!_filterConfigurations.Contains(filterConfiguration))
            {
                _filterConfigurations.Add(filterConfiguration);
            }
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
            sampleFilter.HighlightWhen = "Always";
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
            var sampleFilter = new FilterConfiguration("Duplicated SortedItems", FilterType.SortingFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceAllCharacters = true;
            sampleFilter.SourceAllRetainers = true;
            sampleFilter.DestinationAllRetainers = true;
            sampleFilter.FilterItemsInRetainers = true;
            sampleFilter.DuplicatesOnly = true;
            sampleFilter.HighlightWhen = "Always";
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
            if (PluginService.CharacterMonitor.Characters.ContainsKey(characterId))
            {
                return PluginService.CharacterMonitor.Characters[characterId].Name;
            }
            return "Unknown";
        }

        public ulong GetCurrentCharacterId()
        {
            if (Service.ClientState.IsLoggedIn && Service.ClientState.LocalPlayer != null)
            {
                return Service.ClientState.LocalContentId;
            }
            return 0;
        }

        public bool DisableActiveUiFilter()
        {
            PluginLog.Verbose("PluginLogic: Disabling active ui filter");
            PluginConfiguration.ActiveUiFilter = null;
            ToggleHighlights();
            return true;
        }

        public bool DisableActiveBackgroundFilter()
        {
            PluginLog.Verbose("PluginLogic: Disabling active background filter");
            PluginConfiguration.ActiveBackgroundFilter = null;
            //ToggleHighlights();
            return true;
        }

        public bool ToggleActiveUiFilterByKey(string filterKey)
        {
            PluginLog.Verbose("PluginLogic: Switching active ui filter");
            if (filterKey == PluginConfiguration.ActiveUiFilter)
            {
                PluginConfiguration.ActiveUiFilter = null;
                ToggleHighlights();
                return true;
            }

            if (_filterConfigurations.Any(c => c.Key == filterKey))
            {
                PluginConfiguration.ActiveUiFilter = filterKey;
                ToggleHighlights();
                return true;
            }

            return false;
        }

        public bool ToggleActiveBackgroundFilterByKey(string filterKey)
        {
            PluginLog.Verbose("PluginLogic: Switching active background filter");
            if (filterKey == PluginConfiguration.ActiveBackgroundFilter)
            {
                PluginConfiguration.ActiveBackgroundFilter = null;
                ToggleHighlights();
                return true;
            }

            if (_filterConfigurations.Any(c => c.Key == filterKey))
            {
                PluginConfiguration.ActiveBackgroundFilter = filterKey;
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
                if (filter.Key == PluginConfiguration.ActiveUiFilter)
                {
                    PluginConfiguration.ActiveUiFilter = null;
                    ToggleHighlights();
                    return true;
                }
                PluginConfiguration.ActiveUiFilter = filterName;
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
                if (filter.Key == PluginConfiguration.ActiveBackgroundFilter)
                {
                    Service.Chat.Print("Disabled filter: " + filterName);
                    PluginConfiguration.ActiveBackgroundFilter = null;
                    ToggleHighlights();
                    return true;
                }
                Service.Chat.Print("Switched filter to: " + filterName);
                PluginConfiguration.ActiveBackgroundFilter = filter.Key;
                ToggleHighlights();
                return true;
            }
            Service.Chat.Print("Failed to find filter with name: " + filterName);
            return false;
        }

        private void GameUiOnUiVisibilityChanged(WindowName windowName, bool? isWindowVisible)
        {
            if (isWindowVisible ?? false)
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

        private void InventoryMonitorOnOnInventoryChanged(Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> inventories, InventoryMonitor.ItemChanges itemChanges)
        {
            RegenerateFilter();
            PluginLog.Verbose("PluginLogic: Inventory changed, saving to config.");
            PluginConfiguration.SavedInventories = inventories;
            if (PluginConfiguration.AutomaticallyDownloadMarketPrices)
            {
                foreach (var inventory in PluginService.InventoryMonitor.AllItems)
                {
                    Cache.RequestCheck(inventory.ItemId);
                }
            }

            foreach (var item in itemChanges.NewItems)
            {
                if (_recentlyAddedSeen.ContainsKey(item.ItemId))
                {
                    _recentlyAddedSeen.Remove(item.ItemId);
                }
                _recentlyAddedSeen.Add(item.ItemId, item);
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

        private void ToggleHighlights()
        {
            var activeFilter = GetActiveFilter();
            FilterTable? activeTable = null;
            bool shouldHighlight = false;
            var invertHighlighting = false;
            var invertTabHighlighting = false;
            //Add in ability to turn off highlights
            if (activeFilter != null)
            {
                if (PluginConfiguration.IsVisible)
                {
                    activeTable = GetFilterTable(activeFilter.Key);
                    if (activeTable != null)
                    {
                        //Allow table to override highlight mode on filter
                        if (activeTable.HighlightItems)
                        {
                            shouldHighlight = activeTable.HighlightItems;
                            if (activeFilter.HighlightWhen is "When Searching" || activeFilter.HighlightWhen == null &&
                                PluginConfiguration.HighlightWhen == "When Searching")
                            {
                                if (!activeTable.IsSearching)
                                {
                                    shouldHighlight = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    shouldHighlight = true;
                }
                invertHighlighting = activeFilter.InvertHighlighting ?? PluginConfiguration.InvertHighlighting;
                invertTabHighlighting = activeFilter.InvertTabHighlighting ?? PluginConfiguration.InvertTabHighlighting;
            }
            
            
            FilterResult? filteredList = null;
            if (activeTable != null)
            {
                filteredList = new FilterResult(activeTable.SortedItems.ToList(), new List<InventoryItem>(), new List<Item>());
            }
            else if (activeFilter != null && activeFilter.FilterResult.HasValue)
            {
                filteredList = activeFilter.FilterResult.Value;
            }
            
            PluginService.FilterManager.UpdateState(activeFilter != null ? new FilterState(){ FilterConfiguration = activeFilter, FilterTable = activeTable} : null);
        }

        private bool MatchesFilter(FilterConfiguration activeFilter, SortingResult item, bool invertHighlighting = false)
        {
            bool matches = false;
            if (activeFilter.FilterType == FilterType.SearchFilter &&
                item.SourceRetainerId == Service.ClientState.LocalContentId)
            {
                matches = true;
            }
            else if (activeFilter.FilterType == FilterType.GameItemFilter)
            {
                matches = true;
            }
            
            if (item.SourceRetainerId == Service.ClientState.LocalContentId && (_currentRetainerId == 0 ||
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
            bool matches = (activeFilter.FilterType.HasFlag(FilterType.SearchFilter) || activeFilter.FilterType.HasFlag(FilterType.SortingFilter)) && item.SourceRetainerId == _currentRetainerId;


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

            if (PluginConfiguration.DisplayTooltip)
            {
                description += "\n\n";
                description += "[InventoryTools]\n";

                {
                    var ownedItems = PluginService.InventoryMonitor.AllItems.Where(item => item.ItemId == itemId).ToList();
                    uint storageCount = 0;
                    List<string> locations = new List<string>();
                    foreach (var oItem in ownedItems)
                    {
                        storageCount += oItem.Quantity;
                        if (PluginService.CharacterMonitor.Characters.ContainsKey(oItem.RetainerId))
                        {
                            var name = PluginService.CharacterMonitor.Characters[oItem.RetainerId]?.Name ?? "Unknown";
                            name = name.Trim().Length == 0 ? "Unknown" : name.Trim();

                            locations.Add($"{name} - {oItem.FormattedBagLocation}");
                        }
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
                    if (!(ExcelCache.GetItem((uint)itemId)?.IsUntradable ?? true))
                    {
                        var marketData = Cache.GetPricing((uint)itemId, false);
                        if (marketData != null)
                        {
                            description += "Market Board Data:\n";
                            description += $"{indentation}Average Price: {marketData.averagePriceNQ}\n";
                            description += $"{indentation}Average Price (HQ): {marketData.averagePriceHQ}\n";
                        }
                    }
                }
            }

            tooltip[ItemTooltipString.Description] = description;
        }

        private Dictionary<string,string>? _gridColumns;
        public Dictionary<string,string> GridColumns
        {
            get
            {
                if (_gridColumns == null)
                {
                    _gridColumns = new Dictionary<string, string>();
                    var columnType = typeof(IColumn);
                    var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetTypes())
                        .Where(p => columnType.IsAssignableFrom(p));
                    foreach (var type in types)
                    {
                        if (type.IsClass && type.Name != "RightClickColumn" && !type.IsAbstract)
                        {
                            PluginLog.Log(type.ToString());
                            //I'm entirely sure this is fine
                            IColumn? instance = (IColumn?)Activator.CreateInstance(type);
                            if (instance != null)
                            {
                                _gridColumns.Add(type.Name, instance.Name);
                            }
                            else
                            {
                                
                                PluginLog.Log("field is null");
                            }
                        }
                    }
                }

                return _gridColumns;
            }
        }

        public List<IFilter> AvailableFilters
        {
            get => _availableFilters;
            set => _availableFilters = value;
        }

        private Dictionary<FilterCategory, List<IFilter>>? _groupedFilters;
        public Dictionary<FilterCategory, List<IFilter>> GroupedFilters
        {
            get
            {
                if (_groupedFilters == null)
                {
                    _groupedFilters = AvailableFilters.GroupBy(c => c.FilterCategory).OrderBy(c => IFilter.FilterCategoryOrder.IndexOf(c.Key)).ToDictionary(c => c.Key, c => c.ToList());
                }

                return _groupedFilters;
            }
        }

        public RightClickColumn RightClickColumn => _rightClickColumn;

        public static Type? GetType(string strFullyQualifiedName)
        {
            Type? type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return type;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return type;
            }
            return null;
        }
        
        public static dynamic? GetClassFromString(string className)
        {
            var classAddress = $"InventoryTools.Logic.Columns.{className}";
            Type? type = GetType(classAddress);

            // Check whether the class is existed?
            if (type == null)
                return null;

            // Then create an instance
            object? instance = Activator.CreateInstance(type);
            return instance;
        }
        
        internal void DrawIcon(ushort icon, Vector2 size) {
            if (icon < 65000) {
                if (TextureDictionary.ContainsKey(icon)) {
                    var tex = TextureDictionary[icon];
                    if (tex.ImGuiHandle == IntPtr.Zero) {

                    } else {
                        ImGui.Image(TextureDictionary[icon].ImGuiHandle, size);
                    }
                } else {
                    ImGui.BeginChild("WaitingTexture", size, true);
                    ImGui.EndChild();

                    Task.Run(() => {
                        try {
                            var iconTex = Service.Data.GetIcon(icon);
                            if (iconTex != null)
                            {
                                var tex = Service.Interface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(),
                                    iconTex.Header.Width, iconTex.Header.Height, 4);
                                if (tex.ImGuiHandle != IntPtr.Zero)
                                {
                                    TextureDictionary[icon] = tex;
                                }
                            }
                        } catch {
                        }
                    });
                }
            } else {
                ImGui.BeginChild("NoIcon", size, true);
                ImGui.EndChild();
            }
        }


        internal void DrawUldIcon(GameIcon gameIcon)
        {
            DrawUldIcon(gameIcon.Name, gameIcon.Size, gameIcon.Uv0, gameIcon.Uv1);
        }

        internal void DrawUldIcon(string name, Vector2 size, Vector2? uvStart = null, Vector2? uvEnd = null)
        {
            if (UldTextureDictionary.ContainsKey(name))
            {
                var tex = UldTextureDictionary[name];
                if (tex.ImGuiHandle == IntPtr.Zero)
                {
                    ImGui.BeginChild("FailedTexture", size, true);
                    ImGui.Text(name);
                    ImGui.EndChild();
                }
                else
                {
                    if (uvStart.HasValue && uvEnd.HasValue)
                    {
                        ImGui.Image(UldTextureDictionary[name].ImGuiHandle, size, uvStart.Value,
                            uvEnd.Value);
                    }
                    else if (uvStart.HasValue)
                    {
                        ImGui.Image(UldTextureDictionary[name].ImGuiHandle, size, uvStart.Value);
                    }
                    else
                    {
                        ImGui.Image(UldTextureDictionary[name].ImGuiHandle, size);
                    }
                }
            }
            else
            {
                ImGui.BeginChild("WaitingTexture", size, true);
                ImGui.EndChild();

                Task.Run(() =>
                {
                    try
                    {
                        var iconTex = Service.Data.GetUldIcon(name);
                        if (iconTex != null)
                        {
                            var tex = Service.Interface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(),
                                iconTex.Header.Width, iconTex.Header.Height, 4);
                            if (tex.ImGuiHandle != IntPtr.Zero)
                            {
                                UldTextureDictionary[name] = tex;
                            }
                        }
                    }
                    catch
                    {
                    }
                });
            }
        }
        
        internal bool DrawUldIconButton(GameIcon gameIcon)
        {
            return DrawUldIconButton(gameIcon.Name, gameIcon.Size, gameIcon.Uv0, gameIcon.Uv1);
        }

        internal bool DrawUldIconButton(string name, Vector2 size, Vector2? uvStart = null, Vector2? uvEnd = null) {
            if (UldTextureDictionary.ContainsKey(name)) {
                var tex = UldTextureDictionary[name];
                if (tex.ImGuiHandle == IntPtr.Zero) {
                    ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
                    ImGui.BeginChild("FailedTexture", size, true);
                    ImGui.Text(name);
                    ImGui.EndChild();
                    ImGui.PopStyleColor();
                } else {
                    if (uvStart.HasValue && uvEnd.HasValue)
                    {
                        return ImGui.ImageButton(UldTextureDictionary[name].ImGuiHandle, size, uvStart.Value,
                            uvEnd.Value);
                    }
                    else if (uvStart.HasValue)
                    {
                        return ImGui.ImageButton(UldTextureDictionary[name].ImGuiHandle, size, uvStart.Value);
                    }
                    else
                    {
                        return ImGui.ImageButton(UldTextureDictionary[name].ImGuiHandle, size);
                    }
                }
            } else {
                ImGui.BeginChild("WaitingTexture", size, true);
                ImGui.EndChild();

                Task.Run(() => {
                    try {
                        var iconTex = Service.Data.GetUldIcon(name);
                        if (iconTex != null)
                        {
                            var tex = Service.Interface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(),
                                iconTex.Header.Width, iconTex.Header.Height, 4);
                            if (tex.ImGuiHandle != IntPtr.Zero)
                            {
                                UldTextureDictionary[name] = tex;
                            }
                        }
                    } catch {
                    }
                });
            }

            return false;
        }

        public static Dictionary<uint, bool> CraftWindows = new Dictionary<uint, bool>();

        public static void DrawFilterWindows()
        {
            foreach (var filter in PluginService.PluginLogic.FilterConfigurations)
            {
                var isVisible = filter.OpenAsWindow;
                if (isVisible)
                {
                    ImGui.SetNextWindowSize(new Vector2(350, 500) * ImGui.GetIO().FontGlobalScale,
                        ImGuiCond.FirstUseEver);
                    ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X - 350 - 60, ImGui.GetIO().DisplaySize.Y - 500 - 50),
                        ImGuiCond.FirstUseEver);
                    ImGui.SetNextWindowSizeConstraints(new Vector2(350, 500) * ImGui.GetIO().FontGlobalScale,
                        new Vector2(2000, 2000) * ImGui.GetIO().FontGlobalScale);
                    ImGui.PushStyleColor(ImGuiCol.WindowBg, 0xFF000000);
                    if (ImGui.Begin("Filter: " + filter.Name, ref isVisible))
                    {
                        if (isVisible != filter.OpenAsWindow)
                        {
                            filter.OpenAsWindow = isVisible;
                        }
                        var itemTable = PluginService.PluginLogic.GetFilterTable(filter.Key);
                        itemTable?.Draw();
                    }
                    ImGui.End();
                }
            }
        }

        public static void DrawCraftRequirementsWindow()
        {
            foreach (var item in CraftWindows)
            {
                var isVisible = item.Value;
                var actualItem = ExcelCache.GetItem(item.Key);
                if (actualItem != null && item.Value)
                {
                    ImGui.SetNextWindowSize(new Vector2(350, 500) * ImGui.GetIO().FontGlobalScale,
                        ImGuiCond.FirstUseEver);
                    ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X - 350 - 60, ImGui.GetIO().DisplaySize.Y - 500 - 50),
                        ImGuiCond.FirstUseEver);
                    ImGui.SetNextWindowSizeConstraints(new Vector2(350, 500) * ImGui.GetIO().FontGlobalScale,
                        new Vector2(2000, 2000) * ImGui.GetIO().FontGlobalScale);
                    ImGui.PushStyleColor(ImGuiCol.WindowBg, 0xFF000000);
                    if (ImGui.Begin("Craft Requirements: " + actualItem.Name, ref isVisible))
                    {
                        ImGui.Text(actualItem.Description.ToDalamudString().ToString());
                        ImGui.Separator();
                        var recipes = ExcelCache.GetItemRecipes(item.Key);
                        foreach (var recipe in recipes)
                        {
                            ImGui.Text("Recipe - " + (recipe.CraftType.Value?.Name ?? ""));
                            foreach (var ingredient in recipe.UnkData5)
                            {
                                if (ingredient.ItemIngredient != 0)
                                {
                                    var ingredientItem = ExcelCache.GetItem((uint) ingredient.ItemIngredient);
                                    if (ingredientItem != null)
                                    {
                                        ImGui.Text(ingredientItem.Name + " - " +
                                                   ingredient.AmountIngredient);
                                    }
                                }
                            }
                            ImGui.Separator();
                        }
                    }
                    ImGui.End();
                }

                if (isVisible != item.Value)
                {
                    PluginLog.Log("Trying to close");
                    CraftWindows[item.Key] = isVisible;
                }

            }
        }

        public static void ShowCraftRequirementsWindow(Item item)
        {
            if (!CraftWindows.ContainsKey(item.RowId))
            {
                CraftWindows.Add(item.RowId, true);
            }
            else
            {
                CraftWindows[item.RowId] = true;
            }
        }


        public void Dispose()
        {
            foreach (var filterTables in _filterTables)
            {
                filterTables.Value.Dispose();
            }

            GameInterface.AcquiredItemsUpdated -= GameInterfaceOnAcquiredItemsUpdated;

            UnwatchFilterChanges();

            PluginConfiguration.FilterConfigurations = FilterConfigurations;
            PluginConfiguration.SavedCharacters = PluginService.CharacterMonitor.Characters;
            Service.Framework.Update -= FrameworkOnUpdate;
            PluginService.InventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
            PluginService.CharacterMonitor.OnActiveRetainerChanged -= CharacterMonitorOnOnActiveCharacterChanged;
            PluginService.CharacterMonitor.OnCharacterUpdated -= CharacterMonitorOnOnCharacterUpdated;
            PluginConfiguration.ConfigurationChanged -= ConfigOnConfigurationChanged;
            PluginService.GameUi.UiVisibilityChanged -= GameUiOnUiVisibilityChanged;

            CommonBase.Functions.Tooltips.OnItemTooltip -= this.OnItemTooltip;
            CommonBase.Dispose();
        }
    }
}