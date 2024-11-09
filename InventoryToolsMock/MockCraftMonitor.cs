using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Agents;
using CriticalCommonLib.Crafting;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;


namespace InventoryToolsMock;

public class MockCraftMonitor : ICraftMonitor
{
    public void Dispose()
    {

    }

    public void CompleteCraft(uint itemId, bool isHq, uint quantity)
    {
        CraftCompleted?.Invoke(itemId, isHq ? InventoryItem.ItemFlags.HighQuality : InventoryItem.ItemFlags.None, quantity);
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

    public RecipeRow? CurrentRecipe
    {
        get
        {
            return null;
        }
    }

    public RecipeLevelTableRow? RecipeLevelTable
    {
        get
        {
            return null;
        }
    }
}