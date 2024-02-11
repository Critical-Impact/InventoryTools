using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class CanBePlacedOnMarketFilter : BooleanFilter
    {
        public override string Key { get; set; } = "CanBePlacedOnMarket";
        public override string Name { get; set; } = "Can be Placed on Market?";
        public override string HelpText { get; set; } = "Can this item be placed on the market?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);

            return currentValue == null || currentValue.Value && item.CanBePlacedOnMarket || !currentValue.Value && !item.CanBePlacedOnMarket;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            return currentValue == null || currentValue.Value && item.CanBePlacedOnMarket || !currentValue.Value && !item.CanBePlacedOnMarket;
        }
    }
}