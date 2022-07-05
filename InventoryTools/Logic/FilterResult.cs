using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;

namespace InventoryTools.Logic
{
    public struct FilterResult
    {
        private List<SortingResult> _sortedItems;
        private List<ItemEx> _allItems;
        private List<InventoryItem> _unsortableItems;

        public FilterResult(List<SortingResult> sortedItems, List<InventoryItem> unsortableItems, List<ItemEx> items)
        {
            _sortedItems = sortedItems;
            _unsortableItems = unsortableItems;
            _allItems = items;
        }

        public List<SortingResult> SortedItems => _sortedItems;

        public List<InventoryItem> UnsortableItems => _unsortableItems;
        
        public List<ItemEx> AllItems => _allItems;
    }
}