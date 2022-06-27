using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class TableFreezeRowsFilter : IntegerFilter
    {
        public override string Key { get; set; } = "TableFreezeRows";
        public override string Name { get; set; } = "Freeze Columns";

        public override string HelpText { get; set; } =
            "The number of columns starting at 1 to freeze(always display when scrolling).";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Columns;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, Item item)
        {
            return null;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, int? newValue)
        {
            configuration.FreezeColumns = newValue;
        }

        public override int? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.FreezeColumns;
        }
    }
}