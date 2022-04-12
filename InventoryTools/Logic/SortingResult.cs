using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;

namespace InventoryTools.Logic
{
    public struct SortingResult
    {
        private ulong _sourceRetainerId;

        public ulong SourceRetainerId => _sourceRetainerId;

        public ulong? DestinationRetainerId => _destinationRetainerId;

        public InventoryType SourceBag => _sourceBag;

        public InventoryCategory? DestinationBag => _destinationBag;

        public InventoryItem InventoryItem => _inventoryItem;

        public int Quantity => _quantity;

        private ulong? _destinationRetainerId;
        private InventoryType _sourceBag;
        private InventoryCategory? _destinationBag;
        private InventoryItem _inventoryItem;
        private int _quantity;

        public SortingResult(ulong sourceRetainerId, ulong destinationRetainerId, InventoryType sourceBag, InventoryCategory destinationBag, InventoryItem inventoryItem, int quantity)
        {
            _sourceRetainerId = sourceRetainerId;
            _destinationRetainerId = destinationRetainerId;
            _sourceBag = sourceBag;
            _destinationBag = destinationBag;
            _inventoryItem = inventoryItem;
            _quantity = quantity;
        }

        public SortingResult(ulong sourceRetainerId, InventoryType sourceBag, InventoryItem inventoryItem, int quantity)
        {
            _sourceRetainerId = sourceRetainerId;
            _sourceBag = sourceBag;
            _inventoryItem = inventoryItem;
            _quantity = quantity;
            _destinationBag = null;
            _destinationRetainerId = null;
        }

        public string GetExtraInformation()
        {
            string info = "";
            if (InventoryItem.Item != null)
            {
                info += (InventoryItem.CanBeBought ? "Can be bought" : "Can't be bought") + '\n';

            }

            return info;
        }
    }
}