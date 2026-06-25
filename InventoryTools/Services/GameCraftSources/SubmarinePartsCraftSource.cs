using System.Collections.Generic;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;

namespace InventoryTools.Services.GameCraftSources
{
    public class SubmarinePartsCraftSource : IGameCraftSource
    {
        private readonly IGameUiManager _gameUiManager;

        public SubmarinePartsCraftSource(IGameUiManager gameUiManager)
        {
            _gameUiManager = gameUiManager;
        }

        public IReadOnlyList<GameCraftCategory> GetAvailableCategories()
        {
            if (!_gameUiManager.IsWindowVisible(WindowName.SubmarinePartsMenu))
            {
                return [];
            }

            return [new GameCraftCategory("Submarine Parts", GetItems)];
        }

        private unsafe IReadOnlyList<GameCraftItem> GetItems()
        {
            var items = new List<GameCraftItem>();
            var window = _gameUiManager.GetWindow("SubmarinePartsMenu");
            if (window == null)
            {
                return items;
            }

            var addon = (SubmarinePartsMenuAddon*)window;
            for (byte i = 0; i < 6; i++)
            {
                var part = addon->GetItem(i);
                if (part != null)
                {
                    var amountLeft = part.Value.QtyRemaining;
                    if (amountLeft > 0)
                    {
                        items.Add(new GameCraftItem(part.Value.ItemId, amountLeft));
                    }
                }
            }

            return items;
        }
    }
}