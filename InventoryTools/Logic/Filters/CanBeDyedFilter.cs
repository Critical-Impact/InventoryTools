using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class CanBeDyedFilter : BooleanFilter
    {
        public override string Key { get; set; } = "CanBeDyed";
        public override string Name { get; set; } = "Can be Dyed?";
        public override string HelpText { get; set; } = "Can this be item be dyed?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            var canByDyed = item.IsDyeable;
            return currentValue == null || currentValue.Value && canByDyed || !currentValue.Value && !canByDyed;
        }
    }
}