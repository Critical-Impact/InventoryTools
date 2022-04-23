using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class SearchCategoryColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            if (item.ItemSearchCategory != null)
            {
                return item.FormattedSearchCategory;
            }

            return "";
        }

        public override string? CurrentValue(Item item)
        {
            if (item.ItemSearchCategory != null)
            {
                return item.FormattedSearchCategory();
            }

            return "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "MB Category";
        public override float Width { get; set; } = 200.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}