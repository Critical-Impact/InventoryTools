using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class DisplayFilterInRetainersFilter : BooleanFilter
    {
        public override string Key { get; set; } = "FilterInRetainers";
        public override string Name { get; set; } = "Filter Items when in Retainer?";

        public override string HelpText { get; set; } =
            "When talking with a retainer should the filter adjust itself to only show items that should be put inside the retainer from your inventory?";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;
        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return true;
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            return true;
        }

        public override bool? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.FilterItemsInRetainers;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.FilterItemsInRetainers = newValue;
        }
    }
}