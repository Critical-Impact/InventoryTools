using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class TableCraftFreezeRowsFilter : IntegerFilter
    {
        public override string Key { get; set; } = "TableCraftFreezeRows";
        public override string Name { get; set; } = "Freeze Columns";

        public override string HelpText { get; set; } =
            "The number of columns starting at 1 to freeze(always display when scrolling).";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.CraftColumns;

        public override FilterType AvailableIn { get; set; } =
            FilterType.CraftFilter;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, int? newValue)
        {
            configuration.FreezeCraftColumns = newValue;
        }

        public override int? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.FreezeCraftColumns;
        }

        public TableCraftFreezeRowsFilter(ILogger<TableCraftFreezeRowsFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}