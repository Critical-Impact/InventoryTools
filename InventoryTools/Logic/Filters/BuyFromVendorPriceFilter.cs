using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class BuyFromVendorPriceFilter : StringFilter
    {
        public override string Key { get; set; } = "GSBuyPrice";
        public override string Name { get; set; } = "Buy From Vendor Price";
        public override string HelpText { get; set; } = "The price when bought from shops. !,>,<,>=,<= can be used for comparisons";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

        public override bool FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.CanBeBought)
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

        public override bool FilterItem(FilterConfiguration configuration,Item item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.CanBeBought())
                {
                    return false;
                }
                if (!item.PriceMid.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}