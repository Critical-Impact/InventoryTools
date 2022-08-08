using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class ItemILevelColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            if (item.EquipSlotCategory == null)
            {
                return null;
            }
            if (item.EquipSlotCategory.RowId == 0)
            {
                return null;
            }

            return (int)item.Item.LevelItem.Row;
        }

        public override int? CurrentValue(ItemEx item)
        {
            if (item.EquipSlotCategory.Row == 0)
            {
                return null;
            }

            return (int)item.LevelItem.Row;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "iLevel";
        public override float Width { get; set; } = 50.0f;
        public override string HelpText { get; set; } = "Shows the iLevel of the item.";
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}