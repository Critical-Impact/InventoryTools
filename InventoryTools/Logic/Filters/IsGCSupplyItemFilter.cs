using AllaganLib.GameSheets.Sheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IsGCSupplyItemFilter : BooleanFilter
{
    public IsGCSupplyItemFilter(ILogger<IsGCSupplyItemFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "IsGCSupplyItem";
    public override string Name { get; set; } = "Is GC Turn-in item?";
    public override string HelpText { get; set; } = "Is this item used for grand company supply missions?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
    public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue == null)
        {
            return null;
        }
        return item.HasUsesByType(ItemInfoType.GCDailySupply);
    }
}