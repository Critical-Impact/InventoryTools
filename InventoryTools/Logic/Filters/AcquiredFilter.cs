using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class AcquiredFilter : BooleanFilter
    {
        public override string Key { get; set; } = "Acquired";
        public override string Name { get; set; } = "Is Acquired?";
        public override string HelpText { get; set; } = "Has this item be acquired?";
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
            if (currentValue == null)
            {
                return true;
            }
            var action = item.ItemAction?.Value;
            if (!ActionTypeExt.IsValidAction(action)) {
                return false;
            }
            return currentValue.Value && PluginService.GameInterface.HasAcquired(item) || !currentValue.Value && !PluginService.GameInterface.HasAcquired(item);
        }
    }
}