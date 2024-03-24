using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.Settings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardMinPriceColumn : MarketBoardPriceColumn
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IMarketCache _marketCache;

        public MarketBoardMinPriceColumn(ILogger<MarketBoardMinPriceColumn> logger, ImGuiService imGuiService, MarketboardWorldSetting marketboardWorldSetting, ICharacterMonitor characterMonitor, IMarketCache marketCache) : base(logger, imGuiService, marketboardWorldSetting, characterMonitor, marketCache)
        {
            _characterMonitor = characterMonitor;
            _marketCache = marketCache;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Market;

        public override string HelpText { get; set; } =
            "Shows the minimum price of both the NQ and HQ form of the item. This data is sourced from universalis.";
        
        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }
            var activeCharacter = _characterMonitor.ActiveCharacter;
            if (activeCharacter != null)
            {
                var marketBoardData = _marketCache.GetPricing(item.ItemId, activeCharacter.WorldId, false);
                if (marketBoardData != null)
                {
                    var nq = marketBoardData.MinPriceNq;
                    var hq = marketBoardData.MinPriceHq;
                    return ((int)nq, (int)hq);
                }
            }

            return (Loading, Loading);
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }
            var activeCharacter = _characterMonitor.ActiveCharacter;
            if (activeCharacter != null)
            {
                var marketBoardData = _marketCache.GetPricing(item.RowId, activeCharacter.WorldId, false);
                if (marketBoardData != null)
                {
                    var nq = marketBoardData.MinPriceNq;
                    var hq = marketBoardData.MinPriceHq;
                    return ((int)nq, (int)hq);
                }
            }

            return (Loading, Loading);
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Market Board Minimum Price NQ/HQ";
        public override string RenderName => "MB Min. Price NQ/HQ";
    }
}