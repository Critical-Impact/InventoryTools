using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftTrackerTrackMarketBoardFilter : BooleanFilter
{
    public CraftTrackerTrackMarketBoardFilter(ILogger<CraftTrackerTrackMarketBoardFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool? DefaultValue { get; set; } = true;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;

    public override string Key { get; set; } = "CraftTrackerTrackMarketBoard";
    public override string Name { get; set; } = "Track Market Board?";

    public override string HelpText { get; set; } =
        "When you buy something off the market board and it matches one of the output items in this craft list, should it reduce the quantity of that item? The craft list must be active for this to count.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.CompletionTracking;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }
}