using FFXIVClientStructs.FFXIV.Client.Game;

namespace InventoryTools.Misc
{

    public static class Utils
    {
        public static string ItemFlagsToTypeString(InventoryItem.ItemFlags flags)
        {
            return ((flags & InventoryItem.ItemFlags.HQ) != 0) ? "\uE03c" :
                ((flags & InventoryItem.ItemFlags.Collectable) != 0) ? "\uE03d" : "";;
        }
    }
}