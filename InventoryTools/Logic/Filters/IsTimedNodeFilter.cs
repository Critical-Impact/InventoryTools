using AllaganLib.GameSheets.Sheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsTimedNodeFilter : BooleanFilter
    {
        public IsTimedNodeFilter(ILogger<IsTimedNodeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override string Key { get; set; } = "TimedNode";
        public override string Name { get; set; } = "Is Timed Node?";
        public override string HelpText { get; set; } = "Is the item available in timed nodes?";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Gathering;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;

            if(currentValue.Value && item.Item.HasSourcesByCategory(ItemInfoCategory.TimedGathering))
            {
                return true;
            }

            return !currentValue.Value && !item.Item.HasSourcesByCategory(ItemInfoCategory.TimedGathering);

        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;

            if(currentValue.Value && item.HasSourcesByCategory(ItemInfoCategory.TimedGathering))
            {
                return true;
            }

            return !currentValue.Value && !item.HasSourcesByCategory(ItemInfoCategory.TimedGathering);
        }
    }
}