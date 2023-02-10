using System.Collections.Concurrent;
using InventoryTools.Logic;
using InventoryTools.Services;

namespace InventoryToolsMock;

public class MockFilterService : IFilterService
{
    public void Dispose()
    {
        
    }

    public MockFilterService()
    {

    }

    private ConcurrentDictionary<string, FilterConfiguration>? _filters;
    public ConcurrentDictionary<string, FilterConfiguration> Filters
    {
        get
        {
            if (_filters == null)
            {
                _filters = new ConcurrentDictionary<string, FilterConfiguration>();
                var filterConfiguration = new FilterConfiguration("All", FilterType.SearchFilter);
                filterConfiguration.DisplayInTabs = true;
                _filters.TryAdd("test_filter", filterConfiguration);
            }
            return _filters;
        }
    }

    public List<FilterConfiguration> FiltersList
    {
        get
        {
            return Filters.Values.ToList();
        }
    }
    public bool AddFilter(FilterConfiguration configuration)
    {
        return true;
    }

    public bool AddFilter(string name, FilterType filterType)
    {
        return true;
    }

    public FilterConfiguration AddNewCraftFilter()
    {
        return new FilterConfiguration();
    }

    public bool RemoveFilter(FilterConfiguration configuration)
    {
        return true;
    }

    public bool RemoveFilter(string name)
    {
        return true;
    }

    public bool RemoveFilterByKey(string key)
    {
        return true;
    }

    public FilterConfiguration? GetActiveUiFilter(bool ignoreWindowState)
    {
        return null;
    }

    public FilterConfiguration? GetActiveBackgroundFilter()
    {
        return null;
    }

    public FilterConfiguration? GetActiveFilter()
    {
        return null;
    }

    public FilterConfiguration? GetFilter(string name)
    {
        return _filters.ContainsKey(name) ? _filters[name] : null;
    }

    public FilterConfiguration? GetFilterByKey(string key)
    {
        return null;
    }

    public FilterConfiguration? GetFilterByKeyOrName(string keyOrName)
    {
        return null;
    }

    public bool SetActiveUiFilter(string name)
    {
        return true;
    }

    public bool SetActiveUiFilter(FilterConfiguration configuration)
    {
        return true;
    }

    public bool SetActiveUiFilterByKey(string key)
    {
        return true;
    }

    public bool SetActiveBackgroundFilter(string name)
    {
        return true;
    }

    public bool SetActiveBackgroundFilter(FilterConfiguration configuration)
    {
        return true;
    }

    public bool SetActiveBackgroundFilterByKey(string key)
    {
        return true;
    }

    public bool ClearActiveUiFilter()
    {
        return true;
    }

    public bool ClearActiveBackgroundFilter()
    {
        return true;
    }

    public bool ToggleActiveUiFilter(string name)
    {
        return true;
    }

    public bool ToggleActiveUiFilter(FilterConfiguration configuration)
    {
        return true;
    }

    public bool ToggleActiveBackgroundFilter(string name)
    {
        return true;
    }

    public bool ToggleActiveBackgroundFilter(FilterConfiguration configuration)
    {
        return true;
    }

    public bool MoveFilterUp(FilterConfiguration configuration)
    {
        return true;
    }

    public bool MoveFilterDown(FilterConfiguration configuration)
    {
        return true;
    }

    public void InvalidateFilter(FilterConfiguration configuration)
    {
    }

    public void InvalidateFilters()
    {
    }

    public CraftItemTable? GetCraftTable(FilterConfiguration configuration)
    {
        return configuration.GenerateCraftTable();
    }

    public FilterTable? GetFilterTable(FilterConfiguration configuration)
    {
        return configuration.GenerateTable();
    }

    public CraftItemTable? GetCraftTable(string filterKey)
    {
        return null;
    }

    public FilterTable? GetFilterTable(string filterKey)
    {
        return null;
    }

    public bool HasCraftTable(FilterConfiguration configuration)
    {
        return true;
    }

    public bool HasFilterTable(FilterConfiguration configuration)
    {
        return true;
    }

    public bool HasCraftTable(string filterKey)
    {
        return true;
    }

    public bool HasFilterTable(string filterKey)
    {
        return true;
    }

    public event IFilterService.FilterAddedDelegate? FilterAdded;
    public event IFilterService.FilterRemovedDelegate? FilterRemoved;
    public event IFilterService.FilterModifiedDelegate? FilterModified;
    public event IFilterService.FiltersInvalidatedDelegate? FiltersInvalidated;
    public event IFilterService.FilterInvalidatedDelegate? FilterInvalidated;
    public event IFilterService.FilterToggledDelegate? UiFilterToggled;
    public event IFilterService.FilterToggledDelegate? BackgroundFilterToggled;
    public event IFilterService.FilterTableRefreshedDelegate? FilterTableRefreshed;
    public event IFilterService.FilterRepositionedDelegate? FilterRepositioned;
    public FilterConfiguration? GetDefaultCraftList()
    {
        return new FilterConfiguration();
    }
}