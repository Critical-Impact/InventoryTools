using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class DesynthesisClassColumn : TextColumn
{
    public override string? CurrentValue(InventoryItem item)
    {
        return CurrentValue(item.Item);
    }

    public override string? CurrentValue(ItemEx item)
    {
        if (!item.CanBeDesynthed || item.ClassJobRepair.Row == 0)
        {
            return null;
        }

        return item.ClassJobRepair.Value?.Name.ToString().ToTitleCase() ?? "Unknown";
    }

    public override string? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Desynth Class";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "What class is related to de-synthesising this item?";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}