using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class CanBeGatheredColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return ExcelCache.CanBeGathered(item.ItemId);
        }

        public override bool? CurrentValue(Item item)
        {
            return ExcelCache.CanBeGathered(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return ExcelCache.CanBeGathered(item.InventoryItem.ItemId);
        }

        public override string Name { get; set; } = "Can be Gathered?";
        public override float Width { get; set; } = 100.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}