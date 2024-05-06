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
        
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            if (item.Item.ObtainedGil)
            {
                int buyPrice = (int)item.BuyFromVendorPrice;
                return buyPrice;
            }

            return null;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            if (item.ObtainedGil)
            {
                int buyPrice = (int)item.PriceMid;
                return buyPrice;
            }

            return null;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Buy from Vendor Price";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "How much the item can be purchased from a vendor(gil)";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter;
    }
}