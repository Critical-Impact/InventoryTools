using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Services.GameCraftSources
{
    public readonly struct GameCraftItem
    {
        public GameCraftItem(uint itemId, uint quantity, InventoryItem.ItemFlags flags = InventoryItem.ItemFlags.None)
        {
            ItemId = itemId;
            Quantity = quantity;
            Flags = flags;
        }

        public uint ItemId { get; }
        public uint Quantity { get; }
        public InventoryItem.ItemFlags Flags { get; }
    }
}