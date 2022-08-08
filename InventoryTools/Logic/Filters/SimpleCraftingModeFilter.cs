using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class SimpleCraftingModeFilter : BooleanFilter
    {
        public override string Key { get; set; } = "SimpleCrafting";
        public override string Name { get; set; } = "Simple Crafting?";

        public override string HelpText { get; set; } =
            "Should the craft columns shown in the crafting list be simplified?";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;
        public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
        }

        public override bool? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.SimpleCraftingMode;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
        {
            configuration.SimpleCraftingMode = newValue;
        }
    }
}