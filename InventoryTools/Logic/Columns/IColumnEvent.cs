using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using InventoryItem = CriticalCommonLib.Models.InventoryItem;

namespace InventoryTools.Logic.Columns
{
    public interface IColumnEvent
    {
        public void HandleEvent(SortingResult result);
        public void HandleEvent(InventoryItem inventoryItem);
        public void HandleEvent(Item item);
    }
}