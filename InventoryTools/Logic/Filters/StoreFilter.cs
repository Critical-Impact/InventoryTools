using AllaganLib.GameSheets.Sheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class StoreFilter : BooleanFilter
{
    public override string Key { get; set; } = "StoreFilter";
    public override string Name { get; set; } = "Sold in Square Store";
    public override string HelpText { get; set; } = "Is this item sold in the square store?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

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

        return currentValue.Value && item.HasSourcesByType(ItemInfoType.CashShop) || !currentValue.Value && !item.HasSourcesByType(ItemInfoType.CashShop);
    }

    public StoreFilter(ILogger<StoreFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}