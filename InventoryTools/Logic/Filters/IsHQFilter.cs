using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsHqFilter : BooleanFilter
    {
        public override string Key { get; set; } = "HQ";
        public override string Name { get; set; } = "Is HQ?";
        public override string HelpText { get; set; } = "Is the item High Quality?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;
            
            if(currentValue.Value && item.IsHQ)
            {
                return true;
            }
                
            return !currentValue.Value && !item.IsHQ;

        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return true;
        }

        public IsHqFilter(ILogger<IsHqFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}