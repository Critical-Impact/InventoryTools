using System.Collections.Generic;
using CriticalCommonLib.Models;

namespace InventoryTools.Logic
{
    public struct FilterResult
    {
        private List<SortingResult> _sortedItems;
        private List<InventoryItem> _unsortableItems;

        public FilterResult(List<SortingResult> sortedItems, List<InventoryItem> unsortableItems)
        {
            _sortedItems = sortedItems;
            _unsortableItems = unsortableItems;
        }

        public List<SortingResult> SortedItems => _sortedItems;

        public List<InventoryItem> UnsortableItems => _unsortableItems;
    }
}