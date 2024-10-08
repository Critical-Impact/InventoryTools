using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Columns.Buttons;
using InventoryTools.Logic.Filters;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Lists
{
    public class ListService : DisposableMediatorSubscriberBase, IListService
    {
        private readonly MediatorService _mediatorService;
        private ICharacterMonitor _characterMonitor;
        private IInventoryMonitor _inventoryMonitor;
        private InventoryHistory _history;
        private readonly ConfigurationManagerService _configurationManagerService;
        private readonly InventoryToolsConfiguration _configuration;
        private readonly IChatUtilities _chatUtilities;
        private readonly IFramework _framework;
        private readonly Func<string, IColumn?> _columnFactory;
        private readonly Func<Type, IColumn> _columnTypeFactory;
        private ConcurrentDictionary<string, FilterConfiguration> _lists;

        public ListService(ILogger<ListService> logger, MediatorService mediatorService,
            ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor, HostedInventoryHistory history,
            ConfigurationManagerService configurationManagerService, InventoryToolsConfiguration configuration,
            IChatUtilities chatUtilities, IFramework framework, Func<string, IColumn?> columnFactory,
            Func<Type, IColumn> columnTypeFactory) : base(logger, mediatorService)
        {
            _history = history;
            _configurationManagerService = configurationManagerService;
            _configuration = configuration;
            _chatUtilities = chatUtilities;
            _framework = framework;
            _columnFactory = columnFactory;
            _columnTypeFactory = columnTypeFactory;
            _lists = new ConcurrentDictionary<string, FilterConfiguration>();

            configurationManagerService.ConfigurationChanged += ConfigOnConfigurationChanged;
            _mediatorService = mediatorService;
            _characterMonitor = characterMonitor;
            _characterMonitor.OnCharacterRemoved += CharacterMonitorOnOnCharacterRemoved;
            _characterMonitor.OnCharacterUpdated += CharacterMonitorOnOnCharacterUpdated;
            _characterMonitor.OnCharacterJobChanged += CharacterMonitorOnOnCharacterJobChanged;
            _characterMonitor.OnActiveRetainerChanged += CharacterMonitorOnOnActiveRetainerChanged;
            _characterMonitor.OnCharacterLoggedIn += CharacterLoggedIn;
            _characterMonitor.OnCharacterLoggedOut += CharacterLoggedOut;
            _inventoryMonitor = inventoryMonitor;
            _inventoryMonitor.OnInventoryChanged += InventoryMonitorOnOnInventoryChanged;
            _history.OnHistoryLogged += HistoryOnOnHistoryLogged;
            _mediatorService.Subscribe<ListUpdatedMessage>(this, message => ListUpdated(message.FilterConfiguration) );
            _mediatorService.Subscribe<AddToCraftListMessage>(this, AddToCraftListMessageRecv );
            _mediatorService.Subscribe<AddToNewCraftListMessage>(this, AddToNewCraftListMessageRecv );
            _framework.Update += OnUpdate;
        }

        private void CharacterLoggedIn(ulong characterid)
        {
            InvalidateLists();
        }

        private void CharacterLoggedOut(ulong characterid)
        {
            InvalidateLists();
        }

        private void AddToCraftListMessageRecv(AddToCraftListMessage obj)
        {
            var filter = GetListByKey(obj.FilterKey);
            if (filter != null)
            {
                filter.CraftList.AddCraftItem(obj.ItemId, obj.Quantity, obj.Flags);
            }
        }

        private void AddToNewCraftListMessageRecv(AddToNewCraftListMessage obj)
        {
            var craftList = AddNewCraftList(null, obj.IsEphemeral);
            craftList.CraftList.AddCraftItem(obj.ItemId, obj.Quantity, obj.Flags);
        }

        private ConcurrentDictionary<string, FilterConfiguration> LoadListsFromConfiguration()
        {
            var savedLists = _configuration.GetSavedFilters();
            foreach (var list in savedLists)
            {
                ValidateAndInjectListColumns(list);
            }
            return new ConcurrentDictionary<string, FilterConfiguration>(savedLists.ToDictionary(c => c.Key, c => c));
        }

        private void ValidateAndInjectListColumns(FilterConfiguration list)
        {
            if (list.Columns != null)
            {
                List<ColumnConfiguration> invalidColumns = new();
                foreach (var columnConfiguration in list.Columns)
                {
                    if (!SetupColumn(columnConfiguration))
                    {
                        invalidColumns.Add(columnConfiguration);
                    }
                }

                foreach (var toRemove in invalidColumns)
                {
                    list.Columns.Remove(toRemove);
                }
            }

            if (list.CraftColumns != null)
            {
                List<ColumnConfiguration> invalidColumns = new();
                foreach (var columnConfiguration in list.CraftColumns)
                {
                    if (!SetupColumn(columnConfiguration))
                    {
                        invalidColumns.Add(columnConfiguration);
                    }
                }

                foreach (var toRemove in invalidColumns)
                {
                    list.CraftColumns.Remove(toRemove);
                }
            }
        }

        private bool SetupColumn(ColumnConfiguration columnConfiguration)
        {
            var column = _columnFactory.Invoke(columnConfiguration.ColumnName);
            if (column == null)
            {
                return false;
            }
            columnConfiguration.Column = column;
            return true;
        }

        private void ListUpdated(FilterConfiguration filterConfiguration)
        {
            ListRefreshed?.Invoke(filterConfiguration);
        }

        private void OnUpdate(IFramework framework)
        {
            foreach (var filter in _lists)
            {
                if (filter.Value.ConfigurationDirty)
                {
                    filter.Value.ConfigurationDirty = false;
                    FilterConfigurationChanged(filter.Value);
                }

                if (filter.Value.TableConfigurationDirty)
                {
                    filter.Value.TableConfigurationDirty = false;
                    FilterTableConfigurationChanged(filter.Value);
                }

                if (filter.Value.Columns != null)
                {
                    var isDirty = false;
                    foreach (var column in filter.Value.Columns)
                    {
                        if (column.IsDirty)
                        {
                            column.IsDirty = false;
                            isDirty = true;
                        }
                    }

                    if (isDirty)
                    {
                        FilterTableConfigurationChanged(filter.Value);
                    }
                }

                if (filter.Value.CraftColumns != null)
                {
                    var isDirty = false;
                    foreach (var column in filter.Value.CraftColumns)
                    {
                        if (column.IsDirty)
                        {
                            column.IsDirty = false;
                            isDirty = true;
                        }
                    }

                    if (isDirty)
                    {
                        FilterTableConfigurationChanged(filter.Value);
                    }
                }

                if (filter.Value is { } configuration)
                {
                    if (_configuration.ActiveBackgroundFilter == configuration.Key && configuration.NeedsRefresh)
                    {
                        configuration.AllowRefresh = true;
                    }
                    if ((configuration.NeedsRefresh || configuration.FilterType == FilterType.CraftFilter && configuration.CraftList.NeedsRefresh) && !configuration.Refreshing && configuration.AllowRefresh)
                    {
                        filter.Value.Refreshing = true;
                        MediatorService.Publish(new RequestListUpdateMessage(filter.Value));
                    }
                }
            }
        }

        private void HistoryOnOnHistoryLogged(List<InventoryChange> inventorychanges)
        {
            InvalidateLists(FilterType.HistoryFilter);
        }

        private void InventoryMonitorOnOnInventoryChanged(List<InventoryChange> inventoryChanges, InventoryMonitor.ItemChanges? itemChanges)
        {
            InvalidateLists();
        }

        private void CharacterMonitorOnOnActiveRetainerChanged(ulong retainerid)
        {
            InvalidateLists();
        }

        private void CharacterMonitorOnOnCharacterJobChanged()
        {
            InvalidateLists();
        }

        private void CharacterMonitorOnOnCharacterUpdated(Character? character)
        {
            InvalidateLists();
        }

        private void ConfigOnConfigurationChanged()
        {
            InvalidateLists();
        }

        private void CraftListUpdated(FilterConfiguration filterconfiguration)
        {
            //_mediatorService.Publish(new ListModifiedMessage(filterconfiguration));
            InvalidateList(filterconfiguration);
            _configuration.IsDirty = true;
        }

        private void FilterTableConfigurationChanged(FilterConfiguration filterConfiguration)
        {
            _mediatorService.Publish(new ListModifiedMessage(filterConfiguration));
            ListTableConfigurationChanged?.Invoke(filterConfiguration);
            InvalidateList(filterConfiguration);
            _configuration.IsDirty = true;
        }

        private void FilterConfigurationChanged(FilterConfiguration filterConfiguration)
        {
            _mediatorService.Publish(new ListModifiedMessage(filterConfiguration));
            ListConfigurationChanged?.Invoke(filterConfiguration);
            InvalidateList(filterConfiguration);
            _configuration.IsDirty = true;
        }


        private void CharacterMonitorOnOnCharacterRemoved(ulong characterId)
        {
            foreach (var configuration in _lists.ToArray())
            {
                configuration.Value.SourceInventories.RemoveAll(c => c.Item1 == characterId);
                configuration.Value.DestinationInventories.RemoveAll(c => c.Item1 == characterId);
            }
        }

        public List<FilterConfiguration> Lists => _lists.Select(c => c.Value).OrderBy(c => c.Order).ToList();

        public bool AddList(FilterConfiguration configuration)
        {
            ValidateAndInjectListColumns(configuration);
            var result = _lists.TryAdd(configuration.Key, configuration);
            if (configuration.FilterType == FilterType.CraftFilter)
            {
                var filters = _lists.Where(c => c.Value.FilterType == FilterType.CraftFilter && !c.Value.CraftListDefault).ToList();
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
                var filters = _lists.Where(c => c.Value.FilterType != FilterType.CraftFilter).ToList();
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
                _configuration.FilterConfigurations = Lists;
                _mediatorService.Publish(new ListAddedMessage(configuration));
                ListAdded?.Invoke(configuration);
            }
            _configuration.IsDirty = true;
            return result;
        }

        public bool AddList(string name, FilterType filterType)
        {
            var sampleFilter = new FilterConfiguration(name, filterType);
            AddDefaultColumns(sampleFilter);
            return AddList(sampleFilter);
        }

        public FilterConfiguration DuplicateList(FilterConfiguration configuration, string newName)
        {
            var newConfiguration = configuration.Clone() ?? new FilterConfiguration();
            newConfiguration.Key = Guid.NewGuid().ToString("N");
            newConfiguration.Name = newName;
            AddList(newConfiguration);
            return newConfiguration;
        }

        public FilterConfiguration AddNewCraftList(string? newName = null, bool? isEphemeral = false)
        {
            var isEphemeralNN = isEphemeral ?? false;
            var newNameNN = newName ?? (isEphemeralNN ? "New Ephemeral List" : "New Craft List");

            var names = Lists.Where(c =>
                c.FilterType == FilterType.CraftFilter && !c.CraftListDefault &&
                c.IsEphemeralCraftList == isEphemeralNN).Select(c => c.Name).Distinct().ToHashSet();
            var count = 1;
            var fixedName = newNameNN;
            while (names.Contains(fixedName))
            {
                count++;
                fixedName = newNameNN + " " + count;
            }

            var clonedFilter = GetDefaultCraftList().Clone();
            if (clonedFilter == null)
            {
                var filter = new FilterConfiguration(fixedName,
                    Guid.NewGuid().ToString("N"), FilterType.CraftFilter);
                AddDefaultColumns(filter);
                filter.IsEphemeralCraftList = isEphemeralNN;
                AddList(filter);
                return filter;
            }

            clonedFilter.Name = fixedName;
            clonedFilter.GenerateNewTableId();
            clonedFilter.GenerateNewCraftTableId();
            clonedFilter.CraftListDefault = false;
            clonedFilter.Key = Guid.NewGuid().ToString("N");
            clonedFilter.IsEphemeralCraftList = isEphemeralNN;
            AddList(clonedFilter);
            return clonedFilter;
        }

        public FilterConfiguration AddNewCuratedList(string? name = null)
        {
            name ??= "New Curated List";

            var names = Lists.Where(c =>
                c.FilterType == FilterType.CuratedList).Select(c => c.Name).Distinct().ToHashSet();
            var count = 1;
            var fixedName = name;
            while (names.Contains(fixedName))
            {
                count++;
                fixedName = name + " " + count;
            }

            var filter = new FilterConfiguration(fixedName,
                Guid.NewGuid().ToString("N"), FilterType.CuratedList);
            AddDefaultColumns(filter);
            AddList(filter);
            return filter;
        }

        public bool RemoveList(FilterConfiguration configuration)
        {
            var result = _lists.TryRemove(configuration.Key, out _);
            if (result)
            {
                _configuration.FilterConfigurations = Lists;
                _mediatorService.Publish(new ListRemovedMessage(configuration));
                ListRemoved?.Invoke(configuration);
            }

            return result;
        }

        public bool RemoveList(string name)
        {
            if (_lists.Any(c => c.Value.Name == name))
            {
                return RemoveList(_lists.First(c => c.Value.Name == name).Value);
            }

            return false;
        }

        public bool RemoveFilterByKey(string key)
        {
            if (_lists.TryGetValue(key, out var config))
            {
                return RemoveList(key);
            }

            return false;
        }

        public FilterConfiguration? GetActiveUiList(bool ignoreWindowState = true)
        {
            if (_configuration.ActiveUiFilter != null)
            {
                if (_lists.Any(c => c.Value.Key == _configuration.ActiveUiFilter && (ignoreWindowState || c.Value.Active)))
                {
                    return _lists.First(c => c.Value.Key == _configuration.ActiveUiFilter).Value;
                }
            }

            return null;
        }

        public FilterConfiguration? GetActiveBackgroundList()
        {
            if (_lists.Any(c => c.Value.Active))
            {
                return null;
            }

            if (_configuration.ActiveBackgroundFilter != null)
            {
                if (_lists.Any(c => c.Key == _configuration.ActiveBackgroundFilter))
                {
                    return _lists.First(c => c.Key == _configuration.ActiveBackgroundFilter).Value;
                }
            }

            return null;
        }

        public FilterConfiguration? GetActiveCraftList()
        {
            if (_configuration.ActiveCraftList != null)
            {
                if (_lists.Any(c => c.Value.Key == _configuration.ActiveCraftList))
                {
                    return _lists.First(c => c.Value.Key == _configuration.ActiveCraftList).Value;
                }
            }

            return null;
        }

        public FilterConfiguration? GetActiveList()
        {
            var activeUiFilter = GetActiveUiList(false);
            if (activeUiFilter != null)
            {
                return activeUiFilter;
            }

            return GetActiveBackgroundList();
        }

        public bool HasActiveList()
        {
            return _configuration.ActiveUiFilter != null ||
                   _configuration.ActiveBackgroundFilter != null;
        }

        public bool HasActiveUiList()
        {
            return _configuration.ActiveUiFilter != null;
        }

        public bool HasActiveBackgroundList()
        {
            return _configuration.ActiveBackgroundFilter != null;
        }

        public bool HasActiveCraftList()
        {
            return _configuration.ActiveCraftList != null;
        }

        public FilterConfiguration GetDefaultCraftList()
        {
            if (_lists.Any(c => c.Value.FilterType == FilterType.CraftFilter && c.Value.CraftListDefault))
            {
                return _lists.First(c => c.Value.FilterType == FilterType.CraftFilter && c.Value.CraftListDefault).Value;
            }

            var defaultFilter = GenerateDefaultCraftList();
            AddList(defaultFilter);
            return defaultFilter;
        }

        public bool HasDefaultCraftList()
        {
            if (_lists.Any(c => c.Value.FilterType == FilterType.CraftFilter && c.Value.CraftListDefault))
            {
                return true;
            }

            return false;
        }

        public FilterConfiguration? GetList(string name)
        {
            if (_lists.Any(c => c.Value.Name == name))
            {
                return _lists.First(c => c.Value.Name == name).Value;
            }

            return null;
        }

        public FilterConfiguration? GetListByKey(string key)
        {
            if(_lists.TryGetValue(key, out var filterConfiguration))
            {
                return filterConfiguration;
            }

            return null;
        }

        public FilterConfiguration? GetListByKeyOrName(string keyOrName)
        {
            var filter = GetListByKey(keyOrName);
            if (filter == null)
            {
                filter = GetList(keyOrName);
            }

            return filter;
        }

        public bool SetActiveUiList(string name)
        {
            var configuration = GetList(name);
            if (configuration != null)
            {
                if (_configuration.ActiveUiFilter != configuration.Key)
                {
                    _configuration.ActiveUiFilter = configuration.Key;
                    UiListToggled?.Invoke(configuration, true);
                }

                return true;
            }

            return false;
        }

        public bool SetActiveUiList(FilterConfiguration configuration)
        {
            if (_configuration.ActiveUiFilter != configuration.Key)
            {
                _configuration.ActiveUiFilter = configuration.Key;
                UiListToggled?.Invoke(configuration, true);
            }

            return true;
        }

        public bool SetActiveUiListByKey(string key)
        {
            var filter = GetListByKey(key);
            if (filter != null)
            {
                return SetActiveUiList(filter);
            }

            return false;
        }

        public bool SetActiveBackgroundList(string name)
        {
            var configuration = GetList(name);
            if (configuration != null)
            {
                if (_configuration.ActiveBackgroundFilter != configuration.Key)
                {
                    _configuration.ActiveBackgroundFilter = configuration.Key;
                    configuration.NeedsRefresh = true;
                    BackgroundListToggled?.Invoke(configuration, true);
                }

                return true;
            }

            return false;
        }

        public bool SetActiveBackgroundList(FilterConfiguration configuration)
        {
            if (_configuration.ActiveBackgroundFilter != configuration.Key)
            {
                _configuration.ActiveBackgroundFilter = configuration.Key;
                configuration.NeedsRefresh = true;
                BackgroundListToggled?.Invoke(configuration, true);
            }

            return true;
        }

        public bool SetActiveBackgroundListByKey(string key)
        {
            var filter = GetListByKey(key);
            if (filter != null)
            {
                return SetActiveBackgroundList(filter);
            }

            return false;
        }

        public bool SetActiveCraftList(FilterConfiguration configuration)
        {
            if (_configuration.ActiveCraftList != configuration.Key)
            {
                _configuration.ActiveCraftList = configuration.Key;
                CraftListToggled?.Invoke(configuration, true);
            }

            return true;
        }

        public bool SetActiveCraftListByKey(string key)
        {
            var filter = GetListByKey(key);
            if (filter != null)
            {
                return SetActiveCraftList(filter);
            }

            return false;
        }

        public bool ClearActiveUiList()
        {
            if (_configuration.ActiveUiFilter != null)
            {
                var activeUiFilter = GetActiveUiList();
                _configuration.ActiveUiFilter = null;
                if (activeUiFilter != null)
                {
                    UiListToggled?.Invoke(activeUiFilter, false);
                }
                return true;
            }

            return false;
        }

        public bool ClearActiveBackgroundList()
        {
            if (_configuration.ActiveBackgroundFilter != null)
            {
                var activeBackgroundFilter = GetActiveBackgroundList();
                _configuration.ActiveBackgroundFilter = null;
                if (activeBackgroundFilter != null)
                {
                    BackgroundListToggled?.Invoke(activeBackgroundFilter, false);
                }
                return true;
            }

            return false;
        }

        public bool ClearActiveCraftList()
        {
            if (_configuration.ActiveCraftList != null)
            {
                var activeCraftList = GetActiveCraftList();
                _configuration.ActiveCraftList = null;
                if (activeCraftList != null)
                {
                    CraftListToggled?.Invoke(activeCraftList, false);
                }
                return true;
            }

            return false;
        }

        public bool ToggleActiveUiList(string name)
        {
            var activeUiFilter = GetActiveUiList();
            if (activeUiFilter != null)
            {
                ClearActiveUiList();
                if (activeUiFilter.Name != name)
                {
                    SetActiveUiList(name);
                }
                return true;
            }
            SetActiveUiList(name);
            return true;
        }

        public bool ToggleActiveUiList(FilterConfiguration configuration)
        {
            var activeUiFilter = GetActiveUiList();
            if (activeUiFilter != null)
            {
                ClearActiveUiList();
                if (!activeUiFilter.Equals(configuration))
                {
                    SetActiveUiList(configuration);
                }
                return true;
            }
            SetActiveUiList(configuration);
            return true;
        }

        public bool ToggleActiveBackgroundList(string name)
        {
            var activeBackgroundFilter = GetActiveBackgroundList();
            if (activeBackgroundFilter != null)
            {
                if (activeBackgroundFilter.Name != name)
                {
                    SetActiveBackgroundList(name);
                }
                else
                {
                    ClearActiveBackgroundList();
                }
                return true;
            }
            SetActiveBackgroundList(name);
            return true;
        }

        public bool ToggleActiveBackgroundList(FilterConfiguration configuration)
        {
            var activeBackgroundFilter = GetActiveBackgroundList();
            if (activeBackgroundFilter != null)
            {
                ClearActiveBackgroundList();
                if (activeBackgroundFilter != configuration)
                {
                    SetActiveBackgroundList(configuration);
                }
                return true;
            }
            SetActiveBackgroundList(configuration);
            return true;
        }

        public bool ToggleActiveCraftList(FilterConfiguration configuration)
        {
            var activeCraftList = GetActiveCraftList();
            if (activeCraftList != null)
            {
                ClearActiveCraftList();
                if (activeCraftList != configuration)
                {
                    SetActiveCraftList(configuration);
                }
                return true;
            }
            SetActiveCraftList(configuration);
            return true;
        }

        public bool MoveListUp(FilterConfiguration configuration)
        {
            List<FilterConfiguration> currentList;
            if (configuration.FilterType == FilterType.CraftFilter)
            {
                currentList = Lists.Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault).ToList();
            }
            else
            {
                currentList = Lists.Where(c => c.FilterType != FilterType.CraftFilter).ToList();
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
            ListRepositioned?.Invoke(configuration);
            _mediatorService.Publish(new ListRepositionedMessage(configuration));

            return true;
        }

        public bool MoveListDown(FilterConfiguration configuration)
        {
            List<FilterConfiguration> currentList;
            if (configuration.FilterType == FilterType.CraftFilter)
            {
                currentList = Lists.Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault).ToList();
            }
            else
            {
                currentList = Lists.Where(c => c.FilterType != FilterType.CraftFilter).ToList();
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
            ListRepositioned?.Invoke(configuration);

            return true;
        }

        public void InvalidateList(FilterConfiguration configuration)
        {
            configuration.NeedsRefresh = true;
            var activeFilter = GetActiveList();
            if (activeFilter == configuration)
            {
                activeFilter.NeedsRefresh = true;
            }
        }

        public void InvalidateLists(FilterType? filterType = null)
        {
            _chatUtilities.PrintLog("Filters invalidated");
            foreach (var filter in _lists)
            {
                if(filterType != null && filter.Value.FilterType != filterType) continue;
                filter.Value.NeedsRefresh = true;
            }
        }

        public void RefreshList(FilterConfiguration configuration)
        {

        }

        public void ResetFilter(IEnumerable<IFilter> toReset,FilterConfiguration configuration)
        {
            if (configuration.CraftListDefault)
            {
                configuration.CraftColumns = new List<ColumnConfiguration>();
                configuration.Columns = new List<ColumnConfiguration>();
                foreach (var filter in toReset)
                {
                    if (filter.AvailableIn.HasFlag(FilterType.CraftFilter))
                    {
                        filter.ResetFilter(configuration);
                    }
                }
                AddDefaultColumns(configuration);
                configuration.ApplyDefaultCraftFilterConfiguration();
            }
            else if (configuration.FilterType == FilterType.CraftFilter)
            {
                var defaultConfiguration = GetDefaultCraftList();
                if (configuration == defaultConfiguration)
                {
                    ResetFilter(toReset, defaultConfiguration);
                    return;
                }
                configuration.CraftColumns = new List<ColumnConfiguration>();
                configuration.Columns = new List<ColumnConfiguration>();
                foreach (var filter in toReset)
                {
                    if (filter.AvailableIn.HasFlag(FilterType.CraftFilter))
                    {
                        filter.ResetFilter(defaultConfiguration, configuration);
                    }
                }
            }
        }

        public void ResetFilter(IEnumerable<IFilter> toReset, FilterConfiguration configuration, FilterConfiguration existingConfiguration)
        {
            configuration.CraftColumns = new List<ColumnConfiguration>();
            configuration.Columns = new List<ColumnConfiguration>();
            foreach (var filter in toReset)
            {
                if (filter.AvailableIn.HasFlag(FilterType.CraftFilter))
                {
                    filter.ResetFilter(existingConfiguration, configuration);
                }
            }
        }

        public event IListService.ListAddedDelegate? ListAdded;
        public event IListService.ListRemovedDelegate? ListRemoved;
        public event IListService.ListRepositionedDelegate? ListRepositioned;
        public event IListService.ListConfigurationChangedDelegate? ListConfigurationChanged;
        public event IListService.ListTableConfigurationChangedDelegate? ListTableConfigurationChanged;
        public event IListService.ListRefreshedDelegate? ListRefreshed;
        public event IListService.ListToggledDelegate? UiListToggled;
        public event IListService.ListToggledDelegate? BackgroundListToggled;
        public event IListService.ListToggledDelegate? CraftListToggled;


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _framework.Update -= OnUpdate;
            _configurationManagerService.ConfigurationChanged -= ConfigOnConfigurationChanged;
            _characterMonitor.OnCharacterRemoved -= CharacterMonitorOnOnCharacterRemoved;
            _characterMonitor.OnCharacterUpdated -= CharacterMonitorOnOnCharacterUpdated;
            _characterMonitor.OnCharacterJobChanged -= CharacterMonitorOnOnCharacterJobChanged;
            _characterMonitor.OnActiveRetainerChanged -= CharacterMonitorOnOnActiveRetainerChanged;
            _inventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
            _characterMonitor.OnCharacterLoggedIn -= CharacterLoggedIn;
            _characterMonitor.OnCharacterLoggedOut -= CharacterLoggedOut;
            _history.OnHistoryLogged -= HistoryOnOnHistoryLogged;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
            _lists = LoadListsFromConfiguration();
            return Task.CompletedTask;
        }

        public ColumnConfiguration AddColumn(FilterConfiguration configuration, Type columnType, bool notify = true)
        {
            var column = _columnTypeFactory.Invoke(columnType);
            var newColumn = new ColumnConfiguration(columnType.Name);
            newColumn.Column = column;
            configuration.AddColumn(newColumn, notify);
            return newColumn;
        }

        public ColumnConfiguration AddCraftColumn(FilterConfiguration configuration, Type columnType, bool notify = true)
        {
            var column = _columnTypeFactory.Invoke(columnType);
            var newColumn = new ColumnConfiguration(columnType.Name);
            newColumn.Column = column;
            configuration.AddCraftColumn(newColumn, notify);
            return newColumn;
        }

        public FilterConfiguration GenerateDefaultCraftList()
        {
            var defaultFilter = new FilterConfiguration("Default Craft List", FilterType.CraftFilter);
            AddDefaultColumns(defaultFilter);
            defaultFilter.ApplyDefaultCraftFilterConfiguration();
            return defaultFilter;
        }

        public void AddRecommendedColumns(IEnumerable<IColumn> columns, FilterConfiguration configuration)
        {
            var existingColumnTypes = configuration.Columns?.Select(c => c.Column.GetType()).Distinct().ToHashSet() ?? [];
            var toAdd = columns.Where(c =>
                c.DefaultIn.HasFlag(configuration.FilterType) && !existingColumnTypes.Contains(c.GetType())).ToList();
            foreach (var newColumn in toAdd)
            {
                AddColumn(configuration, newColumn.GetType(), false);
            }
        }

        public void AddDefaultColumns(FilterConfiguration configuration)
        {
            if (configuration.FilterType == FilterType.SearchFilter)
            {
                AddColumn(configuration, typeof(FavouritesColumn));
                AddColumn(configuration,typeof(IconColumn), false);
                var nameColumn = AddColumn(configuration,typeof(NameColumn), false);
                AddColumn(configuration,typeof(TypeColumn), false);
                AddColumn(configuration,typeof(QuantityColumn), false);
                AddColumn(configuration,typeof(SourceColumn), false);
                AddColumn(configuration,typeof(LocationColumn),false);
                configuration.DefaultSortColumn = nameColumn.Key;
                configuration.DefaultSortOrder = ImGuiSortDirection.Ascending;
            }
            else if (configuration.FilterType == FilterType.SortingFilter)
            {
                AddColumn(configuration,typeof(FavouritesColumn), false);
                AddColumn(configuration,typeof(IconColumn), false);
                var nameColumn = AddColumn(configuration,typeof(NameColumn), false);
                AddColumn(configuration,typeof(TypeColumn), false);
                AddColumn(configuration,typeof(QuantityColumn), false);
                AddColumn(configuration,typeof(SourceColumn), false);
                AddColumn(configuration,typeof(LocationColumn), false);
                AddColumn(configuration,typeof(DestinationColumn),false);
                configuration.DefaultSortColumn = nameColumn.Key;
                configuration.DefaultSortOrder = ImGuiSortDirection.Ascending;
            }
            else if (configuration.FilterType == FilterType.GameItemFilter)
            {
                AddColumn(configuration,typeof(FavouritesColumn), false);
                AddColumn(configuration,typeof(IconColumn), false);
                var nameColumn = AddColumn(configuration,typeof(NameColumn), false);
                AddColumn(configuration,typeof(UiCategoryColumn), false);
                AddColumn(configuration,typeof(SearchCategoryColumn), false);
                AddColumn(configuration,typeof(ItemILevelColumn), false);
                AddColumn(configuration,typeof(ItemLevelColumn), false);
                AddColumn(configuration,typeof(RarityColumn), false);
                AddColumn(configuration,typeof(CraftColumn), false);
                AddColumn(configuration,typeof(IsCraftingItemColumn), false);
                AddColumn(configuration,typeof(CanBeGatheredColumn), false);
                AddColumn(configuration,typeof(CanBePurchasedColumn), false);
                AddColumn(configuration,typeof(AcquiredColumn), false);
                AddColumn(configuration,typeof(SellToVendorPriceColumn), false);
                AddColumn(configuration,typeof(BuyFromVendorPriceColumn), false);
                AddColumn(configuration,typeof(AcquisitionSourceIconsColumn), false);
                configuration.DefaultSortColumn = nameColumn.Key;
                configuration.DefaultSortOrder = ImGuiSortDirection.Ascending;
            }
            else if (configuration.FilterType == FilterType.CraftFilter)
            {
                AddColumn(configuration,typeof(IconColumn), false);
                var nameColumn = AddColumn(configuration,typeof(NameColumn), false);
                AddColumn(configuration,typeof(CraftAmountAvailableColumn), false);
                AddColumn(configuration,typeof(QuantityColumn), false);
                AddColumn(configuration,typeof(SourceColumn), false);
                AddColumn(configuration,typeof(LocationColumn), false);
                AddCraftColumn(configuration,typeof(IconColumn), false);
                AddCraftColumn(configuration,typeof(NameColumn), false);
                AddCraftColumn(configuration,typeof(CraftAmountRequiredColumn), false);
                AddCraftColumn(configuration,typeof(CraftSettingsColumn), false);
                AddCraftColumn(configuration,typeof(CraftSimpleColumn), false);
                AddCraftColumn(configuration,typeof(MarketBoardMinPriceColumn), false);
                AddCraftColumn(configuration,typeof(MarketBoardMinTotalPriceColumn), false);
                AddCraftColumn(configuration,typeof(CraftMarketPriceColumn), false);
                AddCraftColumn(configuration,typeof(AcquisitionSourceIconsColumn), false);
                AddCraftColumn(configuration,typeof(CraftGatherColumn), false);
                AddCraftColumn(configuration,typeof(RemoveButtonColumn),false);
                configuration.DefaultSortColumn = nameColumn.Key;
                configuration.DefaultSortOrder = ImGuiSortDirection.Ascending;
            }
            else if (configuration.FilterType == FilterType.HistoryFilter)
            {
                AddColumn(configuration,typeof(IconColumn), false);
                AddColumn(configuration,typeof(NameColumn), false);
                AddColumn(configuration,typeof(HistoryChangeAmountColumn), false);
                AddColumn(configuration,typeof(HistoryChangeReasonColumn), false);
                var changeDateColumn = AddColumn(configuration,typeof(HistoryChangeDateColumn), false);
                AddColumn(configuration,typeof(TypeColumn), false);
                AddColumn(configuration,typeof(QuantityColumn), false);
                AddColumn(configuration,typeof(SourceColumn), false);
                AddColumn(configuration,typeof(LocationColumn), false);
                configuration.DefaultSortColumn = changeDateColumn.Key;
                configuration.DefaultSortOrder = ImGuiSortDirection.Descending;
            }
            else if (configuration.FilterType == FilterType.CuratedList)
            {
                AddColumn(configuration, typeof(FavouritesColumn));
                AddColumn(configuration,typeof(IconColumn), false);
                var nameColumn = AddColumn(configuration,typeof(NameColumn), false);
                AddColumn(configuration,typeof(TypeColumn), false);
                AddColumn(configuration,typeof(QuantityColumn), false);
                AddColumn(configuration,typeof(SourceColumn), false);
                AddColumn(configuration,typeof(LocationColumn),false);
                AddColumn(configuration,typeof(RemoveButtonColumn),false);
                configuration.DefaultSortColumn = nameColumn.Key;
                configuration.DefaultSortOrder = ImGuiSortDirection.Ascending;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}