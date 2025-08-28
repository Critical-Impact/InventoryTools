using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Logic.Features;

public class DefaultFilterHousing : ISampleFilter
{
    private readonly FilterConfiguration.Factory _filterConfigFactory;
    private readonly SourceInventoriesFilter _sourceInventoriesFilter;
    private readonly IListService _listService;

    public DefaultFilterHousing(FilterConfiguration.Factory filterConfigFactory, SourceInventoriesFilter sourceInventoriesFilter, IListService listService)
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
        allItemsFilter.FilterType = FilterType.SearchFilter;
        allItemsFilter.DisplayInTabs = true;
        _sourceInventoriesFilter.UpdateFilterConfiguration(allItemsFilter, new List<InventorySearchScope>()
        {
            new()
            {
                ActiveCharacter = true,
                CharacterTypes = [CharacterType.Housing]
            }
        });
        _listService.AddDefaultColumns(allItemsFilter);
        _listService.AddList(allItemsFilter);
        return allItemsFilter;
    }

    public string Name => "Housing";
    public string SampleDefaultName => "Housing";

    public string SampleDescription =>
        "This will add a list that will be preconfigured to show all items stored in the houses your character owns.";

    public SampleFilterType SampleFilterType => SampleFilterType.Default;
}