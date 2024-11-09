using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;

using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class DesynthesisClassFilter : StringFilter
{

    public DesynthesisClassFilter(ILogger<DesynthesisClassFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "DesynthesisClass";
    public override string Name { get; set; } = "Desynth Class";
    public override string HelpText { get; set; } = "What class is related to de-synthesising this item?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);
        if (!string.IsNullOrEmpty(currentValue))
        {
            if (item.Base.Desynth == 0 || item.Base.ClassJobRepair.RowId == 0)
            {
                return false;
            }

            var valueName = item.Base.ClassJobRepair.ValueNullable?.Name.ExtractText() ?? "Unknown";
            if (!valueName.PassesFilter(currentValue.ToLower()))
            {
                return false;
            }
        }

        return true;
    }
}