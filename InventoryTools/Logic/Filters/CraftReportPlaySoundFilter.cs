using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftReportPlaySoundFilter : GenericBooleanFilter
{
    public CraftReportPlaySoundFilter(ILogger<CraftReportPlaySoundFilter> logger, ImGuiService imGuiService) : base(
        "CraftReportPlaySound", "Play a sound when an item is complete?",
        "Play a sound effect when an item in this list reaches its required quantity.",
        FilterCategory.Notifications, null, null, logger, imGuiService)
    {
        DefaultValue = false;
        AvailableIn = FilterType.CraftFilter;
    }
}