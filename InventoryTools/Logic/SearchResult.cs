using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;


namespace InventoryTools.Logic
{
    public class SearchResult
    {
        private ItemRow _item;
        private InventoryItem? _inventoryItem;
        private SortingResult? _sortingResult;
        private InventoryChange? _inventoryChange;
        private CuratedItem? _curatedItem;
        private CraftItem? _craftItem;

        public ItemRow Item => _item;
        public InventoryItem? InventoryItem => _inventoryItem;

        public SortingResult? SortingResult => _sortingResult;

        public InventoryChange? InventoryChange => _inventoryChange;

        public CuratedItem? CuratedItem => _curatedItem;

        public CraftItem? CraftItem => _craftItem;

        public uint Quantity
        {
            get
            {
                if (CraftItem != null)
                {
                    return CraftItem.QuantityRequired;
                }
                if (CuratedItem != null)
                {
                    return CuratedItem.Quantity;
                }
                if (InventoryChange != null)
                {
                    return InventoryChange.InventoryItem.Quantity;
                }
                if (InventoryItem != null)
                {
                    return InventoryItem.Quantity;
                }

                return 1;
            }
        }
        public FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags Flags
        {
            get
            {
                if (CraftItem != null)
                {
                    return CraftItem.Flags;
                }
                if (CuratedItem != null)
                {
                    return CuratedItem.ItemFlags;
                }
                if (InventoryChange != null)
                {
                    return InventoryChange.InventoryItem.Flags;
                }
                if (InventoryItem != null)
                {
                    return InventoryItem.Flags;
                }

                return FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None;
            }
        }

        public uint ItemId => Item.RowId;

        public SearchResult(ItemRow item)
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

        public SearchResult(ItemRow item, CuratedItem curatedItem)
        {
            _item = item;
            _curatedItem = curatedItem;
        }
    }
}