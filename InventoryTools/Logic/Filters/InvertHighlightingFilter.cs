using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class InvertHighlightingFilter : BooleanFilter
    {
        public override string Key { get; set; } = "InvertHighlighting";
        public override string Name { get; set; } = "Invert Highlighting?";
        public override string HelpText { get; set; } = "Should all the items not matching the filter be highlighted instead? If set to N/A will use the 'Invert Highlighting' setting inside the general configuration.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        
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
    }
}