using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class QuantityColumn : IntegerColumn
    {

        public override int? CurrentValue(InventoryItem item)
        {
            return (int)item.Quantity;
        }

        public override int? CurrentValue(Item item)
        {
            //Add in item counts globally maybe?
            return null;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Quantity";
        public override float Width { get; set; } = 70.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override event IColumn.ButtonPressedDelegate? ButtonPressed;
    }
}