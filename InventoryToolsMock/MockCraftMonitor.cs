using CriticalCommonLib.Agents;
using CriticalCommonLib.Crafting;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;

namespace InventoryToolsMock;

public class MockCraftMonitor : ICraftMonitor
{
    public void Dispose()
    {
        
    }

    public void CompleteCraft(uint itemId, bool isHq)
    {
        CraftCompleted?.Invoke(itemId, isHq ? InventoryItem.ItemFlags.HQ : InventoryItem.ItemFlags.None, 1);
    }

    public event CraftMonitor.CraftStartedDelegate? CraftStarted;
    public event CraftMonitor.CraftFailedDelegate? CraftFailed;
    public event CraftMonitor.CraftCompletedDelegate? CraftCompleted;

    public CraftingAgent? Agent
    {
        get
        {
            return null;
        }
    }

    public SimpleCraftingAgent? SimpleAgent
    {
        get
        {
            return null;
        }
    }

    public Recipe? CurrentRecipe
    {
        get
        {
            return null;
        }
    }

    public RecipeLevelTable? RecipeLevelTable
    {
        get
        {
            return null;
        }
    }
}