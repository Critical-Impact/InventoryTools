using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class TimedNodeColumn : CheckboxColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(InventoryItem item)
        {
            return Service.ExcelCache.IsItemAvailableAtTimedNode(item.ItemId);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return Service.ExcelCache.IsItemAvailableAtTimedNode(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Is From Timed Node?";
        public override string RenderName => "Timed Node?";
        public override float Width { get; set; } = 125.0f;
        public override string HelpText { get; set; } = "Is this item available at a timed node?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}