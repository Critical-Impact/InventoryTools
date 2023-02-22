using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class ItemIdColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return (int)item.ItemId;
        }

        public override int? CurrentValue(ItemEx item)
        {
            return (int)item.RowId;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Debug - Item ID";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Shows the item's internal ID.";
        public override bool HasFilter { get; set; } = false;
        public override bool IsDebug { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}