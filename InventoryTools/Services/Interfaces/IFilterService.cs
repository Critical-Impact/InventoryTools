using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using InventoryTools.Logic;

namespace InventoryTools.Services.Interfaces
{
    public interface IFilterService : IDisposable
    {
        ConcurrentDictionary<string, FilterConfiguration> Filters { get; }
        List<FilterConfiguration> FiltersList { get; }

        bool AddFilter(FilterConfiguration configuration);
        bool AddFilter(string name, FilterType filterType);
        FilterConfiguration DuplicateFilter(FilterConfiguration configuration, string newName);
        FilterConfiguration AddNewCraftFilter(string? name = null,bool? isEphemeral = false);
        bool RemoveFilter(FilterConfiguration configuration);
        bool RemoveFilter(string name);
        bool RemoveFilterByKey(string key);

        FilterConfiguration? GetActiveUiFilter(bool ignoreWindowState);
        FilterConfiguration? GetActiveBackgroundFilter();
        FilterConfiguration? GetActiveCraftList();

        FilterConfiguration? GetActiveFilter();
        bool HasActiveFilter();
        bool HasActiveUiFilter();
        bool HasActiveBackgroundFilter();
        bool HasActiveCraftList();

        FilterConfiguration? GetFilter(string name);
        FilterConfiguration? GetFilterByKey(string key);
        FilterConfiguration? GetFilterByKeyOrName(string keyOrName);
        bool SetActiveUiFilter(string name);
        bool SetActiveUiFilter(FilterConfiguration configuration);
        bool SetActiveUiFilterByKey(string key);
        bool SetActiveBackgroundFilter(string name);
        bool SetActiveBackgroundFilter(FilterConfiguration configuration);
        bool SetActiveBackgroundFilterByKey(string key);
        bool SetActiveCraftList(FilterConfiguration configuration);
        bool SetActiveCraftListByKey(string key);
        bool ClearActiveUiFilter();
        bool ClearActiveBackgroundFilter();
        bool ClearActiveCraftList();
        bool ToggleActiveUiFilter(string name);
        bool ToggleActiveUiFilter(FilterConfiguration configuration);
        bool ToggleActiveBackgroundFilter(string name);
        bool ToggleActiveBackgroundFilter(FilterConfiguration configuration);
        bool ToggleActiveCraftList(FilterConfiguration configuration);

        bool MoveFilterUp(FilterConfiguration configuration);

        bool MoveFilterDown(FilterConfiguration configuration);

        void InvalidateFilter(FilterConfiguration configuration);

        void InvalidateFilters(FilterType? filterType = null);
        
        delegate void FilterAddedDelegate(FilterConfiguration configuration);
        delegate void FilterRemovedDelegate(FilterConfiguration configuration);
        delegate void FilterModifiedDelegate(FilterConfiguration configuration);
        delegate void FilterRecalculatedDelegate(FilterConfiguration configuration);
        delegate void FilterRepositionedDelegate(FilterConfiguration configuration);
        delegate void FiltersInvalidatedDelegate();
        delegate void FilterInvalidatedDelegate(FilterConfiguration configuration);
        delegate void FilterToggledDelegate(FilterConfiguration configuration, bool newState);
        delegate void FilterTableRefreshedDelegate(RenderTableBase table);

        CraftItemTable? GetCraftTable(FilterConfiguration configuration);
        FilterTable? GetFilterTable(FilterConfiguration configuration);

        CraftItemTable? GetCraftTable(string filterKey);
        FilterTable? GetFilterTable(string filterKey);

        bool HasCraftTable(FilterConfiguration configuration);
        bool HasFilterTable(FilterConfiguration configuration);
        
        bool HasCraftTable(string filterKey);
        bool HasFilterTable(string filterKey);
        

        event FilterAddedDelegate FilterAdded;
        event FilterRemovedDelegate FilterRemoved;
        event FilterModifiedDelegate FilterModified;
        event FiltersInvalidatedDelegate FiltersInvalidated;
        event FilterInvalidatedDelegate FilterInvalidated;
        event FilterRecalculatedDelegate FilterRecalculated;
        event FilterToggledDelegate UiFilterToggled;
        event FilterToggledDelegate BackgroundFilterToggled;
        event FilterToggledDelegate CraftListToggled;
        event FilterTableRefreshedDelegate FilterTableRefreshed;
        event FilterRepositionedDelegate FilterRepositioned;

        FilterConfiguration GetDefaultCraftList();
    }
}