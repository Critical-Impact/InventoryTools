using CriticalCommonLib.Models;
using InventoryTools.Misc;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class IsHousingItemColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return item.Item == null ? false : CurrentValue(item.Item);
        }

        public override bool? CurrentValue(Item item)
        {
            return Helpers.HousingCategoryIds.Contains(item.ItemUICategory.Row);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Housing?";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}