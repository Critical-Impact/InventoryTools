using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftWorldPriceUseDefaultsFilter : BooleanFilter
{
    public CraftWorldPriceUseDefaultsFilter(ILogger<CraftWorldPriceUseDefaultsFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "CraftWorldPriceUseDefaults";
    public override string Name { get; set; } = "Use Default Worlds?";

    public override string HelpText { get; set; } =
        "Should the worlds selected in 'Price Worlds' in the main settings be automatically used for pricing?";

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