using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftWorldPriceUseActiveWorldFilter : BooleanFilter
{
    public CraftWorldPriceUseActiveWorldFilter(ILogger<CraftWorldPriceUseActiveWorldFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "CraftWorldPriceUseActiveWorld";
    public override string Name { get; set; } = "Use Active World?";
    public override string HelpText { get; set; } = "Should your currently logged in character's active world be used as a pricing source?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.WorldPricePreference;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? DefaultValue { get; set; } = true;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }
}