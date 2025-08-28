using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Logic.Features;

public class DefaultFilterAllGameItems : ISampleFilter
{
    private readonly FilterConfiguration.Factory _filterConfigFactory;
    private readonly SourceInventoriesFilter _sourceInventoriesFilter;
    private readonly IListService _listService;

    public DefaultFilterAllGameItems(FilterConfiguration.Factory filterConfigFactory, SourceInventoriesFilter sourceInventoriesFilter, IListService listService)
    {
        _filterConfigFactory = filterConfigFactory;
        _sourceInventoriesFilter = sourceInventoriesFilter;
        _listService = listService;
    }
    public bool ShouldAdd { get; set; }
    public FilterConfiguration AddFilter()
    {
        var allItemsFilter = _filterConfigFactory.Invoke();
        allItemsFilter.Name = SampleDefaultName;
        allItemsFilter.FilterType = FilterType.GameItemFilter;
        allItemsFilter.DisplayInTabs = true;
        _listService.AddDefaultColumns(allItemsFilter);
        _listService.AddList(allItemsFilter);
        return allItemsFilter;
    }

    public string Name => "Full Item Catalog";
    public string SampleDefaultName => "Full Item Catalog";

    public string SampleDescription =>
        "This will add a list that will be preconfigured to show all of the game's items.";

    public SampleFilterType SampleFilterType => SampleFilterType.Default;
}