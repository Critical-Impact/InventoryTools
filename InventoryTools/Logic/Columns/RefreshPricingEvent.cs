using CriticalCommonLib.Crafting;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Sheets;

namespace InventoryTools.Logic.Columns
{
    public class RefreshPricingEvent : IColumnEvent
    {
        public void HandleEvent(FilterConfiguration configuration, SortingResult result)
        {
            PluginService.Universalis.QueuePriceCheck(result.InventoryItem.ItemId);
        }

        public void HandleEvent(FilterConfiguration configuration, CriticalCommonLib.Models.InventoryItem inventoryItem)
        {
            PluginService.Universalis.QueuePriceCheck(inventoryItem.ItemId);

        }

        public void HandleEvent(FilterConfiguration configuration, ItemEx item)
        {
            PluginService.Universalis.QueuePriceCheck(item.RowId);
        }

        public void HandleEvent(FilterConfiguration configuration, CraftItem item)
        {
            PluginService.Universalis.QueuePriceCheck(item.ItemId);
        }
    }
}