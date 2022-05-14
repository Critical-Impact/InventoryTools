using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class SourceInventoryCategoriesFilter : MultipleChoiceFilter<InventoryCategory>
    {
        public override List<InventoryCategory> CurrentValue(FilterConfiguration configuration)
        {
            return configuration.SourceCategories?.ToList() ?? new List<InventoryCategory>();
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<InventoryCategory> newValue)
        {
            configuration.SourceCategories = newValue.Count == 0 ? null : newValue.Distinct().ToHashSet();
        }
        
        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "SourceInventoryCategories";
        public override string Name { get; set; } = "Source - Inventory Categories";
        public override string HelpText { get; set; } =
            "This is a list of sources categories to search in. It will attempt to search for items in any bag of the given category.";
        
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter;
        
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return true;
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            return true;
        }

        public override Dictionary<InventoryCategory, string> GetChoices(FilterConfiguration configuration)
        {
           
            var dict = new Dictionary<InventoryCategory, string>();
            dict.Add(InventoryCategory.RetainerBags, "Retainer " + InventoryCategory.RetainerBags.FormattedName());
            dict.Add(InventoryCategory.RetainerMarket, "Retainer " + InventoryCategory.RetainerMarket.FormattedName());
            dict.Add(InventoryCategory.CharacterBags, InventoryCategory.CharacterBags.FormattedName());
            dict.Add(InventoryCategory.CharacterSaddleBags, InventoryCategory.CharacterSaddleBags.FormattedName());
            dict.Add(InventoryCategory.CharacterPremiumSaddleBags, InventoryCategory.CharacterPremiumSaddleBags.FormattedName());
            dict.Add(InventoryCategory.FreeCompanyBags, InventoryCategory.FreeCompanyBags.FormattedName());
            dict.Add(InventoryCategory.CharacterArmoryChest, InventoryCategory.CharacterArmoryChest.FormattedName());
            dict.Add(InventoryCategory.GlamourChest, InventoryCategory.GlamourChest.FormattedName());
            dict.Add(InventoryCategory.Armoire, InventoryCategory.Armoire.FormattedName());
            return dict;
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}