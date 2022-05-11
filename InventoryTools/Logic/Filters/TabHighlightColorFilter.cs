using System.Numerics;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class TabHighlightColorFilter : ColorFilter
    {
        public override string Key { get; set; } = "TabHighlightColor";
        public override string Name { get; set; } = "Tab Highlight Color";

        public override string HelpText { get; set; } =
            "The color to set the highlighted tabs(which contain filtered items) to for this specific filter.";

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

        public override Vector4? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.TabHighlightColor;
        }

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return configuration.TabHighlightColor != null;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, Vector4? newValue)
        {
            configuration.TabHighlightColor = newValue;
        }
    }
}