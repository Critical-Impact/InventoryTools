using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class CanBeGatheredFilter : BooleanFilter
    {
        public override string Key { get; set; } = "Gatherable";
        public override string Name { get; set; } = "Can be Gathered?";
        public override string HelpText { get; set; } = "Can this item be gathered from a node?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            if (item.Item == null)
            {
                return false;
            }
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = CurrentValue(configuration);
            var canBeGathered = ExcelCache.CanBeGathered(item.RowId);
            return currentValue == null || currentValue.Value && canBeGathered || !currentValue.Value && !canBeGathered;
        }
    }
}