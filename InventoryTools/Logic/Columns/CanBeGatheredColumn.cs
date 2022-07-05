using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CanBeGatheredColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return Service.ExcelCache.CanBeGathered(item.ItemId);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return Service.ExcelCache.CanBeGathered(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return Service.ExcelCache.CanBeGathered(item.InventoryItem.ItemId);
        }

        public override string Name { get; set; } = "Can be Gathered?";
        public override float Width { get; set; } = 80.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}