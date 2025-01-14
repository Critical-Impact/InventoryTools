using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Lists;

public class TableService : DisposableMediatorBackgroundService
{
    private readonly IListService _listService;
    private readonly IFramework _framework;
    private readonly Func<FilterConfiguration, CraftItemTable> _craftItemTableFactory;
    private readonly Func<FilterConfiguration, FilterTable> _filterTableFactory;
    private ConcurrentDictionary<string, FilterTable> _itemListTables;
    private ConcurrentDictionary<string, CraftItemTable> _craftItemTables;

    public delegate void TableRefreshedDelegate(RenderTableBase table);
    public event TableRefreshedDelegate TableRefreshed;
    public IBackgroundTaskQueue TableQueue { get; }

    public TableService(ILogger<TableService> logger, MediatorService mediatorService, IListService listService, IBackgroundTaskQueue filterQueue, IFramework framework, Func<FilterConfiguration, CraftItemTable> craftItemTableFactory, Func<FilterConfiguration, FilterTable> filterTableFactory) : base(logger, mediatorService)
    {
        _listService = listService;
        _framework = framework;
        _craftItemTableFactory = craftItemTableFactory;
        _filterTableFactory = filterTableFactory;
        _listService.ListConfigurationChanged += ListConfigurationChanged;
        _listService.ListTableConfigurationChanged += ListTableConfigurationChanged;
        _listService.ListRefreshed += ListRefreshed;
        _itemListTables = new ConcurrentDictionary<string, FilterTable>();
        _craftItemTables = new ConcurrentDictionary<string, CraftItemTable>();
        TableQueue = filterQueue;
        _framework.Update += OnUpdate;
    }

    private void ListRefreshed(FilterConfiguration configuration)
    {
        RefreshTables(configuration);
    }

    private void OnUpdate(IFramework framework)
    {
        foreach (var filter in _itemListTables)
        {
            if (!filter.Value.InitialColumnSetupDone)
            {
                filter.Value.RefreshColumns();
                filter.Value.InitialColumnSetupDone = true;
            }
            if (filter.Value is { NeedsRefresh: true, Refreshing: false, FilterConfiguration.AllowRefresh: true })
            {
                RequestRefresh(filter.Value);
            }
        }
        foreach (var filter in _craftItemTables)
        {
            if (!filter.Value.InitialColumnSetupDone)
            {
                filter.Value.RefreshColumns();
                filter.Value.InitialColumnSetupDone = true;
            }
            if (filter.Value is { NeedsRefresh: true, Refreshing: false, FilterConfiguration.AllowRefresh: true })
            {
                RequestRefresh(filter.Value);
            }
        }
    }

    private void ListTableConfigurationChanged(FilterConfiguration configuration)
    {
        InvalidateTables(configuration);
    }

    private void ListConfigurationChanged(FilterConfiguration configuration)
    {
        InvalidateTables(configuration);
    }

    public void RefreshColumns(RenderTableBase renderTableBase, CancellationToken cancellationToken)
    {
        var filterConfiguration = renderTableBase.FilterConfiguration;
        renderTableBase.FreezeCols = filterConfiguration.FreezeColumns;
        if (filterConfiguration.Columns != null)
        {
            renderTableBase.Columns = filterConfiguration.Columns.ToList();
        }
    }

    public void RefreshCraftColumns(RenderTableBase renderTableBase, CancellationToken cancellationToken)
    {
        var filterConfiguration = renderTableBase.FilterConfiguration;
        renderTableBase.FreezeCols = filterConfiguration.FreezeColumns;
        if (filterConfiguration.CraftColumns != null)
        {
            renderTableBase.Columns = filterConfiguration.CraftColumns.ToList();
        }
    }

    public async Task RefreshTable(CraftItemTable craftItemTable, CancellationToken cancellationToken)
    {
        var filterConfiguration = craftItemTable.FilterConfiguration;

        RefreshCraftColumns(craftItemTable, cancellationToken);

        if (filterConfiguration.SearchResults != null && filterConfiguration.CraftList.BeenGenerated && filterConfiguration.CraftList.BeenUpdated)
        {
            Logger.LogTrace("CraftTable: Refreshing");
            craftItemTable.CraftItems = filterConfiguration.CraftList.GetFlattenedMergedMaterials().Select(c => new SearchResult(c)).ToList();
            filterConfiguration.CraftList.ClearGroupCache();
            var outputList = filterConfiguration.CraftList.GetOutputList();
            craftItemTable.CraftGroups = outputList.Select(c => (c, c.CraftItems.Select(d => new SearchResult(d)).ToList())).ToList();
            craftItemTable.IsSearching = false;
            craftItemTable.NeedsRefresh = false;
            craftItemTable.Refreshing = false;
            TableRefreshed?.Invoke(craftItemTable);
        }
        else
        {
            craftItemTable.NeedsRefresh = false;
            craftItemTable.Refreshing = false;
        }
    }

    public async Task RefreshTable(FilterTable filterTable, CancellationToken cancellationToken)
    {
        RefreshColumns(filterTable, cancellationToken);

        var filterConfiguration = filterTable.FilterConfiguration;

        if (filterConfiguration.SearchResults != null)
        {
            if (filterConfiguration.FilterType == FilterType.SearchFilter
                || filterConfiguration.FilterType == FilterType.SortingFilter
                || filterConfiguration.FilterType == FilterType.CraftFilter)
            {
                var items = filterConfiguration.SearchResults.AsEnumerable();
                filterTable.IsSearching = false;
                var columns = filterTable.Columns;
                for (var index = 0; index < columns.Count; index++)
                {
                    var column = columns[index];
                    if (column.FilterText != "")
                    {
                        filterTable.IsSearching = true;
                    }
                    column.Column.InvalidateSearchCache();
                    items = column.Column.Filter(column, items);
                    if (filterTable.SortColumn != null && index == filterTable.SortColumn)
                    {
                        items = column.Column.Sort(column, filterTable.SortDirection ?? ImGuiSortDirection.None, items);
                    }
                }

                filterTable.SearchResults = items.ToList();
                filterTable.RenderSearchResults = filterTable.SearchResults.Where(item => !item.InventoryItem?.IsEmpty ?? false).ToList();
                filterTable.NeedsRefresh = false;
                TableRefreshed?.Invoke(filterTable);
            }
            else if(filterConfiguration.FilterType == FilterType.GameItemFilter)
            {
                var items = filterConfiguration.SearchResults.AsEnumerable();
                filterTable.IsSearching = false;
                var columns = filterTable.Columns;
                for (var index = 0; index < columns.Count; index++)
                {
                    var column = columns[index];
                    if (column.FilterText != "")
                    {
                        filterTable.IsSearching = true;
                    }
                    column.Column.InvalidateSearchCache();
                    items = column.Column.Filter(column, (IEnumerable<SearchResult>)items);
                    if (filterTable.SortColumn != null && index == filterTable.SortColumn)
                    {
                        items = column.Column.Sort(column, filterTable.SortDirection ?? ImGuiSortDirection.None, (IEnumerable<SearchResult>)items);
                    }
                }

                filterTable.SearchResults = items.Where(c => c.Item.NameString.ToString() != "").ToList();
                filterTable.RenderSearchResults = filterTable.SearchResults.ToList();
                filterTable.NeedsRefresh = false;
                TableRefreshed?.Invoke(filterTable);
            }
            else if(filterConfiguration.FilterType == FilterType.CuratedList)
            {
                var items = filterConfiguration.SearchResults.AsEnumerable();
                filterTable.IsSearching = false;
                var columns = filterTable.Columns;
                for (var index = 0; index < columns.Count; index++)
                {
                    var column = columns[index];
                    if (column.FilterText != "")
                    {
                        filterTable.IsSearching = true;
                    }

                    items = column.Column.Filter(column, items);
                    if (filterTable.SortColumn != null && index == filterTable.SortColumn)
                    {
                        items = column.Column.Sort(column, filterTable.SortDirection ?? ImGuiSortDirection.None, items);
                    }
                }
                filterTable.SearchResults = items.ToList();
                filterTable.RenderSearchResults = filterTable.SearchResults.ToList();
                filterTable.NeedsRefresh = false;
                TableRefreshed?.Invoke(filterTable);
            }
            else
            {
                var items = filterConfiguration.SearchResults.AsEnumerable();
                filterTable.IsSearching = false;
                var columns = filterTable.Columns;
                for (var index = 0; index < columns.Count; index++)
                {
                    var column = columns[index];
                    if (column.FilterText != "")
                    {
                        filterTable.IsSearching = true;
                    }
                    column.Column.InvalidateSearchCache();
                    items = column.Column.Filter(column, items);
                    if (filterTable.SortColumn != null && index == filterTable.SortColumn)
                    {
                        items = column.Column.Sort(column, filterTable.SortDirection ?? ImGuiSortDirection.None, items);
                    }
                }
                filterTable.SearchResults = items.Where(c => (c.InventoryItem?.FormattedName ?? "") != "").ToList();
                filterTable.RenderSearchResults = filterTable.SearchResults.ToList();
                filterTable.NeedsRefresh = false;
                TableRefreshed?.Invoke(filterTable);
            }
        }
        filterTable.NeedsRefresh = false;
        filterTable.Refreshing = false;
    }



    public CraftItemTable GetCraftTable(FilterConfiguration configuration)
    {

        var filterKey = configuration.Key;
        if (!_craftItemTables.ContainsKey(filterKey))
        {
            CraftItemTable newTable = _craftItemTableFactory.Invoke(configuration);
            newTable.NeedsRefresh = true;
            _craftItemTables[filterKey] = newTable;
        }
        return _craftItemTables[filterKey];
    }

    public FilterTable GetListTable(FilterConfiguration configuration)
    {
        var filterKey = configuration.Key;
        if (!_itemListTables.ContainsKey(filterKey))
        {
            FilterTable newTable = _filterTableFactory.Invoke(configuration);
            newTable.NeedsRefresh = true;
            _itemListTables[filterKey] = newTable;
        }
        return _itemListTables[filterKey];
    }

    public bool HasCraftTable(FilterConfiguration configuration)
    {
        return _craftItemTables.ContainsKey(configuration.Key);
    }

    public bool HasListTable(FilterConfiguration configuration)
    {
        return _itemListTables.ContainsKey(configuration.Key);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await BackgroundProcessing(stoppingToken);
    }

    public void RefreshTables(FilterConfiguration filterConfiguration)
    {
        var filterTable = GetListTable(filterConfiguration);
        if (filterTable is { FilterConfiguration.AllowRefresh: true })
        {
            filterTable.NeedsRefresh = true;
            if (filterConfiguration.FilterType == FilterType.CraftFilter)
            {
                var craftItemTable = GetCraftTable(filterConfiguration);
                craftItemTable.NeedsRefresh = true;
            }
        }
    }

    public void InvalidateTables(FilterConfiguration filterConfiguration)
    {
        var filterTable = GetListTable(filterConfiguration);
        if (filterTable is { FilterConfiguration.AllowRefresh: true })
        {
            filterTable.NeedsRefresh = true;
            filterTable.InitialColumnSetupDone = false;
            if (filterConfiguration.FilterType == FilterType.CraftFilter)
            {
                var craftItemTable = GetCraftTable(filterConfiguration);
                craftItemTable.NeedsRefresh = true;
                craftItemTable.InitialColumnSetupDone = false;
            }
        }
    }

    public Task RequestRefresh(FilterTable filterTable)
    {
        if (filterTable is { NeedsRefresh: true, Refreshing: false, FilterConfiguration.AllowRefresh: true })
        {
            filterTable.Refreshing = true;
            return TableQueue.QueueBackgroundWorkItemAsync(token => Task.Run(() => RefreshTable(filterTable, token), token));
        }

        return Task.CompletedTask;
    }

    public Task RequestRefresh(CraftItemTable craftItemTable)
    {
        if (craftItemTable is { NeedsRefresh: true, Refreshing: false, FilterConfiguration.AllowRefresh: true })
        {
            craftItemTable.Refreshing = true;
            return TableQueue.QueueBackgroundWorkItemAsync(token => Task.Run(() => RefreshTable(craftItemTable, token), token));
        }

        return Task.CompletedTask;
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem =
                await TableQueue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error occurred executing {WorkItem}.", nameof(workItem));
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _framework.Update -= OnUpdate;
        _listService.ListConfigurationChanged -= ListConfigurationChanged;
        _listService.ListTableConfigurationChanged -= ListTableConfigurationChanged;
        _listService.ListRefreshed -= ListRefreshed;
    }
}