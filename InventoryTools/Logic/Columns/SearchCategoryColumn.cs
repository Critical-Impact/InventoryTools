using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class SearchCategoryColumn : TextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override string? CurrentValue(InventoryItem item)
        {
            if (item.ItemSearchCategory != null)
            {
                return item.FormattedSearchCategory;
            }

            return "";
        }

        public override string? CurrentValue(ItemEx item)
        {
            if (item.ItemSearchCategory != null)
            {
                return item.FormattedSearchCategory;
            }

            return "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Category (Marketboard)";
        public override string RenderName => "MB Category";
        public override float Width { get; set; } = 200.0f;

        public override string HelpText { get; set; } =
            "The category of the item based off the market board search categories.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}