using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using Autofac;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.ItemRenderers;

namespace InventoryTools.Services;

public interface IFilterService
{
    List<IFilter> AvailableFilters { get; }
    Dictionary<FilterCategory, List<IFilter>> GroupedFilters { get; }
}

public class FilterService : IFilterService
{
    public readonly List<FilterCategory> FilterCategoryOrder = new() { FilterCategory.Settings, FilterCategory.Display, FilterCategory.Inventories, FilterCategory.Columns,FilterCategory.CraftColumns,  FilterCategory.Basic, FilterCategory.Sources, FilterCategory.SourceCategories, FilterCategory.Uses, FilterCategory.UseCategories, FilterCategory.IngredientSourcing,FilterCategory.ZonePreference,FilterCategory.WorldPricePreference, FilterCategory.Acquisition, FilterCategory.Searching, FilterCategory.Market, FilterCategory.Searching, FilterCategory.Crafting, FilterCategory.Gathering, FilterCategory.Advanced};
    public FilterService(IEnumerable<IFilter> filters, IComponentContext componentContext, ItemInfoRenderService itemInfoRenderService)
    {
        _availableFilters = filters.ToList();
        foreach (var itemInfoType in Enum.GetValues<ItemInfoType>())
        {
            if (itemInfoRenderService.HasSourceRenderer(itemInfoType))
            {
                var genericFilter =
                    componentContext.Resolve<GenericHasSourceFilter>(new NamedParameter("itemType", itemInfoType));
                _availableFilters.Add(genericFilter);
            }
            if (itemInfoRenderService.HasUseRenderer(itemInfoType))
            {
                var genericFilter =
                    componentContext.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", itemInfoType));
                _availableFilters.Add(genericFilter);
            }
        }
        foreach (var category in Enum.GetValues<ItemInfoRenderCategory>())
        {
            if (itemInfoRenderService.GetSourcesByCategory(category).Count != 0)
            {
                var genericFilter =
                    componentContext.Resolve<GenericHasSourceCategoryFilter>(new NamedParameter("renderCategory",
                        category));
                _availableFilters.Add(genericFilter);
            }

            if (itemInfoRenderService.GetUsesByCategory(category).Count != 0)
            {
                var genericFilter =
                    componentContext.Resolve<GenericHasUseCategoryFilter>(
                        new NamedParameter("renderCategory", category));
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