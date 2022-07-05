using CriticalCommonLib.Crafting;
using CriticalCommonLib.Sheets;
using InventoryItem = CriticalCommonLib.Models.InventoryItem;

namespace InventoryTools.Logic.Columns
{
    public interface IColumnEvent
    {
        public void HandleEvent(SortingResult result);
        public void HandleEvent(InventoryItem inventoryItem);
        public void HandleEvent(ItemEx item);
        public void HandleEvent(CraftItem item);
    }
}