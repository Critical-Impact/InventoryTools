using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class PatchFilter : StringFilter
{
    public override string Key { get; set; } = "PatchFilter";
    public override string Name { get; set; } = "Patch";
    public override string HelpText { get; set; } = "The patch in which the item was added.";
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
            if (item.GetPatch().PassesFilter(currentValue.ToLower()))
            {
                return true;
            }

            return false;
        }
        return true;
    }

    public PatchFilter(ILogger<PatchFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        ShowOperatorTooltip = true;
    }
}