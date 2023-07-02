using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class LocationColumn : TextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;

        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.CraftFilter;

        public override string? CurrentValue(InventoryItem item)
        {
            return item.FormattedBagLocation;
        }

        public override string? CurrentValue(ItemEx item)
        {
            return null;
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Inventory Location";
        public override string RenderName => "Location";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Shows the location of the item in your inventory.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}