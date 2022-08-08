using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Misc;

namespace InventoryTools.Logic.Columns
{
    public class IsCraftingItemColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return Service.ExcelCache.IsCraftItem(item.ItemUICategory.Row);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Crafting?";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Can this item be used to craft another item?";
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}