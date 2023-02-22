using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CraftColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return Service.ExcelCache.CanCraftItem(item.ItemId);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return Service.ExcelCache.CanCraftItem(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return Service.ExcelCache.CanCraftItem(item.InventoryItem.ItemId);
        }
        

        public override string Name { get; set; } = "Craftable";
        public override float Width { get; set; } = 125.0f;
        public override string HelpText { get; set; } = "Can this item be crafted?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}