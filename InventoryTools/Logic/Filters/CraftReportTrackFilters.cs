using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftReportTrackGatheringFilter : GenericBooleanFilter
{
    public CraftReportTrackGatheringFilter(ILogger<CraftReportTrackGatheringFilter> logger, ImGuiService imGuiService) :
        base(
            "CraftReportTrackGathering", "Report gathered items?",
            "Report progress when you gather an item that is part of this craft list.", FilterCategory.Notifications,
            null, null, logger, imGuiService)
    {
        DefaultValue = true;
        AvailableIn = FilterType.CraftFilter;
    }
}

public class CraftReportTrackCraftingFilter : GenericBooleanFilter
{
    public CraftReportTrackCraftingFilter(ILogger<CraftReportTrackCraftingFilter> logger, ImGuiService imGuiService) :
        base(
            "CraftReportTrackCrafting", "Report crafted items?",
            "Report progress when you craft an item that is part of this craft list.", FilterCategory.Notifications,
            null, null, logger, imGuiService)
    {
        DefaultValue = false;
        AvailableIn = FilterType.CraftFilter;
    }
}

public class CraftReportTrackShoppingFilter : GenericBooleanFilter
{
    public CraftReportTrackShoppingFilter(ILogger<CraftReportTrackShoppingFilter> logger, ImGuiService imGuiService) :
        base(
            "CraftReportTrackShopping", "Report shopped items?",
            "Report progress when you buy an item that is part of this craft list.", FilterCategory.Notifications,
            null, null, logger, imGuiService)
    {
        DefaultValue = false;
        AvailableIn = FilterType.CraftFilter;
    }
}

public class CraftReportTrackCombatDropFilter : GenericBooleanFilter
{
    public CraftReportTrackCombatDropFilter(ILogger<CraftReportTrackCombatDropFilter> logger, ImGuiService imGuiService)
        : base(
            "CraftReportTrackCombatDrop", "Report combat drops?",
            "Report progress when an item that is part of this craft list drops from combat.",
            FilterCategory.Notifications,
            null, null, logger, imGuiService)
    {
        DefaultValue = false;
        AvailableIn = FilterType.CraftFilter;
    }
}

public class CraftReportTrackMarketBoardFilter : GenericBooleanFilter
{
    public CraftReportTrackMarketBoardFilter(ILogger<CraftReportTrackMarketBoardFilter> logger,
        ImGuiService imGuiService) : base(
        "CraftReportTrackMarketBoard", "Report market board purchases?",
        "Report progress when you buy an item that is part of this craft list from the market board.",
        FilterCategory.Notifications,
        null, null, logger, imGuiService)
    {
        DefaultValue = false;
        AvailableIn = FilterType.CraftFilter;
    }
}

public class CraftReportTrackOtherFilter : GenericBooleanFilter
{
    public CraftReportTrackOtherFilter(ILogger<CraftReportTrackOtherFilter> logger, ImGuiService imGuiService) : base(
        "CraftReportTrackOther", "Report other acquisitions?",
        "Report progress when an item that is part of this craft list is acquired through any other means.",
        FilterCategory.Notifications,
        null, null, logger, imGuiService)
    {
        DefaultValue = false;
        AvailableIn = FilterType.CraftFilter;
    }
}