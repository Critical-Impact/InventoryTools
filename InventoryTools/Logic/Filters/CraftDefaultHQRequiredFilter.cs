using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftDefaultHQRequiredFilter : BooleanFilter
{
    public override bool? DefaultValue { get; set; } = false;
    public override string Key { get; set; } = "CraftDefaultHqRequired";
    public override string Name { get; set; } = "HQ Required";
    public override string HelpText { get; set; } = "Should each item in the list require a HQ version of the item(if applicable)?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
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

    public CraftDefaultHQRequiredFilter(ILogger<CraftDefaultHQRequiredFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}