using CriticalCommonLib.Crafting;
using CriticalCommonLib.Sheets;
using InventoryItem = CriticalCommonLib.Models.InventoryItem;

namespace InventoryTools.Logic.Columns
{
    public interface IColumnEvent
    {
        public void HandleEvent(FilterConfiguration configuration, SortingResult result);
        public void HandleEvent(FilterConfiguration configuration, InventoryItem inventoryItem);
        public void HandleEvent(FilterConfiguration configuration, ItemEx item);
        public void HandleEvent(FilterConfiguration configuration, CraftItem item);
    }
}