using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class MarketBoardMinPriceFilter : MarketBoardPriceFilter
    {
        public override string Key { get; set; } = "MBMinPrice";
        public override string Name { get; set; } = "Marketboard Minimum Price";
        public override string HelpText { get; set; } = "The market board minimum price of the item. For this to work you need to have automatic pricing enabled and also note that any background price updates will not be evaluated until an event that refreshes the inventory occurs(this happens fairly often).";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Market;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

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
                        price = marketBoardData.minPriceHQ;
                    }
                    else
                    {
                        price = marketBoardData.minPriceNQ;
                    }
                    return price.PassesFilter(currentValue.ToLower());
                }

                return false;
            }

            return true;
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.CanBeTraded())
                {
                    return false;
                }
                var marketBoardData = Cache.GetPricing(item.RowId, false);
                if (marketBoardData != null)
                {
                    float price = marketBoardData.minPriceNQ;
                    return price.PassesFilter(currentValue.ToLower());
                }

                return false;
            }

            return true;
        }
    }
}