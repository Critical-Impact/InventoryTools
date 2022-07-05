using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class CanBePurchasedFilter : BooleanFilter
    {
        public override string Key { get; set; } = "Purchasable";
        public override string Name { get; set; } = "Can be Purchased?";
        public override string HelpText { get; set; } = "Can this be item be purchased?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {

            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            var canBePurchased = item.CanBeBoughtWithGil;
            return currentValue == null || currentValue.Value && canBePurchased || !currentValue.Value && !canBePurchased;
        }
    }
}