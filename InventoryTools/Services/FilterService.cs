using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Filters;

namespace InventoryTools.Services;

public interface IFilterService
{
    List<IFilter> AvailableFilters { get; }
    Dictionary<FilterCategory, List<IFilter>> GroupedFilters { get; }
}

public class FilterService : IFilterService
{
    public readonly List<FilterCategory> FilterCategoryOrder = new() {FilterCategory.Basic, FilterCategory.Columns,FilterCategory.CraftColumns, FilterCategory.IngredientSourcing,FilterCategory.ZonePreference, FilterCategory.Inventories, FilterCategory.Display, FilterCategory.Acquisition, FilterCategory.Searching, FilterCategory.Market, FilterCategory.Searching, FilterCategory.Crafting, FilterCategory.Gathering, FilterCategory.Advanced};
    public FilterService(IEnumerable<IFilter> filters)
    {
        _availableFilters = filters.ToList();
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
                _groupedFilters = AvailableFilters.OrderBy(c => c.Order).ThenBy(c => c.Name).GroupBy(c => c.FilterCategory).OrderBy(c => FilterCategoryOrder.IndexOf(c.Key)).ToDictionary(c => c.Key, c => c.ToList());
            }

            return _groupedFilters;
        }
    }
}