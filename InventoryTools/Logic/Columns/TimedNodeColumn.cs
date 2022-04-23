using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class TimedNodeColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return ExcelCache.IsItemAvailableAtTimedNode(item.ItemId);
        }

        public override bool? CurrentValue(Item item)
        {
            return ExcelCache.IsItemAvailableAtTimedNode(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "From Timed Node?";
        public override float Width { get; set; } = 125.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}