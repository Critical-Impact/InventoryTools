using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class SourceIncludeCrossCharacterFilter : BooleanFilter
    {
        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "SourceIncludeCrossCharacter";
        public override string Name { get; set; } = "Source - Cross Character?";
        public override string HelpText { get; set; } = "Should items be sourced from cross character? Will default to using the default configuration in the main inventory tools configuration if not selected.";
        public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.CraftFilter;
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;

        public override bool? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.SourceIncludeCrossCharacter;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.SourceIncludeCrossCharacter = newValue;
        }

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, Item item)
        {
            return null;
        }
    }
}