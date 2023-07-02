using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class PatchColumn : DecimalColumn
{
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override decimal? CurrentValue(InventoryItem item)
    {
        return Service.ExcelCache.GetItemPatch(item.ItemId);
    }

    public override decimal? CurrentValue(ItemEx item)
    {
        return Service.ExcelCache.GetItemPatch(item.RowId);
    }

    public override decimal? CurrentValue(SortingResult item)
    {
        return Service.ExcelCache.GetItemPatch(item.InventoryItem.ItemId);
    }

    public override string Name { get; set; } = "Patch Added";
    public override string RenderName => "Patch";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Shows the patch in which the item was added.";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}