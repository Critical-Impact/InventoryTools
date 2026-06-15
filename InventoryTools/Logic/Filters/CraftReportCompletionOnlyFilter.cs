using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftReportCompletionOnlyFilter : GenericBooleanFilter
{
    public CraftReportCompletionOnlyFilter(ILogger<CraftReportCompletionOnlyFilter> logger, ImGuiService imGuiService) :
        base(
            "CraftReportCompletionOnly", "Only report when an item is complete?",
            "Instead of reporting every acquisition, only print a message when an item reaches its required quantity.",
            FilterCategory.Notifications, null, null, logger, imGuiService)
    {
        DefaultValue = false;
        AvailableIn = FilterType.CraftFilter;
    }
}