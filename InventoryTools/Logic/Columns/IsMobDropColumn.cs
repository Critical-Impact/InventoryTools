using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class IsMobDropColumn : CheckboxColumn
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
        return item.HasMobDrops();
    }

    public override bool? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Is Dropped by Mobs?";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Is this item dropped by mobs?";
}