using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class CanBeEquippedColumn : CheckboxColumn
{
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;

    public override bool? CurrentValue(InventoryItem item)
    {
        return CurrentValue(item.Item);
    }

    public override bool? CurrentValue(ItemEx item)
    {
        return item.EquipSlotCategory.Row != 0;
    }

    public override bool? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);    }

    public override string Name { get; set; } = "Can be Equipped?";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Can this item be equipped?";
}