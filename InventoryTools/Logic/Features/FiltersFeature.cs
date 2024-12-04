using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Features;

public class FiltersFeature : Feature
{
    public FiltersFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(SampleFilter1Setting),
            typeof(SampleFilter2Setting),
            typeof(SampleFilter3Setting),
        },
        settings)
    {
    }

    public override string Name { get; } = "Sample Item Lists";
    public override string Description { get; } = "Select which sample item lists you'd like to install by default. These are good examples of the types of lists that are possible within Allagan Tools.";

    public override void OnFinish()
    {
        foreach (var setting in RelatedSettings.Select(c => c as ISampleFilterSetting))
        {
            if (setting != null && setting.ShouldAdd)
            {
                setting.AddFilter();
            }
        }
    }

}

public interface ISampleFilterSetting
{
    public bool ShouldAdd { get; }
    public void AddFilter();
}

public class SampleFilter1Setting : BooleanSetting, ISampleFilterSetting
{
    private readonly IListService _listService;
    private readonly BuyFromVendorPriceFilter buyFromVendorPriceFilter;

    public SampleFilter1Setting(ILogger<SampleFilter1Setting> logger, ImGuiService imGuiService, IListService listService, BuyFromVendorPriceFilter buyFromVendorPriceFilter) : base(logger, imGuiService)
    {
        _listService = listService;
        this.buyFromVendorPriceFilter = buyFromVendorPriceFilter;
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
    public override string HelpText { get; set; } = "Shows you any items that sell for under 100 gil at shops.";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.None;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.None;
    public override string Version => "1.7.0.0";
    public bool ShouldAdd => _shouldAdd;
    public void AddFilter()
    {
        var sampleFilter = new FilterConfiguration(Name, FilterType.SearchFilter);
        sampleFilter.DisplayInTabs = true;
        sampleFilter.SourceAllCharacters = true;
        sampleFilter.SourceAllRetainers = true;
        sampleFilter.SourceAllFreeCompanies = true;
        sampleFilter.CanBeBought = true;
        buyFromVendorPriceFilter.UpdateFilterConfiguration(sampleFilter, "<=100");
        _listService.AddDefaultColumns(sampleFilter);
        _listService.AddList(sampleFilter);
    }
}

public class SampleFilter2Setting : BooleanSetting, ISampleFilterSetting
{
    private readonly IListService _listService;

    public SampleFilter2Setting(ILogger<SampleFilter2Setting> logger, ImGuiService imGuiService, IListService listService) : base(logger, imGuiService)
    {
        _listService = listService;
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
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.None;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.None;
    public override string Version => "1.7.0.0";
    public bool ShouldAdd => _shouldAdd;
    public void AddFilter()
    {
        var sampleFilter = new FilterConfiguration(Name, FilterType.SortingFilter);
        sampleFilter.DisplayInTabs = true;
        sampleFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags,InventoryCategory.RetainerBags};
        sampleFilter.DestinationCategories =  new HashSet<InventoryCategory>() {InventoryCategory.RetainerBags};
        sampleFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
        sampleFilter.DuplicatesOnly = true;
        sampleFilter.HighlightWhen = "Always";
        _listService.AddDefaultColumns(sampleFilter);
        _listService.AddList(sampleFilter);
    }
}

public class SampleFilter3Setting : BooleanSetting, ISampleFilterSetting
{
    private readonly IListService _listService;
    private readonly Func<ItemInfoRenderCategory, GenericHasSourceCategoryFilter> _hasSourceCategoryFactory;

    public SampleFilter3Setting(ILogger<SampleFilter3Setting> logger, ImGuiService imGuiService, IListService listService, Func<ItemInfoRenderCategory, GenericHasSourceCategoryFilter> hasSourceCategoryFactory) : base(logger, imGuiService)
    {
        _listService = listService;
        _hasSourceCategoryFactory = hasSourceCategoryFactory;
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
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.None;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.None;
    public override string Version => "1.7.0.0";
    public bool ShouldAdd => _shouldAdd;
    public void AddFilter()
    {
        var sampleFilter = new FilterConfiguration(Name, FilterType.SortingFilter);
        sampleFilter.DisplayInTabs = true;
        sampleFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags};
        sampleFilter.DestinationCategories =  new HashSet<InventoryCategory>() {InventoryCategory.RetainerBags};
        sampleFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
        sampleFilter.HighlightWhen = "Always";
        var gatherFilter = _hasSourceCategoryFactory.Invoke(ItemInfoRenderCategory.Gathering);
        gatherFilter.UpdateFilterConfiguration(sampleFilter, true);
        _listService.AddDefaultColumns(sampleFilter);
        _listService.AddList(sampleFilter);
    }
}

//Need to add in hide category or make category optional/null
//Need to add in put in armoire/glamour sample
//Maybe other samples?
