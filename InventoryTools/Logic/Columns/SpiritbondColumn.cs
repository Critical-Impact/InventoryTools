using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class SpiritbondColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return item.ActualSpiritbond + "%%";

        }

        public override string? CurrentValue(ItemEx item)
        {
            return null;
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter;

        public override string Name { get; set; } = "Spiritbond";
        public override float Width { get; set; } = 90.0f;
        public override string HelpText { get; set; } = "Shows the spiritbond % of the item.";
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}