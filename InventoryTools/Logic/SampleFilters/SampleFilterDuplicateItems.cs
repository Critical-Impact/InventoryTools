using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Features;

public class SampleFilterDuplicateItems : BooleanSetting, ISampleFilter
{
    private readonly IListService _listService;
    private readonly FilterConfiguration.Factory _filterConfigFactory;
    private readonly SourceInventoriesFilter _sourceInventoriesFilter;
    private readonly DestinationInventoriesFilter _destinationInventoriesFilter;

    public SampleFilterDuplicateItems(ILogger<SampleFilterDuplicateItems> logger, ImGuiService imGuiService, IListService listService, FilterConfiguration.Factory filterConfigFactory, SourceInventoriesFilter sourceInventoriesFilter, DestinationInventoriesFilter destinationInventoriesFilter) : base(logger, imGuiService)
    {
        _listService = listService;
        _filterConfigFactory = filterConfigFactory;
        _sourceInventoriesFilter = sourceInventoriesFilter;
        _destinationInventoriesFilter = destinationInventoriesFilter;
    }
    private bool _shouldAdd;
    public override bool DefaultValue { get; set; }
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return _shouldAdd;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        _shouldAdd = newValue;
    }

    public override string Key { get; set; } = "sample2";
    public override string Name { get; set; } = "Duplicate Items";
    public override string HelpText { get; set; } = "Finds any items where there are 2 seperate stacks in retainers & characters and attempts to sort them into a single stack. This is great for making sure your retainers are as compacted as possible.";
    public string SampleDefaultName => "Duplicated items";
    public string SampleDescription =>
        "This will add a list that will provide a list of all the distinct stacks that appear in 2 sets of inventories. You can use this to make sure only one retainer has a specific type of item.";
    public SampleFilterType SampleFilterType => SampleFilterType.Sample;
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.None;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.None;
    public override string Version => "1.7.0.0";
    public bool ShouldAdd => _shouldAdd;
    public FilterConfiguration AddFilter()
    {
        var sampleFilter = _filterConfigFactory.Invoke();
        sampleFilter.Name = Name;
        sampleFilter.FilterType = FilterType.SortingFilter;
        sampleFilter.DisplayInTabs = true;
        _sourceInventoriesFilter.UpdateFilterConfiguration(sampleFilter, [
            new InventorySearchScope()
            {
                ActiveCharacter = true,
                Categories = [InventoryCategory.CharacterBags, InventoryCategory.RetainerBags]
            }
        ]);
        _destinationInventoriesFilter.UpdateFilterConfiguration(sampleFilter, [
            new InventorySearchScope()
            {
                ActiveCharacter = true,
                Categories = [InventoryCategory.RetainerBags]
            }
        ]);
        sampleFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
        sampleFilter.DuplicatesOnly = true;
        sampleFilter.HighlightWhen = "Always";
        _listService.AddDefaultColumns(sampleFilter);
        _listService.AddList(sampleFilter);
        return sampleFilter;
    }
}