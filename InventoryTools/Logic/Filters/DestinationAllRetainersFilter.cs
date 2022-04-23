using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class DestinationAllRetainersFilter : BooleanFilter
    {
        public override string Key { get; set; } = "DestinationAllRetainers";
        public override string Name { get; set; } = "Destination from all Retainers?";
        public override string HelpText { get; set; } = "Use every retainer's inventory as a destination.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        public override FilterType AvailableIn { get; set; } = FilterType.SortingFilter;
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
            return configuration.DestinationAllRetainers;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.DestinationAllRetainers = newValue;
        }
    }
}