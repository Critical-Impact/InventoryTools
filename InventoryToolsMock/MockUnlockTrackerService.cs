using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Services;

namespace InventoryToolsMock;

public class MockUnlockTrackerService : IUnlockTrackerService
{
    public event IUnlockTrackerService.ItemUnlockStatusChangedDelegate? ItemUnlockStatusChanged;
    public HashSet<uint> UnlockedItems { get; set; } = new();

    public bool? IsUnlocked(ItemRow item, bool notify)
    {
        return null;
    }

    public void QueueUnlockCheck(uint itemId)
    {

    }

    public void QueueUnlockCheck(ItemRow item)
    {

    }
}