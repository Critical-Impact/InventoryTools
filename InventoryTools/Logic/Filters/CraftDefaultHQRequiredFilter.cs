using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters;

public class CraftDefaultHQRequiredFilter : BooleanFilter
{
    public override bool? DefaultValue { get; set; } = false;
    public override string Key { get; set; } = "CraftDefaultHqRequired";
    public override string Name { get; set; } = "HQ Required";
    public override string HelpText { get; set; } = "Should each item in the list require a HQ version of the item(if applicable)?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        return null;
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
    {
        configuration.CraftList.HQRequired = newValue ?? false;
        configuration.NotifyConfigurationChange();
    }

    public override bool? CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.HQRequired;
    }
}