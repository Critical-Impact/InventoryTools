using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsAquariumItemFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsAquarium";
        public override string Name { get; set; } = "Is Aquarium Item?";
        public override string HelpText { get; set; } = "Can this item be put into a aquarium?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;


        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return null;
            }

            return currentValue.Value && item.IsAquariumItem || !currentValue.Value && !item.IsAquariumItem;
        }

        public IsAquariumItemFilter(ILogger<InvertTabHighlightingFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}