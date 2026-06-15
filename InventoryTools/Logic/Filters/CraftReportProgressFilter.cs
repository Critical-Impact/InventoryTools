using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftReportProgressFilter : GenericBooleanFilter
{
    public CraftReportProgressFilter(ILogger<CraftReportProgressFilter> logger, ImGuiService imGuiService) : base(
        "CraftReportProgress", "Report acquisition progress to chat?",
        "When acquiring items that are part of this craft list, print progress (e.g. \"Ironwood Log 30 remaining\") to chat. The craft list must be active.",
        FilterCategory.Notifications, null, null, logger, imGuiService)
    {
        DefaultValue = true;
        AvailableIn = FilterType.CraftFilter;
    }
}