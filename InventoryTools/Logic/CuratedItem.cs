using FFXIVClientStructs.FFXIV.Client.Game;

namespace InventoryTools.Logic
{
    public class CuratedItem
    {
        public CuratedItem(uint itemId, uint quantity, InventoryItem.ItemFlags itemFlags)
        {
            ItemId = itemId;
            Quantity = quantity;
            ItemFlags = itemFlags;
        }

        public CuratedItem(uint itemId)
        {
            ItemId = itemId;
            Quantity = 0;
            ItemFlags = InventoryItem.ItemFlags.None;
        }

        public CuratedItem()
        {
            
        }

        public uint ItemId { get; set; }
        public uint Quantity { get; set; }
        public InventoryItem.ItemFlags ItemFlags { get; set; }
    }
}