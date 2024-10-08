using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class TableFreezeRowsFilter : IntegerFilter
    {
        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.CraftFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter | FilterType.CuratedList;
        public override string Key { get; set; } = "TableFreezeRows";
        public override string Name { get; set; } = "Freeze Columns";

        public override string HelpText { get; set; } =
            "The number of columns starting at 1 to freeze(always display when scrolling).";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Columns;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
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

        public TableFreezeRowsFilter(ILogger<TableFreezeRowsFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}