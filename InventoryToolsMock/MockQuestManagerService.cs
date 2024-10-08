using InventoryTools.Services;

namespace InventoryToolsMock;

public class MockQuestManagerService : IQuestManagerService
{
    public bool IsRecipeComplete(uint recipeId)
    {
        return recipeId % 2 == 0;
    }
}