using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class MarketBoardPriceFilter : StringFilter
    {
        public override string Key { get; set; } = "MBPrice";
        public override string Name { get; set; } = "Marketboard Price";
        public override string HelpText { get; set; } = "The market board price of the item. For this to work you need to have automatic pricing enabled and also note that any background price updates will not be evaluated until an event that refreshes the inventory occurs(this happens fairly often).";
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
                        price = marketBoardData.averagePriceHQ;
                    }
                    else
                    {
                        price = marketBoardData.averagePriceNQ;
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
                    float price = marketBoardData.averagePriceNQ;
                    return price.PassesFilter(currentValue.ToLower());
                }

                return false;
            }

            return true;
        }
    }
}