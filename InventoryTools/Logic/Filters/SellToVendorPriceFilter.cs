using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class SellToVendorPriceFilter : StringFilter
    {
        public override string Key { get; set; } = "GSSalePrice";
        public override string Name { get; set; } = "Sell to Shop Price";
        public override string HelpText { get; set; } = "The price when bought from shops. !,>,<,>=,<= can be used for comparisons";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

        public override bool FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.SellToVendorPrice.PassesFilter(currentValue.ToLower()))
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
                if (!item.PriceLow.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}