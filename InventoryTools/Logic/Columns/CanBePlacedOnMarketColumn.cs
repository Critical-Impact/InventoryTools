using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CanBePlacedOnMarketColumn : CheckboxColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(InventoryItem item)
        {
            return item.CanBePlacedOnMarket;
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return item.CanBePlacedOnMarket;
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Can be Placed on Market?";
        public override float Width { get; set; } = 90.0f;
        public override string HelpText { get; set; } = "Can the item be placed on the marketboard?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}