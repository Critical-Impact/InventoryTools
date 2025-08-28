using System;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Features;

public class SampleFilter100GillOrLess : BooleanSetting, ISampleFilter
{
    private readonly IListService _listService;
    private readonly BuyFromVendorPriceFilter _buyFromVendorPriceFilter;
    private readonly FilterConfiguration.Factory _filterConfigFactory;
    private readonly Lazy<SourceInventoriesFilter> _sourceInventoriesFilter;

    public SampleFilter100GillOrLess(ILogger<SampleFilter100GillOrLess> logger, ImGuiService imGuiService, IListService listService, BuyFromVendorPriceFilter buyFromVendorPriceFilter, FilterConfiguration.Factory filterConfigFactory, Lazy<SourceInventoriesFilter> sourceInventoriesFilter) : base(logger, imGuiService)
    {
        _listService = listService;
        _buyFromVendorPriceFilter = buyFromVendorPriceFilter;
        _filterConfigFactory = filterConfigFactory;
        _sourceInventoriesFilter = sourceInventoriesFilter;
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

    public override string Key { get; set; } = "sample1";
    public override string Name { get; set; } = "100 gil or less";
    public string SampleDefaultName => "100 gil or less";

    public string SampleDescription =>
        "This will add a list that will show all items that can be purchased from gil shops under 100 gil. It will look in both character and retainer inventories.";

    public SampleFilterType SampleFilterType => SampleFilterType.Sample;
    public override string HelpText { get; set; } = "Shows you any items that sell for under 100 gil at shops.";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.None;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.None;
    public override string Version => "1.7.0.0";
    public bool ShouldAdd => _shouldAdd;
    public FilterConfiguration AddFilter()
    {
        var sampleFilter = _filterConfigFactory.Invoke();
        sampleFilter.Name = Name;
        sampleFilter.FilterType = FilterType.SearchFilter;
        sampleFilter.DisplayInTabs = true;
        _sourceInventoriesFilter.Value.UpdateFilterConfiguration(sampleFilter, [
            new InventorySearchScope()
            {
                ActiveCharacter = true,
                CharacterTypes = [CharacterType.Character, CharacterType.Retainer, CharacterType.FreeCompanyChest]
            }
        ]);
        _buyFromVendorPriceFilter.UpdateFilterConfiguration(sampleFilter, "<=100");
        _listService.AddDefaultColumns(sampleFilter);
        _listService.AddList(sampleFilter);
        return sampleFilter;
    }
}