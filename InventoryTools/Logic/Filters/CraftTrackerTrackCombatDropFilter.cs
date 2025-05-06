using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

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