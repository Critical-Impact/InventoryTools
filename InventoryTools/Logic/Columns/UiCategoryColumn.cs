using CriticalCommonLib.Models;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class UiCategoryColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            if (item.ItemUICategory == null)
            {
                return null;
            }

            return item.FormattedUiCategory;
        }

        public override string? CurrentValue(Item item)
        {
            var itemItemUiCategory = item.ItemUICategory;
            if (itemItemUiCategory == null)
            {
                return null;
            }

            return itemItemUiCategory.Value?.Name.ToDalamudString().ToString() ?? "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Category";
        public override float Width { get; set; } = 200.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override event IColumn.ButtonPressedDelegate? ButtonPressed;
    }
}