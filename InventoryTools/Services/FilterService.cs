using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using Autofac;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.GenericFilters;
using InventoryTools.Logic.ItemRenderers;

namespace InventoryTools.Services;

public interface IFilterService
{
    List<IFilter> AvailableFilters { get; }
    Dictionary<FilterCategory, List<IFilter>> GroupedFilters { get; }
}

public class FilterService : IFilterService
{
    private readonly GenericBooleanFilter.Factory _booleanFilterFactory;
    private readonly GenericHasSourceFilter.Factory _hasSourceFactory;
    private readonly GenericHasUseFilter.Factory _hasUseFactory;
    private readonly GenericHasSourceCategoryFilter.Factory _hasSourceCategoryFactory;
    private readonly GenericHasUseCategoryFilter.Factory _hasUseCategoryFactory;
    public readonly List<FilterCategory> FilterCategoryOrder = new() { FilterCategory.Settings, FilterCategory.Display, FilterCategory.Inventories, FilterCategory.Columns,FilterCategory.CraftColumns,  FilterCategory.Basic, FilterCategory.Sources, FilterCategory.SourceCategories, FilterCategory.Uses, FilterCategory.UseCategories, FilterCategory.IngredientSourcing,FilterCategory.ZonePreference,FilterCategory.WorldPricePreference, FilterCategory.Acquisition, FilterCategory.Searching, FilterCategory.Market, FilterCategory.Searching, FilterCategory.Crafting, FilterCategory.Gathering, FilterCategory.Advanced};
    public FilterService(IEnumerable<IFilter> filters,
        GenericBooleanFilter.Factory booleanFilterFactory,
        GenericHasSourceFilter.Factory hasSourceFactory,
        GenericHasUseFilter.Factory hasUseFactory,
        GenericHasSourceCategoryFilter.Factory hasSourceCategoryFactory,
        GenericHasUseCategoryFilter.Factory hasUseCategoryFactory,
        ItemInfoRenderService itemInfoRenderService)
    {
        _booleanFilterFactory = booleanFilterFactory;
        _hasSourceFactory = hasSourceFactory;
        _hasUseFactory = hasUseFactory;
        _hasSourceCategoryFactory = hasSourceCategoryFactory;
        _hasUseCategoryFactory = hasUseCategoryFactory;

        _availableFilters = filters.ToList();

        _availableFilters.Add(_booleanFilterFactory.Invoke("grCombined", "Glamour Ready Combined",
            "Is the item combined in the glamour chest?", FilterCategory.Basic,
            item => item.SortedCategory == InventoryCategory.GlamourChest && item.GlamourId != 0, null));

        foreach (var itemInfoType in Enum.GetValues<ItemInfoType>())
        {
            if (itemInfoRenderService.HasSourceRenderer(itemInfoType))
            {
                var genericFilter = _hasSourceFactory.Invoke(itemInfoType);
                _availableFilters.Add(genericFilter);
            }
            if (itemInfoRenderService.HasUseRenderer(itemInfoType))
            {
                var genericFilter = _hasUseFactory.Invoke(itemInfoType);
                _availableFilters.Add(genericFilter);
            }
        }
        foreach (var category in Enum.GetValues<ItemInfoRenderCategory>())
        {
            if (itemInfoRenderService.GetSourcesByCategory(category).Count != 0)
            {
                var genericFilter = hasSourceCategoryFactory.Invoke(category);
                _availableFilters.Add(genericFilter);
            }

            if (itemInfoRenderService.GetUsesByCategory(category).Count != 0)
            {
                var genericFilter = hasUseCategoryFactory.Invoke(category);
                _availableFilters.Add(genericFilter);
            }
        }
    }

    public List<IFilter> AvailableFilters => _availableFilters;

    private Dictionary<FilterCategory, List<IFilter>>? _groupedFilters;
    private List<IFilter> _availableFilters;

    public Dictionary<FilterCategory, List<IFilter>> GroupedFilters
    {
        get
        {
            if (_groupedFilters == null)
            {
                _groupedFilters = AvailableFilters.OrderBy(c => c.Order).ThenBy(c => c.Name).GroupBy(c => c.FilterCategory).OrderBy(c => FilterCategoryOrder.IndexOf(c.Key)).ToDictionary(c => c.Key, c => c.OrderBy(d => d.Name).ToList());
            }

            return _groupedFilters;
        }
    }
}