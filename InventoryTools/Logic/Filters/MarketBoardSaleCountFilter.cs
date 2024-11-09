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
    public class MarketBoardSaleCountFilter : StringFilter
    {
        private readonly InventoryToolsConfiguration _configuration;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IMarketCache _marketCache;

        public MarketBoardSaleCountFilter(ILogger<MarketBoardSaleCountFilter> logger, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ICharacterMonitor characterMonitor, IMarketCache marketCache) : base(logger, imGuiService)
        {
            _configuration = configuration;
            _characterMonitor = characterMonitor;
            _marketCache = marketCache;
            Name = "Marketboard " + configuration.MarketSaleHistoryLimit + " Sale Counter";
            HelpText = "Shows the number of sales that have been made within " + configuration.MarketSaleHistoryLimit +
                       " days.";
            ShowOperatorTooltip = true;
        }

        public override string Key { get; set; } = "MBSaleCount";
        public override string Name { get; set; } = "Marketboard Sale Counter";

        public override string HelpText { get; set; } = "Shows the number of sales that have been made within X days.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Market;



        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            if (HasValueSet(configuration))
            {
                if (!item.CanBeTraded)
                {
                    return false;
                }
                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    var marketBoardData = _marketCache.GetPricing(item.RowId, activeCharacter.WorldId, false);
                    if (marketBoardData != null)
                    {
                        return marketBoardData.SevenDaySellCount.PassesFilter(currentValue.ToLower());
                    }
                }

                return false;
            }

            return null;
        }
    }
}