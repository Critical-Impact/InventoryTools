using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class EphemeralNodeFilter : BooleanFilter
    {
        public override string Key { get; set; } = "EphemeralNode";
        public override string Name { get; set; } = "Is Ephemeral Node?";
        public override string HelpText { get; set; } = "Is the item available in ephemeral nodes?";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Gathering;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;

            if(currentValue.Value && item.HasSourcesByCategory(ItemInfoCategory.EphemeralGathering))
            {
                return true;
            }

            return !currentValue.Value && !item.HasSourcesByCategory(ItemInfoCategory.EphemeralGathering);
        }

        public EphemeralNodeFilter(ILogger<EphemeralNodeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}