using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Logic.Features;

public class DefaultFilterGrouped : ISampleFilter
{
    private readonly FilterConfiguration.Factory _filterConfigFactory;
    private readonly SourceInventoriesFilter _sourceInventoriesFilter;
    private readonly IListService _listService;

    public DefaultFilterGrouped(FilterConfiguration.Factory filterConfigFactory, SourceInventoriesFilter sourceInventoriesFilter, IListService listService)
    {
        _filterConfigFactory = filterConfigFactory;
        _sourceInventoriesFilter = sourceInventoriesFilter;
        _listService = listService;
    }

    public bool ShouldAdd { get; set; } = false;
    public FilterConfiguration AddFilter()
    {
        var allItemsFilter = _filterConfigFactory.Invoke();
        allItemsFilter.Name = SampleDefaultName;
        allItemsFilter.FilterType = FilterType.GroupedList;
        allItemsFilter.DisplayInTabs = true;
        _sourceInventoriesFilter.UpdateFilterConfiguration(allItemsFilter, new List<InventorySearchScope>()
        {
            new()
            {
                ActiveCharacter = false
            }
        });
        _listService.AddDefaultColumns(allItemsFilter);
        _listService.AddList(allItemsFilter);
        return allItemsFilter;
    }

    public string Name => "Grouped";
    public string SampleDefaultName => "Grouped";

    public string SampleDescription =>
        "This will add a list that will be preconfigured to group items together and show their combined quantity.";

    public SampleFilterType SampleFilterType => SampleFilterType.Default;
}