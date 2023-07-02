using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class StoreColumn : CheckboxColumn
{
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override bool? CurrentValue(InventoryItem item)
    {
        return CurrentValue(item.Item);
    }

    public override bool? CurrentValue(ItemEx item)
    {
        return item.PurchasedSQStore;
    }

    public override bool? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Is sold in Square Store?";
    public override string RenderName => "Is Square Store Item?";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "Is this item sold in the square store?";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
}