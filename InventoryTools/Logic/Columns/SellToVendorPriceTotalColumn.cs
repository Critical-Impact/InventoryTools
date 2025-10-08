using System;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class SellToVendorPriceTotalColumn : GilColumn
    {
        public SellToVendorPriceTotalColumn(ILogger<SellToVendorPriceTotalColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem != null)
            {
                return (int)(searchResult.InventoryItem.SellToVendorPrice * searchResult.Quantity);
            }
            return (int)(searchResult.Item.Base.PriceLow * searchResult.Quantity);
        }
        public override string Name { get; set; } = "Sell to Vendor Price (Total)";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "The total amount this item can be sold to a vendor for in gil(price * quantity).";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}