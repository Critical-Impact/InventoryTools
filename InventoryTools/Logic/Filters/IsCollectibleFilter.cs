using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsCollectibleFilter : BooleanFilter
    {
        public override string Key { get; set; } = "Collectible";
        public override string Name { get; set; } = "Is Collectible?";
        public override string HelpText { get; set; } = "Is the item Collectible?";
        
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Gathering;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;
            
            if(currentValue.Value && item.IsCollectible)
            {
                return true;
            }
                
            return !currentValue.Value && !item.IsCollectible;

        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;
            
            if(currentValue.Value && item.IsCollectable)
            {
                return true;
            }
                
            return !currentValue.Value && !item.IsCollectable;
        }

        public IsCollectibleFilter(ILogger<IsCollectibleFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}