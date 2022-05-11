using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class DuplicatesOnlyFilter : BooleanFilter
    {
        public override string Key { get; set; } = "DuplicatesOnly";
        public override string Name { get; set; } = "Duplicates Only?";

        public override string HelpText { get; set; } =
            "Filter out any items that do not appear in both the source and destination?";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;
        public override FilterType AvailableIn { get; set; } = FilterType.SortingFilter | FilterType.SearchFilter;
        
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
            return configuration.DuplicatesOnly;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.DuplicatesOnly = newValue;
        }
    }
}