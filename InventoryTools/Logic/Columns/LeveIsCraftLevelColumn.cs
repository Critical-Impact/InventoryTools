using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class LeveIsCraftLevelColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return ExcelCache.IsItemCraftLeve(item.ItemId);
        }

        public override bool? CurrentValue(Item item)
        {
            return ExcelCache.IsItemCraftLeve(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Leves: For Craft Leve?";
        public override float Width { get; set; } = 100.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}