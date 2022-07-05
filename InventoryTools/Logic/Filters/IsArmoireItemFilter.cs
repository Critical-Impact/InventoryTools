using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class IsArmoireItemFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsArmoire";
        public override string Name { get; set; } = "Is Armoire Item?";
        public override string HelpText { get; set; } = "Only show items that can be put into the armoire.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => Service.ExcelCache.IsArmoireItem(item.Item.RowId),
                _ => !Service.ExcelCache.IsArmoireItem(item.Item.RowId)
            };
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => Service.ExcelCache.IsArmoireItem(item.RowId),
                _ => !Service.ExcelCache.IsArmoireItem(item.RowId)
            };
        }
    }
}