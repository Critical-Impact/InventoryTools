using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class DestinationIncludeCrossCharacterFilter : BooleanFilter
    {
        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "DestinationIncludeCrossCharacter";
        public override string Name { get; set; } = "Destination - Cross Character?";
        public override string HelpText { get; set; } = "Should items be sorted cross character? Will default to using the default configuration in the main allagan tools configuration if not selected.";
        public override FilterType AvailableIn { get; set; } = FilterType.SortingFilter | FilterType.CraftFilter;
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;

        public override bool? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.DestinationIncludeCrossCharacter;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.DestinationIncludeCrossCharacter = newValue;
        }

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public DestinationIncludeCrossCharacterFilter(ILogger<DestinationIncludeCrossCharacterFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}