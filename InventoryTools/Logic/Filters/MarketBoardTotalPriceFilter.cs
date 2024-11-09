using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class MarketBoardTotalPriceFilter : StringFilter
    {
        protected readonly ICharacterMonitor CharacterMonitor;
        protected readonly IMarketCache MarketCache;

        public MarketBoardTotalPriceFilter(ILogger<MarketBoardTotalPriceFilter> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, IMarketCache marketCache) : base(logger, imGuiService)
        {
            CharacterMonitor = characterMonitor;
            MarketCache = marketCache;
            ShowOperatorTooltip = true;
        }
        public override string Key { get; set; } = "MBTotalPrice";
        public override string Name { get; set; } = "Market Board Avg. Total Price";
        public override string HelpText { get; set; } = "The total market board price of the item(price * quantity). For this to work you need to have automatic pricing enabled and also note that any background price updates will not be evaluated until an event that refreshes the inventory occurs(this happens fairly often).";
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
                            price = marketBoardData.AveragePriceHq;
                        }
                        else
                        {
                            price = marketBoardData.AveragePriceNq;
                        }

                        price *= item.Quantity;
                        return price.PassesFilter(currentValue.ToLower());
                    }
                }

                return false;
            }

            return true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return true;
        }
    }
}