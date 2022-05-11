using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Misc;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class IsCraftingItemFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsCrafting";
        public override string Name { get; set; } = "Is Crafting Item?";
        public override string HelpText { get; set; } = "Only show items that relate to crafting.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

        
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.Item != null && Helpers.CraftingMaterialIds.Contains(item.Item.ItemUICategory.Row),
                _ => item.Item != null && !Helpers.CraftingMaterialIds.Contains(item.Item.ItemUICategory.Row)
            };
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => Helpers.CraftingMaterialIds.Contains(item.ItemUICategory.Row),
                _ => !Helpers.CraftingMaterialIds.Contains(item.ItemUICategory.Row)
            };
        }
    }
}