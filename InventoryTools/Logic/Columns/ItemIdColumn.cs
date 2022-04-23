using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class ItemIdColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return (int)item.ItemId;
        }

        public override int? CurrentValue(Item item)
        {
            return (int)item.RowId;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Item ID(Debug)";
        public override float Width { get; set; } = 100.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}