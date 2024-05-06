using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class StainColumn : TextColumn
{
    public StainColumn(ILogger<StainColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return item.StainEntry?.Name ?? "";
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return "";
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return CurrentValue(columnConfiguration, item.InventoryItem);
    }

    public override string Name { get; set; } = "Dye";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The current dye of the item";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}