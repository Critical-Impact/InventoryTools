using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class MarketBoardMinPriceFilter : MarketBoardPriceFilter
    {

        public MarketBoardMinPriceFilter(ILogger<MarketBoardMinPriceFilter> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, IMarketCache marketCache) : base(logger, imGuiService, characterMonitor, marketCache)
        {
            ShowOperatorTooltip = true;
        }
        public override string Key { get; set; } = "MBMinPrice";
        public override string Name { get; set; } = "Marketboard Minimum Price";
        public override string HelpText { get; set; } = "The market board minimum price of the item. For this to work you need to have automatic pricing enabled and also note that any background price updates will not be evaluated until an event that refreshes the inventory occurs(this happens fairly often).";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Market;



        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.CanBeTraded)
                {
                    return false;
                }
                var activeCharacter = CharacterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    var marketBoardData = MarketCache.GetPricing(item.ItemId, activeCharacter.WorldId, false);
                    if (marketBoardData != null)
                    {
                        float price;
                        if (item.IsHQ)
                        {
                            price = marketBoardData.MinPriceHq;
                        }
                        else
                        {
                            price = marketBoardData.MinPriceNq;
                        }

                        return price.PassesFilter(currentValue.ToLower());
                    }
                }

                return false;
            }

            return true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.CanBeTraded)
                {
                    return false;
                }
                var activeCharacter = CharacterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    var marketBoardData = MarketCache.GetPricing(item.RowId, activeCharacter.WorldId, false);
                    if (marketBoardData != null)
                    {
                        float price = marketBoardData.MinPriceNq;
                        return price.PassesFilter(currentValue.ToLower());
                    }
                }

                return false;
            }

            return true;
        }
    }
}