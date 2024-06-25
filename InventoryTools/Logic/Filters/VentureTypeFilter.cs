using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class VentureTypeFilter : StringFilter
{
    public override string Key { get; set; } = "VentureTypeFilter";
    public override string Name { get; set; } = "Venture Type";
    public override string HelpText { get; set; } = "The type of the venture";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var currentValue = CurrentValue(configuration);
        if (!string.IsNullOrEmpty(currentValue))
        {
            if (!item.RetainerTaskNames.ToLower().PassesFilter(currentValue.ToLower()))
            {
                return false;
            }
        }

        return true;
    }

    public VentureTypeFilter(ILogger<VentureTypeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}