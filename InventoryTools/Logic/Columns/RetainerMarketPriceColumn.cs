using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class RetainerMarketPriceColumn : GilColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return (int)item.RetainerMarketPrice;
        }

        public override int? CurrentValue(ItemEx item)
        {
            return null;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Retainer Unit Price";
        public override float Width { get; set; } = 100;

        public override string HelpText { get; set; } =
            "If the item is selling on the market, this is the price it has been put up for.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}