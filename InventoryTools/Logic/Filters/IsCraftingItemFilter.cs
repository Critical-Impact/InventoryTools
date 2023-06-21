using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class IsCraftingItemFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsCrafting";
        public override string Name { get; set; } = "Is Crafting Item?";
        public override string HelpText { get; set; } = "Only show items that relate to crafting.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;

        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => null,
                true => Service.ExcelCache.IsCraftItem(item.Item.RowId),
                _ =>  !Service.ExcelCache.IsCraftItem(item.Item.RowId)
            };
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = this.CurrentValue(configuration);
            
            return currentValue switch
            {
                null => null,
                true => Service.ExcelCache.IsCraftItem(item.RowId),
                _ => !Service.ExcelCache.IsCraftItem(item.RowId)
            };
        }
    }
}