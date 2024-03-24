using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class GatheredByColumn : TextColumn
{
    public GatheredByColumn(ILogger<GatheredByColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return CurrentValue(columnConfiguration, item.Item);
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        var currentValue = item.GatheringTypes.Select(c => c.Value!.FormattedName).ToList();
        if (item.ObtainedFishing)
        {
            currentValue.Add("Fishing");
        }

        return string.Join(",", currentValue);
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return CurrentValue(columnConfiguration, item.InventoryItem);
    }

    public override string Name { get; set; } = "Gathered By?";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "How is this item gathered?";
}