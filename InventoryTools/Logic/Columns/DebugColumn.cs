using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class DebugColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            if (item.Item != null) return CurrentValue(item.Item);
            return "";
        }

        public override string? CurrentValue(Item item)
        {
            return "Item Search: " + item.ItemSearchCategory.Row + " - Ui Category: " + item.ItemUICategory.Row + " - Sort Category: " + item.ItemSortCategory.Row + " - Equip Slot Category: " + item.EquipSlotCategory.Row + " - Class Job Category: " + item.ClassJobCategory.Row + " - Buy: " + item.PriceMid + " - Unknown: " + item.Unknown19;
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Debug";
        public override float Width { get; set; } = 200;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}