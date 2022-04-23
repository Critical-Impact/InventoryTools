using System;
using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class SellToVendorPriceColumn : GilColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return (int)item.SellToVendorPrice;
        }

        public override int? CurrentValue(Item item)
        {
            return (int)item.PriceLow;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Sell to Vendor Price";
        public override float Width { get; set; } = 100.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override event IColumn.ButtonPressedDelegate? ButtonPressed
        {
            add { throw new NotSupportedException(); }
            remove { }
        }
    }
}