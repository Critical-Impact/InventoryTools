using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Stats;

public class MagicalDamageFilter : StringFilter
{
    public MagicalDamageFilter(ILogger<MagicalDamageFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        ShowOperatorTooltip = true;
    }

    public override string Key { get; set; } = "MagicalDamageFilter";
    public override string Name { get; set; } = "Magical Damage";
    public override string HelpText { get; set; } = "The magical damage of the item";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Stats;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        if (item.Base.DamageMag == 0)
        {
            return null;
        }
        var currentValue = CurrentValue(configuration);
        if (!string.IsNullOrEmpty(currentValue))
        {
            if (((int)item.Base.DamageMag).PassesFilter(currentValue.ToLower()))
            {
                return true;
            }

            return false;
        }
        return true;
    }
}