using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IsMobDropFilter : BooleanFilter
{
    public IsMobDropFilter(ILogger<IsMobDropFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "IsMobDrop";
    public override string Name { get; set; } = "Is Dropped by Mobs?";
    public override string HelpText { get; set; } = "Is this item dropped by mobs?";
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

        switch (currentValue.Value)
        {
            case false:
                return !item.HasSourcesByType(ItemInfoType.Monster);
            case true:
                return item.HasSourcesByType(ItemInfoType.Monster);
        }
    }
}