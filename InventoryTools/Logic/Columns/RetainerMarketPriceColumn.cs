using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class RetainerMarketPriceColumn : GilColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return (int)item.RetainerMarketPrice;
        }

        public override int? CurrentValue(Item item)
        {
            return null;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Retainer Unit Price";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}