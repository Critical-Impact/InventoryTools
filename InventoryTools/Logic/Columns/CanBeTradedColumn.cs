using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CanBeTradedColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return item.CanBeTraded;
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return item.CanBeTraded;
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Can be Traded?";
        public override float Width { get; set; } = 90.0f;
        public override string HelpText { get; set; } = "Can the item be traded?";
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}