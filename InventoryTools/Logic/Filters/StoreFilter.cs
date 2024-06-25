using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
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

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue == null)
        {
            return null;
        }

        return currentValue.Value && item.PurchasedSQStore || !currentValue.Value && !item.PurchasedSQStore;
    }

    public StoreFilter(ILogger<StoreFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}