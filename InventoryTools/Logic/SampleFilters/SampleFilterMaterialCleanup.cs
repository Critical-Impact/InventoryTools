using System;
using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Features;

public class SampleFilterMaterialCleanup : BooleanSetting, ISampleFilter
{
    private readonly IListService _listService;
    private readonly Func<ItemInfoRenderCategory, GenericHasSourceCategoryFilter> _hasSourceCategoryFactory;
    private readonly FilterConfiguration.Factory _filterConfigFactory;
    private readonly SourceInventoriesFilter _sourceInventoriesFilter;
    private readonly DestinationInventoriesFilter _destinationInventoriesFilter;
    private readonly HighlightWhenFilter _highlightWhenFilter;

    public SampleFilterMaterialCleanup(ILogger<SampleFilterMaterialCleanup> logger, ImGuiService imGuiService,
        IListService listService, Func<ItemInfoRenderCategory, GenericHasSourceCategoryFilter> hasSourceCategoryFactory,
        FilterConfiguration.Factory filterConfigFactory, SourceInventoriesFilter sourceInventoriesFilter,
        DestinationInventoriesFilter destinationInventoriesFilter, HighlightWhenFilter highlightWhenFilter) : base(logger, imGuiService)
    {
        _listService = listService;
        _hasSourceCategoryFactory = hasSourceCategoryFactory;
        _filterConfigFactory = filterConfigFactory;
        _sourceInventoriesFilter = sourceInventoriesFilter;
        _destinationInventoriesFilter = destinationInventoriesFilter;
        _highlightWhenFilter = highlightWhenFilter;
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

    public override string Key { get; set; } = "sample3";
    public override string Name { get; set; } = "Material clean-up";
    public override string HelpText { get; set; } = "Finds all gatherable items in your characters inventory and attempts to show you where to put them in your retainers.";
    public string SampleDefaultName => "100 gil or less";
    public string SampleDescription =>
        "This will add a list that will be setup to quickly put away any excess materials. It will have all the material categories automatically added. When calculating where to put items it will try to prioritise existing stacks of items.";
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
                Categories = [InventoryCategory.CharacterBags]
            }
        ]);
        _destinationInventoriesFilter.UpdateFilterConfiguration(sampleFilter, [
            new InventorySearchScope()
            {
                ActiveCharacter = true,
                Categories = [InventoryCategory.RetainerBags]
            }
        ]);
        _highlightWhenFilter.UpdateFilterConfiguration(sampleFilter, HighlightWhen.Always);

        sampleFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
        var gatherFilter = _hasSourceCategoryFactory.Invoke(ItemInfoRenderCategory.Gathering);
        gatherFilter.UpdateFilterConfiguration(sampleFilter, true);
        _listService.AddDefaultColumns(sampleFilter);
        _listService.AddList(sampleFilter);
        return sampleFilter;
    }
}