using AllaganLib.GameSheets.Model;

namespace InventoryTools.Compendium.Interfaces;

public interface IItem
{
    public ItemInfo? GetItem(uint rowId);
}