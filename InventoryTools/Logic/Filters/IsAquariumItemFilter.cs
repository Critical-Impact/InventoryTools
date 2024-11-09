using AllaganLib.GameSheets.Sheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

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

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return null;
            }

            return currentValue.Value && item.HasUsesByType(ItemInfoType.Aquarium) || !currentValue.Value && !item.HasUsesByType(ItemInfoType.Aquarium);
        }

        public IsAquariumItemFilter(ILogger<InvertTabHighlightingFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}