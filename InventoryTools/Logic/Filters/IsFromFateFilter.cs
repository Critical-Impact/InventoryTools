using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IsFromFateFilter : BooleanFilter
{
    public IsFromFateFilter(ILogger<IsFromFateFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        
    }

    public override string Key { get; set; } = "IsFromFate";
    public override string Name { get; set; } = "Is From Fate?";
    public override string HelpText { get; set; } = "Is this item dropped/acquired in a fate?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

    public override FilterType AvailableIn { get; set; } =
        FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;
        
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

        switch (currentValue.Value)
        {
            case false:
                return item.FateItems.Count == 0;
            case true:
                return item.FateItems.Count == 1;
        }
    }
}