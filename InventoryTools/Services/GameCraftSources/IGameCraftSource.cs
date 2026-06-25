using System.Collections.Generic;

namespace InventoryTools.Services.GameCraftSources
{
    public interface IGameCraftSource
    {
        IReadOnlyList<GameCraftCategory> GetAvailableCategories();
    }
}