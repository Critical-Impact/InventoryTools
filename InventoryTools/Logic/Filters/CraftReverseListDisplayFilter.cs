using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftReverseListDisplayFilter : BooleanFilter
{
    public CraftReverseListDisplayFilter(ILogger<CraftReverseListDisplayFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "CraftReverseListDisplay";
    public override string Name { get; set; } = "Reverse Craft List Order?";

    public override string HelpText { get; set; } =
        "Should the craft list be displayed in reverse order? i.e. Should outputs start at the bottom? (This is only applicable when the Craft Display Mode is single table)";

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }
}