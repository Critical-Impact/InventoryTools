using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class BuyFromVendorPriceColumn : GilColumn
    {
        public BuyFromVendorPriceColumn(ILogger<BuyFromVendorPriceColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem != null && searchResult.InventoryItem.Item.ObtainedGil)
            {
                int buyPrice = (int)searchResult.InventoryItem.BuyFromVendorPrice;
                return buyPrice;
            }
            
            if (searchResult.Item.ObtainedGil)
            {
                int buyPrice = (int)searchResult.Item.PriceMid;
                return buyPrice;
            }

            return null;
        }

        public override string Name { get; set; } = "Buy from Vendor Price";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "How much the item can be purchased from a vendor(gil)";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter;
    }
}