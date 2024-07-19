using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
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
        FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter |
        FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue == null) return true;
            
        if(currentValue.Value && item.ObtainedCalamitySalvager)
        {
            return true;
        }
                
        return !currentValue.Value && !item.ObtainedCalamitySalvager;
    }
}