using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class QuantityFilter : StringFilter
    {
        public override string Key { get; set; } = "Qty";
        public override string Name { get; set; } = "Quantity";
        public override string HelpText { get; set; } = "The quantity of the item.";
        public override FilterType AvailableIn { get; set; }  = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.Quantity.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                var qty = 0;
                if (PluginService.InventoryMonitor.ItemCounts.ContainsKey((item.RowId,
                        FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)))
                {
                    qty += PluginService.InventoryMonitor.ItemCounts[(item.RowId,
                        FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)];
                }
                if (PluginService.InventoryMonitor.ItemCounts.ContainsKey((item.RowId,
                        FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ)))
                {
                    qty += PluginService.InventoryMonitor.ItemCounts[(item.RowId,
                        FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ)];
                }
                if (!qty.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}