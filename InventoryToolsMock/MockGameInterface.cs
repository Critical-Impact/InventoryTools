using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using Dalamud.Logging;

namespace InventoryToolsMock;

public class MockGameInterface : IGameInterface
{
    public void Dispose()
    {
    }

    public event GameInterface.AcquiredItemsUpdatedDelegate? AcquiredItemsUpdated;

    public HashSet<uint> AcquiredItems
    {
        get
        {
            return new HashSet<uint>();
        }
        set
        {
            
        }
    }

    public bool IsGatheringItemGathered(uint item)
    {
        return false;
    }

    public bool? IsItemGathered(uint itemId)
    {
        return false;
    }

    public unsafe void OpenGatheringLog(uint itemId)
    {
        PluginLog.Log("Game Event: Gathering Log Opened for Item #" + itemId);
    }

    public unsafe void OpenFishingLog(uint itemId, bool isSpearFishing)
    {
        PluginLog.Log("Game Event: Fishing Log Opened for Item #" + itemId);
    }

    public unsafe bool HasAcquired(ItemEx item, bool debug = false)
    {
        return false;
    }

    public unsafe bool IsInArmoire(uint itemId)
    {
        return false;
    }

    public uint? ArmoireIndexIfPresent(uint itemId)
    {
        return null;
    }

    public unsafe void OpenCraftingLog(uint itemId)
    {
        PluginLog.Log("Game Event: Crafting Log Opened for Item #" + itemId);
    }

    public unsafe void OpenCraftingLog(uint itemId, uint recipeId)
    {
        PluginLog.Log("Game Event: Crafting Log Opened for Item #" + itemId + " and recipe " + recipeId);
    }
}