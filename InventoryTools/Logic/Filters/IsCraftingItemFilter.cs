using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
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

        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => null,
                true => item.Item != null && ExcelCache.IsCraftItem(item.Item.RowId),
                _ => item.Item != null && !ExcelCache.IsCraftItem(item.Item.RowId)
            };
        }

        public override bool? FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = this.CurrentValue(configuration);
            
            return currentValue switch
            {
                null => null,
                true => ExcelCache.IsCraftItem(item.RowId),
                _ => !ExcelCache.IsCraftItem(item.RowId)
            };
        }
    }
}