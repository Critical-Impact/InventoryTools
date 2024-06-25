using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;

namespace InventoryTools.Logic
{
    public class SearchResult
    {
        private ItemEx _item;
        private InventoryItem? _inventoryItem;
        private SortingResult? _sortingResult;
        private InventoryChange? _inventoryChange;
        private CuratedItem? _curatedItem;
        private CraftItem? _craftItem;

        public ItemEx Item => _item;
        public InventoryItem? InventoryItem => _inventoryItem;

        public SortingResult? SortingResult => _sortingResult;

        public InventoryChange? InventoryChange => _inventoryChange;

        public CuratedItem? CuratedItem => _curatedItem;
        
        public CraftItem? CraftItem => _craftItem;

        public SearchResult(ItemEx item)
        {
            _item = item;
        }

        public SearchResult(InventoryItem inventoryItem)
        {
            _item = inventoryItem.Item;
            _inventoryItem = inventoryItem;
        }

        public SearchResult(SortingResult sortingResult)
        {
            _item = sortingResult.Item;
            _inventoryItem = sortingResult.InventoryItem;
            _sortingResult = sortingResult;
        }

        public SearchResult(InventoryChange inventoryChange)
        {
            _item = inventoryChange.Item;
            _inventoryItem = inventoryChange.InventoryItem;
            _inventoryChange = inventoryChange;
        }

        public SearchResult(CraftItem craftItem)
        {
            _item = craftItem.Item;
            _craftItem = craftItem;
        }

        public SearchResult(ItemEx item, CuratedItem curatedItem)
        {
            _item = item;
            _curatedItem = curatedItem;
        }
    }
}