using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Extensions;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardMinPriceColumn : MarketBoardPriceColumn
    {
        public override (int,int)? CurrentValue(InventoryItem item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }

            var marketBoardData = Cache.GetPricing(item.ItemId, false);
            if (marketBoardData != null)
            {
                var nq = marketBoardData.minPriceNQ;
                var hq = marketBoardData.minPriceHQ;
                return ((int)nq, (int)hq);
            }

            return (Loading, Loading);
        }

        public override (int,int)? CurrentValue(Item item)
        {
            if (!item.CanBeTraded())
            {
                return (Untradable, Untradable);
            }

            var marketBoardData = Cache.GetPricing(item.RowId, false);
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

        public override string Name { get; set; } = "MB Minimum Price";
    }
}