using CriticalCommonLib.Crafting;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Sheets;

namespace InventoryTools.Logic.Columns
{
    public class RefreshPricingEvent : IColumnEvent
    {
        public void HandleEvent(FilterConfiguration configuration, SortingResult result)
        {
            Universalis.QueuePriceCheck(result.InventoryItem.ItemId);
        }

        public void HandleEvent(FilterConfiguration configuration, CriticalCommonLib.Models.InventoryItem inventoryItem)
        {
            Universalis.QueuePriceCheck(inventoryItem.ItemId);

        }

        public void HandleEvent(FilterConfiguration configuration, ItemEx item)
        {
            Universalis.QueuePriceCheck(item.RowId);
        }

        public void HandleEvent(FilterConfiguration configuration, CraftItem item)
        {
            Universalis.QueuePriceCheck(item.ItemId);
        }
    }
}