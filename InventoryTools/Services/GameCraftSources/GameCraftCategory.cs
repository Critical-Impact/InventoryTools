using System;
using System.Collections.Generic;

namespace InventoryTools.Services.GameCraftSources
{
    public class GameCraftCategory
    {
        public GameCraftCategory(string name, Func<IReadOnlyList<GameCraftItem>> getItems)
        {
            Name = name;
            GetItems = getItems;
        }

        public string Name { get; }

        public Func<IReadOnlyList<GameCraftItem>> GetItems { get; }
    }
}