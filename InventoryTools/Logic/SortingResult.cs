using System.Numerics;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;


namespace InventoryTools.Logic
{
    public class SortingResult : IItem
    {
        private ulong _sourceRetainerId;

        public ulong SourceRetainerId => _sourceRetainerId;

        public ulong? DestinationRetainerId => _destinationRetainerId;

        public InventoryType SourceBag => _sourceBag;

        public InventoryType? DestinationBag => _destinationBag;

        public InventoryItem InventoryItem => _inventoryItem;
        public InventoryItem DestinationItem => _destinationItem;

        public Vector2? DestinationSlot => _destinationSlot;
        public bool? IsEmptyDestinationSlot => _isEmptyDestinationSlot;
        public int Quantity => _quantity;
        public bool IsSortable => _isSortable;

        private ulong? _destinationRetainerId;
        private InventoryType _sourceBag;
        private InventoryType? _destinationBag;
        private InventoryItem _inventoryItem;
        private InventoryItem _destinationItem;
        private Vector2? _destinationSlot;
        private bool? _isEmptyDestinationSlot;
        private int _quantity;
        private bool _isSortable;

        public SortingResult(ulong sourceRetainerId, ulong destinationRetainerId, InventoryType sourceBag, InventoryType destinationBag, Vector2 destinationSlot, bool isEmptyDestinationSlot, InventoryItem inventoryItem, InventoryItem destinationItem, int quantity)
        {
            _sourceRetainerId = sourceRetainerId;
            _destinationRetainerId = destinationRetainerId;
            _sourceBag = sourceBag;
            _destinationSlot = destinationSlot;
            _destinationBag = destinationBag;
            _inventoryItem = inventoryItem;
            _destinationItem = destinationItem;
            _quantity = quantity;
            _isEmptyDestinationSlot = isEmptyDestinationSlot;
            _isSortable = true;
        }

        public SortingResult(ulong sourceRetainerId, InventoryType sourceBag, InventoryItem inventoryItem, int quantity, bool isSortable = true)
        {
            _sourceRetainerId = sourceRetainerId;
            _sourceBag = sourceBag;
            _inventoryItem = inventoryItem;
            _quantity = quantity;
            _destinationBag = null;
            _destinationRetainerId = null;
            _destinationSlot = null;
            _isEmptyDestinationSlot = null;
            _isSortable = isSortable;
        }

        public Vector2 BagLocation => InventoryItem.BagLocation(_sourceBag);

        public uint ItemId {
            get => InventoryItem.ItemId;
            set
            {

            }
        }

        public ItemRow Item => InventoryItem.Item;
    }
}