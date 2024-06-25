using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsTimedNodeFilter : BooleanFilter
    {
        public override string Key { get; set; } = "TimedNode";
        public override string Name { get; set; } = "Is Timed Node?";
        public override string HelpText { get; set; } = "Is the item available in timed nodes?";
        
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Gathering;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;
            
            if(currentValue.Value && item.Item.IsItemAvailableAtTimedNode)
            {
                return true;
            }
                
            return !currentValue.Value && !item.Item.IsItemAvailableAtTimedNode;

        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;
            
            if(currentValue.Value && item.IsItemAvailableAtTimedNode)
            {
                return true;
            }
                
            return !currentValue.Value && !item.IsItemAvailableAtTimedNode;
        }

        public IsTimedNodeFilter(ILogger<IsTimedNodeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}