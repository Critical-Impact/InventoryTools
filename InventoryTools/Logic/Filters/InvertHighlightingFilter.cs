using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class InvertHighlightingFilter : BooleanFilter
    {
        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.CraftFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter | FilterType.CuratedList;
        public override string Key { get; set; } = "InvertHighlighting";
        public override string Name { get; set; } = "Invert Highlighting?";
        public override string HelpText { get; set; } = "Should all the items not matching the filter be highlighted instead? If set to N/A will use the 'Invert Highlighting' setting inside the general configuration.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

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
            return configuration.InvertHighlighting;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.InvertHighlighting = newValue;
        }

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return configuration.InvertHighlighting != null;
        }

        public InvertHighlightingFilter(ILogger<InvertHighlightingFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}