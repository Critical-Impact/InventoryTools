using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

namespace InventoryToolsMock;

public class MockInventoryMonitor : IInventoryMonitor
{
    public void Dispose()
    {
        
    }

    public Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> Inventories
    {
        get
        {
            return new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();
        }
    }

    public IEnumerable<InventoryItem> AllItems
    {
        get
        {
            return new List<InventoryItem>();
        }
    }
    public Dictionary<(uint, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags, ulong), int> ItemCounts
    {
        get
        {
            return new Dictionary<(uint, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags, ulong), int>();
        }
    }

    public event InventoryMonitor.InventoryChangedDelegate? OnInventoryChanged;
    public List<InventoryItem> GetSpecificInventory(ulong characterId, InventoryCategory category)
    {
        return new List<InventoryItem>();
    }

    public void ClearCharacterInventories(ulong characterId)
    {
        
    }

    public void LoadExistingData(Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> inventories)
    {
        
    }
}