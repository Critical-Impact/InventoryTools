using CriticalCommonLib.Crafting;
using CriticalCommonLib.MarketBoard;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class RefreshPricingEvent : IColumnEvent
    {
        public void HandleEvent(SortingResult result)
        {
            Universalis.QueuePriceCheck(result.InventoryItem.ItemId);
        }

        public void HandleEvent(CriticalCommonLib.Models.InventoryItem inventoryItem)
        {
            Universalis.QueuePriceCheck(inventoryItem.ItemId);

        }

        public void HandleEvent(Item item)
        {
            Universalis.QueuePriceCheck(item.RowId);
        }

        public void HandleEvent(CraftItem item)
        {
            Universalis.QueuePriceCheck(item.ItemId);
        }
    }
}