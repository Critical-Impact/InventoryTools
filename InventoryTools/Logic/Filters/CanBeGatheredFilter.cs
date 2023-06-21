using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class CanBeGatheredFilter : BooleanFilter
    {
        public override string Key { get; set; } = "Gatherable";
        public override string Name { get; set; } = "Can be Gathered?";
        public override string HelpText { get; set; } = "Can this item be gathered from a node or caught?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {

            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            var canBeGathered = item.CanBeGathered || item.ObtainedFishing;
            return currentValue == null || currentValue.Value && canBeGathered || !currentValue.Value && !canBeGathered;
        }
    }
}