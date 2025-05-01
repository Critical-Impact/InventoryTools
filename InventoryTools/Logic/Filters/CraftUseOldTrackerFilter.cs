using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftTrackerTrackCraftsFilter : BooleanFilter
{
    public CraftTrackerTrackCraftsFilter(ILogger<CraftTrackerTrackCraftsFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool? DefaultValue { get; set; } = true;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;

    public override string Key { get; set; } = "CraftTrackerTrackCrafts";
    public override string Name { get; set; } = "Track Crafts?";

    public override string HelpText { get; set; } =
        "When a craft is completed and it matches one of the output items in this craft list, should it reduce the quantity of that craft item? The craft list must be active for this to count.";

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

public class CraftTrackerTrackGatheringFilter : BooleanFilter
{
    public CraftTrackerTrackGatheringFilter(ILogger<CraftTrackerTrackGatheringFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool? DefaultValue { get; set; } = true;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;

    public override string Key { get; set; } = "CraftTrackerTrackGathering";
    public override string Name { get; set; } = "Track Gathering?";

    public override string HelpText { get; set; } =
        "When gathering is performed and it matches one of the output items in this craft list, should it reduce the quantity of that item? The craft list must be active for this to count.";

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

public class CraftTrackerTrackShoppingFilter : BooleanFilter
{
    public CraftTrackerTrackShoppingFilter(ILogger<CraftTrackerTrackShoppingFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool? DefaultValue { get; set; } = true;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;

    public override string Key { get; set; } = "CraftTrackerTrackShopping";
    public override string Name { get; set; } = "Track Shopping?";

    public override string HelpText { get; set; } =
        "When an item is purchased from a shop and it matches one of the output items in this craft list, should it reduce the quantity of that item? The craft list must be active for this to count.";

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

public class CraftTrackerTrackCombatDropFilter : BooleanFilter
{
    public CraftTrackerTrackCombatDropFilter(ILogger<CraftTrackerTrackCombatDropFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool? DefaultValue { get; set; } = false;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;

    public override string Key { get; set; } = "CraftTrackerTrackCombatDrop";
    public override string Name { get; set; } = "Track Combat Drops?";

    public override string HelpText { get; set; } =
        "When you are in combat and an item drops and it matches one of the output items in this craft list, should it reduce the quantity of that item? The craft list must be active for this to count.";

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

public class CraftTrackerTrackOtherFilter : BooleanFilter
{
    public CraftTrackerTrackOtherFilter(ILogger<CraftTrackerTrackOtherFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool? DefaultValue { get; set; } = false;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;

    public override string Key { get; set; } = "CraftTrackerTrackOther";
    public override string Name { get; set; } = "Track Other?";

    public override string HelpText { get; set; } =
        "When any other event causes you to gain an item and it matches one of the output items in this craft list, should it reduce the quantity of that item? The craft list must be active for this to count.";

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