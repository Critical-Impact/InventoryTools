using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;

namespace InventoryTools.Logic.Columns
{
    class ItemIconPressedColumnEvent : IColumnEvent
    {
        public void HandleEvent(FilterConfiguration configuration, SortingResult result)
        {
            HandleEvent(configuration, result.InventoryItem);
        }

        public void HandleEvent(FilterConfiguration configuration, InventoryItem inventoryItem)
        {
            HandleEvent(configuration, inventoryItem.Item);
        }

        public void HandleEvent(FilterConfiguration configuration, ItemEx item)
        {
            PluginService.WindowService.OpenItemWindow(item.RowId);
        }

        public void HandleEvent(FilterConfiguration configuration, CraftItem item)
        {
            HandleEvent(configuration, item.Item);
        }
    }
}