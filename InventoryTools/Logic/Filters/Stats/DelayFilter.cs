using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Stats;

public class DelayFilter : StringFilter
{
    public DelayFilter(ILogger<DelayFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        ShowOperatorTooltip = true;
    }

    public override string Key { get; set; } = "DelayFilter";
    public override string Name { get; set; } = "Delay";
    public override string HelpText { get; set; } = "The time it takes between each automatic attack while engaged with and in range of an enemy in seconds.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Stats;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        if (item.Base.Delayms == 0)
        {
            return null;
        }
        var currentValue = CurrentValue(configuration);
        if (!string.IsNullOrEmpty(currentValue))
        {
            if (((decimal)item.Base.Delayms / 1000).PassesFilter(currentValue.ToLower()))
            {
                return true;
            }

            return false;
        }
        return true;
    }
}