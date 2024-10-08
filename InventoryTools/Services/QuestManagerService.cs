using FFXIVClientStructs.FFXIV.Client.Game;

namespace InventoryTools.Services;

public class QuestManagerService : IQuestManagerService
{
    public unsafe bool IsRecipeComplete(uint recipeId)
    {
        return QuestManager.IsRecipeComplete(recipeId);
    }
}

public interface IQuestManagerService
{
    public bool IsRecipeComplete(uint recipeId);
}