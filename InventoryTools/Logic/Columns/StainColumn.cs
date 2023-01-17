using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class StainColumn : TextColumn
{
    public override string? CurrentValue(InventoryItem item)
    {
        return item.StainEntry?.Name ?? "";
    }

    public override string? CurrentValue(ItemEx item)
    {
        return "";
    }

    public override string? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Stain";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The stain of the item";
    public override string FilterText { get; set; } = "";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}