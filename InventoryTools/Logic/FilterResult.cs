using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;

namespace InventoryTools.Logic
{
    public class FilterResult
    {
        private List<SortingResult> _sortedItems;
        private List<ItemEx> _allItems;
        private List<InventoryItem> _unsortableItems;
        private List<InventoryChange> _inventoryHistory;

        public FilterResult(List<SortingResult> sortedItems, List<InventoryItem> unsortableItems, List<ItemEx> items, List<InventoryChange> inventoryHistory)
        {
            _sortedItems = sortedItems;
            _unsortableItems = unsortableItems;
            _allItems = items;
            _inventoryHistory = inventoryHistory;
        }

        public List<SortingResult> SortedItems => _sortedItems;

        public List<InventoryItem> UnsortableItems => _unsortableItems;
        
        public List<ItemEx> AllItems => _allItems;
        public List<InventoryChange> InventoryHistory => _inventoryHistory;
    }
}