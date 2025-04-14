using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Stats;

public class PhysicalDamageFilter : StringFilter
{
    public PhysicalDamageFilter(ILogger<PhysicalDamageFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        ShowOperatorTooltip = true;
    }

    public override string Key { get; set; } = "PhysicalDamageFilter";
    public override string Name { get; set; } = "Physical Damage";
    public override string HelpText { get; set; } = "The physical damage of the item";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Stats;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        if (item.Base.DamagePhys == 0)
        {
            return null;
        }
        var currentValue = CurrentValue(configuration);
        if (!string.IsNullOrEmpty(currentValue))
        {
            if (((int)item.Base.DamagePhys).PassesFilter(currentValue.ToLower()))
            {
                return true;
            }

            return false;
        }
        return true;
    }
}