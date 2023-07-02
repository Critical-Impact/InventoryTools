using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Utility;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class UiCategoryColumn : TextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override string? CurrentValue(InventoryItem item)
        {
            if (item.ItemUICategory == null)
            {
                return null;
            }

            return item.FormattedUiCategory;
        }

        public override string? CurrentValue(ItemEx item)
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

        public override string Name { get; set; } = "Category (Basic)";
        public override string RenderName => "Category";
        public override float Width { get; set; } = 200.0f;
        public override string HelpText { get; set; } = "The category of the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}