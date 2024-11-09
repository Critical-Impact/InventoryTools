using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class DestinationAllHousesFilter : BooleanFilter
    {
        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "DestinationAllFreeHouses";
        public override string Name { get; set; } = "Destination - All Houses?";
        public override string HelpText { get; set; } = "Use every house inventory as a destination.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        public override FilterType AvailableIn { get; set; } =  FilterType.SortingFilter | FilterType.CraftFilter;
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public override bool? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.DestinationAllHouses;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.DestinationAllHouses = newValue;
        }

        public DestinationAllHousesFilter(ILogger<DestinationAllHousesFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}