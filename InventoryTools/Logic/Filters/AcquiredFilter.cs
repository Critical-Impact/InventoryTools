using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

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

        public override bool FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            if (item.Item == null)
            {
                return false;
            }
            return FilterItem(configuration, item.Item);
        }

        public override bool FilterItem(FilterConfiguration configuration,Item item)
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
            return currentValue.Value && GameInterface.HasAcquired(item) || !currentValue.Value && !GameInterface.HasAcquired(item);
        }
    }
}