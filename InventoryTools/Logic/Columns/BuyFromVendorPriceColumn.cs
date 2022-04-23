using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class BuyFromVendorPriceColumn : GilColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            if (item.CanBeBought)
            {
                int buyPrice = (int)item.BuyFromVendorPrice;
                return buyPrice;
            }

            return null;
        }

        public override int? CurrentValue(Item item)
        {
            if (item.CanBeBought())
            {
                int buyPrice = (int)item.PriceMid;
                return buyPrice;
            }

            return null;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Buy from Vendor Price";
        public override float Width { get; set; } = 100.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}