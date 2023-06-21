using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardMinPriceColumn : MarketBoardPriceColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Market;

        public override string HelpText { get; set; } =
            "Shows the minimum price of both the NQ and HQ form of the item. This data is sourced from universalis.";
        
        public override (int,int)? CurrentValue(InventoryItem item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }

            var marketBoardData = PluginService.MarketCache.GetPricing(item.ItemId, false);
            if (marketBoardData != null)
            {
                var nq = marketBoardData.minPriceNQ;
                var hq = marketBoardData.minPriceHQ;
                return ((int)nq, (int)hq);
            }

            return (Loading, Loading);
        }

        public override (int, int)? CurrentValue(ItemEx item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }

            var marketBoardData = PluginService.MarketCache.GetPricing(item.RowId, false);
            if (marketBoardData != null)
            {
                var nq = marketBoardData.minPriceNQ;
                var hq = marketBoardData.minPriceHQ;
                return ((int)nq, (int)hq);
            }

            return (Loading, Loading);
        }

        public override (int,int)? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Market Board Minimum Price NQ/HQ";
        public override string RenderName => "MB Min. Price NQ/HQ";
    }
}