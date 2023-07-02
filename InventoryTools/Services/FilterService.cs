using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Logging;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Services
{
    public class FilterService : IDisposable, IFilterService
    {
        private ICharacterMonitor _characterMonitor;
        private IInventoryMonitor _inventoryMonitor;
        private InventoryHistory _history;
        private ConcurrentDictionary<string, FilterConfiguration> _filters;
        private ConcurrentDictionary<string, FilterTable> _filterTables;
        private ConcurrentDictionary<string, CraftItemTable> _craftItemTables;
        public FilterService(ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor, InventoryHistory history)
        {
            _filters = new ConcurrentDictionary<string, FilterConfiguration>(ConfigurationManager.Config.GetSavedFilters().ToDictionary(c => c.Key, c => c));
            _filterTables = new ConcurrentDictionary<string, FilterTable>();
            _craftItemTables = new ConcurrentDictionary<string, CraftItemTable>();
            ConfigurationManager.Config.ConfigurationChanged += ConfigOnConfigurationChanged;
            _characterMonitor = characterMonitor;
            _characterMonitor.OnCharacterRemoved += CharacterMonitorOnOnCharacterRemoved;
            _characterMonitor.OnCharacterUpdated += CharacterMonitorOnOnCharacterUpdated;
            _characterMonitor.OnCharacterJobChanged += CharacterMonitorOnOnCharacterJobChanged;
            _characterMonitor.OnActiveRetainerChanged += CharacterMonitorOnOnActiveRetainerChanged;
            _inventoryMonitor = inventoryMonitor;
            _inventoryMonitor.OnInventoryChanged += InventoryMonitorOnOnInventoryChanged;
            _history = history;
            _history.OnHistoryLogged += HistoryOnOnHistoryLogged; 
            WatchFilterChanges();
            PluginService.OnPluginLoaded += PluginServiceOnOnPluginLoaded;
        }

        private void HistoryOnOnHistoryLogged(List<InventoryChange> inventorychanges)
        {
            InvalidateFilters(FilterType.HistoryFilter);
        }

        private void PluginServiceOnOnPluginLoaded()
        {
            InvalidateFilters();
        }

#pragma warning disable CS8618
        public FilterService()
#pragma warning restore CS8618
        {
            _filters = new ConcurrentDictionary<string, FilterConfiguration>(ConfigurationManager.Config.GetSavedFilters().ToDictionary(c => c.Key, c => c));
            _filterTables = new ConcurrentDictionary<string, FilterTable>();
            _craftItemTables = new ConcurrentDictionary<string, CraftItemTable>();
        }

        private void InventoryMonitorOnOnInventoryChanged(List<InventoryChange> inventoryChanges, InventoryMonitor.ItemChanges? itemChanges)
        {
            if (PluginService.PluginLoaded)
            {
                InvalidateFilters();
            }
        }

        private void CharacterMonitorOnOnActiveRetainerChanged(ulong retainerid)
        {
            if (PluginService.PluginLoaded)
            {
                InvalidateFilters();
            }
        }

        private void CharacterMonitorOnOnCharacterJobChanged()
        {
            if (PluginService.PluginLoaded)
            {
                InvalidateFilters();
            }
        }

        private void CharacterMonitorOnOnCharacterUpdated(Character? character)
        {
            if (PluginService.PluginLoaded)
            {
                InvalidateFilters();
            }
        }

        private void ConfigOnConfigurationChanged()
        {
            if (PluginService.PluginLoaded)
            {
                InvalidateFilters();
                ConfigurationManager.SaveAsync();
            }
        }

        private void WatchFilterChanges()
        {
            foreach (var filterConfiguration in _filters)
            {
                WatchFilter(filterConfiguration.Value);
            }
        }

        private void FilterConfigurationOnTableConfigurationChanged(FilterConfiguration filterconfiguration)
        {
            if (PluginService.PluginLoaded)
            {
                ConfigurationManager.SaveAsync();
            }
        }

        private void FilterConfigurationOnConfigurationChanged(FilterConfiguration filterconfiguration, bool filterInvalidated = false)
        {
            if (PluginService.PluginLoaded)
            {
                if (filterInvalidated)
                {
                    FilterInvalidated?.Invoke(filterconfiguration);
                }
                FilterModified?.Invoke(filterconfiguration);
                InvalidateFilter(filterconfiguration);
                ConfigurationManager.SaveAsync();
            }
        }
        

        private void CharacterMonitorOnOnCharacterRemoved(ulong characterId)
        {
            if (PluginService.PluginLoaded)
            {
                foreach (var configuration in _filters.ToArray())
                {
                    configuration.Value.SourceInventories.RemoveAll(c => c.Item1 == characterId);
                    configuration.Value.DestinationInventories.RemoveAll(c => c.Item1 == characterId);
                }
            }
        }

        public ConcurrentDictionary<string, FilterConfiguration> Filters => _filters;
        public List<FilterConfiguration> FiltersList => _filters.Select(c => c.Value).OrderBy(c => c.Order).ToList();

        public bool AddFilter(FilterConfiguration configuration)
        {
            var result = _filters.TryAdd(configuration.Key, configuration);
            if (configuration.FilterType == FilterType.CraftFilter)
            {
                var filters = _filters.Where(c => c.Value.FilterType == FilterType.CraftFilter && !c.Value.CraftListDefault).ToList();
                if (filters.Any())
                {
                    configuration.Order = filters
                        .Max(c => c.Value.Order) + 1;
                }
                else
                {

                    configuration.Order = 1;
                }
            }
            else
            {
                var filters = _filters.Where(c => c.Value.FilterType != FilterType.CraftFilter).ToList();
                if (filters.Any())
                {
                    configuration.Order = filters
                        .Max(c => c.Value.Order) + 1;
                }
                else
                {
                    configuration.Order = 1;
                }
            }
            if (result)
            {
                WatchFilter(configuration);
                ConfigurationManager.Config.FilterConfigurations = FiltersList;
                FilterAdded?.Invoke(configuration);
            }
            ConfigurationManager.SaveAsync();
            return result;
        }

        public void WatchFilter(FilterConfiguration filterConfiguration)
        {
            filterConfiguration.ConfigurationChanged += FilterConfigurationOnConfigurationChanged;
            filterConfiguration.TableConfigurationChanged += FilterConfigurationOnTableConfigurationChanged;
            filterConfiguration.ListUpdated += ValueOnListUpdated;
        }

        public void UnWatchFilter(FilterConfiguration filterConfiguration)
        {
            filterConfiguration.ConfigurationChanged -= FilterConfigurationOnConfigurationChanged;
            filterConfiguration.TableConfigurationChanged -= FilterConfigurationOnTableConfigurationChanged;
            filterConfiguration.ListUpdated -= ValueOnListUpdated;
        }
        
        private void ValueOnListUpdated(FilterConfiguration filterConfiguration)
        {
            FilterRecalculated?.Invoke(filterConfiguration);
        }

        public bool AddFilter(string name, FilterType filterType)
        {
            var sampleFilter = new FilterConfiguration(name, filterType);
            return AddFilter(sampleFilter);
        }

        public FilterConfiguration DuplicateFilter(FilterConfiguration configuration, string newName)
        {
            var newConfiguration = configuration.Clone() ?? new FilterConfiguration();
            newConfiguration.Key = Guid.NewGuid().ToString("N");
            newConfiguration.Name = newName;
            AddFilter(newConfiguration);
            return newConfiguration;
        }

        public FilterConfiguration AddNewCraftFilter()
        {
            var clonedFilter = GetDefaultCraftList().Clone();
            if (clonedFilter == null)
            {
                var filter = new FilterConfiguration("New Craft List",
                    Guid.NewGuid().ToString("N"), FilterType.CraftFilter);
                PluginService.FilterService.AddFilter(filter);
                return filter;
            }

            clonedFilter.Name = "New Craft List";
            clonedFilter.GenerateNewTableId();
            clonedFilter.GenerateNewCraftTableId();
            clonedFilter.CraftListDefault = false;
            clonedFilter.Key = Guid.NewGuid().ToString("N");
            PluginService.FilterService.AddFilter(clonedFilter);
            return clonedFilter;
        }

        public bool RemoveFilter(FilterConfiguration configuration)
        {
            var result = _filters.TryRemove(configuration.Key, out _);
            if (result)
            {
                UnWatchFilter(configuration);
                ConfigurationManager.Config.FilterConfigurations = FiltersList;
                FilterRemoved?.Invoke(configuration);
            }

            return result;
        }

        public bool RemoveFilter(string name)
        {
            if (_filters.Any(c => c.Value.Name == name))
            {
                return RemoveFilter(_filters.First(c => c.Value.Name == name).Value);
            }

            return false;
        }

        public bool RemoveFilterByKey(string key)
        {
            if (_filters.TryGetValue(key, out var config))
            {
                return RemoveFilter(key);
            }

            return false;
        }

        public FilterConfiguration? GetActiveUiFilter(bool ignoreWindowState = true)
        {
            if (ConfigurationManager.Config.ActiveUiFilter != null)
            {
                if (_filters.Any(c => c.Value.Key == ConfigurationManager.Config.ActiveUiFilter && (ignoreWindowState || PluginService.WindowService.HasFilterWindowOpen)))
                {
                    return _filters.First(c => c.Value.Key == ConfigurationManager.Config.ActiveUiFilter).Value;
                }
            }

            return null;
        }

        public FilterConfiguration? GetActiveBackgroundFilter()
        {
            if (!PluginService.WindowService.HasFilterWindowOpen)
            {
                if (ConfigurationManager.Config.ActiveBackgroundFilter != null)
                {
                    if (_filters.Any(c => c.Key == ConfigurationManager.Config.ActiveBackgroundFilter))
                    {
                        return _filters.First(c => c.Key == ConfigurationManager.Config.ActiveBackgroundFilter).Value;
                    }
                }
            }

            return null;
        }

        public FilterConfiguration? GetActiveFilter()
        {
            var activeUiFilter = GetActiveUiFilter(false);
            if (activeUiFilter != null)
            {
                return activeUiFilter;
            }

            return GetActiveBackgroundFilter();
        }

        public FilterConfiguration GetDefaultCraftList()
        {
            if (_filters.Any(c => c.Value.FilterType == FilterType.CraftFilter && c.Value.CraftListDefault))
            {
                return _filters.First(c => c.Value.FilterType == FilterType.CraftFilter && c.Value.CraftListDefault).Value;
            }

            var defaultFilter = FilterConfiguration.GenerateDefaultFilterConfiguration();
            AddFilter(defaultFilter);
            return defaultFilter;
        }

        public FilterConfiguration? GetFilter(string name)
        {
            if (_filters.Any(c => c.Value.Name == name))
            {
                return _filters.First(c => c.Value.Name == name).Value;
            }

            return null;
        }

        public FilterConfiguration? GetFilterByKey(string key)
        {
            if(_filters.TryGetValue(key, out var filterConfiguration))
            {
                return filterConfiguration;
            }

            return null;
        }

        public FilterConfiguration? GetFilterByKeyOrName(string keyOrName)
        {
            var filter = GetFilterByKey(keyOrName);
            if (filter == null)
            {
                filter = GetFilter(keyOrName);
            }

            return filter;
        }

        public bool SetActiveUiFilter(string name)
        {
            var configuration = GetFilter(name);
            if (configuration != null)
            {
                if (ConfigurationManager.Config.ActiveUiFilter != configuration.Key)
                {
                    ConfigurationManager.Config.ActiveUiFilter = configuration.Key;
                    UiFilterToggled?.Invoke(configuration, true);
                }

                return true;
            }

            return false;
        }

        public bool SetActiveUiFilter(FilterConfiguration configuration)
        {
            if (ConfigurationManager.Config.ActiveUiFilter != configuration.Key)
            {
                ConfigurationManager.Config.ActiveUiFilter = configuration.Key;
                UiFilterToggled?.Invoke(configuration, true);
            }

            return true;
        }

        public bool SetActiveUiFilterByKey(string key)
        {
            var filter = GetFilterByKey(key);
            if (filter != null)
            {
                return SetActiveUiFilter(filter);
            }

            return false;
        }

        public bool SetActiveBackgroundFilter(string name)
        {
            var configuration = GetFilter(name);
            if (configuration != null)
            {
                if (ConfigurationManager.Config.ActiveBackgroundFilter != configuration.Key)
                {
                    ConfigurationManager.Config.ActiveBackgroundFilter = configuration.Key;
                    BackgroundFilterToggled?.Invoke(configuration, true);
                }

                return true;
            }

            return false;
        }

        public bool SetActiveBackgroundFilter(FilterConfiguration configuration)
        {
            if (ConfigurationManager.Config.ActiveBackgroundFilter != configuration.Key)
            {
                ConfigurationManager.Config.ActiveBackgroundFilter = configuration.Key;
                BackgroundFilterToggled?.Invoke(configuration, true);
            }

            return true;
        }

        public bool SetActiveBackgroundFilterByKey(string key)
        {
            var filter = GetFilterByKey(key);
            if (filter != null)
            {
                return SetActiveBackgroundFilter(filter);
            }

            return false;
        }

        public bool ClearActiveUiFilter()
        {
            if (ConfigurationManager.Config.ActiveUiFilter != null)
            {
                var activeUiFilter = GetActiveUiFilter();
                ConfigurationManager.Config.ActiveUiFilter = null;
                if (activeUiFilter != null)
                {
                    UiFilterToggled?.Invoke(activeUiFilter, false);
                }
                return true;
            }

            return false;
        }

        public bool ClearActiveBackgroundFilter()
        {
            if (ConfigurationManager.Config.ActiveBackgroundFilter != null)
            {
                var activeBackgroundFilter = GetActiveBackgroundFilter();
                ConfigurationManager.Config.ActiveBackgroundFilter = null;
                if (activeBackgroundFilter != null)
                {
                    BackgroundFilterToggled?.Invoke(activeBackgroundFilter, false);
                }
                return true;
            }

            return false;
        }

        public bool ToggleActiveUiFilter(string name)
        {
            var activeUiFilter = GetActiveUiFilter();
            if (activeUiFilter != null)
            {
                ClearActiveUiFilter();
                if (activeUiFilter.Name != name)
                {
                    SetActiveUiFilter(name);
                }
                return true;
            }
            SetActiveUiFilter(name);
            return true;
        }

        public bool ToggleActiveUiFilter(FilterConfiguration configuration)
        {
            var activeUiFilter = GetActiveUiFilter();
            if (activeUiFilter != null)
            {
                ClearActiveUiFilter();
                if (!activeUiFilter.Equals(configuration))
                {
                    SetActiveUiFilter(configuration);
                }
                return true;
            }
            SetActiveUiFilter(configuration);
            return true;
        }

        public bool ToggleActiveBackgroundFilter(string name)
        {
            var activeBackgroundFilter = GetActiveBackgroundFilter();
            if (activeBackgroundFilter != null)
            {
                if (activeBackgroundFilter.Name != name)
                {
                    SetActiveBackgroundFilter(name);
                }
                else
                {
                    ClearActiveBackgroundFilter();
                }
                return true;
            }
            SetActiveBackgroundFilter(name);
            return true;
        }

        public bool ToggleActiveBackgroundFilter(FilterConfiguration configuration)
        {
            var activeBackgroundFilter = GetActiveBackgroundFilter();
            if (activeBackgroundFilter != null)
            {
                ClearActiveBackgroundFilter();
                if (activeBackgroundFilter != configuration)
                {
                    SetActiveBackgroundFilter(configuration);
                }
                return true;
            }
            SetActiveBackgroundFilter(configuration);
            return true;
        }

        public bool MoveFilterUp(FilterConfiguration configuration)
        {
            List<FilterConfiguration> currentList;
            if (configuration.FilterType == FilterType.CraftFilter)
            {
                currentList = FiltersList.Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault).ToList();
            }
            else
            {
                currentList = FiltersList.Where(c => c.FilterType != FilterType.CraftFilter).ToList();
            }
            currentList = currentList.MoveUp( configuration);
            var order = 0u;
            foreach (var item in currentList)
            {
                if (item.Order != order)
                {
                    item.SetOrder(order);
                }

                order++;
            }
            configuration.NotifyConfigurationChange();
            FilterRepositioned?.Invoke(configuration);

            return true;
        }

        public bool MoveFilterDown(FilterConfiguration configuration)
        {
            List<FilterConfiguration> currentList;
            if (configuration.FilterType == FilterType.CraftFilter)
            {
                currentList = FiltersList.Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault).ToList();
            }
            else
            {
                currentList = FiltersList.Where(c => c.FilterType != FilterType.CraftFilter).ToList();
            }
            currentList = currentList.MoveDown( configuration);
            var order = 0u;
            foreach (var item in currentList)
            {
                if (item.Order != order)
                {
                    item.SetOrder(order);
                }
                order++;
            }
            configuration.NotifyConfigurationChange();
            FilterRepositioned?.Invoke(configuration);

            return true;
        }

        public void InvalidateFilter(FilterConfiguration configuration)
        {
            configuration.NeedsRefresh = true;
            if (_filterTables.ContainsKey(configuration.Key))
            {
                _filterTables[configuration.Key].NeedsRefresh = true;
            }
            var activeFilter = GetActiveFilter();
            if (activeFilter == configuration)
            {
                activeFilter.StartRefresh();
            }
        }

        public void InvalidateFilters(FilterType? filterType = null)
        {
            PluginService.ChatUtilities.PrintLog("Filters invalidated");
            foreach (var filter in _filters)
            {
                if(filterType != null && filter.Value.FilterType != filterType) continue;
                filter.Value.NeedsRefresh = true;
                if (_filterTables.ContainsKey(filter.Key))
                {
                    _filterTables[filter.Key].NeedsRefresh = true;
                }

                var activeFilter = GetActiveFilter();
                if (activeFilter != null)
                {
                    activeFilter.StartRefresh();
                }
            }
        }

        public CraftItemTable? GetCraftTable(FilterConfiguration configuration)
        {
            return GetCraftTable(configuration.Key);
        }

        public FilterTable? GetFilterTable(FilterConfiguration configuration)
        {
            return GetFilterTable(configuration.Key);
        }

        public CraftItemTable? GetCraftTable(string filterKey)
        {
            if (_craftItemTables.ContainsKey(filterKey))
            {
                return _craftItemTables[filterKey];
            }
            if (_filters.Any(c => c.Key == filterKey))
            {
                var filterConfig = _filters.First(c => c.Key == filterKey);
                CraftItemTable generateTable = filterConfig.Value.GenerateCraftTable();
                if (_craftItemTables.TryAdd(filterKey, generateTable))
                {
                    generateTable.Refreshed += TableOnRefreshed;
                    return _craftItemTables[filterKey];
                }
                generateTable.Dispose();
            }

            return null;
        }

        public FilterTable? GetFilterTable(string filterKey)
        {
            if (_filterTables.ContainsKey(filterKey))
            {
                return _filterTables[filterKey];
            }
            if (_filters.Any(c => c.Key == filterKey))
            {
                var filterConfig = _filters.First(c => c.Key == filterKey);
                FilterTable generateTable = filterConfig.Value.GenerateTable();
                if (_filterTables.TryAdd(filterKey, generateTable))
                {
                    generateTable.Refreshed += TableOnRefreshed;
                    return _filterTables[filterKey];
                }
                generateTable.Dispose();
            }

            return null;
        }

        public bool HasCraftTable(FilterConfiguration configuration)
        {
            return HasCraftTable(configuration.Key);
        }

        public bool HasFilterTable(FilterConfiguration configuration)
        {
            return HasFilterTable(configuration.Key);
        }

        public bool HasCraftTable(string filterKey)
        {
            return _craftItemTables.ContainsKey(filterKey);
        }

        public bool HasFilterTable(string filterKey)
        {
            return _filterTables.ContainsKey(filterKey);
        }

        private void TableOnRefreshed(RenderTableBase itemtable)
        {
            FilterTableRefreshed?.Invoke(itemtable);
        }

        public event IFilterService.FilterAddedDelegate? FilterAdded;
        public event IFilterService.FilterRemovedDelegate? FilterRemoved;
        public event IFilterService.FilterRepositionedDelegate? FilterRepositioned;
        public event IFilterService.FilterModifiedDelegate? FilterModified;
        public event IFilterService.FilterRecalculatedDelegate? FilterRecalculated;
        public event IFilterService.FiltersInvalidatedDelegate? FiltersInvalidated;
        public event IFilterService.FilterInvalidatedDelegate? FilterInvalidated;
        public event IFilterService.FilterToggledDelegate? UiFilterToggled;
        public event IFilterService.FilterToggledDelegate? BackgroundFilterToggled;
        public event IFilterService.FilterTableRefreshedDelegate? FilterTableRefreshed;
        
                            
        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed && disposing)
            {
                PluginService.OnPluginLoaded -= PluginServiceOnOnPluginLoaded;
                ConfigurationManager.Config.ConfigurationChanged -= ConfigOnConfigurationChanged;
                if (_characterMonitor != null)
                {
                    _characterMonitor.OnCharacterRemoved -= CharacterMonitorOnOnCharacterRemoved;
                    _characterMonitor.OnCharacterUpdated -= CharacterMonitorOnOnCharacterUpdated;
                    _characterMonitor.OnCharacterJobChanged -= CharacterMonitorOnOnCharacterJobChanged;
                    _characterMonitor.OnActiveRetainerChanged -= CharacterMonitorOnOnActiveRetainerChanged;
                    _inventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
                }
                _history.OnHistoryLogged -= HistoryOnOnHistoryLogged; 

                foreach (var filterConfiguration in _filters)
                {
                    UnWatchFilter(filterConfiguration.Value);
                }

                foreach (var filterTable in _filterTables)
                {
                    filterTable.Value.Refreshed -= TableOnRefreshed;
                    filterTable.Value.Dispose();
                }

                foreach (var craftItemTable in _craftItemTables)
                {
                    craftItemTable.Value.Refreshed -= TableOnRefreshed;
                    craftItemTable.Value.Dispose();
                }
            }
            _disposed = true;         
        }
        
        ~FilterService()
        {
#if DEBUG
            // In debug-builds, make sure that a warning is displayed when the Disposable object hasn't been
            // disposed by the programmer.

            if( _disposed == false )
            {
                PluginLog.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
            }
#endif
            Dispose (true);
        }
    }
}