using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using Dalamud.Logging;
using Dalamud.Plugin.Services;

namespace InventoryToolsMock;

public class MockGameInterface : IGameInterface
{
    private readonly IPluginLog _pluginLog;

    public MockGameInterface(IPluginLog pluginLog)
    {
        _pluginLog = pluginLog;
    }
    
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
        _pluginLog.Info("Game Event: Gathering Log Opened for Item #" + itemId);
    }

    public unsafe void OpenFishingLog(uint itemId, bool isSpearFishing)
    {
        _pluginLog.Info("Game Event: Fishing Log Opened for Item #" + itemId);
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
        _pluginLog.Info("Game Event: Crafting Log Opened for Item #" + itemId);
    }

    public unsafe void OpenCraftingLog(uint itemId, uint recipeId)
    {
        _pluginLog.Info("Game Event: Crafting Log Opened for Item #" + itemId + " and recipe " + recipeId);
    }
}