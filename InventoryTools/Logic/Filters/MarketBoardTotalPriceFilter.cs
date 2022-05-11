using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class MarketBoardTotalPriceFilter : StringFilter
    {
        public override string Key { get; set; } = "MBTotalPrice";
        public override string Name { get; set; } = "Market Board Total Price";
        public override string HelpText { get; set; } = "The total market board price of the item(price * quantity). For this to work you need to have automatic pricing enabled and also note that any background price updates will not be evaluated until an event that refreshes the inventory occurs(this happens fairly often).";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Market;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter;

        public override bool FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.CanBeTraded)
                {
                    return false;
                }
                var marketBoardData = Cache.GetPricing(item.ItemId, false);
                if (marketBoardData != null)
                {
                    float price;
                    if (item.IsHQ)
                    {
                        price = marketBoardData.averagePriceHQ;
                    }
                    else
                    {
                        price = marketBoardData.averagePriceNQ;
                    }

                    price *= item.Quantity;
                    return price.PassesFilter(currentValue.ToLower());
                }

                return false;
            }

            return true;
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            return true;
        }
    }
}