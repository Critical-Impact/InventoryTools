using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class VentureTypeFilter : StringFilter
{
    public VentureTypeFilter(ILogger<VentureTypeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "VentureTypeFilter";
    public override string Name { get; set; } = "Venture Type";
    public override string HelpText { get; set; } = "The type of the venture";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);
        if (!string.IsNullOrEmpty(currentValue))
        {
            var ventureNames = string.Join(",", item.GetSourcesByCategory<ItemVentureSource>(ItemInfoCategory.AllVentures)
                    .Select(c => c.RetainerTaskRow.FormattedName));
            if (!ventureNames.ToLower().PassesFilter(currentValue.ToLower()))
            {
                return false;
            }
        }

        return true;
    }
}