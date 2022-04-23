using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class DestinationInventoriesFilter : MultipleChoiceFilter<(ulong, InventoryCategory)>
    {
        public override string Key { get; set; } = "DestinationInventories";
        public override string Name { get; set; } = "Destination Inventories";
        public override string HelpText { get; set; } =
            "This is a list of destinations to sort items from source into based on the filter configuration.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        public override FilterType AvailableIn { get; set; } = FilterType.SortingFilter;
        
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return true;
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            return true;
        }

        public override Dictionary<(ulong, InventoryCategory), string> GetChoices(FilterConfiguration configuration)
        {
            var allCharacters = PluginService.CharacterMonitor.AllCharacters();
            if (!PluginLogic.PluginConfiguration.DisplayCrossCharacter)
            {
                allCharacters = allCharacters.Where(c =>
                    PluginService.CharacterMonitor.BelongsToActiveCharacter(c.Key)).ToArray();
            }
            
            var dict = new Dictionary<(ulong, InventoryCategory), string>();
            foreach (var character in allCharacters)
            {
                if (PluginService.CharacterMonitor.IsRetainer(character.Key))
                {
                    dict.Add((character.Key, InventoryCategory.RetainerBags), character.Value.Name + " - Bags");
                    dict.Add((character.Key, InventoryCategory.RetainerMarket), character.Value.Name + " - Market");
                }
                else
                {
                    dict.Add((character.Key, InventoryCategory.CharacterBags), character.Value.Name + " - Bags");
                    dict.Add((character.Key, InventoryCategory.CharacterSaddleBags), character.Value.Name + " - Saddle Bags");
                    dict.Add((character.Key, InventoryCategory.CharacterPremiumSaddleBags), character.Value.Name + " - Premium Saddle Bags");
                    dict.Add((character.Key, InventoryCategory.FreeCompanyBags), character.Value.Name + " - Free Company Bags");
                    dict.Add((character.Key, InventoryCategory.CharacterArmoryChest), character.Value.Name + " - Armoury Chest");
                   
                }
            }

            return dict;
        }

        public override List<(ulong, InventoryCategory)> CurrentValue(FilterConfiguration configuration)
        {
            return configuration.DestinationInventories.ToList();
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<(ulong, InventoryCategory)> newValue)
        {
            List<(ulong, InventoryCategory)> newDestinationInventories = new();
            foreach (var item in newValue)
            {
                newDestinationInventories.Add(item);
            }

            configuration.DestinationInventories = newDestinationInventories;
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}