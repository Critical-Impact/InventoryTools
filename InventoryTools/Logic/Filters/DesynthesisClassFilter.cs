using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class DesynthesisClassFilter : StringFilter
{
    public override string Key { get; set; } = "DesynthesisClass";
    public override string Name { get; set; } = "Desynth Class";
    public override string HelpText { get; set; } = "What class is related to de-synthesising this item?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
    
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var currentValue = CurrentValue(configuration);
        if (!string.IsNullOrEmpty(currentValue))
        {
            if (item.Desynth == 0 || item.ClassJobRepair.Row == 0)
            {
                return false;
            }

            var valueName = item.ClassJobRepair.Value?.Name.ToString() ?? "Unknown";
            if (!valueName.PassesFilter(currentValue.ToLower()))
            {
                return false;
            }
        }

        return true;
    }

    public DesynthesisClassFilter(ILogger<DesynthesisClassFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}