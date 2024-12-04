using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;

using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class BuyFromVendorPriceFilter : StringFilter
    {
        public override string Key { get; set; } = "GSBuyPrice";
        public override string Name { get; set; } = "Buy From Vendor Price";
        public override string HelpText { get; set; } = "The price when bought from shops.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.Item.HasSourcesByType(ItemInfoType.GilShop, ItemInfoType.CalamitySalvagerShop))
                {
                    return false;
                }
                if (!item.BuyFromVendorPrice.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.HasSourcesByType(ItemInfoType.GilShop, ItemInfoType.CalamitySalvagerShop))
                {
                    return false;
                }
                if (!item.Base.PriceMid.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }

        public BuyFromVendorPriceFilter(ILogger<BuyFromVendorPriceFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
            ShowOperatorTooltip = true;
        }
    }
}