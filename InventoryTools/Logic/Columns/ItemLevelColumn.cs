using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class ItemLevelColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            if (item.Item == null)
            {
                return null;
            }
            return CurrentValue(item.Item);
        }

        public override int? CurrentValue(Item item)
        {
            return item.LevelEquip;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Item Level";
        public override float Width { get; set; } = 80.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override event IColumn.ButtonPressedDelegate? ButtonPressed;
    }
}