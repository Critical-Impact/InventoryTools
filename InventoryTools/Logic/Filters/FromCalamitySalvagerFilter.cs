using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class FromCalamitySalvagerFilter : BooleanFilter
{
    public FromCalamitySalvagerFilter(ILogger<FromCalamitySalvagerFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "FromCalamitySalvager";
    public override string Name { get; set; } = "Is from Calamity Salvager?";
    public override string HelpText { get; set; } = "Is this item available at a calmity salvager?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

    public override FilterType AvailableIn { get; set; } =
        FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue == null) return true;

        if(currentValue.Value && item.HasSourcesByType(ItemInfoType.CalamitySalvagerShop))
        {
            return true;
        }

        return !currentValue.Value && !item.HasSourcesByType(ItemInfoType.CalamitySalvagerShop);
    }
}