using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class DestinationAllFreeCompaniesFilter : BooleanFilter
    {
        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "DestinationAllFreeCompanies";
        public override string Name { get; set; } = "Destination - All Free Companies?";
        public override string HelpText { get; set; } = "Use every free company's inventory as a destination.";
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
            return configuration.DestinationAllFreeCompanies;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.DestinationAllFreeCompanies = newValue;
        }

        public DestinationAllFreeCompaniesFilter(ILogger<DestinationAllFreeCompaniesFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}