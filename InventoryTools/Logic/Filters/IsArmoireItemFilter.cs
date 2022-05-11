using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Misc;
using Lumina.Excel.GeneratedSheets;

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

        
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.Item != null && ExcelCache.IsArmoireItem(item.Item.RowId),
                _ => item.Item != null && !ExcelCache.IsArmoireItem(item.Item.RowId)
            };
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => ExcelCache.IsArmoireItem(item.RowId),
                _ => !ExcelCache.IsArmoireItem(item.RowId)
            };
        }
    }
}