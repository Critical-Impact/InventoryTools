using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class CraftColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return ExcelCache.CanCraftItem(item.ItemId);
        }

        public override bool? CurrentValue(Item item)
        {
            return ExcelCache.CanCraftItem(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return ExcelCache.CanCraftItem(item.InventoryItem.ItemId);
        }
        

        public override string Name { get; set; } = "Craftable";
        public override float Width { get; set; } = 125.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}