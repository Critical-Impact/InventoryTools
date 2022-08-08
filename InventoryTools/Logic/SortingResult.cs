using System.Numerics;
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

        public InventoryType? DestinationBag => _destinationBag;

        public InventoryItem InventoryItem => _inventoryItem;
        
        public Vector2? DestinationSlot => _destinationSlot;
        public bool? IsEmptyDestinationSlot => _isEmptyDestinationSlot;

        public int Quantity => _quantity;

        private ulong? _destinationRetainerId;
        private InventoryType _sourceBag;
        private InventoryType? _destinationBag;
        private InventoryItem _inventoryItem;
        private Vector2? _destinationSlot;
        private bool? _isEmptyDestinationSlot;
        private int _quantity;

        public SortingResult(ulong sourceRetainerId, ulong destinationRetainerId, InventoryType sourceBag, InventoryType destinationBag, Vector2 destinationSlot, bool isEmptyDestinationSlot, InventoryItem inventoryItem, int quantity)
        {
            _sourceRetainerId = sourceRetainerId;
            _destinationRetainerId = destinationRetainerId;
            _sourceBag = sourceBag;
            _destinationSlot = destinationSlot;
            _destinationBag = destinationBag;
            _inventoryItem = inventoryItem;
            _quantity = quantity;
            _isEmptyDestinationSlot = isEmptyDestinationSlot;
        }

        public SortingResult(ulong sourceRetainerId, InventoryType sourceBag, InventoryItem inventoryItem, int quantity)
        {
            _sourceRetainerId = sourceRetainerId;
            _sourceBag = sourceBag;
            _inventoryItem = inventoryItem;
            _quantity = quantity;
            _destinationBag = null;
            _destinationRetainerId = null;
            _destinationSlot = null;
            _isEmptyDestinationSlot = null;
        }
        
        public Vector2 BagLocation => InventoryItem.BagLocation(_sourceBag);

        public string GetExtraInformation()
        {
            string info = "";
            info += (InventoryItem.Item.ObtainedGil ? "Can be bought" : "Can't be bought") + '\n';
            return info;
        }
    }
}