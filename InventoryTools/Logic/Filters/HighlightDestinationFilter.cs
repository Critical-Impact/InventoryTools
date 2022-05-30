using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class HighlightDestinationFilter : BooleanFilter
    {
        public override string Key { get; set; } = "HighlightDestination";
        public override string Name { get; set; } = "Highlight Destination Duplicates?";
        public override string HelpText { get; set; } =
            "Should any items that match in the destination bag be highlighted?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;
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
            return configuration.HighlightDestination;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.HighlightDestination = newValue;
        }
    }
}